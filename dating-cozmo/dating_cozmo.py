import asyncio
from pubnub import Pubnub
import cozmo
import random
from PIL import Image, ImageDraw, ImageFont
from cozmo.util import degrees, Pose

'''
Dating Cozmo Game
- Dating cozmo story based game with Unity platform
@class DatingCozmo
@author - Team Cozplay
'''

class DatingCozmo:

    PUBLISH_CHANNEL = "cozmo_channel"
    SUBSCRIBE_CHANNEL = "unity_channel"
    PUBLISH_KEY = "pub-c-5f1ce503-e42e-4627-a511-fdd093ab3e45"
    SUBSCRIBE_KEY = "sub-c-7800e8b2-ab71-11e6-be20-0619f8945a4f"
    SLEEP_ANIM = "anim_gotosleep_sleeploop_01"
    ASK_QUESTION_ANIMS = ["anim_rtpkeepaway_askforgame_02","anim_rtpkeepaway_askforgame_01","anim_speedtap_ask2play_01"]
    COOKIE_DISTANCE_Y = 70

    def __init__(self):
        self.pubnub = Pubnub(publish_key=DatingCozmo.PUBLISH_KEY, subscribe_key=DatingCozmo.SUBSCRIBE_KEY)
        self.pubnub.subscribe(channels=DatingCozmo.SUBSCRIBE_CHANNEL, callback=self.on_message_received, error=self.error,
                         connect=self.connect, reconnect=self.reconnect, disconnect=self.disconnect)
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    async def run(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        # setup robot
        self.coz = await coz_conn.wait_for_robot()
        await self.coz.go_to_pose(Pose(0, 0, 0, angle_z=degrees(0)), relative_to_robot=True).wait_for_completed()
        await self.start_game()
        while True:
            pass


    async def start_game(self):
        self.answer = None
        self.is_tap_active = False
        self.sleep_flag = True
        self.playerName = ""
        self.homePose = None
        self.cookiePose = None
        self.coffeePose = None
        self.cube = None
        self.workout_flag = 0
        # connect cube

        await self.connect_cube()
        asyncio.ensure_future(self.sleep_loop())

        await self.meeting_cozmo_scene()
        await self.friendship_interaction_scene()
        await self.escalating_relationship_scene()

    async def sleep_loop(self):
        while self.sleep_flag:
            await asyncio.sleep(0)
        self.coz.abort_all_actions()

    async def connect_cube(self):
        self.cube = await self.coz.world.wait_for_observed_light_cube()
        self.cube.set_lights(cozmo.lights.white_light)
        self.coz.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)

    def on_object_tapped(self, evt, obj=None, tap_count=None, **kwargs):
        if self.is_tap_active:
            #self.is_tap_active = False
            self.cube.set_lights(cozmo.lights.white_light.flash(50, 50))
            self.sleep_flag = False
            self.answer = "wake up"

    async def ask_coffee(self, index, anim=None):
        await self.coz.play_anim(random.choice(anim) if anim else random.choice(DatingCozmo.ASK_QUESTION_ANIMS)).wait_for_completed()
        self.publish("Coffee:" + str(index))

    async def ask_fb_status(self, frame, anim=None):
        await self.coz.play_anim(random.choice(anim) if anim else random.choice(DatingCozmo.ASK_QUESTION_ANIMS)).wait_for_completed()
        self.publish("Facebook:"+str(frame))


    async def ask_cookie(self, index, anim=None):
        await self.coz.play_anim(random.choice(anim) if anim else random.choice(DatingCozmo.ASK_QUESTION_ANIMS)).wait_for_completed()
        self.publish("Cookie:" + str(index))

    async def ask_question(self, index, anim=None):
        await self.coz.play_anim(random.choice(anim) if anim else random.choice(DatingCozmo.ASK_QUESTION_ANIMS)).wait_for_completed()
        self.publish("Cozmo:"+ str(index))

    async def meeting_cozmo_scene(self):
        self.is_tap_active = True
        await self.sleep()
        self.publish("Wokeup")
        self.is_tap_active = False
        self.homePose = self.coz.pose;
        self.cube.set_lights(cozmo.lights.off_light)

        #sleep getout anim
        await self.coz.play_anim("anim_gotosleep_getout_04", loop_count=1).wait_for_completed()
        #cozmo face look up and down, shifty eyes enimation here
        await self.coz.play_anim("anim_launch_firsttimewakeup_helloworld", loop_count=1).wait_for_completed()
        await self.ask_question(1, ["anim_speedtap_ask2play_01"])
        await asyncio.sleep(2)
        # ask name
        await self.ask_question(2, ["anim_speedtap_ask2play_01"])
        await self.wait_for_answer(None)

    async def eat_cookie_anim(self):
        await self.turn(-50)
        await self.drive(DatingCozmo.COOKIE_DISTANCE_Y)
        await self.ask_cookie(1, ["anim_speedtap_ask2play_01"])
        await self.ask_cookie(2, ["anim_speedtap_ask2play_01"])
        await self.ask_cookie(3, ["anim_speedtap_ask2play_01"])
        await self.drive(-DatingCozmo.COOKIE_DISTANCE_Y*0.7)
        await self.turn(50)

    async def friendship_interaction_scene(self):
        await self.coz.say_text(self.playerName, duration_scalar=1.3, play_excited_animation=True).wait_for_completed()
        await self.ask_question(3,["anim_explorer_huh_01_head_angle_20"])
        await self.wait_for_answer()
        await self.ask_question(4, ["anim_explorer_huh_01_head_angle_20"])
        await asyncio.sleep(4)

        #add cookie and coffee to the scene
        await self.ask_coffee(0)
        await self.wait_for_answer()
        await self.coz.say_text("Coffee!", duration_scalar=1).wait_for_completed()
        await asyncio.sleep(2)

        await self.ask_cookie(0)
        await self.wait_for_answer()
        await asyncio.sleep(2)
        await self.eat_cookie_anim()
        await self.coz.say_text("Cookie!, My Favourite", duration_scalar=1).wait_for_completed()

        await asyncio.sleep(2)
        await self.coz.play_anim("anim_freeplay_reacttoface_wiggle_01", loop_count=1).wait_for_completed()
        await self.ask_question(5, ["anim_freeplay_reacttoface_identified_02_head_angle_40"])
        await self.wait_for_answer(["ID_pokedB"])

        await self.pickup_cube();
        if(int(self.answer) == 1):
            if self.cube:
                self.cube.set_lights(cozmo.lights.red_light)
                await self.coz.say_text("red", duration_scalar=1).wait_for_completed()
        elif (int(self.answer) == 2):
            if self.cube:
                self.cube.set_lights(cozmo.lights.green_light)
                await self.coz.say_text("green", duration_scalar=1).wait_for_completed()
        elif (int(self.answer) == 3):
            if self.cube:
                self.cube.set_lights(cozmo.lights.blue_light)
                await self.coz.say_text("blue", duration_scalar=1).wait_for_completed()
        await self.coz.play_anim("anim_freeplay_reacttoface_like_01", loop_count=1).wait_for_completed()
        await self.ask_question(6, ["anim_explorer_idle_02_head_angle_30"])
        await self.wait_for_answer(["anim_explorer_driving01_turbo_01_head_angle_20"])

    async def pickup_cube(self):
        await self.coz.pickup_object(self.cube).wait_for_completed()
        await self.coz.place_object_on_ground_here(self.cube).wait_for_completed()


    async def exit_anim(self):
        await self.coz.pickup_object(self.cube).wait_for_completed()
        await self.turn(130);
        await self.drive(50)
        await self.coz.place_object_on_ground_here(self.cube).wait_for_completed()

    async def throw_cube_anim(self):
        await self.turn(-85)
        await self.drive(85)
        await self.coz.play_anim("anim_pounce_long_01", loop_count=1).wait_for_completed()
        await self.drive(-80)
        await self.turn(90);

    async def escalating_relationship_scene(self):
        await self.ask_question(7, ["anim_keepaway_wingame_03"])
        await self.wait_for_answer()
        await self.ask_question(8)
        await self.throw_cube_anim()

        await self.ask_question(9, ["anim_explorer_idle_02_head_angle_30"])
        # show three cube images panel now
        await self.wait_for_answer()

        await self.ask_question(10, ["anim_reacttoblock_happydetermined_01"])
        await self.wait_for_answer()

        if (int(self.answer) == 1):
            #A Love
            await self.coz.play_anim("anim_reacttoblock_happydetermined_02", loop_count=1).wait_for_completed()
            await self.ask_question(11, ["anim_freeplay_reacttoface_identified_02_head_angle_40"])
            self.is_tap_active = True
            await self.wait_for_answer(["ID_pokedB"])
            self.cube.set_lights(cozmo.lights.off_light)
            await self.facebook_status_scene()

        elif (int(self.answer) == 2):
            #B Insecure
            await self.ask_question(21, ["anim_explorer_huh_01_head_angle_10"])
            await self.wait_for_answer(["id_rollblock_fail_01"])
            await self.ask_question(22)
            await asyncio.sleep(2)
            await self.ask_question(23,["anim_sparking_fail_01"])
            await self.wait_for_answer(["ID_pokedB"])
            if (int(self.answer) == 1):
                await self.video_seq_scene()
            else:
                await asyncio.sleep(1)

                # ask to change facebook picture
                await self.ask_question(24)
                await self.wait_for_answer(["anim_explorer_driving01_turbo_01_head_angle_20"])
                await self.photo_seq_scene()
                await self.video_seq_scene()

        else:
            #C Stalker
            await self.ask_question(31, ["anim_explorer_huh_01_head_angle_10"])
            await self.wait_for_answer()
            await self.ask_question(32)
            await self.wait_for_answer(["id_rollblock_fail_01"])
            await self.ask_question(33)
            await self.wait_for_workout_answer()

            if (int(self.answer) == 1):
                #super angry
                await self.ask_question(34, ["anim_reacttocliff_stuckrightside_03"])
                await asyncio.sleep(5)
                await self.video_seq_scene()
            else:
                #optimus branch
                await self.ask_question(35, ["anim_explorer_huh_01_head_angle_10"])
                await self.coz.play_anim("anim_reacttoblock_happydetermined_02", loop_count=1).wait_for_completed()
                await asyncio.sleep(2)
                await self.ask_fb_status(frame=2)
                await self.wait_for_answer()

                # ask to change facebook picture
                await self.ask_question(25)
                await self.wait_for_answer()
                await self.photo_seq_scene()
                await self.video_seq_scene(index=17)

    async def facebook_status_scene(self):
        await self.coz.play_anim("anim_keepaway_wingame_01", loop_count=1).wait_for_completed()
        await self.ask_question(12, ["anim_meetcozmo_celebration_02"])
        await self.wait_for_answer(["anim_explorer_driving01_turbo_01_head_angle_20"], wait_img="Heart")
        await asyncio.sleep(1)
        await self.ask_fb_status(frame=1)
        await self.wait_for_answer()
        #ask to change facebook picture
        await self.ask_question(13)
        await self.wait_for_answer()
        await self.photo_seq_scene()
        await self.video_seq_scene()

    async def photo_seq_scene(self):
        await asyncio.sleep(1)
        await self.ask_question(14, ["anim_explorer_huh_01_head_angle_10"])
        await self.wait_for_answer()
        await self.coz.play_anim("anim_reacttoblock_frustrated_int2_01", loop_count=1).wait_for_completed()
        await self.ask_question(15, ["anim_keepaway_losegame_03"])
        await self.wait_for_answer(["id_rollblock_fail_01"])

    async def video_seq_scene(self, index=16):
        await self.coz.play_anim("anim_speedtap_playerno_01", loop_count=1).wait_for_completed()
        await self.ask_question(index, ["majorfail"])
        await self.wait_for_answer()

    async def drive(self, dis, speed=3000):
        await self.coz.drive_straight(distance=cozmo.util.Distance(dis),
                                 speed=cozmo.util.Speed(speed), should_play_anim=True).wait_for_completed()

    async def sleep(self):
        sleep_anim = None
        while self.sleep_flag:
            sleep_anim = self.coz.play_anim(DatingCozmo.SLEEP_ANIM, loop_count=1)
            await sleep_anim.wait_for_completed()
        self.coz.abort_all_actions()

    async def wait_for_answer(self, wait_anim=None, wait_img=None):
        self.answer = None
        while not self.answer:
            if wait_anim:
                await self.coz.play_anim(random.choice(wait_anim), loop_count=1).wait_for_completed()
                if wait_img:
                    await self.load_heart_image(wait_img)
                await asyncio.sleep(1)
            pass

    async def wait_for_workout_answer(self):
        self.answer = None
        await self.coz.pickup_object(self.cube).wait_for_completed()
        self.publish("Workout:1")
        while not self.answer:
            await self.coz.set_lift_height(0.5, duration=0.5).wait_for_completed()
            await self.coz.set_lift_height(1, duration=0.5).wait_for_completed()
            await asyncio.sleep(0)
            pass
        await self.coz.place_object_on_ground_here(self.cube).wait_for_completed()
        await self.coz.play_anim("anim_triple_backup", loop_count=1).wait_for_completed()


    async def load_heart_image(self, image_name):
        images = {"Heart": "images/heart.png"}
        image = Image.open(images[image_name])
        resized_image = image.resize(cozmo.oled_face.dimensions(), Image.NEAREST)
        face_image = cozmo.oled_face.convert_image_to_screen_data(resized_image)
        self.coz.display_oled_face_image(face_image, 5000.0)


    async def turn(self, degree):
        await self.coz.turn_in_place(angle=cozmo.util.Angle(degrees=degree)).wait_for_completed()


    '''
        PUBNUB API METHODS
    '''

    def on_message_received(self, message, channel):
        '''
        This is called from pubnub api whenever it receives new message
        '''
        message = message.replace("\"", "")
        if (message.find("Answer") != -1):
            self.answer = int(message.split(":")[1])
        if (message.find("Tap") != -1):
            self.on_object_tapped(self,None)
        if (message.find("Continue") != -1):
            self.answer = "Continue"
        if (message.find("Name") != -1):
            self.playerName = message.split(":")[1]
            self.answer = self.playerName


    def error(self, message):
        print("ERROR : " + str(message))


    def connect(self, message):
        print("PUBNUB PYTHON : CONNECTED")

    def reconnect(message):
        print("RECONNECTED")


    def disconnect(message):
        print("DISCONNECTED")


    def publish(self, message):
        print(message)
        self.pubnub.publish(channel=DatingCozmo.PUBLISH_CHANNEL, message=message)



if __name__ == '__main__':
    DatingCozmo()