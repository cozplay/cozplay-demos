import asyncio
import random
import time
import cozmo
from cozmo.util import degrees, Pose
from common.custom_cube import CustomCube
from common.cube_color import CubeColor
from common.cozmo_animation import CozmoAnimation
from common.cozgame import CozGame

'''
Odd One Out Game - Simple odd one out game with 3 cubes
@class OddOneOutGame
@author - Team Cozplay
'''


class OddOneOutGame(CozGame):
    def __init__(self, *a, **kw):
        CozGame.__init__(self)

        self._answer = None
        self._coz = None
        self._cubes = None

        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    @property
    def answer(self):
        return self._answer

    @answer.setter
    def answer(self, value):
        self._answer = value

    @property
    def coz(self):
        return self._coz

    @coz.setter
    def coz(self, value):
        self._coz = value

    @property
    def cubes(self):
        return self._cubes

    @cubes.setter
    def cubes(self, value):
        self._cubes = value

    async def run(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self.coz = await coz_conn.wait_for_robot()

        asyncio.ensure_future(self.update())
        while not self.exit_flag:
            await asyncio.sleep(0)

    async def update(self):
        await self.init_game()
        while True:
            await asyncio.sleep(0)
        self.coz.abort_all_actions()

    async def init_game(self):
        self.coz_anim = CozmoAnimation()
        await self.coz.go_to_pose(Pose(0, 0, 0, angle_z=degrees(0)), relative_to_robot=True).wait_for_completed()
        self.cubes = await self.coz.world.wait_until_observe_num_objects(num=3, object_type=cozmo.objects.LightCube)
        for i in range(len(self.cubes)):
            self.cubes[i].set_lights(cozmo.lights.white_light)

        # Success on finding cubes
        self.origin = self.coz.pose

        await self.coz.say_text(text="let's play odd one out", play_excited_animation=True).wait_for_completed()

        self.coz.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)
        self.coz.world.add_event_handler(cozmo.anim.EvtAnimationCompleted, self.on_anim_completed)
        await self.coz.say_text(text="Tap the odd color", play_excited_animation=True).wait_for_completed()
        self.answer = self.set_color_sequence(self.cubes)

    def set_color_sequence(self, cubes):
        colors = [CubeColor.RED, CubeColor.GREEN, CubeColor.BLUE, CubeColor.YELLOW, CubeColor.MAGENTA, CubeColor.CYAN]
        random.shuffle(colors)
        random.shuffle(cubes)
        self.set_cube_color(cubes[0], colors[0])
        self.set_cube_color(cubes[1], colors[0])
        self.set_cube_color(cubes[2], colors[1])
        return cubes[2]

    def set_cube_color(self, cube, color, flash=False):
        cube.color = color.name
        if flash:
            cube.set_lights(cozmo.lights.Light(color).flash())
        else:
            cube.set_lights(cozmo.lights.Light(color))

    async def on_object_tapped(self, evt, obj=None, tap_count=None, **kwargs):
        if self.answer == None:
            return

        if (obj != self.answer):
            self.turn_of_lights()
            self.answer = None
            await self.on_negative_response()

        else:
            self.turn_of_lights()
            self.answer = None
            await self.on_positive_response()

        await self.coz.go_to_pose(self.origin).wait_for_completed()
        await self.on_anim_completed(None)

    def change_pattern(self):
        for i in range(20):
            self.set_color_sequence(self.cubes)
            time.sleep(0.06)

        self.answer = self.set_color_sequence(self.cubes)

    async def on_positive_response(self):
        await self.coz.say_text(
            text=random.choice(["good job", "awesome", "correct", "right", "perfect"])).wait_for_completed()
        anim = self.coz_anim.get_random_pos_anim()
        await self.coz.play_anim(anim).wait_for_completed()

    async def on_negative_response(self):
        await self.coz.say_text(
            text=random.choice(["no", "noway", "nope", "no no", "incorrect", "false"])).wait_for_completed()
        anim = self.coz_anim.get_random_neg_anim()
        await self.coz.play_anim(anim).wait_for_completed()
        await self.coz.say_text(text="try again").wait_for_completed()

    async def on_anim_completed(self, evt, obj=None, tap_count=None, **kwargs):
        if not self.answer:
            self.change_pattern()

    def turn_of_lights(self):
        for cube in self.cubes:
            cube.set_lights_off()


if __name__ == '__main__':
    cozmo.world.World.light_cube_factory = CustomCube
    OddOneOutGame()
