import asyncio
import time
import cozmo
import random
from cozmo.util import degrees, Pose

'''This is a script for game Peek a Boo.
It will first store the players' names and then play peek a boo with the player.
'''

class PeekABoo:

    def __init__(self, *a, **kw):
        self.coz = None
        self.face = None
        self.count = 0
        cozmo.connect(self.run)

    async def run(self, coz_conn):
        self.coz = await coz_conn.wait_for_robot()
        
        while True:
            find_face = self.coz.start_behavior(cozmo.behavior.BehaviorTypes.FindFaces)
            time.sleep(3)
            try:
                self.face = await self.coz.world.wait_for_observed_face(timeout=30)
                print("Found face", self.face)
            except asyncio.TimeoutError:
                print("Didn't find a face")

            finally:
                find_face.stop()

            if self.face is None:
                anim = await self.coz.play_anim_trigger(cozmo.anim.Triggers.PounceFail).wait_for_completed()
            else:
                anim = await self.coz.play_anim_trigger(cozmo.anim.Triggers.PounceSuccess).wait_for_completed()
                await self.coz.set_head_angle(degrees(45.0)).wait_for_completed()
                if self.face.name == "":
                    await self.coz.say_text("My name is, Cozmo").wait_for_completed()
                    await self.coz.say_text("What is your name?").wait_for_completed()
                    new_name = input("Input your name.")
                    await self.face.name_face(new_name).wait_for_completed()
                    await self.coz.say_text("Hi" + new_name).wait_for_completed()
                else:
                    await self.coz.say_text("Hi" + self.face.name).wait_for_completed()

                await self.coz.say_text("Let's play peek a boo").wait_for_completed()
                await self.start_game()

    async def start_game(self):
        await self.coz.play_anim_trigger(cozmo.anim.Triggers.CubePounceWinSession).wait_for_completed()
        count = 0
        while True:
            if count >= 8:
                await self.end_game()
            peek_a_boo_find_face = self.coz.start_behavior(cozmo.behavior.BehaviorTypes.FindFaces)
            time.sleep(3)
            count = count + 1
            new_face = None
            try:
                new_face = await self.coz.world.wait_for_observed_face(timeout=15)
                print("Found face", new_face)
            except asyncio.TimeoutError:
                print("Didn't find a face")
            finally:
                peek_a_boo_find_face.stop()

            if new_face is not None:
                if new_face.face_id == self.face.face_id:
                    seeyou_text = [", I see you", ", you are here", ", haha"]
                    anim = await self.coz.play_anim("anim_sparking_success_01").wait_for_completed()
                    await self.coz.say_text(self.face.name + random.choice(seeyou_text)).wait_for_completed()
                else:
                    await self.coz.drive_wheels(-400, -400, duration=1)
                    await self.coz.say_text("who are you").wait_for_completed()
                    await self.coz.turn_in_place(degrees(90)).wait_for_completed()
                    await self.coz.drive_wheels(400, 400, duration=1)
                    await self.coz.say_text("You are not, " + self.face.name).wait_for_completed()
                    await self.coz.turn_in_place(degrees(170)).wait_for_completed()
                    await self.coz.say_text("Where is, " + self.face.name).wait_for_completed()
                    await self.coz.drive_wheels(400, 400, duration=1)
                    await self.coz.turn_in_place(degrees(90)).wait_for_completed()
                    await self.coz.drive_wheels(400, 400, duration=0.5)
                    await self.coz.say_text("I want to play with, " + self.face.name).wait_for_completed()
            else:
                anim = await self.coz.play_anim("id_rollblock_fail_01").wait_for_completed()
                await self.coz.say_text("Where are you.").wait_for_completed()
                
    async def end_game(self):
        await self.coz.say_text("I want to sleep").wait_for_completed()
        await self.coz.play_anim("anim_launch_sleeping_01").wait_for_completed()



if __name__ == '__main__':
    cozmo.setup_basic_logging()
    PeekABoo()