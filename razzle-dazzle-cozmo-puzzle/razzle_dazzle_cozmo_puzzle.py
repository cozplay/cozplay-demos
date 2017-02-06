import os
import inflect
import math
import asyncio
import cozmo
import pygame
import random
import PIL.Image
import PIL.ImageFont

# Run Configuration
os.environ['COZMO_PROTOCOL_LOG_LEVEL'] = 'DEBUG'
os.environ['COZMO_LOG_LEVEL'] = 'DEBUG'
USE_VIEWER = False
USE_LOGGING = False
# Note: To use a custom font, uncomment the below line, set path/size, and restore font parameter on lines 166 and 167.
# FONT = PIL.ImageFont.truetype("./fonts/Avenir-Roman.ttf", 18)

# Constants
MAX_DURATION_S = 10
MIN_DURATION_S = 3
NUM_CUBES_REQUIRED = 3
ROBOT_SEARCH_TIMEOUT = 3
CUBE_SEARCH_TIMEOUT = 3
STACK_THRESHOLD_MM = cozmo.util.Position(30, 30, 50)

RED = cozmo.lights.Color(rgba=(255, 0, 0, 255))
BLUE = cozmo.lights.Color(rgba=(0, 0, 255, 255))
YELLOW = cozmo.lights.Color(rgba=(255, 255, 0, 255))
PURPLE = cozmo.lights.Color(rgba=(138, 43, 226 , 255))
GREEN = cozmo.lights.Color(rgba=(0, 255, 0, 255))
ORANGE = cozmo.lights.Color(rgba=(255, 165, 0, 255))
BLACK = cozmo.lights.Color(rgba=(100, 100, 100, 255))
PRIMARY = [RED, BLUE, YELLOW]
PRIMARY_GOAL = "PRIMARY"
GOALS = {PURPLE: "purple", GREEN: "green", ORANGE: "orange", BLACK: "black", PRIMARY_GOAL: "primary"}
MIX = {frozenset([RED, BLUE]): PURPLE, frozenset([BLUE, YELLOW]): GREEN, frozenset([RED, YELLOW]): ORANGE}

