# Treasure Hunt [Cozplay #18]
## Project Description

_Treasure Hunt_ is an example of integrating the Cozmo SDK with Unreal Engine 4. Unlike our Unity-based games, this game makes calls to the Cozmo SDK without any networking code. Instead, it uses the UnrealEnginePython plugin to integrate the SDK more directly into the game engine. The game itself is a simple experience in which Cozmo drives around a beach digging up treasure. The player must alert Cozmo of treasure locations while also keeping Cozmo’s battery charged.

## Video
https://youtu.be/iwuc4bThi14

## Implementation Details

In order to properly run the UE4 project, you’ll need to download UnrealEnginePython, make a few modifications, and then add it to the project:

1. Download the source for the plugin here: https://github.com/20tab/UnrealEnginePython
2. If you’re using macOS with Homebrew (rather than Python.org installation):
 3. Open ./Source/UnrealEnginePython/UnrealEnginePython.Build.cs
 4. Change line 92 to point to your Homebrew Python installation. So 
 `string mac_python = "/Library/Frameworks/Python.framework/Versions/3.5/";`
 Becomes something similar to:
 `string mac_python = "/usr/local/Cellar/python3/3.5.2_3/Frameworks/Python.framework/Versions/3.5/";`
8. To enable threading:
 9. Open ./Source/UnrealEnginePython/Private/UnrealEnginePythonPrivatePCH.h
 10. Uncomment line 4. So:
 `//#define UEPY_THREADING 1`
 Becomes:
 `#define UEPY_THREADING 1`
14. Add the plugin to the UE4 project by placing the entire downloaded directory in the ‘Plugins’ directory of the project (create this directory if it doesn’t already exist).
15. You’ll be prompted to build the plugin the next time you open the UE4 project.


Broadly speaking, the game is structured so that all actors communicate with the Cozmo SDK through the `CozmoUE` class. `ACozmoUE::RunCozmoCoroutine` invokes coroutines in the `cozmo_bridge` Python script, with optional callback functionality to C++ functions (made possible through Unreal’s reflection system). The `cozmo_bridge` Python script in turns calls the Cozmo SDK. Note that Python scripts are placed in ./Content/Scripts.


The `PoseTracker` class (and `RobotTracker` subclass) are used to track Cozmo’s pose and map it to an Unreal actor. Cozmo’s and Unreal’s coordinate systems have a one-to-one relationship (that is, a centimeter is a centimeter in both systems). However, Unreal’s coordinate system is offset by Cozmo’s initial pose (poses sent from Python to C++ have the inverse of Cozmo’s initial pose applied). This allows us to assume that Cozmo will begin unrotated, at the origin in Unreal.

## Instructions

The game is best played with a controller (we’ve tested it with Dual Shock 4) but is also playable using the keyboard.


It begins with a calibration screen that indicates both where to place Cozmo and how large the projected play area should be scaled. Once everything is set up, press ‘Options’ on DS4 or ‘R’ on keyboard to begin.
<img src="/uploads/anki/original/1X/2a2f71d740cff911942deb516adbb2f5af2efb07.png" width="394" height="296">

The game is played as follows:

1. Your goal is to help Cozmo dig up treasure while also keeping his battery charged.
2. To alert Cozmo of treasure, hover a treasure spot with the cursor and select it with X / Space.
3. To charge Cozmo’s battery, hover him with the cursor and charge him with X / Space.
4. Both Cozmo’s battery level and the number of treasures you’ve collected appear at the bottom of the play area.
5. The speed at which Cozmo’s battery depletes increases over time. For a more leisurely play style, the battery mechanic can be removed entirely by pressing L1 / ‘B’.

**Claiming Treasure:**
Hover a treasure spot (Left Stick / WASD):
<img src="/uploads/anki/original/1X/5997cdb1589151a3b002632005b43602946b2749.png" width="350" height="250">

Select it (X / Space):
<img src="/uploads/anki/original/1X/a18ed129f8e2924e03bbacf286cceb6490a23d4d.png" width="350" height="250">

Cozmo drives to it:
<img src="/uploads/anki/original/1X/774c3545210705446f53bf59f9738c9a00f809a7.png" width="350" height="250">

Cozmo gets the treasure!
<img src="/uploads/anki/original/1X/eb4f24cd54b87f471167bb2c36c01fabe0b409a4.png" width="350" height="250">

**Recharging Cozmo:**
Hover Cozmo (Left Stick / WASD):
<img src="/uploads/anki/original/1X/42466d160a7131d55b274c674d069864084ba81d.png" width="370" height="280">

Select Cozmo to recharge (X / Space):
<img src="/uploads/anki/original/1X/9996feab3012590b377979b09efa40dfb2062f5c.png" width="370" height="280">


**Controls:**

    Left Stick / WASD	Move the cursor
    X / Space 		Charge Cozmo / Alert Cozmo
    R1 / Right Mouse 	Toggle debug visuals
    Options / ‘R’		Return to calibration
    L1 / ‘B’		Toggle battery depletion mechanic

## Thoughts for the Future

This is a simple game with placeholder art, but it shows how one might integrate the Cozmo SDK directly into a game engine--which could lead to some powerful AR experiences. Mapping Cozmo to an in-game avatar makes for lots of interesting possibilities (e.g. Cozmo colliding with virtual objects, shooting virtual lasers, and so on). And cubes could be tracked similarly to Cozmo, opening up even more avenues of exploration.