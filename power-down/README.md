## Project description

_Power Down_ is a simple game where Guests assist Cozmo by tapping cubes to prevent him from powering down. Cozmo requests a color which is also displayed on his backpack and the guest must tap a cube to cycle colors until it matches the requested one.

## YouTube Video

https://youtu.be/iKsGpiILW8A

## Implementation Details

1. Movement of head and arm is performed via `move_lift()` and `move_head()` methods as this allows Cozmo to speak simultaneously.
2. The speech is slowed down using the `duration_scalar` parameter of the `say_text()` method.
3. Round manager handles the number of iterations. Colors can be added via the protected member `self._colors` of class `PowerDown`.
4. Round colors are handled via the variable `round_colors` in the game loop.

## Instructions

1. Cozmo is powering down continuously and the only way to recharge him is to power up the cubes.
2. The cubes switch color constantly. The player must tap on the cube to cycle colors and stop when the color matches the one displayed on Cozmo's backpack.
3. On changing all three cubes to the requested color, Cozmo will spring back to life.

## Thoughts for the Future

1. The animation for arm and head lowering was not very smooth as it would update in discrete value jumps. This could not be updated per frame as it interferes with speech / other animations.
2. The backpack light wasn’t large enough to display the requested color. We had to add speech but it can be difficult to understand especially for new guests. Having LED lights or a screen behind Cozmo might convey this information better.
3. There are only few combinations of RGB values that are distinctive: Red, Green, Yellow.
4. Default blue is pretty dark and is better displayed as Cyan (#00FFFF). Magenta works but is tough for Cozmo to pronounce. White has a bluish tinge to it. Would be nice to experiment with more colors.
5. Lights are more visible when the room lighting is low but this hinders Cozmo’s vision so you cannot make him interact with the world reliably. Avoid adding such functionality if you decide to dim the lights.