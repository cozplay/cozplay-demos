import asyncio
import sys
import cozmo
from PyDictionary import PyDictionary
from random_words import RandomWords

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    sys.exit("Cannot import from PIL: Do `pip3 install Pillow` to install")


'''
Word of the day interaction
- Displays random dictionary word on cozmo screen when he finds any face
- Also says word meaning
@class WordOfTheDay
@author Ankit Patel
'''
class WordOfTheDay:
    GAME_TIME = 10 * 60

    def __init__(self, *a, **kw):
        # init cozmo
        cozmo.setup_basic_logging()
        cozmo.connect_with_tkviewer(self.run)

    async def run(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self.is_saying = False
        self.say_action = None
        self.dictionary = PyDictionary()
        self.rw = RandomWords()

        self.robot = await coz_conn.wait_for_robot()
        await self.robot.set_head_angle(cozmo.util.Angle(degrees=40)).wait_for_completed()

        try:
            await self.find_face()

        except KeyboardInterrupt:
            print("")
            print("Exit requested by user")

        await asyncio.sleep(WordOfTheDay.GAME_TIME)

    async def find_face(self):

        while True:
            # find a visible face, timeout if nothing found after a short while
            try:
                await self.robot.world.wait_for_observed_face(timeout=30)
                await self.disp_dic_word()

            except asyncio.TimeoutError:
                print("Didn't find a face - exiting!")
                return

            asyncio.sleep(.1)

    async def disp_dic_word(self, len=5):

        word = self.rw.random_word()
        meaning_dic = self.dictionary.meaning(word)
        if meaning_dic and meaning_dic['Noun']:
            meaning = meaning_dic['Noun'][0]
            print(word)
            await self.robot.say_text(text=word, duration_scalar = 1.3, play_excited_animation=False).wait_for_completed()
            self.display_text(word, duration=100000, size=25)
            await asyncio.sleep(3)
            await self.robot.say_text(text=meaning, duration_scalar=1.3, play_excited_animation=False).wait_for_completed()
            await asyncio.sleep(4)

    def display_text(self, text, duration, size):
        # get a font - location depends on OS so try a couple of options
        # failing that the default of None will just use a default font
        font = None
        try:
            font = ImageFont.truetype("arial.ttf", size)
        except IOError:
            try:
                font = ImageFont.truetype("/Library/Fonts/Arial.ttf", 24)
            except IOError:
                pass

        text_image = self.make_text_image(text, 15, 2, font)
        lcd_face_data = cozmo.oled_face.convert_image_to_screen_data(text_image)
        self.robot.display_oled_face_image(lcd_face_data, duration)

    def make_text_image(self,text_to_draw, x, y, font=None):
        '''Make a Pillow.Image with the current time printed on it

        Args:
            text_to_draw (string): the text to draw to the image
            x (int): x pixel location
            y (int): y pixel location
            font (PIL.ImageFont): the font to use

        Returns:
            :class:(`PIL.Image.Image`): a Pillow image with the text drawn on it
        '''

        # make a blank image for the text, initialized to opaque black
        txt = Image.new('RGBA', cozmo.oled_face.dimensions(), (0, 0, 0, 255))

        # get a drawing context
        dc = ImageDraw.Draw(txt)

        # draw the text
        dc.text((x, y), text_to_draw, fill=(255, 255, 255, 255), font=font)

        return txt

if __name__ == '__main__':
    WordOfTheDay()

