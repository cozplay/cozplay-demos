import asyncio
import codecs
import json
import urllib.request
import cozmo
import random
from cozmo.util import degrees, Pose
from common.custom_display import CustomDisplay
from random import shuffle
from common.custom_cube import CustomCube
from common.cube_color import CubeColor
from common.cozgame import CozGame

'''
3-letter anagram game
- cozmo's screen will display 3 letter word in random order and each letter on screen will be mapped to cube by index
- goal of the player is to quickly rearrange letters by moving cubes as fast as possible before it times out
@class Anagram
@author - Team Cozplay
'''


class Anagram(CozGame):
    Z_THRESHOLD = 15
    DISP_THRESHOLD = 30
    DEBUG = False

    def __init__(self, *a, **kw):
        CozGame.__init__(self)

        # init cozmo
        cozmo.world.World.light_cube_factory = CustomCube
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    async def run(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        #setup robot
        self.robot = await coz_conn.wait_for_robot()

        asyncio.ensure_future(self.update())
        while not self.exit_flag:
            await asyncio.sleep(0)
        self.robot.abort_all_actions()

    async def update(self):
        #init game
        await self.init_game()

        await self.disp_word()
        while True:
            await self.on_update()
            await asyncio.sleep(0)


    async def init_game(self):
        await self.robot.set_lift_height(0, duration=0.5).wait_for_completed()
        await self.robot.go_to_pose(Pose(0, 0, 0, angle_z=degrees(0)), relative_to_robot=True).wait_for_completed()
        await self.robot.set_head_angle(cozmo.util.Angle(degrees=10)).wait_for_completed()
        # setup cubes
        self.cubes = await self.robot.world.wait_until_observe_num_objects(num=3, object_type=cozmo.objects.LightCube)
        self.ori_z = self.cubes[0].pose.position.z


        self.ori_seq = None
        self.cur_word = None
        self.cur_seq = None
        self.disp_timer = 0
        self.correct_check_enabled = False
        self.paused = False
        self.word_index = -1
        self.score = 0
        self.idle_count = 0
        self.pos_index = 0
        self.neg_index = 0
        self.anagram_words = ["apt","tap", "are", "ear", "arm", "mar", "ram", "art", "rat", "tar", "asp",
                              "pas", "sap", "spa", "ate", "eat", "era", "bat", "tab", "now", "own", "won", "opt",
                              "pot", "top", "own", "now", "won","zoo", "yet", "yes", "wow","win","who","vow","rug","rub","rip","put","pub",
                              "pie","pet","pen","pic","pig","out","oil","odd","off","nut","nod","aim","air","awe","ban","big","bio","bin"]
        shuffle(self.anagram_words)

        if not Anagram.DEBUG:
            await self.robot.say_text(text="let's play, anagram", play_excited_animation=False,
                                duration_scalar=1.5).wait_for_completed()
        self.ori_pose = self.robot.pose

        await self.disp_word()

    async def disp_word(self):

        #await self.robot.go_to_pose(self.ori_pose, relative_to_robot=False).wait_for_completed()
        await self.robot.set_head_angle(cozmo.util.Angle(degrees=10)).wait_for_completed()

        self.word_index += 1
        if self.word_index == self.anagram_words.__len__():
            shuffle(self.anagram_words)
            self.word_index = 0
        self.cur_word = self.anagram_words[self.word_index]

        # await self.robot.say_text(text="", duration_scalar=1.7, play_excited_animation=True).wait_for_completed()
        seq = ''.join(random.sample(self.cur_word, self.cur_word.__len__()))
        while seq in self.anagram_words:
            seq = ''.join(random.sample(self.cur_word, self.cur_word.__len__()))

        self.ori_seq = self.cur_seq = seq
        self.paused = False
        self.correct_check_enabled = True
        self.turn_on_lights(cozmo.lights.Light(on_color=CubeColor.MAGENTA, off_color=CubeColor.MAGENTA))
        await self.map_cube_keys(self.cur_seq)
        CustomDisplay.display_text(robot=self.robot, text=self.get_disp_text(self.cur_seq), duration=5, size=32, xoffset=25, yoffset=0)

    async def on_update(self):
        all_grounded = True
        for c in self.cubes:
            diff = c.pose.position.z - self.ori_z
            if diff > Anagram.Z_THRESHOLD:
                all_grounded = False
                self.idle_count = 0
                break

        self.idle_count += 1
        if(self.idle_count == 1000):
            self.idle_count = 0
            await self.robot.play_anim("ID_test_shiver").wait_for_completed()

        if all_grounded and not self.paused:
            await self.update_cube_keys()
            if (self.cur_seq in self.anagram_words):
                self.idle_count = 0

                if(self.disp_timer < Anagram.DISP_THRESHOLD):
                    self.disp_timer += 1
                    CustomDisplay.display_text(robot=self.robot, text=self.get_disp_text(self.cur_seq), duration=10, size=32, xoffset=25, yoffset=0)
                elif (self.disp_timer == Anagram.DISP_THRESHOLD):
                    self.disp_timer = 0
                    self.score += 1
                    self.paused = True
                    await self.on_positive_response()
                    await self.disp_word()
            elif self.ori_seq != self.cur_seq and self.correct_check_enabled:
                self.idle_count = 0

                if (self.disp_timer < Anagram.DISP_THRESHOLD):
                    self.disp_timer += 1
                    CustomDisplay.display_text(robot=self.robot, text=self.get_disp_text(self.cur_seq), duration=10,
                                               size=32, xoffset=25, yoffset=0)
                elif (self.disp_timer == Anagram.DISP_THRESHOLD):
                    self.disp_timer = 0
                    self.paused = True
                    await self.on_negative_response()
                    await self.disp_word()

        await asyncio.sleep(0)

    async def on_positive_response(self):
        self.turn_on_lights(cozmo.lights.green_light)
        await self.robot.say_text(text=self.cur_seq, play_excited_animation=False,
                                  duration_scalar=1.5).wait_for_completed()
        self.pos_index += 1
        anim_arr = ["good","great","awesome","correct","right","perfect"]
        await self.robot.say_text(text=anim_arr[self.pos_index % anim_arr.__len__()], duration_scalar = 1.3).wait_for_completed()
        await self.robot.play_anim(random.choice(
            ["anim_hiking_rtnewarea_01","anim_hiking_rtnewarea_02","anim_hiking_rtnewarea_03"])).wait_for_completed()

    async def on_negative_response(self):
        self.turn_on_lights(cozmo.lights.red_light)
        self.neg_index += 1
        anim_arr = ["no","nope","incorrect"]
        await self.robot.say_text(text=anim_arr[self.neg_index % anim_arr.__len__()]).wait_for_completed()
        await self.robot.say_text(text="it was, " + self.cur_word, play_excited_animation=False,
                                  duration_scalar=1.3).wait_for_completed()
        await self.robot.play_anim(random.choice(
            ["anim_hiking_rtpmarker_01","anim_hiking_observe_01","anim_hiking_react_01","anim_hiking_react_02","anim_hiking_react_03","anim_hiking_react_05"])).wait_for_completed()

    async def map_cube_keys(self, word):
        await self.sort_cubes(self.cubes)
        index = 0
        for cube in self.cubes:
            cube.key = word[index]
            index += 1

        if word != None:
            CustomDisplay.display_text(robot=self.robot, text=word, duration=5, size=32, xoffset=25, yoffset=0)

    async def update_cube_keys(self):
        await self.sort_cubes(self.cubes)
        index = 0
        newseq = ''
        for cube in self.cubes:
            newseq += cube.key
            index += 1
        self.cur_seq = newseq
        CustomDisplay.display_text(robot=self.robot, text=self.get_disp_text(self.cur_seq), duration=5, size=32, xoffset=25, yoffset=0)

    def get_disp_text(self, word):
        disp = ""
        for c in word:
            disp += c + " "
        return disp

    async def sort_cubes(self,l):
        for passes_left in range(len(l) - 1, 0, -1):
            for index in range(passes_left):
                if l[index].pose.position.y > l[index + 1].pose.position.y:
                    l[index], l[index + 1] = l[index + 1], l[index]

    def turn_on_lights(self, light = cozmo.lights.white_light):
        for i in range(len(self.cubes)):
            self.cubes[i].set_lights(light)


    def get_json_data(self, url):
        reader = codecs.getreader("utf-8")
        response = urllib.request.urlopen(url)
        obj = json.load(reader(response))
        return obj


if __name__ == '__main__':
    Anagram()
