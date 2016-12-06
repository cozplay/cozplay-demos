import asyncio
import cozmo
from cozmo.util import degrees
from cozmo.lights import Color

'''
@author - Team Cozplay
place_cubes.py -
    Place cubes is a simple prototype that involves Cozmo building a mini fort using cubes chosen by the player.
    The idea behind the prototype was to combine Cozmo’s strongest traits, his emotions and his interaction with cubes,
    into a small fun activity.

Gameplay -
1.  First detects 3 cubes and then asks player to tap them.
2.  Picks up the tapped cube and places them in semi-circle around him.

Instructions -
1.  Cozmo needs to find three cubes before he can interact with them.
2.  Works best when all three cubes are placed in front of Cozmo. However, the lookaround in place behavior will
    activate at the start to locate the cubes.
'''


class PlaceCubesAround:
    """ Class to handle cube interactions between player and Cozmo """
    def __init__(self):
        print("Class 'PlaceCubesAround' Init...")
        self._coz = None
        self._cubes = None
        self._origin = None

        cozmo.setup_basic_logging()
        cozmo.connect_with_tkviewer(self.run)

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

    @property
    def origin(self):
        return self._origin

    @origin.setter
    def origin(self, value):
        self._origin = value

    async def on_object_tapped(self, evt, obj=None, tap_count=None, **kwargs):
        """ Custom handler to identify taps on cubes """
        print("on_object_tapped", evt, obj)
        # Ignore taps if object is already placed
        if obj.placed:
            return

        # Deactivate chaser effect
        for cube in self.cubes:
            # Exit if another cube is already being picked up
            if cube.active:
                return
            cube.stop_light_chaser()
            cube.set_lights_off()
        # Activate lights on tapped object and set active flag for it to be picked up
        obj.activate_lights()
        obj.active = True

    async def place_cube(self, obj, turn_degree):
        """ Method to place cubes around the origin pose captured at init
        @Params
        cube_index - The id of the cube to be picked up and placed
        turn_degree - The value in int for amount of degrees to turn before placing cube once at origin pose
        """
        await self.coz.pickup_object(obj).wait_for_completed()
        await self.coz.go_to_pose(self.origin).wait_for_completed()
        await self.coz.turn_in_place(degrees(turn_degree)).wait_for_completed()
        # Check if cozmo needs to turn
        if not turn_degree == 0:
            # Move forward to provide some space for animations and maneuverability after turning
            await self.coz.drive_wheels(100, 100, duration=1)
        await self.coz.place_object_on_ground_here(obj).wait_for_completed()
        # Animation to be played after placing a cube
        await self.coz.play_anim("anim_reacttoblock_success_01").wait_for_completed()
        # Deactivate lights on active cube
        obj.cube_placed()
        # Activate chasers on remaining cubes
        await self.coz.turn_in_place(degrees(-turn_degree)).wait_for_completed()
        for cube in self.cubes:
            if not cube.placed:
                cube.start_light_chaser()
        # Ask for a new cube or signal that game is over
        if turn_degree == 0:
            await self.coz.say_text("Tie tie", duration_scalar=0.75, voice_pitch=1.0).wait_for_completed()
        else:
            await self.coz.play_anim("anim_reacttoblock_ask_01").wait_for_completed()

    async def game_loop(self):
        """ Define the placement of each cube around Cozmo """
        await self.coz.say_text("Korbash", duration_scalar=1.0, voice_pitch=1.0).wait_for_completed()
        count = 0
        cube_turn = [-90, 90, 0]
        while count < 3:
            for cube in self.cubes:
                if cube.active:
                    await self.coz.play_anim("anim_memorymatch_successhand_cozmo_02").wait_for_completed()
                    await self.place_cube(cube, cube_turn[count])
                    count += 1

            await asyncio.sleep(1)

    async def run(self, coz_conn):
        """ Main function to be run """
        asyncio.set_event_loop(coz_conn._loop)
        self.coz = await coz_conn.wait_for_robot()
        print("Connection established!")
        await self.coz.set_lift_height(0).wait_for_completed()
        print("Arm reset!")
        self.origin = self.coz.pose
        await self.coz.say_text("Korbash", duration_scalar=1.0, voice_pitch=1.0).wait_for_completed()

        # Look around for cubes
        lookaround = self.coz.start_behavior(cozmo.behavior.BehaviorTypes.LookAroundInPlace)
        try:
            self.cubes = await self.coz.world.wait_until_observe_num_objects(num=3, object_type=cozmo.objects.LightCube,
                                                                             timeout=30)
        except TimeoutError:
            print("Could not find all 3 cubes! :( Only found ", len(self.cubes), "Cube(s)")
            return
        finally:
            print("Stopping lookaround behavior")
            lookaround.stop()

        # Success on finding cubes
        await self.coz.play_anim("anim_memorymatch_successhand_cozmo_02").wait_for_completed()

        # On finding all cubes turn on their lights
        for cube in self.cubes:
            if not cube.placed:
                cube.start_light_chaser()

        self.coz.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)

        # Execute the game loop
        await self.game_loop()

        # Ending cleanup
        await self.coz.play_anim("majorwin").wait_for_completed()
        print("Ending program...")
        for cube in self.cubes:
            cube.stop_light_chaser()
            cube.set_lights_off()


class CustomCube (cozmo.objects.LightCube):
    """ Subclass of LightCube with light chaser effect """
    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)
        self._color = None
        self._chaser = None
        self._placed = False
        self._active = False

    @property
    def color(self):
        return self._color

    @color.setter
    def color(self, value):
        self._color = value

    @property
    def placed(self):
        return self._placed

    @placed.setter
    def placed(self, value):
        self._placed = value

    @property
    def active(self):
        return self._active

    @active.setter
    def active(self, value):
        self._active = value

    def start_light_chaser(self):
        """ Start running lights on the cube """
        if self._chaser:
            raise ValueError("Light chaser already running")
        async def _chaser():
            while True:
                for i in range(4):
                    cols = [cozmo.lights.off_light] * 4
                    cols[i] = cozmo.lights.Light(CubeColor.CYAN)
                    self.set_light_corners(*cols)
                    await asyncio.sleep(0.1, loop=self._loop)
        self._chaser = asyncio.ensure_future(_chaser(), loop=self._loop)

    def stop_light_chaser(self):
        """ Stop the chaser effect """
        if self._chaser:
            self._chaser.cancel()
            self._chaser = None

    def activate_lights(self):
        """ Set selected cube to flash green """
        self.set_lights(cozmo.lights.Light(CubeColor.GREEN).flash())

    def cube_placed(self):
        """ Clean up after cube was placed """
        self.set_lights_off()
        self.active = False
        self.placed = True


class CubeColor:
    """ Class to define common colors to be used """
    CYAN = Color(name="cyan", int_color=0x00ffffff)
    MAGENTA = Color(name="magenta", int_color=0xff00ffff)
    YELLOW = Color(name="yellow", int_color=0xffff00ff)
    GREEN = Color(name="green", int_color=0x00ff00ff)
    RED = Color(name="red", int_color=0xff0000ff)
    BLUE = Color(name="blue", int_color=0x0000ffff)
    WHITE = Color(name="white", int_color=0xffffffff)
    OFF = Color(name="off")

# Set entry point
if __name__ == '__main__':
    cozmo.world.World.light_cube_factory = CustomCube
    PlaceCubesAround()
