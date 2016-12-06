import asyncio
import random
import threading
import cozmo
from common.cube_color import CubeColor
from common.custom_cube import CustomCube
from common.cozgame import CozGame
from cozmo.util import degrees, Pose

'''
Quick Tap Game - Simple reaction game with cozmo
@class QuickTapGame
@author - Team Cozplay
'''
class QuickTapGame(CozGame):
    GAME_ROUNDS = 5
    DEBUG = False

    def __init__(self):
        CozGame.__init__(self)

        self._score = 10
        self._coz = None
        self._cube = None
        self._tap_dealy = 0
        self.wait_for_tap_flag = False
        self._game_flag = 0
        self._round = 0
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    @property
    def tap_dealy(self):
        return self._tap_dealy

    @tap_dealy.setter
    def tap_dealy(self, value):
        self._tap_dealy = value

    @property
    def score(self):
        return self._score

    @score.setter
    def score(self, value):
        self._score = value

    @property
    def round(self):
        return self._round

    @round.setter
    def round(self, value):
        self._round = value

    @property
    def coz(self):
        return self._coz

    @coz.setter
    def coz(self, value):
        self._coz = value

    @property
    def cube(self):
        return self._cube

    @cube.setter
    def cube(self, value):
        self._cube = value

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


    async def init_game(self):
        await self.coz.set_lift_height(1, duration=0.5).wait_for_completed()
        await self.coz.go_to_pose(Pose(0, 0, 0, angle_z=degrees(0)), relative_to_robot=True).wait_for_completed()
        self.cube = await self.coz.world.wait_for_observed_light_cube()
        self.coz.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)

        self.round = 0
        self.score = 0
        self.coz_score = 0
        self.player_score = 0

        # activate cube
        self.cube.set_lights(cozmo.lights.white_light.flash())

        if not QuickTapGame.DEBUG:
            await self.coz.say_text(text="let's play Quick Tap",play_excited_animation=False, duration_scalar=1.3).wait_for_completed()
            await self.coz.say_text(text="best of "+ self.get_score_text(QuickTapGame.GAME_ROUNDS), play_excited_animation=False, duration_scalar=1.3).wait_for_completed()
            await self.coz.say_text(text="tap the white cube to start", play_excited_animation=False, duration_scalar=1.3).wait_for_completed()

        self.game_flag = 1

    async def start_game(self):
        await asyncio.sleep(2)
        await self.change_pattern()
        self.ori_pose = self.coz.pose

    async def reset_cozmo_position(self, newpose = None):
        await self.coz.go_to_pose(self.ori_pose if not newpose else newpose, relative_to_robot=False).wait_for_completed()

    async def on_object_tapped(self, evt=None, obj=None, tap_count=None, **kwargs):
        if not self.game_flag:
            return
        if self.game_flag == 1:
            self.turn_of_lights()
            await self.coz.say_text(text="ok let's start",
                                    play_excited_animation=False).wait_for_completed()
            self.game_flag = 2
            await self.start_game()
            return

        if self.wait_for_tap_flag:
            self.wait_for_tap_flag = False

            if self.did_cozmo_tap:
                self.turn_of_lights()
                self.score = self.score - 1
                self.coz_score = self.coz_score + 1

                await asyncio.sleep(1)
                text_Arr = ["you missed it"]
                await self.coz.say_text(text=random.choice(text_Arr), duration_scalar=1.3).wait_for_completed()
                await self.coz.play_anim(random.choice(
                    ["anim_speedtap_winhand_01", "anim_speedtap_winhand_02", "anim_speedtap_winhand_03",
                     "anim_speedtap_playeryes_01", "anim_keepaway_winhand_02"])).wait_for_completed()
            else:
                self.turn_of_lights()
                self.score = self.score + 1
                self.player_score = self.player_score + 1

                await asyncio.sleep(1)
                text_Arr = ["oh no", "you got it", "no way", "you are doing good"]
                await self.coz.say_text(text=random.choice(text_Arr), duration_scalar=1.3).wait_for_completed()
                await self.coz.play_anim(random.choice(
                    ["anim_speedtap_losehand_01", "anim_speedtap_losehand_02", "anim_speedtap_losehand_03",
                     "ID_test_shiver", "anim_keepaway_fakeout_02"])).wait_for_completed()

            await self.on_tap_round_complete()

    async def on_tap_round_complete(self):
        await self.say_score()
        self.round = self.round + 1
        if self.round == QuickTapGame.GAME_ROUNDS:
            await self.coz.say_text("Game Over").wait_for_completed()
            if self.score > 0:
                await self.coz.say_text("You beat me in quick tap").wait_for_completed()
                await self.coz.play_anim("anim_speedtap_losegame_intensity03_02").wait_for_completed()
            else:
                await self.coz.drive_wheels(-400, -400, duration=1)
                await self.coz.set_lift_height(0).wait_for_completed()
                await self.coz.say_text("i beat you in quick tap").wait_for_completed()
                await self.coz.play_anim("anim_speedtap_wingame_intensity02_01").wait_for_completed()

            return

        await asyncio.sleep(1)
        await self.change_pattern()

    async def change_pattern(self):
        magic = random.randint(30,60)
        magic_index = 0
        while magic_index < magic:
            self.set_color_sequence()
            magic_index = magic_index+1
            await asyncio.sleep(0.06)

        #activate tap
        await self.activate_tap()

    async def activate_tap(self):
        self.did_cozmo_tap = False
        self.wait_for_tap_flag = True
        # set random delay for cozmo tap and also wait for player tap
        await self.coz_tap()

    async def coz_tap(self):
        await asyncio.sleep(random.uniform(0.35,0.5))
        if not self.wait_for_tap_flag:
            return

        threading.Timer(0.5, self.trigger_cozmo_tap).start()
        tap_anim = random.choice(["anim_speedtap_tap_01", "anim_speedtap_tap_02", "anim_speedtap_tap_03"])
        await self.coz.play_anim(tap_anim).wait_for_completed()

    def trigger_cozmo_tap(self):
        self.did_cozmo_tap = True

    def set_color_sequence(self,flash=False):
        colors = [CubeColor.RED, CubeColor.GREEN, CubeColor.BLUE, CubeColor.YELLOW, CubeColor.MAGENTA, CubeColor.CYAN]
        random.shuffle(colors)
        rand_color = random.choice(colors)
        self.cube.color = rand_color.name
        if flash:
            self.cube.set_lights(cozmo.lights.Light(rand_color).flash())
        else:
            self.cube.set_lights(cozmo.lights.Light(rand_color))

    def turn_of_lights(self):
        self.cube.set_lights_off()

    async def say_score(self):
        score_text = "score " + self.get_score_text(self.coz_score) + " " + self.get_score_text(self.player_score)
        await self.coz.say_text(text=score_text, play_excited_animation=False,
                                duration_scalar=1).wait_for_completed()

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

if __name__ == '__main__':
    cozmo.world.World.light_cube_factory = CustomCube
    QuickTapGame()
