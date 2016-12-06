#!/usr/bin/env python3
"""
power_down.py -
    Reflex game based on colors displayed on Cozmo's backpack
    - Cozmo is powering down continuously and the only way to recharge him is to power up the cubes
    - The cubes switch color constantly. The player must tap on the cube when the color matches
      with the one displayed on Cozmo's backpack

@author - Team Cozplay
"""

import asyncio
import random
import sys

import cozmo

from cozmo.util import degrees
from cozmo.lights import Light, Color


class PowerDown:
    """ Class to backpack color and cube interactions between player and Cozmo """
    def __init__(self):
        print("Class 'PowerDown' Init...")
        self._coz = None
        self._cubes = None
        self._target = None
        self._colors = [RefColor.GREEN, RefColor.RED, RefColor.CYAN]
        self.complete = False
        self.action = None
        self.round_end = False
        self.failure = False

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
    def target(self):
        return self._target

    @target.setter
    def target(self, value):
        self._target = value

    @property
    def colors(self):
        return self._colors

    @colors.setter
    def colors(self, value):
        self._colors = value

    def set_cube_start(self, target_color):
        """ Reset cube colors to random colors not equal to the target color"""
        self.target = target_color
        for cube in self.cubes:
            index = random.randint(0, len(self.colors) - 1)
            if self.colors[index] == target_color:
                index += 1
            cube.color = self.colors[index % len(self.colors)]
            cube.activate_lights()

    async def on_object_tapped(self, evt, obj=None, tap_count=None, **kwargs):
        """ Custom handler to identify taps on cubes """
        print("on_object_tapped", evt, obj)
        if self.round_end:
            return
        index = self.colors.index(obj.color)
        obj.color = self.colors[(index+1) % len(self.colors)]
        obj.activate_lights()

    async def power_down(self, backpack_color, color_text):
        """ Simulate Cozmo powering down using arm, head and voice"""
        print("In power_down")
        # Vars
        speech_duration = 1.5
        arm_lower_speed = -0.05
        head_lower_speed = -0.1
        arm_threshold_low = 36
        speech_list = ["I need " + color_text,
                       "Get " + color_text + " NOW!",
                       "Quickly, tap" + color_text]

        # Init
        self.coz.set_all_backpack_lights(backpack_color)
        await self.coz.set_lift_height(1).wait_for_completed()
        await self.coz.set_head_angle(degrees(44.5)).wait_for_completed()

        # Begin powering down head and arm motion
        self.coz.move_lift(arm_lower_speed)
        self.coz.move_head(head_lower_speed)

        # Manipulate speech
        while True:
            # Speech
            self.action = self.coz.say_text(random.choice(speech_list), voice_pitch=-1.0,
                                            duration_scalar=speech_duration)
            await self.action.wait_for_completed()
            speech_duration += 1
            # Update current height
            await asyncio.sleep(0)
            # Poll for current height
            current_arm_height = self.coz.lift_height.distance_mm
            print(current_arm_height)
            if current_arm_height < arm_threshold_low:
                break
            if self.complete:
                return

        # Failure condition
        print("Failure")
        if not self.action.is_completed:
            self.action.abort()
        self.coz.move_head(0)
        await self.coz.set_head_angle(degrees(20)).wait_for_completed()
        await self.coz.play_anim("anim_rtpkeepaway_playerno_01").wait_for_completed()
        self.failure = True

    async def check_cubes(self):
        """ Check if all cubes are in target color state """
        count = 0
        print("In check_cubes")
        while True:
            for cube in self.cubes:
                if cube.color == self.target:
                    count += 1
                else:
                    count = 0
                    break
            if count == 3:
                print("Complete!")
                self.complete = True
                print("Abort")
                if not self.action.is_completed:
                    self.action.abort()
                for cube in self.cubes:
                    cube.set_lights(self.target.flash())
                return True
            await asyncio.sleep(0.5)

    async def round_manager(self, round_color, speech_text):
        """ Handle the calls to different methods at the start of each round"""
        print("Starting round")
        self.complete = False
        self.set_cube_start(round_color)
        future = asyncio.ensure_future(self.check_cubes())
        await self.power_down(round_color, speech_text)
        try:
            if future.result():
                print("Round complete")
                await self.round_ending()
        except asyncio.InvalidStateError:
            print("Failed to tap in time. Exiting program.")
        return

    async def round_ending(self):
        """ Clean up at the end of each round """
        success_anim_list = ["anim_memorymatch_successhand_cozmo_02",
                             "anim_keepaway_winhand_03",
                             "anim_upgrade_reaction_lift_01"]
        self.round_end = True
        self.coz.move_lift(0)
        self.coz.move_head(0)
        await self.coz.play_anim(random.choice(success_anim_list)).wait_for_completed()
        self.round_end = False

    # Game Loop
    async def game_loop(self):
        """ The main game loop """
        round_colors = {RefColor.RED: "Red", RefColor.GREEN: "Green", RefColor.CYAN: "Blue"}
        await self.coz.say_text("Hello my name is Cozmo!", play_excited_animation=True, duration_scalar=1.0).\
            wait_for_completed()
        self.coz.move_head(-0.15)
        await self.coz.say_text("Oh no, something's wrong", duration_scalar=4.5, voice_pitch=-1.0).wait_for_completed()
        for round_color, speech_text in round_colors.items():
            await self.round_manager(round_color, speech_text)
            if self.failure:
                return

    async def run(self, coz_conn):
        """ Main function to be run """
        asyncio.set_event_loop(coz_conn._loop)
        self.coz = await coz_conn.wait_for_robot()
        print("Connection established!")

        await self.coz.set_head_angle(degrees(0)).wait_for_completed()
        await self.coz.set_lift_height(0).wait_for_completed()

        # Look around for cubes
        lookaround = self.coz.start_behavior(cozmo.behavior.BehaviorTypes.LookAroundInPlace)
        try:
            self.cubes = await self.coz.world.wait_until_observe_num_objects(num=3, object_type=cozmo.objects.LightCube
                                                                             , timeout=30)
        except TimeoutError:
            print("Could not find all 3 cubes! :( Only found ", len(self.cubes), "Cube(s)")
            return
        finally:
            print("Stopping lookaround behavior")
            lookaround.stop()

        # Success on finding cubes
        await self.coz.play_anim("anim_memorymatch_successhand_cozmo_02").wait_for_completed()

        # Add method to tap handler
        self.coz.world.add_event_handler(cozmo.objects.EvtObjectTapped, self.on_object_tapped)

        # Execute the game loop
        await self.game_loop()

        # Ending cleanup
        if not self.failure:
            await self.coz.say_text("Thank you!", play_excited_animation=True, duration_scalar=1.0).wait_for_completed()
            await self.coz.play_anim("anim_meetcozmo_celebration_02").wait_for_completed()

        print("Ending program...")


class CustomCube (cozmo.objects.LightCube):
    """ Subclass of LightCube with light chaser effect """
    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)
        self._color = None

    @property
    def color(self):
        return self._color

    @color.setter
    def color(self, value):
        self._color = value

    def activate_lights(self):
        """ Activate the lights of the cube """
        self.set_lights(self.color)


class RefColor:
    """ Class to define common colors to be used """
    CYAN = Light(Color(name="cyan", int_color=0x00ffffff))
    MAGENTA = Light(Color(name="magenta", int_color=0xff00ffff))
    YELLOW = Light(Color(name="yellow", int_color=0xffff00ff))
    GREEN = Light(Color(name="green", int_color=0x00ff00ff))
    RED = Light(Color(name="red", int_color=0xff0000ff))
    BLUE = Light(Color(name="blue", int_color=0x0000ffff))
    WHITE = Light(Color(name="white", int_color=0xffffffff))
    OFF = Light(Color(name="off"))

# Set entry point
if __name__ == '__main__':
    cozmo.world.World.light_cube_factory = CustomCube
    PowerDown()
