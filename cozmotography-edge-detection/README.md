# Cozmotography - Edge Detection [Cozplay #13]

## Project Description

_Cozmotography_ consists of two camera-focused tech demos.

In _Edge Detection_, Cozmo looks around and  we process what he’s seeing using OpenCV 3. Specifically, we run Canny Edge Detection on the camera feed, and both the pre- and post-processed images are displayed in a tkinter window.

## Video

https://youtu.be/AwgFobwJkh8

## Implementation Details

_Edge Detection_ requires installation of OpenCV 3. On Mac, install using Homebrew:
`brew install opencv3 --HEAD --with-contrib --with-python3`


Then find the path to the site-packages directory of your OpenCV installation. You’ll need to navigate to /usr/local/Cellar/opencv3/. Then open the folder beginning with “Head” and then open ./lib/python3.5. You should then option-click the site-packages folder and choose “Copy site-packages as pathname”.


You’ll then edit ~/.bash_profile by adding the following lines:
    PYTHONPATH="[pasted path to site-packages]:$PYTHONPATH"
    export PYTHONPATH


For installation on Windows, these resource might help (untested):
http://docs.opencv.org/trunk/d5/de5/tutorial_py_setup_in_windows.html
https://www.scivision.co/install-opencv-3-0-x-for-python-on-windows?

## Instructions

_Edge Detection_ starts immediately. You should see a window containing a split view of Cozmo’s pre- and post-processed camera feeds.

## Thoughts for the Future

_Edge Detection_ could serve as the basis for any application that processes Cozmo’s camera feed using OpenCV. There are lots of possibilities here for creating your own markers beyond those built into the SDK or possibly even turning Cozmo’s camera feed into an augmented reality view.