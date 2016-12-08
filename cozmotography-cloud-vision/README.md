# Cozmotography - Cloud Vision [Cozplay #13]

## Project Description

_Cozmotography_ consists of two camera-focused tech demos.


In _Cloud Vision_, the player taps a cube and Cozmo says what he thinks he’s seeing. This experience is powered by the Google Cloud Vision API.

## Photos/Videos
https://youtu.be/cRy5ZT6g98k

## Implementation Details

_Cloud Vision_ requires installation of the Google API Client Library for Python. To install run the following command:
`pip3 install --upgrade google-api-python-client`


It also requires a [key](https://cloud.google.com/vision/docs/common/auth) file for the Cloud Vision service. Once you’ve downloaded the file, name it ‘cozmo-cloud-vision’ and place it in ./keys (relative to the CloudVisionTest.py script). Then set the environment variable GOOGLE_APPLICATION_CREDENTIALS to ./keys/cozmo-vision-key. 


For licensing reasons, our custom display font has been removed from this release, so the text on Cozmo’s screen will appear quite small. To restore it, you’ll want to look at lines 21 and 59. We recommend 24pt Avenir.

## Instructions

_Cloud Vision_ begins once Cozmo sees a cube. When the player taps the cube, Cozmo will analyze what he’s seeing and report back to the player. This process repeats until it’s manually terminated.

## Thoughts for the Future

_Cloud Vision_ could be improved through additional filtering of results using confidence scores. Or it could be “gamified” by requiring the player to show Cozmo what he requests.