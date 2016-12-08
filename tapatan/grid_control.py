"""
grid_control.py:
    - Support class for Tapatan game
    - Draws the grid using Pygame and updates the cells based on the current state of the game

@author - Team Cozplay
"""

# Standard python modules
import pygame
from pygame.locals import *
import math

# Import Tapatan GameState
from tapatan import GameState

# PyGame window dimensions
SCREEN_SIZE = [1000, 1000]
# Grid values
GRID_EMPTY = 0
COZMO_GRID_VAL = 1
COZMO_GRID_SELECTION = 2
PLAYER_GRID_VAL = 3
PLAYER_GRID_SELECTION = 4
PLAYER_INDICATOR = 5


class GridControl:
    """ Class for handling of a 3 X 3 grid created using PyGame as the visual to accompany the game """
    # Static vars for colors
    # Define the colors we will use in RGB format
    WHITE = (255, 255, 255)
    BLUE = (0, 0, 255)
    GREEN = (0, 255, 0)
    RED = (255, 0, 0)
    YELLOW = (255, 255, 0)
    CYAN = (0, 255, 255)
    GRAY = (128, 128, 128)

    # Grid tile color based on grid value
    color_map = {
        GRID_EMPTY: WHITE,
        COZMO_GRID_VAL: RED,
        PLAYER_GRID_VAL: BLUE,
        PLAYER_GRID_SELECTION: GREEN,
        COZMO_GRID_SELECTION: YELLOW,
        PLAYER_INDICATOR: CYAN
    }

    def __init__(self):
        # Init
        print("Class 'GridControl' Init...")
        pygame.init()
        pygame.display.set_caption("Tapatan Grid")

        # Spacing parameters for tiles on the grid
        self.width = SCREEN_SIZE[0] / 4
        self.height = SCREEN_SIZE[1] / 4
        self.margin = min(SCREEN_SIZE[0], SCREEN_SIZE[1]) / 12
        self.offset = min(SCREEN_SIZE[0], SCREEN_SIZE[1]) / 30
        self.top = 0
        self.left = 0
        self.indicator_width = int(self.width / 10)

        # Set the height and width of the screen
        self.screen = pygame.display.set_mode(SCREEN_SIZE)

        # Grid to store state of the game
        self.grid = []
        self.grid_copy = []
        self.cursor_node = [0, 0]

        # SFX
        self.hover = pygame.mixer.Sound("./sfx/hover.wav")
        self.click_sfx = pygame.mixer.Sound("./sfx/ting.wav")

    def update_color(self):
        """ Update the colors of the tiles in the PyGame window based on value stored in the grid """
        for row in range(3):
            for column in range(3):
                self.top = row * (self.margin + self.height)
                self.left = column * (self.margin + self.width)
                pygame.draw.rect(self.screen, GridControl.color_map[self.grid[row][column]],
                                 [self.offset + self.left, self.offset + self.top, self.width, self.height])

        # Play audio feedback
        self.hover.play()

    def build_grid(self):
        """ Create an empty 3 x 3 to store game state. Initialized with a value of 0 """
        for row in range(3):
            # Add an empty array that will hold each cell in this row
            self.grid.append([])
            for column in range(3):
                # Initialize all cells with 0
                self.grid[row].append(0)

        # Draw the lines to show grid connections (Draw once)
        margin = self.margin * 4
        offset = self.offset * 5
        width = 15
        for row in range(3):
            y = row * margin
            # Draw horizontal lines
            pygame.draw.line(self.screen, GridControl.GRAY, (offset, y + offset),
                             (SCREEN_SIZE[0] - offset, y + offset), width)
            # Draw diagonal 1
            if row == 0:
                pygame.draw.line(self.screen, GridControl.GRAY, (offset, y + offset),
                                 (SCREEN_SIZE[0] - offset, SCREEN_SIZE[1] - y - offset), width)
            # Draw diagonal 2 - Had to make separate case because of misalignment :(
            elif row == 2:
                pygame.draw.line(self.screen, GridControl.GRAY, (offset - width, y + offset),
                                 (SCREEN_SIZE[0] - offset - width, SCREEN_SIZE[1] - y - offset), width)

            # Draw diamond
            else:
                pygame.draw.line(self.screen, GridControl.GRAY, (offset - width, y + offset), (x - offset - width,
                                                                                               y - offset), width)
                pygame.draw.line(self.screen, GridControl.GRAY, (x - offset, y - offset), (x + offset, y + offset),
                                 width)
                pygame.draw.line(self.screen, GridControl.GRAY, (offset, y + offset), (x - offset, 2 * y + offset),
                                 width)
                pygame.draw.line(self.screen, GridControl.GRAY, (x - offset - width, 2 * y + offset),
                                 (x + offset - width, 2 * y - offset), width)

            # Draw Vertical lines
            for column in range(3):
                x = column * margin
                pygame.draw.line(self.screen, GridControl.GRAY, (x + offset, offset),
                                 (x + offset, SCREEN_SIZE[1] - offset), width)

        self.update_color()

    def update_grid(self, selection, val, grid=None):
        """ Simple helper function to update the grid value of a particular cell and then update its color """
        # Check if copied grid is used
        if grid is None:
            grid = self.grid
            update_color = True
        else:
            update_color = False
        # Convert node 1 - 9 to corresponding row / col cell in the grid
        row = math.floor((selection - 1) / 3)
        col = (selection - 1) % 3

        # Update value and color
        grid[row][col] = val
        if update_color:
            self.update_color()

    def check_valid_input(self, selection, grid=None):
        """ Simple helper function to check if a grid cell is empty to be assigned a marker """
        # Check if copied grid is used
        if grid is None:
            grid = self.grid

        # Convert node 1 - 9 to corresponding row / col cell in the grid
        row = math.floor((selection - 1) / 3)
        col = (selection - 1) % 3
        return grid[row][col] == GRID_EMPTY

    def get_cell_value(self, selection):
        """ Helper to return the value of the cell (owner) in the grid at the selection value passed to the function """
        # Convert node 1 - 9 to corresponding row / col cell in the grid
        row = math.floor((selection - 1) / 3)
        col = (selection - 1) % 3
        return self.grid[row][col]

    def create_grid_copy(self):
        """ Make a copy of the board list and return the duplicate """
        self.grid_copy.clear()
        for row in range(3):
            self.grid_copy.append([])
            for column in range(3):
                self.grid_copy[row].append(self.grid[row][column])

    def check_win(self, grid_val, grid=None):
        """ Function to check if a certain player (grid_value) is victorious """
        # Check if copied grid is used
        if grid is None:
            grid = self.grid
        return ((grid[0].count(grid_val) == 3) or
                (grid[1].count(grid_val) == 3) or
                (grid[2].count(grid_val) == 3) or
                (grid[0][0] == grid_val and grid[1][0] == grid_val and grid[2][0] == grid_val) or
                (grid[0][1] == grid_val and grid[1][1] == grid_val and grid[2][1] == grid_val) or
                (grid[0][2] == grid_val and grid[1][2] == grid_val and grid[2][2] == grid_val) or
                (grid[0][0] == grid_val and grid[1][1] == grid_val and grid[2][2] == grid_val) or
                (grid[0][2] == grid_val and grid[1][1] == grid_val and grid[2][0] == grid_val))

    def get_marker_positions(self, grid_val):
        """ Function to return list of positions currently occupied by marker with given grid value """
        # Prepare list to be returned
        marker_positions = []
        for i in range(1, 10):
            if self.get_cell_value(i) == grid_val:
                marker_positions.append(i)

        # Return list
        return marker_positions

    def get_possible_moves(self, selection):
        """ Get list of neighbors for a given point in the grid """
        # Neighbor list to be returned
        moves = []
        # Fixed values for grid
        min_row = 0
        min_col = 0
        max_row = 2
        max_col = 2

        # Convert node 1 - 9 to corresponding row / col cell in the grid
        row = math.floor((selection - 1) / 3)
        col = (selection - 1) % 3

        # Find neighbors keeping bounds in mid
        row_start = row if row - 1 < min_row else row - 1
        col_start = col if col - 1 < min_col else col - 1
        row_end = row if row + 1 > max_row else row + 1
        col_end = col if col + 1 > max_col else col + 1

        # Check if cell is empty
        for i in range(row_start, row_end + 1):
            for j in range(col_start, col_end + 1):
                if not self.grid[i][j]:
                    moves.append(3 * i + j + 1)

        # Return list
        return moves

    def input_handler(self, input_key, game_state, player_moving=False, moves=None):
        """ Function to handle movement of the mouse on the grid and update color of the highlighted cell

         :param input_key: The key pressed during input
         :param game_state: The current state of the game
         :param player_moving: Flag to check if a marker has already been selected. Only needed during INGAME stage
         :param moves: List of possible cells if player has already selected and moving a marker
         """

        row = int(self.cursor_node[0])
        col = int(self.cursor_node[1])
        count = 0

        while count != 4:
            count += 1
            if input_key == K_LEFT:
                col = (col - 1) % 3
            elif input_key == K_RIGHT:
                col = (col + 1) % 3
            elif input_key == K_UP:
                row = (row - 1) % 3
            elif input_key == K_DOWN:
                row = (row + 1) % 3
            else:
                return

            # Check if no free cells in this row or column
            if count == 3:
                # Switch Row
                if input_key == K_LEFT:
                    row = (row - 1) % 3
                elif input_key == K_RIGHT:
                    row = (row + 1) % 3
                # Switch Column
                elif input_key == K_UP:
                    col = (col - 1) % 3
                elif input_key == K_DOWN:
                    col = (col + 1) % 3
                count = 0

            if game_state == GameState.SETUP and self.grid[row][col] == GRID_EMPTY:
                break
            elif player_moving and 3 * row + col + 1 in moves:
                break
            elif game_state == GameState.INGAME and not player_moving and self.grid[row][col] == PLAYER_GRID_VAL:
                break

        # Update cursor position
        self.cursor_node = [row, col]

        # Update the grid
        self.update_color()

    def draw_indicator(self):
        """ Draw the indicator for the player selection """
        if self.cursor_node:
            top = self.cursor_node[0] * (self.margin + self.height)
            left = self.cursor_node[1] * (self.margin + self.width)
            margin = self.indicator_width / 2
            pygame.draw.rect(self.screen, GridControl.color_map[PLAYER_INDICATOR],
                             [self.offset + left + margin , self.offset + top + margin,
                              self.width - self.indicator_width, self.height - self.indicator_width],
                             self.indicator_width)

    def place_indicator(self, game_state, player_moving, moves=None):
        """ Function to place indicator in an empty cell """
        for row in range(3):
            for column in range(3):
                # Find empty cell
                if ((game_state == GameState.SETUP and self.grid[row][column] == GRID_EMPTY) or
                        (player_moving and 3 * row + column + 1 in moves and self.grid[row][column] == GRID_EMPTY)
                        or (game_state == GameState.INGAME and not player_moving and
                                    self.grid[row][column] == PLAYER_GRID_VAL)):
                    self.cursor_node = [row, column]
                    self.update_color()
                    self.draw_indicator()
                    return
                else:
                    continue

    def mouse_click(self):
        """ Function to handle confirmation of selected cell and return cell value (1-9) """

        selection = 3 * self.cursor_node[0] + self.cursor_node[1] + 1

        if self.grid[self.cursor_node[0]][self.cursor_node[1]] != PLAYER_GRID_VAL:
            self.grid[self.cursor_node[0]][self.cursor_node[1]] = PLAYER_GRID_VAL
            # Hide the selection cursor
            self.cursor_node = None
        else:
            self.grid[self.cursor_node[0]][self.cursor_node[1]] = PLAYER_GRID_SELECTION

        # Play audio feedback
        self.click_sfx.play()

        return selection

    def display_winner(self, winner):
        """ Display text at end state """
        font = pygame.font.SysFont("Avenir", int(SCREEN_SIZE[0] / 15))              # Set your font here
        label = font.render(winner + " won the game!", 1, (255, 255, 255))
        self.screen.fill((0, 0, 0))
        text_rect = label.get_rect(center=(SCREEN_SIZE[0] / 2, SCREEN_SIZE[1] / 2))
        self.screen.blit(label, text_rect)
        pygame.display.flip()
