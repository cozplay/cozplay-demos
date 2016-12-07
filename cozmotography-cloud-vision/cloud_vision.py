import os
import asyncio
import cozmo
import PIL.Image
import base64
import time
import PIL.ImageFont

from googleapiclient import discovery
from oauth2client.client import GoogleCredentials

# Run Configuration
os.environ['COZMO_PROTOCOL_LOG_LEVEL'] = 'DEBUG'
os.environ['COZMO_LOG_LEVEL'] = 'DEBUG'
USE_VIEWER = True
USE_LOGGING = False
USE_FILTER = True
CUBE_SEARCH_TIMEOUT = 3
DISPLAY_TEXT_DURATION = 0.75
# Note: To use a custom font, uncomment the below line, set path/size, and restore font parameter on line 59.
# FONT = PIL.ImageFont.truetype("./fonts/Avenir-Roman.ttf", 24)
PURPLE = cozmo.lights.Color(rgba=(138, 43, 226 , 255))
GREEN = cozmo.lights.Color(rgba=(0, 255, 0, 255))
ORANGE = cozmo.lights.Color(rgba=(255, 165, 0, 255))

# Constants
FILTERED = frozenset(["black and white", "monochrome photography", "black", "white", "monochrome", "image", "photography"])

# Requires Google API Client Library for Python:
# pip3 install --upgrade google-api-python-client

'''
Google Cloud Vision Test
-Experimenting with Cozmo's camera and Google Cloud Vision API
-When you tap a cube, Cozmo says what he thinks he's seeing
-Certain results are filtered
-Images are saved as png files
-Note: Requires setting key file in environment variable GOOGLE_APPLICATION_CREDENTIALS

@author Team Cozplay
'''
class CloudVisionTest:
    def __init__(self):
        self._count = 0
        self._cube = 0
        self._robot = None
        self._is_busy = False
        if USE_LOGGING:
            cozmo.setup_basic_logging()
        if USE_VIEWER:
            cozmo.connect_with_tkviewer(self.run)
        else:
            cozmo.connect(self.run)

    async def display_text(self, text):
        [width, height] = cozmo.oled_face.dimensions()
        words = text.split(" ")
        for word in words:
            text_image = PIL.Image.new('RGBA', cozmo.oled_face.dimensions(), (0, 0, 0, 255))
            context = PIL.ImageDraw.Draw(text_image)
            context.text((0, 0), word, fill=(255, 255, 255, 255))#, font=FONT)
            oled_face_data = cozmo.oled_face.convert_image_to_screen_data(text_image)
            self._robot.display_oled_face_image(oled_face_data, 1000*DISPLAY_TEXT_DURATION/len(words))
            await asyncio.sleep(DISPLAY_TEXT_DURATION/len(words))

    async def set_up_cozmo(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self._robot = await coz_conn.wait_for_robot()
        self._robot.camera.image_stream_enabled = True
        await self._robot.set_head_angle(cozmo.util.Angle(degrees=0)).wait_for_completed()
        try:
            cubes = await self._robot.world.wait_until_observe_num_objects(1, cozmo.objects.LightCube)
            self._cube = cubes[0]
        except TimeoutError:
            print("Could not find cube")
            return False
        await self._robot.play_anim("anim_reacttoblock_happydetermined_01").wait_for_completed()
        await self._robot.set_head_angle(cozmo.util.Angle(degrees=44.5)).wait_for_completed()
        await self._robot.say_text("Hey. Tap the cube and I'll investigate", duration_scalar=1.2).wait_for_completed()
        await self._robot.set_head_angle(cozmo.util.Angle(degrees=0.0)).wait_for_completed()
        self._cube.color = PURPLE
        self._robot.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)

    async def on_object_tapped(self, event, *, obj, tap_count, tap_duration, **kw):
        if self._is_busy:
            return
        else:
            self._is_busy = True
            self._cube.color = ORANGE
            response = await self.send_label_request()
            if response:
                await self.process_label_response(response)
            self._is_busy = False

    # Send an image label request using Cozmo's current camera image
    async def send_label_request(self):
        print("Label Request")
        cozmo_image = self._robot.world.latest_image
        if not cozmo_image:
            return None

        filename = "vision_test_" + str(self._count) + "_" + str(time.time()) + ".png"
        cozmo_image.raw_image.save(filename)
        self._count += 1

        credentials = GoogleCredentials.get_application_default()
        service = discovery.build('vision', 'v1', credentials=credentials)

        with open(filename, 'rb') as image:
            image_content = base64.b64encode(image.read())
            service_request = service.images().annotate(body={
                'requests': [{
                    'image': {
                        'content': image_content.decode('utf-8')
                    },
                    'features': [{
                        'type': 'LABEL_DETECTION',
                        'maxResults': 10
                    }]
                }]
            })
        response = service_request.execute()
        return response

    # Cozmo says and shows the results
    async def process_label_response(self, response):
        await self._robot.set_head_angle(cozmo.util.Angle(degrees=0)).wait_for_completed()
        await self._robot.play_anim("anim_hiking_edgesquintgetin_01").wait_for_completed()
        await self._robot.set_head_angle(cozmo.util.Angle(degrees=44.5)).wait_for_completed()

        self._cube.color = GREEN

        annotations = []
        for annotation in response['responses'][0]['labelAnnotations']:
            if (not USE_FILTER) or (not FILTERED.__contains__(annotation['description'])):
                annotations.append(annotation['description'])

        if len(annotations) == 0:
            await self._robot.say_text("I didn't see anything interesting.", duration_scalar=1.2).wait_for_completed()
        else:
            await self._robot.say_text("I think I saw " + annotations[0], duration_scalar=1.2).wait_for_completed()
            print(annotations[0])
            await self.display_text(annotations[0])
            for annotation in annotations[1:]:
                await self._robot.say_text("and also " + annotation, duration_scalar=1.2).wait_for_completed()
                print(annotation)
                await self.display_text(annotation)

            await self._robot.say_text("That's everything.", duration_scalar=1.2).wait_for_completed()
            await self._robot.play_anim("anim_memorymatch_pointcenter_02").wait_for_completed()

        await self._robot.say_text("Let's look some more.", duration_scalar=1.2).wait_for_completed()
        await self._robot.set_head_angle(cozmo.util.Angle(degrees=0.0)).wait_for_completed()

        self._cube.color = PURPLE

    async def run(self, coz_conn):
        # Set up Cozmo
        await self.set_up_cozmo(coz_conn)

        while True:
            await asyncio.sleep(0)
        pass


class CloudVisionCube(cozmo.objects.LightCube):
    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)
        self._color = cozmo.lights.off

    @property
    def color(self):
        return self._color

    @color.setter
    def color(self, value: cozmo.lights.Color):
        self._color = value
        self.set_lights(cozmo.lights.Light(value))

if __name__ == '__main__':
    cozmo.world.World.light_cube_factory = CloudVisionCube
    CloudVisionTest()