# Razzle Dazzle Cozmo Puzzle [Cozplay #06]

## Project Description

_Razzle Dazzle Cozmo Puzzle_ is a simple demo based on a cube stacking mechanic. The player must stack the appropriate primary-colored cubes to create the colors that Cozmo requests.


These are Cozmo’s possible requests:
Green - stack blue and yellow
Purple - stack red and blue
Orange - stack red and yellow
Black - stack the three cubes in a pyramid shape (easier for Cozmo to see than a three-cube tower)
Primary - unstack all cubes


There’s also time limit for creating Cozmo’s request, and it lowers after each successful round--until it reaches three seconds. We’ve determined this to be the lowest reasonable time limit given Cozmo’s detection speed. The game ends when the player fails to create the goal color before the clock runs out.

## Photos/Video

https://youtu.be/N7jFAqhSQeQ

## Implementation Details

This game is primarily based around Cozmo’s tracking of cube poses. This information is used to detect which cubes are stacked on each other. Each cube is also associated with a unique primary color over the course of the game. This is accomplished by subclassing cozmo.objects.LightCube and setting cozmo.world.World.light_cube_factory.

## Instructions

There are a couple dependencies on other Python packages: 
pygame is used to play a couple sound effects out of the computer speaker.
inflect is used for Cozmo to speak a numerical score.

The game starts once Cozmo sees three cubes in front of him. All three cubes must remain visible to him at all times.

For licensing reasons, our custom display font has been removed from this release, so the text on Cozmo’s screen will appear quite small. To restore it, you’ll want to look at lines 17, 166, and 167. We recommend 18pt Avenir.

## Thoughts for the Future

At the moment this game is quite easy for all but the youngest players and doesn’t increase in complexity over time. Perhaps it could be expanded with additional variation. Additionally Cozmo remains stationary throughout. There’s definitely some room for additional animation between rounds.