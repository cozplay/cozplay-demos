import _thread
import asyncio
import math
import random
import cozmo
from cozmo.util import degrees, Pose
from horseshoe.horse_shoe_slot import HorseShoeSlot
from horseshoe.horse_shoe_robot import HorseShoeRobot
from horseshoe.horse_shoe_player import HorseShoePlayer
import time
from pubnub import Pubnub

'''
Horseshoe Board Game
- This board game requires two cozmo which will compete against player.
- It is a turn based game similar to Tic Tac Toe where each player has two objects to play with.
- It is turn based game where both players move one of their object to the empty slot and goal of the player is to block the opponent to win
@class HorseShoe
@author - Team Cozplay
'''


class HorseShoe:

    PUBLISH_CHANNEL = "cozmo_channel"
    SUBSCRIBE_CHANNEL = "unity_channel"
    PUBLISH_KEY = "demo"
    SUBSCRIBE_KEY = "demo"
    DEBUG = False
    COZMO1 = "coz1"
    COZMO2 = "coz2"
    COZMO_SPEECH_SCALAR = 1.3
    INTRO_MSG_2 = "Let's play horse shoe"
    INTRO_MSG_1 = "We are team Cozmo!"
    INTRO_MSG_3 = "Let me begin"
    WAIT_MSG = "our term"
    PLAYER_TURN_MSG = "your term"
    PLAYER_WIN_MSG = ["you won!", "oh no"]
    COZ_WIN_MSG = ["you lost", "haha"]
    COZMO_START_ANIM = ["anim_explorer_getin_01", "anim_freeplay_reacttoface_identified_01"]

    def __init__(self):
        cozmo.conn.CozmoConnection.robot_factory = HorseShoeRobot
        cozmo.setup_basic_logging()

        self.pubnub = Pubnub(publish_key=HorseShoe.PUBLISH_KEY, subscribe_key=HorseShoe.SUBSCRIBE_KEY)
        self.pubnub.subscribe(channels=HorseShoe.SUBSCRIBE_CHANNEL, callback=self.on_message_received,
                              error=self.error,
                              connect=self.connect, reconnect=self.reconnect, disconnect=self.disconnect)
        self.init_game()

    def init_game(self):
        self.game_flag = 0
        self.slot_movement_dict = {
            1: [3, 5],
            2: [4, 5],
            3: [1, 4, 5],
            4: [2, 5, 3],
            5: [1, 2, 3, 4]
        }

        self.slot_state_dic = {
            1: HorseShoeSlot(state=0, active=0),
            2: HorseShoeSlot(state=0, active=0),
            3: HorseShoeSlot(state=0, active=0),
            4: HorseShoeSlot(state=0, active=0),
            5: HorseShoeSlot(state=0, active=0)
        }

        self.coz1 = None
        self.coz2 = None
        self.coz1_end_anim_flag = True
        self.coz2_end_anim_flag = True
        self.coz_end_anim_index = 0
        self.last_changed_slot = None
        self.game_turn = 0  # 0,1 - 0-cozmo's turn, 1- player's turn
        self.prev = 1
        self.start_anim_state = 0
        self.player_timer = None
        self.removed_slot = None
        self.meet_anim_green = True
        self.meet_anim_red = True
        self.meet_cozmo_flag = 0
        self.game_started = False
        self.did_cozmo_won = False
        _thread.start_new_thread(self.init_first_cozmo, (HorseShoe.COZMO1,))
        _thread.start_new_thread(self.init_second_cozmo, (HorseShoe.COZMO2,))

        # set player states
        self.player = HorseShoePlayer()

    @property
    def perpendicular_distance(self):
        return 460

    @property
    def diagonal_distance(self):
        return math.sqrt(2 * math.pow(self.perpendicular_distance, 2))

    def init_first_cozmo(self, name):
        cozmo.connect(self.run1)

    def init_second_cozmo(self, name):
        cozmo.connect(self.run2)

    async def run1(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self.coz1 = await coz_conn.wait_for_robot()
        self.coz1.id = HorseShoe.COZMO1
        await self.coz1.set_lift_height(0).wait_for_completed()

        await self.coz1.go_to_pose(Pose(0, 0, 0, angle_z=degrees(0)), relative_to_robot=True).wait_for_completed()
        if self.coz1 and self.coz2:
            if (self.coz1.pose.position.y < self.coz2.pose.position.y):
                self.coz1.set_light(cozmo.lights.green_light)
                self.coz2.set_light(cozmo.lights.red_light)
                self.coz1.slot = 1
                self.coz2.slot = 2
            else:
                self.coz1.set_light(cozmo.lights.red_light)
                self.coz2.set_light(cozmo.lights.green_light)
                self.coz1.slot = 2
                self.coz2.slot = 1

            self.empty_slot = 5
            if not HorseShoe.DEBUG:
                await self.coz1.say_text(HorseShoe.INTRO_MSG_1, duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()

            self.start_anim_state = 1

        while True:
            if (self.meet_anim_green and self.coz1.slot == 1) or (self.meet_anim_red and self.coz1.slot == 2):
                if self.meet_anim_green:
                    self.meet_anim_green = False
                if self.meet_anim_red:
                    self.meet_anim_red = False
                await self.coz1.play_anim("anim_meetcozmo_lookface_getin").wait_for_completed()

            if self.meet_cozmo_flag:
                self.meet_cozmo_flag -= 1
                if self.coz1.slot == 1:
                    await self.turn(self.coz1, 90)
                    await self.coz1.play_anim("id_react2block_02").wait_for_completed()
                    await self.turn(self.coz1, -90)
                else:
                    await self.turn(self.coz1, -90)
                    await self.coz1.play_anim("id_react2block_02").wait_for_completed()
                    await self.turn(self.coz1, 90)
                if self.meet_cozmo_flag == 0 and self.game_started == False:
                    self.game_started = True
                    self.start_game()

            if self.start_anim_state == 2:
                self.start_anim_state = 3
                if not HorseShoe.DEBUG:
                    await self.coz1.say_text(HorseShoe.INTRO_MSG_2,duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()

            if self.game_flag == 1:
                if (self.coz1.enabled and self.game_turn == 0):
                    self.coz1.enabled = False
                    # start backpack flash
                    self.coz1.flash(True)

                    if self.start_anim_state == 3:
                        self.start_anim_state = 0
                        if not HorseShoe.DEBUG:
                            await self.coz1.say_text(HorseShoe.INTRO_MSG_3, duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()

                    else:
                        await self.coz1.play_anim(self.get_random_idle_anim()).wait_for_completed()


                    await self.drive_to_slot(self.coz1, self.coz1.slot, self.empty_slot)
                    # stop backpack flash
                    self.coz1.flash(False)

                    if self.check_game_state():
                        self.game_turn = 1
                        await self.coz1.play_anim(self.get_random_idle_anim()).wait_for_completed()
                        

            elif self.game_flag == 2 and self.coz1_end_anim_flag:
                self.coz1_end_anim_flag = False

                if not HorseShoe.DEBUG:
                    await self.coz1.say_text(HorseShoe.COZ_WIN_MSG[self.coz_end_anim_index],
                                             duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()
                self.coz_end_anim_index += 1
                await self.coz1.play_anim("anim_speedtap_wingame_intensity02_01").wait_for_completed()
                self.did_cozmo_won = True

            elif self.game_flag == 3 and self.coz1_end_anim_flag:
                self.coz1_end_anim_flag = False
                if not HorseShoe.DEBUG:
                    await self.coz1.say_text(HorseShoe.PLAYER_WIN_MSG[self.coz_end_anim_index],
                                             duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()
                self.coz_end_anim_index += 1
                await self.coz1.play_anim("anim_speedtap_losegame_intensity03_02").wait_for_completed()
                self.did_cozmo_won = False

            await asyncio.sleep(0)

    async def run2(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self.coz2 = await coz_conn.wait_for_robot()
        self.coz2.id = HorseShoe.COZMO2
        await self.coz2.set_lift_height(0).wait_for_completed()

        await self.coz2.go_to_pose(Pose(0, 0, 0, angle_z=degrees(0)), relative_to_robot=True).wait_for_completed()
        if self.coz1 and self.coz2:
            if (self.coz2.pose.position.y < self.coz1.pose.position.y):
                self.coz2.set_all_backpack_lights(cozmo.lights.green_light)
                self.coz1.set_all_backpack_lights(cozmo.lights.red_light)
                self.coz2.slot = 1
                self.coz1.slot = 2
            else:
                self.coz1.set_all_backpack_lights(cozmo.lights.green_light)
                self.coz2.set_all_backpack_lights(cozmo.lights.red_light)
                self.coz2.slot = 2
                self.coz1.slot = 1

            self.empty_slot = 5
            if not HorseShoe.DEBUG:
                await self.coz2.say_text(HorseShoe.INTRO_MSG_1, duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()
            self.start_anim_state = 2

        while True:
            if (self.meet_anim_green and self.coz2.slot == 1) or (self.meet_anim_red and self.coz2.slot == 2):
                if self.meet_anim_green:
                    self.meet_anim_green = False
                if self.meet_anim_red:
                    self.meet_anim_red = False
                await self.coz2.play_anim("anim_meetcozmo_lookface_getin").wait_for_completed()

            if self.meet_cozmo_flag:
                self.meet_cozmo_flag -= 1
                if self.coz2.slot == 1:
                    await self.turn(self.coz2, 90)
                    await self.coz2.play_anim("id_react2block_02").wait_for_completed()
                    await self.turn(self.coz2, -90)
                else:
                    await self.turn(self.coz2, -90)
                    await self.coz2.play_anim("id_react2block_02").wait_for_completed()
                    await self.turn(self.coz2, 90)
                if self.meet_cozmo_flag == 0 and self.game_started == False:
                    self.game_started = True
                    self.start_game()

            if self.start_anim_state == 1:
                self.start_anim_state = 3
                if not HorseShoe.DEBUG:
                    await self.coz2.say_text(HorseShoe.INTRO_MSG_2,
                                             duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()

            if self.game_flag == 1:
                if (self.coz2.enabled and self.game_turn == 0):
                    self.coz2.enabled = False
                    # start backpack flash
                    self.coz2.flash(True)

                    if self.start_anim_state == 3:
                        self.start_anim_state = 0
                        if not HorseShoe.DEBUG:
                            await self.coz2.say_text(HorseShoe.INTRO_MSG_3, duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()

                    if self.last_changed_slot == 5:
                        await self.coz2.play_anim("").wait_for_completed()
                    else:
                        await self.coz2.play_anim(self.get_random_idle_anim()).wait_for_completed()

                    await self.drive_to_slot(self.coz2, self.coz2.slot, self.empty_slot)
                    # stop backpack flash
                    self.coz2.flash(False)

                    if self.check_game_state():
                        self.game_turn = 1
                        await self.coz2.play_anim(self.get_random_idle_anim()).wait_for_completed()


            elif self.game_flag == 2 and self.coz2_end_anim_flag:
                self.coz2_end_anim_flag = False

                if not HorseShoe.DEBUG:
                    if not HorseShoe.DEBUG:
                        await self.coz2.say_text(HorseShoe.COZ_WIN_MSG[self.coz_end_anim_index],
                                                 duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()
                self.coz_end_anim_index += 1
                await self.coz2.play_anim("anim_speedtap_wingame_intensity02_01").wait_for_completed()
                self.did_cozmo_won = True

            elif self.game_flag == 3 and self.coz2_end_anim_flag:
                self.coz2_end_anim_flag = False

                if not HorseShoe.DEBUG:
                    if not HorseShoe.DEBUG:
                        await self.coz2.say_text(HorseShoe.PLAYER_WIN_MSG[self.coz_end_anim_index],
                                                 duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()
                self.coz_end_anim_index += 1
                await self.coz2.play_anim("anim_speedtap_losegame_intensity03_02").wait_for_completed()
                self.did_cozmo_won = False

            await asyncio.sleep(0)

    def enable_cozmo(self):
        coz1_feasible_slot = self.empty_slot in self.slot_movement_dict[self.coz1.slot]
        coz2_feasible_slot = self.empty_slot in self.slot_movement_dict[self.coz2.slot]

        if (coz1_feasible_slot and coz2_feasible_slot):
            if random.uniform(0.0, 1.0) > 0.5:
                self.coz2.enabled = True
            else:
                self.coz1.enabled = True
        elif coz1_feasible_slot:
            self.coz1.enabled = True
        elif coz2_feasible_slot:
            self.coz2.enabled = True


    def check_game_state(self):
        if not self.coz1 or not self.coz2:
            return 1

        if (self.coz1.slot == 1 and self.coz2.slot == 3 and (
                        self.player.slots == [5, 4] or self.player.slots == [4, 5])) or (
                            self.coz1.slot == 3 and self.coz2.slot == 1 and (
                                self.player.slots == [5, 4] or self.player.slots == [4, 5])) or (
                            self.coz1.slot == 2 and self.coz2.slot == 4 and (
                                self.player.slots == [5, 3] or self.player.slots == [3, 5])) or (
                            self.coz1.slot == 4 and self.coz2.slot == 2 and (
                                self.player.slots == [5, 3] or self.player.slots == [3, 5])):
            self.game_flag = 3
            return -1

        elif (self.coz1.slot == 5 and self.coz2.slot == 4 and (
                        self.player.slots == [1, 3] or self.player.slots == [3, 1])) or (
                            self.coz1.slot == 4 and self.coz2.slot == 5 and (
                                self.player.slots == [1, 3] or self.player.slots == [3, 1])) or (
                            self.coz1.slot == 5 and self.coz2.slot == 3 and (
                                self.player.slots == [2, 4] or self.player.slots == [4, 2])) or (
                            self.coz1.slot == 3 and self.coz2.slot == 5 and (
                                self.player.slots == [2, 4] or self.player.slots == [4, 2])):
            self.game_flag = 2
            return -1

        return 1

    def start_game(self):
        self.player.slots = [3, 4]
        self.on_player_move(3, 4, True)
        self.game_flag = 1

    def on_player_move(self, player_from, player_to, on_start = False):
        # when cube is placed on slot
        if not on_start:
            self.empty_slot = player_from
            if player_from in self.player.slots:
                self.player.slots.remove(player_from)
            if player_to not in self.player.slots:
                self.player.slots.append(player_to)

        self.last_player_move = player_to
        if self.check_game_state():
            self.game_turn = 0
            self.enable_cozmo()
            self.player_timer = 0


    async def drive_to_slot(self, target_cozmo, from_slot, to_slot, say_slot=False):
        self.publish("CozmoMoveStart:" + str(from_slot) + "," + str(to_slot))

        if from_slot == 1 and to_slot == 3:
            await self.drive(target_cozmo, self.perpendicular_distance * 0.75)
        elif from_slot == 1 and to_slot == 5:
            await self.turn(target_cozmo, 45)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.5)
            await self.turn(target_cozmo, -45)
        elif from_slot == 2 and to_slot == 5:
            await self.turn(target_cozmo, -45)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.55)
            await self.turn(target_cozmo, 45)
        elif from_slot == 2 and to_slot == 4:
            await self.drive(target_cozmo, self.perpendicular_distance * 0.75)
        elif from_slot == 3 and to_slot == 4:
            await self.turn(target_cozmo, 90)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.8)
            await self.turn(target_cozmo, -90)
        elif from_slot == 3 and to_slot == 5:
            await self.turn(target_cozmo, 135)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.5)
            await self.turn(target_cozmo, -135)
        elif from_slot == 3 and to_slot == 1:
            await self.drive(target_cozmo, -self.perpendicular_distance * 0.75)
        elif from_slot == 4 and to_slot == 2:
            await self.drive(target_cozmo, -self.perpendicular_distance * 0.75)
        elif from_slot == 4 and to_slot == 5:
            await self.turn(target_cozmo, -135)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.55)
            await self.turn(target_cozmo, 135)
        elif from_slot == 4 and to_slot == 3:
            await self.turn(target_cozmo, -90)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.8)
            await self.turn(target_cozmo, 90)
        elif from_slot == 5 and to_slot == 1:
            await self.turn(target_cozmo, -135)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.5)
            await self.turn(target_cozmo, 135)
        elif from_slot == 5 and to_slot == 3:
            await self.turn(target_cozmo, -45)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.5)
            await self.turn(target_cozmo, 45)
        elif from_slot == 5 and to_slot == 4:
            await self.turn(target_cozmo, 45)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.5)
            await self.turn(target_cozmo, -45)
        elif from_slot == 5 and to_slot == 2:
            await self.turn(target_cozmo, 135)
            await self.drive(target_cozmo, self.perpendicular_distance * 0.5)
            await self.turn(target_cozmo, -135)

        # update cozmo slot
        target_cozmo.slot = to_slot
        # update empty slot
        self.empty_slot = from_slot

        if (say_slot):
            if not HorseShoe.DEBUG:
                await target_cozmo.say_text(HorseShoe.PLAYER_TURN_MSG, duration_scalar=HorseShoe.COZMO_SPEECH_SCALAR).wait_for_completed()

        self.player_timer = time.time()
        self.publish("CozmoMoveEnd:"+str(from_slot)+","+str(to_slot))
        await target_cozmo.play_anim(random.choice(["ID_pokedB","anim_rtc_react_01"])).wait_for_completed()

    async def drive(self, coz, dis, speed=3000):
        await coz.drive_straight(distance=cozmo.util.Distance(dis),
                                 speed=cozmo.util.Speed(speed), should_play_anim=True).wait_for_completed()

    async def turn(self, coz, degree):
        await coz.turn_in_place(angle=cozmo.util.Angle(degrees=degree)).wait_for_completed()

    def get_random_idle_anim(self):
        idle_anims = [
            "anim_hiking_react_04",
            "anim_explorer_driving01_start_01",
            "anim_explorer_driving01_start_02",
            "anim_explorer_driving01_turbo_01",
            "anim_explorer_drvback_start_01",
            "anim_explorer_idle_01",
            "anim_explorer_idle_02",
            "anim_explorer_idle_03",
            "anim_sparking_driving_loop_01",
            "anim_sparking_driving_loop_02",
            "anim_sparking_driving_loop_03",

        ]

        return random.choice(idle_anims)

    '''
            PUBNUB API METHODS
        '''

    def on_message_received(self, message, channel):
        '''
        This is called from pubnub api whenever it receives new message
        '''
        print(message)
        message = message.replace("\"","")
        if(message.find("PlayerReady") != -1):
            player_index = int(message.split(":")[1])
            if(player_index == 3):
                self.meet_anim_green = True
            if (player_index == 4):
                self.meet_anim_red = True

        if(message.find("PlayerMove") != -1):
            moves = message.split(":")[1].split(",")
            self.on_player_move(int(moves[0]), int(moves[1]))
        elif(message.find("StartGame") != -1):
            print("start game")
            self.meet_cozmo_flag = 2



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
        self.pubnub.publish(channel=HorseShoe.PUBLISH_CHANNEL, message=message)




if __name__ == '__main__':
    HorseShoe()