'''
Razzle Dazzle Cozmo Puzzle
- The game will start once all three cubes are visible to Cozmo. They must remain visible for the duration of the game.
- Create the colors Cozmo requests by stacking the appropriate primary-colored blocks.
- For instance, orange is created by stacking red on yellow or vice versa.
- Black is created by making a pyramid.
- "Primary" is made by unstacking all the blocks.

@author Team Cozplay
'''
class RazzleDazzleCozmoPuzzle:
    def __init__(self):
        self._robot = None
        self._cubes = []

        # initial setting ensures we don't first randomize to PRIMARY_GOAL
        self._goal = PRIMARY_GOAL

        self._sfx_success = pygame.mixer.Sound('./sfx/success.wav')
        self._sfx_tick = pygame.mixer.Sound('./sfx/tick.wav')

        if USE_LOGGING:
            cozmo.setup_basic_logging()
        if USE_VIEWER:
            cozmo.connect_with_tkviewer(self.run)
        else:
            cozmo.connect(self.run)

    async def set_up_cozmo(self, coz_conn):
        print("SETTING UP COZMO")
        asyncio.set_event_loop(coz_conn._loop)
        try:
            self._robot = await coz_conn.wait_for_robot(ROBOT_SEARCH_TIMEOUT)
            self._robot.camera.image_stream_enabled = True
            return True
        except TimeoutError:
            print("No Cozmo :(")
            return False

    async def set_up_cubes(self) -> bool:
        print("SETTING UP CUBES")
        await self._robot.set_head_angle(cozmo.util.Angle(degrees=0)).wait_for_completed()

        # For why we're not just calling wait_until_observe_num_objects, see this post:
        # https://forums.anki.com/t/vision-issues-missing-cubes-and-tkviewer-growing/1451/
        for obj in self._robot.world.visible_objects:
            if isinstance(obj, cozmo.objects.LightCube):
                self._cubes.append(obj)
        if len(self._cubes) < NUM_CUBES_REQUIRED:
            cubes = await self._robot.world.wait_until_observe_num_objects(NUM_CUBES_REQUIRED - len(self._cubes),
                                                                           cozmo.objects.LightCube,
                                                                           CUBE_SEARCH_TIMEOUT)
            self._cubes = list(set(self._cubes + cubes))

        if len(self._cubes) < NUM_CUBES_REQUIRED:
            print("Only found ", len(self._cubes), " cubes")
            return False

        # Assign cubes primary colors
        for index, cube in enumerate(self._cubes):
            cube.primary = PRIMARY[index]
            cube.color = PRIMARY[index]

        return True

    def are_cubes_stacked(self, cube0: cozmo.objects.LightCube, cube1: cozmo.objects.LightCube) -> bool:
        if (abs(cube1.pose.position.x - cube0.pose.position.x) < STACK_THRESHOLD_MM.x and
                    abs(cube1.pose.position.y - cube0.pose.position.y) < STACK_THRESHOLD_MM.y and
                    abs(cube1.pose.position.z - cube0.pose.position.z) < STACK_THRESHOLD_MM.z):
            return True
        else:
            return False

    async def randomize_goal(self):
        goal = random.choice(list(GOALS.keys()))
        while goal is self._goal:
            goal = random.choice(list(GOALS.keys()))
        # TODO: Why do backpack lights look wrong / non-uniform?
        if goal is PRIMARY_GOAL:
            self._robot.set_backpack_lights(light1=cozmo.lights.off_light,
                                            light2=cozmo.lights.Light(on_period_ms=100000, on_color=RED),
                                            light3=cozmo.lights.Light(on_period_ms=100000, on_color=BLUE),
                                            light4=cozmo.lights.Light(on_period_ms=100000, on_color=YELLOW),
                                            light5=cozmo.lights.off_light)
        else:
            backpack_light = cozmo.lights.Light(on_period_ms=100000, on_color=goal)
            self._robot.set_all_backpack_lights(cozmo.lights.Light(goal))
        self._goal = goal
        await self._robot.say_text(GOALS[goal]).wait_for_completed()

    # returns True if goal color was achieved
    def update_cubes(self) -> bool:
        stacked_cubes = set()
        if self.are_cubes_stacked(self._cubes[0], self._cubes[1]):
            stacked_cubes.add(self._cubes[0])
            stacked_cubes.add(self._cubes[1])
        if self.are_cubes_stacked(self._cubes[1], self._cubes[2]):
            stacked_cubes.add(self._cubes[1])
            stacked_cubes.add(self._cubes[2])
        if self.are_cubes_stacked(self._cubes[0], self._cubes[2]):
            stacked_cubes.add(self._cubes[0])
            stacked_cubes.add(self._cubes[2])

        stacked_cubes = list(stacked_cubes)
        mixed_color = -1
        if len(stacked_cubes) == 0:
            mixed_color = PRIMARY_GOAL
            for cube in self._cubes:
                cube.color = cube.primary
        elif len(stacked_cubes) == 2:
            mixed_color = MIX[frozenset([stacked_cubes[0].primary, stacked_cubes[1].primary])]
            stacked_cubes[0].color = mixed_color
            stacked_cubes[1].color = mixed_color
            for cube in self._cubes:
                if cube not in stacked_cubes:
                    cube.color = cube.primary
        else:
            mixed_color = BLACK
            for cube in self._cubes:
                cube.color = BLACK

        return mixed_color is self._goal

    def display_timer(self, seconds:int):
        [width, height] = cozmo.oled_face.dimensions()
        text_image = PIL.Image.new('RGBA', cozmo.oled_face.dimensions(), (0, 0, 0, 255))
        context = PIL.ImageDraw.Draw(text_image)
        context.text((0, height/2-10), str(seconds), fill=(255, 255, 255, 255))#, font=FONT)
        context.text((width/2, height/2-10), str(GOALS[self._goal]), fill=(255, 255, 255, 255))#, font=FONT)
        oled_face_data = cozmo.oled_face.convert_image_to_screen_data(text_image)
        self._robot.display_oled_face_image(oled_face_data, 30000.0)

    def get_flavor_text(self, score:int) -> str:
        if score >= 20:
            return "You're incredible"
        elif score >= 8:
            return "You're good"
        elif score >= 3:
            return "You're okay"
        else:
            return "You suck"

    async def perform_ending_animation(self, score:int):
        if score >= 8:
            await self._robot.play_anim("anim_upgrade_reaction_tracks_01").wait_for_completed()
        elif score >= 3:
            await self._robot.play_anim("anim_speedtap_playerno_01").wait_for_completed()
        else:
            await self._robot.play_anim("anim_reacttoblock_frustrated_int2_01").wait_for_completed()

    async def clean_up(self):
        print("CLEANING UP")
        for cube in self._cubes:
            cube.color = cozmo.lights.off
        self._robot.set_backpack_lights_off()
        self._robot.abort_all_actions()
        self._robot.stop_all_motors()

    async def run(self, coz_conn):
        # Set up Cozmo
        if not await self.set_up_cozmo(coz_conn):
            return

        # Set up cubes
        if not await self.set_up_cubes():
            await self.clean_up()
            return

        # Intro
        await self._robot.play_anim("anim_reacttoblock_happydetermined_01").wait_for_completed()
        await self._robot.say_text("Let's play Razzle Dazzle Cozmo Puzzle", duration_scalar=1.3).wait_for_completed()

        # Main game loop
        await self.randomize_goal()
        timer_duration = MAX_DURATION_S
        timer_remaining = timer_duration
        score = 0
        self._sfx_tick.play()
        self.display_timer(MAX_DURATION_S)
        while True:
            success = self.update_cubes()
            if success:
                score += 1
                self._sfx_success.play()
                await self.randomize_goal()
                if timer_duration - 1 >= MIN_DURATION_S:
                    timer_duration -= 1
                timer_remaining = timer_duration
                self._sfx_tick.play()
                self.display_timer(timer_remaining)
            await asyncio.sleep(1/30)
            old_remaining = timer_remaining
            timer_remaining -= 1/30
            if math.ceil(old_remaining) != math.ceil(timer_remaining):
                if timer_remaining <= 0:
                    # Game over
                    break
                else:
                    self._sfx_tick.play()
                    self.display_timer(int(math.ceil((timer_remaining))))

        # Game over
        await self.perform_ending_animation(score)
        end_text = "You scored " + inflect.engine().number_to_words(score) + " points"
        await self._robot.say_text(end_text, duration_scalar=1.3).wait_for_completed()
        await self._robot.play_anim("anim_memorymatch_pointcenter_02").wait_for_completed()
        await self._robot.say_text(self.get_flavor_text(score), duration_scalar=1.3).wait_for_completed()

        # Cleanup
        await self.clean_up()


class RazzleCube(cozmo.objects.LightCube):
    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)
        self._color = cozmo.lights.off
        self._primary = RED

    @property
    def color(self):
        return self._color

    @color.setter
    def color(self, value: cozmo.lights.Color):
        self._color = value
        self.set_lights(cozmo.lights.Light(value))

    @property
    def primary(self):
        return self._primary

    @primary.setter
    def primary(self, value: cozmo.lights.Color):
        self._primary = value


if __name__ == '__main__':
    pygame.mixer.init(frequency=44100, size=-16, channels=1, buffer=1024)
    cozmo.world.World.light_cube_factory = RazzleCube
    RazzleDazzleCozmoPuzzle()
