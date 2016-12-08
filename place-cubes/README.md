## Project Description

_Place cubes_ is a simple prototype that involves Cozmo building a mini fort using cubes chosen by the guest. The idea behind the prototype was to combine Cozmo’s strongest traits, his emotions and his interaction with cubes, into a small fun activity.

## YouTube Video
https://youtu.be/z4BKYJY4hbY

## Implementation Details

1. Cozmo needs to find three cubes before he can interact with them.
2. Cozmo’s position is stored at the very beginning and he places the cubes around this point.
3. Small movements were added to make sure there is enough space for Cozmo to turn around and play some of his excited animations.

## Instructions

1. Ensure that all three cubes are within Cozmo’s field of vision to begin the experience. 
2. The experience works best when all three cubes are placed in front of Cozmo. However, the lookaround in place behavior will activate at the start to locate the cubes.

## Thoughts for the Future

1. Initially, this idea had a second stage where Cozmo would be very possessive about his cubes and would stare down any face trying to get close to his cubes.
2. Cozmo would keep turning around, tapping each cube and the objective was to steal away a cube when he wasn’t looking. Think of it as a reverse ‘Keepaway’.
3. However, the turn in place, while accurate, was too slow to implement this idea. Maybe if Cozmo is kept perpetually in motion along a circular path, he might be able to react quicker to the guest’s interference.
4. Using `play_anim_trigger` would be a more robust solution than playing individual animation files which get modifed during SDK updates.