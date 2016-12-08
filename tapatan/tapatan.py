#!/usr/bin/env python3
"""
tapatan.py:
    - Cozmo plays Tapatan with you using a virtual board
    - Rules can be found here: https://en.wikipedia.org/wiki/Tapatan
    - Cozmo's AI works in the following fashion:
        a. He looks for a move that will let him win
        b. Otherwise he looks for a move that will block you from winning
        c. If these two conditions are not satisfied, he plays a randomly selected marker to a random selection

@author - Team Cozplay
"""

# Standard python modules
import asyncio
import sys
import random

# Cozmo SDK
import cozmo

# GridControl
from grid_control import *

# Helper modules
from cozmo.util import degrees, distance_mm, speed_mmps

# Global variables for config
# Volume for sanity
ROBOT_VOLUME = 1
# Distance moved (mm) by Cozmo for 1 cell unit
MOVE_DISTANCE = 165


class Direction:
    """ Enum to handle all possible direction inputs

    LEFT / RIGHT / UP / DOWN
    DIAG_DR - Diagonal Down Right
    DIAG_UR - Diagonal Up Right
    DIAG_UL - Diagonal Up Left
    DIAG_DL - Diagonal Down Left
    """
    RIGHT = 1
    LEFT = -1
    UP = -1
    DOWN = 1
    DIAG_DR = 0
    DIAG_UR = 1
    DIAG_UL = 2
    DIAG_DL = 3


class GameState:
    """ Enum for different states of the game

       SETUP - Initial phase where player and Cozmo take turns placing down markers
       INGAME - Actual gameplay
       END - Post game cleanup
       """
    SETUP = 1,
    INGAME = 2,
    END = 0


