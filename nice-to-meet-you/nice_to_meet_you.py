#!/usr/bin/env python3
"""
nice_to_meet_you.py :
    - Cozmo initiates conversation with a human face that he encounters
    - After introduction, phone numbers are exchanged and Cozmo uses messaging to continue the conversation
    - Read the README file for more info

@author - Team Cozplay
"""

from flask import Flask, request
import common.flask_helpers as flask_helpers
import twilio.twiml
from twilio.rest import TwilioRestClient

import asyncio
import time
import requests

import cozmo
from cozmo.util import degrees, distance_mm, speed_mmps
from PIL import Image, ImageDraw, ImageFont

from common.thread_timer import ThreadTimer, Event

# Volume for the robot
ROBOT_VOLUME = 0.5
# Reset faces
ERASE_FACES = False
# Global variables for thread
thread_timer = None
STOP_FLAG = Event()
THREAD_INTERVAL = 3  # seconds. How often the background method to update the stage is run.


class NiceToMeetYou:
    """ This class will handle the robot behavior as well as the flask/twilio interface for text messaging """
    def __init__(self):
        print("Initializing class...")
        # Var to store Robot object returned by SDK conn
        self._robot = None
        # Name input for the face
        self._face_name = None
        # Font to be used to display text on Cozmo's screen
        self._screen_font = None

        # Flask interface related vars
        self.app = Flask(__name__)

        # SMS related vars
        self.received_message = False
        self.from_number = None
        self.message_text = None
        self.caller = {}

        # Twilio account details
        self.accountSID = ''        # Enter your Account SID here
        self.authToken = ''         # Enter your Auth Token here
        self.twilioCli = TwilioRestClient(self.accountSID, self.authToken)
        self.twilioNumber = '+1'    # Enter your linked phone number here

        # Flask interface stage handler
        self.flask_stage = {"Wait": self.stage_wait,
                            "Love": self.stage_love,
                            "Shy": self.stage_shy,
                            "Video": self.stage_video,
                            "End": self.stage_end,
                            "Finale": self.stage_finale}
        self.curr_stage = None

        # Start the robot
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    @property
    def robot(self):
        return self._robot

    @robot.setter
    def robot(self, value):
        self._robot = value

    @property
    def face_name(self):
        return self._face_name

    @face_name.setter
    def face_name(self, value):
        self._face_name = value

    @property
    def screen_font(self):
        return self._screen_font

    @screen_font.setter
    def screen_font(self, value):
        self._screen_font = value

    async def on_object_tapped(self, evt, obj=None, tap_count=None, **kwargs):
        """ Custom handler to identify taps on cubes """
        print("on_object_tapped", evt, obj)

    def identify_face(self):
        """ This method looks for a face and requests for a name on successfully finding one """
        self.robot.say_text("Hi I'm Cozmo!", play_excited_animation=True, duration_scalar=1.0).wait_for_completed()

        # Initiate find face behavior
        find_face = self.robot.start_behavior(cozmo.behavior.BehaviorTypes.FindFaces)
        face = None
        try:
            face = self.robot.world.wait_for_observed_face(timeout=30)
            print("Found a face!", face)
        except asyncio.TimeoutError:
            print("Didn't find a face")
        finally:
            find_face.stop()

        # Assign a name to the face
        if face is None:
            self.robot.play_anim("anim_freeplay_hitground").wait_for_completed()
            exit(1)
        else:
            self.robot.play_anim("anim_meetcozmo_celebration_02").wait_for_completed()
            if face.name == "":
                self.robot.say_text("Nice to meet you! What's your name?", play_excited_animation=True,
                                          duration_scalar=1.25).wait_for_completed()
                self.face_name = input("Input your name: ")
                self.robot.turn_towards_face(face).wait_for_completed()
                self._robot.say_text("Hi " + self.face_name).wait_for_completed()
                face.name_face(self.face_name).wait_for_completed()
            else:
                self.robot.turn_towards_face(face).wait_for_completed()
                self.robot.say_text("Hi " + face.name).wait_for_completed()
                self.face_name = face.name

    def request_block(self):
        """ Requests for a block to be placed on his arms as first form of interaction """
        self.robot.play_anim("anim_reacttoblock_ask_01").wait_for_completed()
        cube = self.robot.world.wait_until_observe_num_objects(num=1, object_type=cozmo.objects.LightCube,
                                                                     timeout=60)
        target = cube[0]
        target.set_lights(cozmo.lights.green_light.flash())
        self.robot.play_anim("anim_memorymatch_successhand_cozmo_02").wait_for_completed()

        # Wait for player to put down the cube
        time.sleep(1)
        self.robot.set_head_angle(degrees(-10)).wait_for_completed()
        self.robot.world.wait_until_observe_num_objects(num=1, object_type=cozmo.objects.LightCube, timeout=60)
        self.robot.pickup_object(target).wait_for_completed()
        self.robot.play_anim("anim_launch_reacttocubepickup").wait_for_completed()
        self.robot.place_object_on_ground_here(target).wait_for_completed()
        target.set_lights(cozmo.lights.white_light)
        self.robot.drive_straight (distance_mm(-50), speed_mmps(50), should_play_anim=True).wait_for_completed()

    def make_text_image(self, text_to_draw, x, y, font=None):
        """
        Make a PIL.Image with the given text printed on it
        :param text_to_draw: (string): the text to draw to the image
        :param x: (int): x pixel location
        :param y: (int): y pixel location
        :param font: (PIL.ImageFont): the font to use
        :return: :class:(`PIL.Image.Image`): a PIL image with the text drawn on it
        """

        # make a blank image for the text, initialized to opaque black
        text_image = Image.new('RGBA', cozmo.oled_face.dimensions(), (0, 0, 0, 255))

        # get a drawing context
        dc = ImageDraw.Draw(text_image)

        # draw the text
        dc.text((x, y), text_to_draw, fill=(255, 255, 255, 255), font=font)

        return text_image

    def load_image(self, image_name):
        images = {"Heart": "images/heart.png",
                  "Message": "images/message.png"}
        image = Image.open(images[image_name])
        resized_image = image.resize(cozmo.oled_face.dimensions(), Image.NEAREST)
        face_image = cozmo.oled_face.convert_image_to_screen_data(resized_image)
        self.robot.display_oled_face_image(face_image, 5000.0)

    def offer_number(self):
        """ Offer telephone number to stay in touch. Last action before relieving control to Flask app """
        self.robot.say_text("I want to be friends with you!", duration_scalar=1.5, play_excited_animation=True)\
            .wait_for_completed()
        self.robot.set_head_angle(degrees(40)).wait_for_completed()
        self.robot.say_text("Send me a message", duration_scalar=1.25).wait_for_completed()

        # Initialize font
        try:
            self.screen_font = ImageFont.truetype("fonts/", 14) # Set your font here
        except IOError:
            pass

        # Display image
        text_image = self.make_text_image("Text Me - \n412  201  0200", 0, 0, self.screen_font)
        oled_face_data = cozmo.oled_face.convert_image_to_screen_data(text_image)
        # display for 30 seconds
        self.robot.display_oled_face_image(oled_face_data, 30000.0)

    # The different stages of Cozmo behavior once numbers have been exchanged are implemented below
    def stage_wait(self):
        """ Cozmo waits for the first message to be sent after displaying phone number """
        if self.received_message:
            print("Message received")
            self.robot.say_text("I got your message!", duration_scalar=1.3,
                                play_excited_animation=True).wait_for_completed()
            self.caller = {self.from_number: self.face_name}
            self.received_message = False
            self.curr_stage = "Love"

    def stage_love(self):
        """ Cozmo admits that he likes you! """
        self.robot.play_anim("anim_triple_backup").wait_for_completed()
        self.robot.play_anim("anim_explorer_idle_02_head_angle_-20").wait_for_completed()
        self.robot.say_text("umm", voice_pitch=-1.0, duration_scalar=2.5).wait_for_completed()
        self.robot.play_anim("ID_test_shiver").wait_for_completed()
        self.robot.play_anim("anim_explorer_idle_01_head_angle_40").wait_for_completed()
        self.robot.say_text("I love, you", duration_scalar=1.5).wait_for_completed()
        self.load_image("Heart")
        self.curr_stage = "Shy"

    def stage_shy(self):
        """ Cozmo becomes shy  """
        self.robot.play_anim("id_poked_giggle").wait_for_completed()
        self.robot.turn_in_place(degrees(170)).wait_for_completed()
        self.robot.play_anim("anim_hiking_react_05").wait_for_completed()
        self.robot.turn_in_place(degrees(-170)).wait_for_completed()
        self.robot.play_anim("anim_hiking_lookaround_01").wait_for_completed()
        self.curr_stage = "Video"

    def stage_video(self):
        """ Cozmo sends you a video - RHCP ftw! """
        self.robot.play_anim("anim_explorer_idle_01_head_angle_40").wait_for_completed()
        self.robot.say_text("I sent you a message", duration_scalar=1.5).wait_for_completed()
        self.load_image("Message")
        self.twilioCli.messages.create(
            to=self.from_number,
            from_=self.twilioNumber,
            body="Here's a video I think you might like\nhttps://youtu.be/-66h7hHWx8Q")  # Change to any other media
        self.curr_stage = "End"

    def stage_end(self):
        """ Cozmo asks if you liked the video """
        self.robot.play_anim("anim_reacttoface_unidentified_02").wait_for_completed()
        self.robot.play_anim("anim_explorer_idle_01_head_angle_40").wait_for_completed()
        self.robot.say_text("umm", voice_pitch=-1.0, duration_scalar=2.5).wait_for_completed()
        self.load_image("Message")
        self.twilioCli.messages.create(
            to=self.from_number,
            from_=self.twilioNumber,
            body="Did you like it?")
        self.curr_stage = "Finale"

    def stage_finale(self):
        """" Wrapping it up """
        if self.received_message:
            if "no" in self.message_text.lower():
                self.robot.play_anim("anim_speedtap_losegame_intensity02_01").wait_for_completed()
                # Final message
                self.twilioCli.messages.create(
                    to=self.from_number,
                    from_=self.twilioNumber,
                    body="I thought we had something special...  :(")
            else:
                self.robot.play_anim("anim_petdetection_dog_02").wait_for_completed()
                # Final message
                self.twilioCli.messages.create(
                    to=self.from_number,
                    from_=self.twilioNumber,
                    body="I knew you would like it ;)",
                    media_url="http://www.etc.cmu.edu/projects/cozplay/wp-content/uploads/2016/10/cozmoheart.png")

            # Reset flag
            self.received_message = False

            # Clean up
            global STOP_FLAG
            STOP_FLAG.set()
            requests.post("http://localhost:5000/kill")

    def kill_server(self):
        """ Handle flask shutdown """
        func = request.environ.get('werkzeug.server.shutdown')
        if func is None:
            raise RuntimeError('Not running with the Werkzeug Server')
        func()
        return ("Shutting down...")

    def flask_interaction_loop(self):
        """ Background method while flask app is running """
        print("BG method")
        self.flask_stage[self.curr_stage]()

    def cozmo_response(self):
        """ Flask method to handle incoming messages """

        # Parse incoming message
        if self.from_number is None:
            self.from_number = request.values.get('From', None)
        self.message_text = request.values.get('Body', None)
        print(self.message_text)

        resp = twilio.twiml.Response()
        # Form reply to be sent
        if self.curr_stage != "Finale":
            if self.face_name:
                message = "Hey " + self.face_name + "! Thanks for replying :)"
            else:
                message = "Hey ! Thanks for replying :)"

            resp.message(message)

        if self.message_text is not None:
            self.received_message = True

        # Start the background thread to handle cozmo actions on the first execution of this method
        global thread_timer
        if not thread_timer:
            self.curr_stage = "Wait"
            thread_timer = ThreadTimer(THREAD_INTERVAL, self.flask_interaction_loop, STOP_FLAG)
            thread_timer.start()

        # Return response
        return str(resp)

    def interaction_loop(self):
        """ This method will call all the interaction methods in sequence """
        # 1 - Look for face
        self.identify_face()

        # 2 - Request for a block
        self.request_block()

        # 3 - Offer phone number
        self.offer_number()

    def run(self, sdk_conn):
        """ First method that is run to initialize the experience """
        # Retrieve robot object
        self.robot = sdk_conn.wait_for_robot()

        # Volume - trying not to annoy nearby people
        self.robot.set_robot_volume(ROBOT_VOLUME)

        # Reset faces and arm
        if ERASE_FACES:
            cozmo.faces.erase_all_enrolled_faces(sdk_conn)
        self.robot.set_lift_height(0).wait_for_completed()

        # Handler for keeping track of cube taps if required
        self.robot.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)

        # Call game loop first
        self.interaction_loop()

        # 4 - Switch control to Flask server
        # add_url_rule is alternative to @app.route as function is now inside a class. It performs the same redirection
        # http://flask.pocoo.org/docs/0.11/api/#flask.Flask.add_url_rule
        self.app.add_url_rule("/", 'cozmo_response', self.cozmo_response, methods=['GET', 'POST'])
        self.app.add_url_rule("/kill", 'kill_server', self.kill_server, methods=['POST'])
        flask_helpers.run_flask(self.app)

# Set entry point
if __name__ == "__main__":
    NiceToMeetYou()
