import asyncio
import codecs
import json
import urllib.request
import cozmo
from common.cozgame import CozGame

'''
FotuneTeller - Simple reaction game with Cozmo where Cozmo and player needs to tap cube when light comes to their end
- If player misses hitting the cube at right time, he will lose point
- If Cozmo misses hitting the cube at right time, he will lose point
- If both hit the cube at right time, speed will increase for next round and this will continue until game ends

@class PongGame
@author - Team Cozplay
'''

'''
Fortune teller class
- Whenever cozmo sees a new face, it will say some fortune text
@class FotuneTeller
@author Ankit Patel
'''


class FotuneTeller(CozGame):
    def __init__(self, *a, **kw):
        CozGame.__init__(self)
        # init cozmo
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    async def run(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self.is_saying = False
        self.say_action = None
        self.robot = await coz_conn.wait_for_robot()

        asyncio.ensure_future(self.update())
        while not self.exit_flag:
            await asyncio.sleep(0)
        self.robot.abort_all_actions()

    async def update(self):
        # init game
        await self.init_game()
        while True:
            await asyncio.sleep(0)

    async def init_game(self):
        await self.robot.set_head_angle(cozmo.util.Angle(degrees=40)).wait_for_completed()

        try:
            await self.find_face()

        except KeyboardInterrupt:
            print("")
            print("Exit requested by user")

    async def find_face(self):

        while True:
            # find a visible face, timeout if nothing found after a short while
            try:
                await self.robot.world.wait_for_observed_face(timeout=20)
                await self.say_fortune()
            except asyncio.TimeoutError:
                print("Didn't find a face - exiting!")
                return

            asyncio.sleep(.1)

    async def say_fortune(self):
        obj = self.get_json_data("http://garrod.isri.cmu.edu/webapps/fortune")
        fortune = obj["fortune"]
        print(fortune)
        await self.robot.say_text(text=fortune, duration_scalar=1.5, play_excited_animation=True,
                                  voice_pitch=1.0).wait_for_completed()

    def get_json_data(self, url):
        reader = codecs.getreader("utf-8")
        response = urllib.request.urlopen(url)
        obj = json.load(reader(response))
        return obj


if __name__ == '__main__':
    FotuneTeller()