class Tapatan:
    """ Class to handle the game logic of Cozmo playing Tapatan against the player """
    def __init__(self):
        print("Class 'Tapatan' Init...")

        # Cozmo and cube objects
        self._robot = None
        self._cubes = None

        # Pose capture variables
        self._origin = None
        self.angle_z = None

        # Movement related parameters
        # 170 mm - DiamondTouch board
        # 110 mm - Smaller Grid on plywood
        self.step_distance = distance_mm(MOVE_DISTANCE)
        self.diagonal_scale = 1.47
        self.speed = speed_mmps(100)
        self.angle_list = [45, 135, -135, -45]

        # Dictionaries to map out nodes and movement between each of them
        self.node_map = {}
        self.path_map = {}

        # Game state related variables
        self.cur_cozmo_node = 1
        self.player_turn = False
        self.player_moving = False
        self.cur_game_state = GameState.SETUP

        # Helper variables
        self.markers_placed = 0
        self.moves = []
        self.selected_node = 0

        # Result
        self.winner = None

        # Initialize Cozmo
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)

    @property
    def robot(self):
        return self._robot

    @robot.setter
    def robot(self, value):
        self._robot = value

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

    # ################################ MOVEMENT METHODS ###########################################
    # Methods to move Cozmo in a particular direction
    # params:
    #   direction  -> Positive or negative
    #   multiplier -> Move by one or two nodes
    # #############################################################################################

    async def move_horizontal(self, direction, multiplier):
        """ Horizontal movement of Cozmo. Directions based on Cozmo facing the player.

         :param direction: Chosen from Direction enum. Positive value is right and negative value is left\
          (Cozmo's internal Y - axis)
         :param multiplier: Value of 1 is one node movement, 2 means traverse two nodes in given direction
        """

        # Motor movement
        await self.robot.turn_in_place(degrees(90 * direction)).wait_for_completed()
        driving = self.robot.drive_straight(self.step_distance * multiplier, self.speed, should_play_anim=True)
        while driving.is_running:
            await asyncio.sleep(0.5)
        await self.robot.turn_in_place(degrees(-90 * direction)).wait_for_completed()

    async def move_vertical(self, direction, multiplier):
        """ Vertical movement of Cozmo. Directions based on Cozmo facing the player.

         :param direction: Chosen from Direction enum. Positive value is down and negative value is up\
          (Cozmo's internal X axis)
         :param multiplier: Value of 1 is one node movement, 2 means traverse two nodes in given direction
        """

        # Motor movement
        # Simple forward and backward movement determined by direction since Cozmo always faces player
        driving = self.robot.drive_straight(self.step_distance * direction * multiplier, self.speed,
                                            should_play_anim=True)
        while driving.is_running:
            await asyncio.sleep(0.5)

    async def move_diagonal(self, direction, multiplier):
        """ Diagonal movement of Cozmo. Directions based on Cozmo facing the player.

         :param direction:  Chosen from Direction enum.\n
                            DIAG_DR - Diagonal Down Right\n
                            DIAG_UR - Diagonal Up Right\n
                            DIAG_UL - Diagonal Up Left\n
                            DIAG_DL - Diagonal Down Left
         :param multiplier: Value of 1 is one node movement, 2 means traverse two nodes in given direction
        """

        # Actual motor movement
        #  Direction enum for diagonals set to index of angle list
        await self.robot.turn_in_place(degrees(self.angle_list[direction])).wait_for_completed()
        driving = self.robot.drive_straight(self.step_distance * self.diagonal_scale * multiplier, self.speed,
                                            should_play_anim=True)
        while driving.is_running:
            await asyncio.sleep(0.5)
        await self.robot.turn_in_place(degrees(-self.angle_list[direction])).wait_for_completed()

    async def move_multiple(self, first_action, second_action):
        """ Diagonal movement of Cozmo. Directions based on Cozmo facing the player.

         :param first_action: First movement method to be called
         :param second_action: Second movement method to be called
        """
        await self.move_cozmo(first_action)
        await self.move_cozmo(second_action)

    # ##################################### DICTIONARIES ##################################################
    def build_nodes(self):
        """ Build the node map """
        self.node_map = {
            1: 0,   # Top left
            2: 1,   # Top center
            3: 2,   # Top right
            4: 10,  # Mid left
            5: 11,  # Mid center
            6: 12,  # Mid right
            7: 20,  # Bot left
            8: 21,  # Bot center
            9: 22   # Bot right
        }

    def build_paths(self):
        """ Build the path map """
        self.path_map = {
            # Basic up / down / left / right
            1: ["move_horizontal", Direction.RIGHT, 1],
            -1: ["move_horizontal", Direction.LEFT, 1],
            10: ["move_vertical", Direction.DOWN, 1],
            -10: ["move_vertical", Direction.UP, 1],

            # Double movement
            2: ["move_horizontal", Direction.RIGHT, 2],
            -2: ["move_horizontal", Direction.LEFT, 2],
            20: ["move_vertical", Direction.DOWN, 2],
            -20: ["move_vertical", Direction.UP, 2],

            # Diagonals
            11: ["move_diagonal", Direction.DIAG_DR, 1],
            -11: ["move_diagonal", Direction.DIAG_UL, 1],
            9: ["move_diagonal", Direction.DIAG_DL, 1],
            -9: ["move_diagonal", Direction.DIAG_UR, 1],

            # Double movement in diagonal
            22: ["move_diagonal", Direction.DIAG_DR, 2],
            -22: ["move_diagonal", Direction.DIAG_UL, 2],
            18: ["move_diagonal", Direction.DIAG_DL, 2],
            -18: ["move_diagonal", Direction.DIAG_UR, 2],

            # Remaining special cases
            12: ["move_multiple", 2, 10],
            -12: ["move_multiple", -10, -2],
            8: ["move_multiple", -2, 10],
            -8: ["move_multiple", -10, 2],
            21: ["move_multiple", 1, 20],
            -21: ["move_multiple", -20, -1],
            19: ["move_multiple", -1, 20],
            -19: ["move_multiple", -20, 1]
         }

    # ################################### MOVEMENT HELPERS #################################################
    async def move_cozmo(self, path):
        """ The method to call the movement methods based on the mapping in path_map"""
        method_name, arg1, arg2 = self.path_map[path]

        # Retrieve function name from arguments
        method = getattr(self, method_name)
        # Invoke function
        await method(arg1, arg2)

    async def simulate_tap(self):
        """ Simulate tap on the button """
        await self.robot.set_lift_height(0.35, max_speed=1.0, duration=0.05).wait_for_completed()
        await self.robot.set_lift_height(0, max_speed=1.0, duration=0.05).wait_for_completed()

    # ################################### CHECK FOR END CONDITION ###########################################
    def check_winner(self, game_grid, whom_to_check_for):
        """ Checks if the specified player has won or not

         :param game_grid: The grid with the state of the game
         :param whom_to_check_for: String value - Player or Cozmo
         :return: :bool: True if winner was found, False otherwise
         """
        if whom_to_check_for == "Player":
            if game_grid.check_win(PLAYER_GRID_VAL):
                self.winner = "Player"
                self.cur_game_state = GameState.END
                return True
        else:
            if game_grid.check_win(COZMO_GRID_VAL):
                self.winner = "Cozmo"
                self.cur_game_state = GameState.END
                return True

        return False

    # ################################### GAME LOGIC FUNCTIONS ###########################################
    async def greet_player(self):
        """ Greet the player"""
        self.robot.set_robot_volume(ROBOT_VOLUME)
        await self.robot.say_text("Ta pa tan!", duration_scalar=1.0, play_excited_animation=True).wait_for_completed()

        # Decide animation
        await self.robot.play_anim("ID_pokedB").wait_for_completed()
        # Randomly choose who goes first
        if random.randint(0, 1):
            print("Player starts")
            await self.robot.say_text("Okay, You start, the game!", duration_scalar=1.5, voice_pitch=0.2)\
                .wait_for_completed()
            self.player_turn = True

        else:
            print("Cozmo starts")
            await self.robot.say_text("Get ready! I, will start the game!", duration_scalar=1.5, voice_pitch=0.2)\
                .wait_for_completed()

    async def set_up_markers(self, game_grid, selection):
        """ Cozmo and player take turns placing cubes

        :param game_grid: The instance of GameGrid holding the state of the grid
        :param selection: The node entered by the Player (1-9). The value is -1 for Cozmo's turn
        """

        # Check if there are still markers left to be placed
        if self.markers_placed < 6:

            # Handle player's turn
            if self.player_turn:

                # Update state variables and grid
                game_grid.update_grid(selection, PLAYER_GRID_VAL)
                self.markers_placed += 1
                self.player_turn = False

                # Check if Player won already
                if self.check_winner(game_grid, "Player"):
                    return

            # Cozmo's turn
            else:
                lines_random = ["Let me think!", "Okay, let's see", "Aha!", "What, to do!"]
                lines_blocking = ["Haaaaa", "No way", "It won't be so easy"]

                choice = 0
                # Try to block the player if they are going for a win during setup
                if self.markers_placed > 2:
                    for target in range(1, 10):
                        # Start with a copy of the game and try to check which move will allow Cozmo to win
                        game_grid.create_grid_copy()
                        if game_grid.check_valid_input(target, game_grid.grid_copy):
                            game_grid.update_grid(target, COZMO_GRID_VAL, game_grid.grid_copy)
                            if game_grid.check_win(COZMO_GRID_VAL, game_grid.grid_copy):
                                choice = target
                                await self.robot.say_text("I know, what to do", duration_scalar=1.3, voice_pitch=0.3)\
                                    .wait_for_completed()
                                print("Muahahahaha! I win with: ", choice)
                                break

                    # Block if winning move does not exist
                    if not choice:
                        for target in range(1, 10):
                            # Start with a copy of the game and try to check which move will allow player to win
                            game_grid.create_grid_copy()
                            if game_grid.check_valid_input(target, game_grid.grid_copy):
                                game_grid.update_grid(target, PLAYER_GRID_VAL, game_grid.grid_copy)
                                if game_grid.check_win(PLAYER_GRID_VAL, game_grid.grid_copy):
                                    choice = target
                                    await self.robot.say_text(random.choice(lines_blocking), duration_scalar=1.3,
                                                              voice_pitch=0.3).wait_for_completed()
                                    print("Blocking: ", choice)
                                    break

                if not choice:
                    # Randomly choose a cell
                    choice = random.randint(1, 9)
                    # Repeat if already occupied
                    while not game_grid.check_valid_input(choice):
                        choice = random.randint(1, 9)
                    await self.robot.say_text(random.choice(lines_random), duration_scalar=1.3, voice_pitch=0.2)\
                        .wait_for_completed()

                # Make Cozmo move to destination and tap
                end = self.node_map[choice]
                start = self.node_map[self.cur_cozmo_node]
                path = end - start
                # Handle case when Cozmo choose to start at 1 and path = 0
                if path != 0:
                    await self.move_cozmo(path)
                await self.simulate_tap()

                # Update grid
                game_grid.update_grid(choice, COZMO_GRID_VAL)

                # Update state variables
                self.cur_cozmo_node = choice
                self.markers_placed += 1
                self.player_turn = True

                # Check if Cozmo won already
                if self.check_winner(game_grid, "Cozmo"):
                    return

                # Prompt player for their turn
                await self.robot.say_text("It's, your turn", duration_scalar=1.3, voice_pitch=0.2).wait_for_completed()

            # Check if that was the last marker
            if self.markers_placed == 6:
                self.cur_game_state = GameState.INGAME

            # Update indicator
            if self.player_turn:
                    game_grid.update_color()
                    game_grid.place_indicator(self.cur_game_state, self.player_moving)

    async def the_game(self, game_grid, selection):
        """ This is where the magic happens. The core loop once all the markers have been set up.
        Instead of making Cozmo perfect, all selections are randomized to give the player a chance to win.

        :param game_grid: The instance of GameGrid holding the state of the grid
        :param selection: The node entered by the Player (1-9). The value is -1 for Cozmo's turn
        """

        # Handle player's turn
        if self.player_turn:

            # Check if player has already selected marker or not
            if not self.player_moving:

                # Debug possible options
                self.moves = game_grid.get_possible_moves(selection)

                # Cozmo prompts if invalid choice
                if not self.moves:
                    print("No moves possible!")
                    await self.robot.say_text("That, is a bad move!", duration_scalar=1.3).wait_for_completed()
                    game_grid.update_grid(selection, PLAYER_GRID_VAL)
                    game_grid.draw_indicator()
                    return
                # Otherwise set flag to signal that player is now moving marker to a new location
                else:
                    # Allow cell to be de-selected
                    self.moves.append(selection)

                    # Keep track of selected node
                    self.selected_node = selection
                    self.player_moving = True
                    game_grid.place_indicator(self.cur_game_state, self.player_moving, self.moves)

            # Loop until one of them chosen
            else:
                # Check if move is legal
                if selection in self.moves:

                    # Deactivate if same node
                    if selection == self.selected_node:
                        # Revert to deselected state
                        game_grid.update_grid(self.selected_node, PLAYER_GRID_VAL)
                        self.player_moving = False
                        game_grid.place_indicator(self.cur_game_state, self.player_moving)
                        await asyncio.sleep(1)
                        return

                    # Change value of selected cell
                    game_grid.update_grid(selection, PLAYER_GRID_VAL)

                    # Reset old cell to 0 / WHITE
                    game_grid.update_grid(self.selected_node, GRID_EMPTY)

                    # End turn
                    self.player_moving = False
                    self.player_turn = False

                    # Check if Player won already
                    if self.check_winner(game_grid, "Player"):
                        return

        # Cozmo's turn
        else:
            lines_random = ["Let me think!", "Okay, let's see", "Aha!", "What, to do!"]
            lines_blocking = ["Haaaaa", "No way", "It won't be so easy"]

            # Reset neighbors
            self.moves.clear()

            # Flag to check if target found for win / block
            win_block_flag = False
            new_marker_position = 0

            # Randomly choose a cell which has a marker owned by Cozmo and has valid neighbors
            current_marker = game_grid.get_marker_positions(COZMO_GRID_VAL)

            for choice in current_marker:
                self.moves = game_grid.get_possible_moves(choice)
                # Skip if no possible moves
                if self.moves:
                    # Go for win
                    for target in self.moves:
                        # Start with a copy of the game and try to check which move will allow Cozmo to win
                        game_grid.create_grid_copy()
                        game_grid.update_grid(choice, GRID_EMPTY, game_grid.grid_copy)
                        game_grid.update_grid(target, COZMO_GRID_VAL, game_grid.grid_copy)

                        # Check if the move can lead to win
                        if game_grid.check_win(COZMO_GRID_VAL, game_grid.grid_copy):
                            new_marker_position = target
                            win_block_flag = True
                            await self.robot.say_text("I know, what to do", duration_scalar=1.3, voice_pitch=0.3)\
                                .wait_for_completed()
                            print("Muahahahaha! I win with: ", choice)
                            break

                    # Check if win target found
                    if win_block_flag:
                        break

                    # Go for block
                    for target in self.moves:
                        # Start with a copy of the game and try to check which move will allow player to win
                        game_grid.create_grid_copy()

                        # Check if the move can lead to a block
                        game_grid.update_grid(target, PLAYER_GRID_VAL, game_grid.grid_copy)
                        if game_grid.check_win(PLAYER_GRID_VAL, game_grid.grid_copy):
                            new_marker_position = target
                            win_block_flag = True
                            await self.robot.say_text(random.choice(lines_blocking), duration_scalar=1.3,
                                                      voice_pitch=0.3).wait_for_completed()
                            print("Blocking: ", new_marker_position)
                            break

                    # Check if block target found
                    if win_block_flag:
                        break

            # Skip if choice is already found
            if not win_block_flag:
                print("Didn't find win/block")
                choice = random.choice(current_marker)
                while True:
                    print("selected: ", choice)
                    # Get neighbors
                    self.moves = game_grid.get_possible_moves(choice)
                    # Choose another marker if no moves exist
                    if not self.moves:
                        current_marker.remove(choice)
                        choice = random.choice(current_marker)
                        continue
                    else:
                        await self.robot.say_text(random.choice(lines_random), duration_scalar=1.3, voice_pitch=0.2)\
                            .wait_for_completed()
                        break

            # Drive Cozmo to marker that will be moved
            end = self.node_map[choice]
            start = self.node_map[self.cur_cozmo_node]
            path = end - start

            # Make Cozmo move to destination and tap
            if path != 0:
                await self.move_cozmo(path)
            await self.simulate_tap()

            # Update position
            self.cur_cozmo_node = choice

            # Update grid to show Cozmo has selected a marker
            game_grid.update_grid(choice, COZMO_GRID_SELECTION)

            # Choose a random neighbor if not winning/blocking
            if not win_block_flag:
                new_marker_position = random.choice(self.moves)

            # Drive Cozmo to new marker position
            end = self.node_map[new_marker_position]
            start = self.node_map[self.cur_cozmo_node]
            path = end - start

            # Make Cozmo move to destination and tap
            await self.move_cozmo(path)
            await self.simulate_tap()

            # Update grid again
            game_grid.update_grid(new_marker_position, COZMO_GRID_VAL)
            game_grid.update_grid(self.cur_cozmo_node, GRID_EMPTY)

            # Update state variables
            self.cur_cozmo_node = new_marker_position
            self.player_turn = True

            # Check if Cozmo won already
            if self.check_winner(game_grid, "Cozmo"):
                return

            # Prompt player for their turn
            await self.robot.say_text("It's, your turn", duration_scalar=1.3, voice_pitch=0.2).wait_for_completed()
            game_grid.place_indicator(self.cur_game_state, self.player_moving)

    async def game_end(self, game_grid):
        """ Final actions if the game has been completed """
        if self.cur_game_state == GameState.END:
            print("The winner is: ", self.winner)
            if self.winner == "Cozmo":
                game_grid.display_winner("Cozmo")
                await self.robot.play_anim("anim_speedtap_wingame_intensity02_01").wait_for_completed()
            else:
                game_grid.display_winner("You")
                await self.robot.play_anim("anim_memorymatch_failgame_cozmo_01").wait_for_completed()
            return True
        else:
            return False

    # ################################### REFRESH PYGAME SCREEN ###########################################
    async def pygame_refresh(self):
        """ Helper function to redraw the PyGame window at 60 fps """
        while True:
            pygame.display.flip()
            await asyncio.sleep(1/60)
            if self.cur_game_state == GameState.END:
                pygame.display.flip()
                await asyncio.sleep(1)
                break

    # ################################### MAIN GAME LOOP ###########################################
    async def game_loop(self):
        """ The main game loop """

        # Set up the nodes and the paths between them
        self.build_nodes()
        self.build_paths()

        # Instantiate an object of GridControl to keep track of the game state visually
        game_grid = GridControl()

        # Start with an empty grid
        game_grid.build_grid()

        # Run the functions to refresh the game window and check mouse concurrently
        asyncio.ensure_future(self.pygame_refresh())

        # Handle initial placement of the cubes by the player and Cozmo
        await self.greet_player()

        # Loop for PyGame to run and handle events
        while True:
            # Check for new events
            for event in pygame.event.get():
                # Exit handler
                if event.type == QUIT or (event.type == KEYDOWN and event.key == K_ESCAPE):
                    pygame.quit()
                    sys.exit()

                # Setup phase handler
                if self.cur_game_state == GameState.SETUP:
                    # Check if Cozmo's turn
                    if not self.player_turn:
                        await self.set_up_markers(game_grid, -1)
                        # Safety
                        pygame.event.clear()
                        continue

                    # Otherwise draw player selection indicator
                    else:
                        game_grid.draw_indicator()

                    # If player turn, wait for input
                    if event.type == KEYDOWN and event.key != K_RETURN:
                        # Handle input
                        game_grid.input_handler(event.key, self.cur_game_state)

                    # Confirm the cell once mouse has been clicked
                    elif event.type == MOUSEBUTTONDOWN or (event.type == KEYDOWN and event.key == K_RETURN):
                        selection = game_grid.mouse_click()
                        # Transfer control to game logic handler along with selection
                        await self.set_up_markers(game_grid, selection)

                    # Cleanup event to ensure that actions do not get queued up during Cozmo's turn
                    pygame.event.clear()

                # Core game loop handler
                if self.cur_game_state == GameState.INGAME:
                    # Check if Cozmo's turn
                    if not self.player_turn:
                        await self.the_game(game_grid, -1)
                        # Safety
                        pygame.event.clear()
                        continue

                    # Otherwise draw player selection indicator
                    else:
                        game_grid.draw_indicator()

                    # If player turn, wait for input
                    if event.type == KEYDOWN and event.key != K_RETURN:
                        # Handle input to change grid color, checking whether a marker was already selected
                        if self.player_moving:
                            game_grid.input_handler(event.key, self.cur_game_state, self.player_moving, self.moves)
                        else:
                            game_grid.input_handler(event.key, self.cur_game_state)

                    # Confirm the cell once mouse has been clicked
                    elif event.type == MOUSEBUTTONDOWN or (event.type == KEYDOWN and event.key == K_RETURN):
                        selection = game_grid.mouse_click()
                        # Transfer control to game logic handler along with selection
                        await self.the_game(game_grid, selection)

                    # Cleanup event to ensure that actions do not get queued up during Cozmo's turn
                    pygame.event.clear()

            # Sync with screen refresh
            await asyncio.sleep(1 / 30)

            # Check if we have a winner
            if await self.game_end(game_grid):
                break

    async def run(self, sdk_conn):
        """ Main function to be run """
        asyncio.set_event_loop(sdk_conn._loop)
        self.robot = await sdk_conn.wait_for_robot()
        print("Connection established!")

        # Capture Origin Pose
        self.origin = self.robot.pose
        self.angle_z = self.origin.rotation.angle_z

        # Reset arm / head / expression
        await self.robot.set_lift_height(0, duration=0.2).wait_for_completed()
        await self.robot.set_head_angle(degrees(0), duration=0.2).wait_for_completed()
        await self.robot.play_anim("anim_explorer_idle_01").wait_for_completed()

        # Execute the game loop
        await self.game_loop()

        # Ending cleanup
        print("Ending program...")

# Set entry point
if __name__ == '__main__':
    Tapatan()
