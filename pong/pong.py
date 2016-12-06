import asyncio
import random
import time
from common.cozmo_animation import CozmoAnimation
import cozmo
from common.custom_cube import CustomCube
from cozmo.util import degrees, Pose
from common.cozgame import CozGame

'''
Pong Game - Simple reaction game with Cozmo where Cozmo and player needs to tap cube when light comes to their end
- If player misses hitting the cube at right time, he will lose point
- If Cozmo misses hitting the cube at right time, he will lose point
- If both hit the cube at right time, speed will increase for next round and this will continue until game ends

@class PongGame
@author - Team Cozplay
'''


class PongGame(CozGame):
    GAME_ROUNDS = 5
    DEBUG = False
    COZMO = "cozmo"
    PLAYER = "player"
    START_DELAY = 0.4

    def __init__(self, *a, **kw):
        CozGame.__init__(self)

        self._score = 10
        self._coz = None
        self._cube = None
        self._game_flag = 0
        self._round = 0
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    @property
    def coz(self):
        return self._coz

    @coz.setter
    def coz(self, value):
        self._coz = value

    @property
    def game_flag(self):
        return self._game_flag

    @game_flag.setter
    def game_flag(self, value):
        self._game_flag = value

    async def run(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self.coz = await coz_conn.wait_for_robot()

        asyncio.ensure_future(self.update())
        while not self.exit_flag:
            await asyncio.sleep(0)
        self.coz.abort_all_actions()

    async def update(self):
        await self.init_game()
        while True:
            await asyncio.sleep(0)

    async def reorder_cubes(self):
        cube1 = self.get_cozmo_cube()
        cube3 = self.get_player_cube()
        cube2 = None
        for cube in self.cubes:
            if cube != cube1 and cube != cube3:
                cube2 = cube
        self.cubes[0] = cube1
        self.cubes[1] = cube2
        self.cubes[2] = cube3

    async def init_game(self):
        await self.coz.set_lift_height(1, duration=0.5).wait_for_completed()
        await self.coz.go_to_pose(Pose(0, 0, 0, angle_z=degrees(0)), relative_to_robot=True).wait_for_completed()
        self.cubes = await self.coz.world.wait_until_observe_num_objects(num=3, object_type=cozmo.objects.LightCube)
        self.coz.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)


        self.cubes[0].set_light_corners(light1=cozmo.lights.white_light, light2=cozmo.lights.off_light,
                                        light3=cozmo.lights.off_light, light4=cozmo.lights.off_light)
        self.cubes[1].set_light_corners(light1=cozmo.lights.white_light, light2=cozmo.lights.white_light,
                                        light3=cozmo.lights.off_light, light4=cozmo.lights.off_light)
        self.cubes[2].set_light_corners(light1=cozmo.lights.white_light, light2=cozmo.lights.white_light,
                                        light3=cozmo.lights.white_light, light4=cozmo.lights.off_light)

        self.delay = PongGame.START_DELAY
        self.coz_score = 0
        self.player_score = 0
        self.game_flag = 1
        self.cozmo_last_tap_time = 0
        self.player_last_tap_time = 0
        self.max_reaction_time = 0.8
        self.player_hit = 0
        self.cozmo_hit = 0
        self.coz_happy_index = 0
        self.coz_sad_index = 0
        self.coz_anim = CozmoAnimation()

    async def start_game(self):
        await self.coz.say_text(text="ahaa      pong pong", play_excited_animation=False, duration_scalar=1).wait_for_completed()
        await self.coz.say_text(text="let's play", play_excited_animation=False,
                                duration_scalar=1.3).wait_for_completed()
        await self.change_pattern(pattern=1, start_with=PongGame.COZMO)

    def get_current_time(self):
        return time.time()

    def inc_speed(self):
        self.delay = self.delay - 0.05 if self.delay >= 0.1 else 0.1

    async def on_object_tapped(self, evt, obj=None, tap_count=None, **kwargs):
        if (evt.tap_intensity < 120 and self.game_flag == 1):
            return
        if not self.game_flag:
            return

        if self.game_flag == 1:
            self.turn_of_lights()
            self.game_flag = 2
            await self.start_game()
            return

        coz_hit_factor = self.delay * 2
        player_hit_factor = self.delay * 1.5
        if obj == self.cubes[0]:
            _diff = self.get_current_time() - self.cozmo_last_tap_time
            if (_diff < coz_hit_factor):
                self.cozmo_hit = 1
            
        elif obj == self.cubes[2]:
            _diff = self.get_current_time() - self.player_last_tap_time
            if (_diff < player_hit_factor):
                self.player_hit = 1
            
    async def start_round_cozmo(self):
        self.set_cube_corner_light(self.cubes[0], 1, light=cozmo.lights.white_light.flash())
        await asyncio.sleep(1)
        await self.coz_tap(delay=0)
        await asyncio.sleep(1)
        self.cozmo_hit = 0
        self.player_hit = 0

    async def start_round_player(self):
        self.set_cube_corner_light(self.cubes[2], 3, light=cozmo.lights.white_light.flash())
        await self.cubes[2].wait_for_tap()
        await asyncio.sleep(1)
        self.cozmo_hit = 0
        self.player_hit = 0

    async def check_cozmo_tap(self):
        if self.cozmo_hit == 0:
            return 0

        self.cozmo_hit = 0
        return 1

    async def check_player_tap(self):
        if self.player_hit == 0:
            return 0

        self.player_hit = 0
        return 1

    async def coz_tap(self, delay=None):
        rand_delay = delay if delay != None else random.uniform(0,self.delay*0.65)
        self.cozmo_last_tap_time = self.get_current_time()
        await asyncio.sleep(rand_delay)
        tap_anim = random.choice(["anim_speedtap_tap_01","anim_speedtap_tap_02","anim_speedtap_tap_03"])
        await self.coz.play_anim(tap_anim).wait_for_completed()

    async def on_cozmo_miss(self, pattern):
        self.cubes[0].set_lights(cozmo.lights.red_light.flash())
        anim_arr = ["anim_speedtap_losehand_01","anim_speedtap_losehand_03","anim_freeplay_hitground","anim_keepaway_fakeout_02","anim_speedtap_losehand_02","ID_test_shiver"]
        await self.coz.play_anim(anim_arr[self.coz_sad_index]).wait_for_completed()
        self.coz_sad_index = (self.coz_sad_index + 1) % len(anim_arr)

        # update score
        self.player_score = self.player_score + 1
        await self.say_score()
        self.turn_of_lights()
        if(self.player_score == PongGame.GAME_ROUNDS):
            await self.on_player_win()
            return
        await self.change_pattern(pattern=pattern, start_with=PongGame.PLAYER)

    async def on_player_miss(self, pattern):
        self.cubes[2].set_lights(cozmo.lights.red_light.flash())
        anim_arr = ["anim_speedtap_winhand_01","anim_speedtap_winhand_02","anim_speedtap_winhand_03","anim_speedtap_playeryes_01","anim_keepaway_winhand_02"]
        await self.coz.play_anim(anim_arr[self.coz_happy_index]).wait_for_completed()
        self.coz_happy_index = (self.coz_happy_index + 1) % len(anim_arr)

        # update score
        self.coz_score = self.coz_score + 1
        await self.say_score()
        self.turn_of_lights()
        if (self.coz_score == PongGame.GAME_ROUNDS):
            await self.on_cozmo_win()
            return
        await self.change_pattern(pattern=pattern, start_with=PongGame.COZMO)

    async def change_pattern(self, pattern, start_with):
        self.delay = PongGame.START_DELAY
        if pattern == 1:
            if start_with == PongGame.COZMO:
                await self.start_round_cozmo()
                while True:

                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 3)

                    # set player ideal tap time
                    self.player_last_tap_time = self.get_current_time()
                    await asyncio.sleep(self.delay)

                    # check player tap
                    await asyncio.sleep(self.delay*0.5)
                    result = await self.check_player_tap()
                    if result == 0:
                        await self.on_player_miss(pattern=random.choice([1,2]))
                        break

                    # reverse
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)

                    self.set_cube_corner_light(self.cubes[0], 1)
                    await self.coz_tap()
                    await asyncio.sleep(self.delay)
                    # check cozmo tap
                    result = await self.check_cozmo_tap()
                    if result == 0:
                        await self.on_cozmo_miss(pattern=random.choice([1,2]))
                        break

                    self.inc_speed()

            elif start_with == PongGame.PLAYER:
                await self.start_round_player()
                while True:

                    # reverse
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)

                    self.set_cube_corner_light(self.cubes[0], 1)
                    await self.coz_tap()
                    await asyncio.sleep(self.delay)
                    # check cozmo tap
                    result = await self.check_cozmo_tap()
                    if result == 0:
                        await self.on_cozmo_miss(pattern=1)
                        break

                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)

                    self.set_cube_corner_light(self.cubes[2], 3)
                    # set player ideal tap time
                    self.player_last_tap_time = self.get_current_time()
                    await asyncio.sleep(self.delay)

                    # check player tap
                    await asyncio.sleep(self.delay*0.5)
                    result = await self.check_player_tap()
                    if result == 0:
                        await self.on_player_miss(pattern=random.choice([1,2]))
                        break

                    self.inc_speed()

        elif pattern == 2:
            if start_with == PongGame.COZMO:
                await self.start_round_cozmo()
                while True:

                    self.set_cube_corner_light(self.cubes[0], 2)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 4)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 2)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 3)
                    await asyncio.sleep(self.delay)

                    # set player ideal tap time
                    self.player_last_tap_time = self.get_current_time()
                    await asyncio.sleep(self.delay)

                    # check player tap
                    await asyncio.sleep(self.delay*0.5)
                    result = await self.check_player_tap()
                    if result == 0:
                        await self.on_player_miss(pattern=random.choice([1,2]))
                        break

                    # reverse
                    self.set_cube_corner_light(self.cubes[2], 4)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 2)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 4)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 1)

                    await self.coz_tap()
                    await asyncio.sleep(self.delay)
                    # check cozmo tap
                    result = await self.check_cozmo_tap()
                    if result == 0:
                        await self.on_cozmo_miss(pattern=random.choice([1,2]))
                        break

                    self.inc_speed()


            elif start_with == PongGame.PLAYER:
                await self.start_round_player()
                while True:

                    self.set_cube_corner_light(self.cubes[2], 4)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 2)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 4)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 1)

                    await self.coz_tap()
                    await asyncio.sleep(self.delay)
                    # check cozmo tap
                    result = await self.check_cozmo_tap()
                    if result == 0:
                        await self.on_cozmo_miss(pattern=random.choice([1,2]))
                        break

                    self.set_cube_corner_light(self.cubes[0], 2)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[0], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 4)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[1], 3)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 1)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 2)
                    await asyncio.sleep(self.delay)
                    self.set_cube_corner_light(self.cubes[2], 3)
                    await asyncio.sleep(self.delay)

                    # set player ideal tap time
                    self.player_last_tap_time = self.get_current_time()
                    await asyncio.sleep(self.delay)

                    # check player tap
                    await asyncio.sleep(self.delay*0.5)
                    result = await self.check_player_tap()
                    if result == 0:
                        await self.on_player_miss(pattern=random.choice([1,2]))
                        break

                    self.inc_speed()

    async def say_score(self):
        score_text = "score " + self.get_score_text(self.coz_score) + " " + self.get_score_text(self.player_score)
        await self.coz.say_text(text=score_text, play_excited_animation=False,
                                duration_scalar=1).wait_for_completed()
        if(self.coz_score == PongGame.GAME_ROUNDS - 1):
            await self.coz.say_text(text="common", play_excited_animation=False,duration_scalar = 1).wait_for_completed()

    def get_score_text(self, d):
        if d == 0:
            return "zero"
        if d == 1:
            return "one"
        if d == 2:
            return "two"
        if d == 3:
            return "three"
        if d == 4:
            return "four"
        if d == 5:
            return "five"
        if d == 6:
            return "six"
        if d == 7:
            return "seven"
        if d == 8:
            return "eight"
        if d == 9:
            return "nine"


    def get_cozmo_cube(self):
        min_cube = self.cubes[0]
        min_dis = self.coz.pose.position.x - min_cube.pose.position.x
        for cube in self.cubes:
            dis = self.coz.pose.position.x - cube.pose.position.x
            if dis < min_dis:
                min_cube = cube
                min_dis = dis
        return min_cube


    def get_player_cube(self):
        max_cube = self.cubes[0]
        max_dis = self.coz.pose.position.x - max_cube.pose.position.x
        for cube in self.cubes:
            dis = self.coz.pose.position.x - cube.pose.position.x
            if dis > max_dis:
                max_cube = cube
                max_dis = dis
        return max_cube


    def turn_of_lights(self):
        for cube in self.cubes:
            cube.set_lights_off()


    def turn_on_lights(self):
        for cube in self.cubes:
            cube.set_lights(cozmo.lights.white_light)


    def set_cube_corner_light(self, cube, index, light=cozmo.lights.white_light):
        self.turn_of_lights()
        if index == 1:
            cube.set_light_corners(light1=light, light2=cozmo.lights.off_light, light3=cozmo.lights.off_light,
                                   light4=cozmo.lights.off_light)
        elif index == 2:
            cube.set_light_corners(light1=cozmo.lights.off_light, light2=light, light3=cozmo.lights.off_light,
                                   light4=cozmo.lights.off_light)
        elif index == 3:
            cube.set_light_corners(light1=cozmo.lights.off_light, light2=cozmo.lights.off_light, light3=light,
                                   light4=cozmo.lights.off_light)
        elif index == 4:
            cube.set_light_corners(light1=cozmo.lights.off_light, light2=cozmo.lights.off_light,
                                   light3=cozmo.lights.off_light, light4=light)
    async def on_cozmo_win(self):
        await self.coz.say_text("Game Over. I won pong pong challenge").wait_for_completed()
        await self.coz.play_anim("anim_speedtap_wingame_intensity02_01").wait_for_completed()

    async def on_player_win(self):
        await self.coz.say_text("Game Over. You won pong pong challenge").wait_for_completed()
        await self.coz.play_anim("anim_speedtap_losegame_intensity03_02").wait_for_completed()


if __name__ == '__main__':
    cozmo.world.World.light_cube_factory = CustomCube
    PongGame()
