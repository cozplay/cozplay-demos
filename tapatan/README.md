## Project description

Cozmo plays a virtual version of Tapatan with the guest. The rules of the game are as follows: https://en.wikipedia.org/wiki/Tapatan

The board used is actually based on a similar game called Picaria as shown below:
![Game Grid](/tapatan/readme_img/grid.png?raw=true "Game Grid")

## YouTube Video

https://youtu.be/IHKuvYrivQ4

## Implementation Details

1. For the 3x3 grid, we initially built a physical board that used Makey Makey connections to capture a physical button press. This didn’t turn out to be a very robust solution, so we decided to use PyGame instead to show the visuals which are just comprised of several rectangles.

2. We use a top-down projection and the goal behind this and the Makey Makey board was to have a common play-area so that your attention is not divided too much between Cozmo and some external screen.

3. For input, the grid selector uses keyboard input of `LEFT/DOWN/UP/RIGHT` and `ENTER` keys. We experimented with multiple mechanisms like mouse cursors, Myo Armband, Wiimote etc before we decided to go with the trusty gamepad. Most controllers should work for these basic inputs but we decided to go with a PlayStation DualShock 4 controller.

4. There are various mapping tools to set up the key mappings for the controller. For the DS4 these were the ones we used:
  > **OSX** - Joystick Mapper
  
  > **Windows** - DS4Windows
  
  > **Linux** - ds4drv

5. For Cozmo’s movement, we tried using `go_to_pose()` method but it was extremely inconsistent, even within the same session. Since we couldn’t rely on it for the movement, we decided to manually code in the movement as a three step process:
  >a. Turn towards appropriate direction.
  
  >b. Move using `drive_straight()` method.
  
  >c. Turn back to face the guest.

6. We built a dictionary which can move Cozmo between any two points on the grid as a combination of three custom methods: `move_horizontal`, `move_vertical` and `move_diagonal`.

7. Cozmo’s AI works as follows:
  > a. He looks for a move that will let him win.
  
  > b. Otherwise he looks for a move that will block you from winning.
  
  > c. If these two conditions are not satisfied, he plays a randomly selected marker to a random selection.

## Instructions

Pygame is required for this game. You can download it from here: http://www.pygame.org/download.shtml

**In tapatan.py:**

1. Robot volume can be adjusted via the global variable `ROBOT_VOLUME` at the start of the script.
2. The physical distance between two cells in the grid can be set via `MOVE_DISTANCE` (in mm).

**In grid_control.py**

1. Set the dimensions of the output window in `SCREEN_SIZE`. Works best as a square window.
2. Color of the cells can be adjusted via the `color_map` variable in class `GridControl`.
3. Font for the final screen can be set in **line 331**

**Gameplay:**

1. Place Cozmo in the top left cell (first column of the first row).
2. Cozmo randomly chooses who goes first.
3. In the first half of the game, you get a selector that can be moved to any empty cell using `LEFT/RIGHT/UP/DOWN` arrow keys and pressing `ENTER` or `left mouse click` to place your marker.
4. Once you have placed down 3 markers, the selector now lets you choose any one of the existing markers and move it to a valid cell - adjacent and empty.
5. You can also press `ENTER/left mouse click` on a selected marker again to deselect it.
6. The game completes when you or Cozmo get three in a row.

## Thoughts for the Future

1. There was still drift in Cozmo’s movement as a result of our surface that would introduce an offset after 5 - 7 turns. We needed the screen for the projection but the movement code works fairly well on a solid flat surface.

2. The turn in place and even straight movement is still slow enough to affect the pace of the experience. We chose this game over just Tic Tac Toe as there is a bit more strategy involved and the slow pace allows guests and Cozmo to think.

3. We did not pursue the physical board for too long but we still believe it can be a great medium to play with Cozmo to move beyond the traditional cube input. Switching to a virtual display helped make the world consistent and reliable but we lost the tactility which is associated with most interactions with Cozmo. Having a functioning physical board or tactile buttons/markers to play with would make this more engaging. However, make sure that your setup is robust, especially if kids are involved.

4. Having a different screen for visuals and mouse based inputs were cumbersome, especially for non-gamers. Switching to a projector and simple selector model helped to make the experience more seamless and intuitive.

5. The experience is more enjoyable for people who like such logic games. For a general audience, it might be better to just make Cozmo host a game or be the modifier rather than have him participate which slows down the game.

6. Work can be done to improve the asynchronous model of Python and efficiently relieve control to introduce ‘idle’ behavior and ‘trash talk’ for Cozmo. If simultaneous actions are released in the future, this aspect should definitely be revisited.
