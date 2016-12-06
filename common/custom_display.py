import cozmo
import sys
try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    sys.exit("Cannot import from PIL: Do `pip3 install Pillow` to install")


'''
Custom display class responsible for displaying custom text
@class CustomDisplay
@author Ankit Patel
'''


class CustomDisplay:

    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)

    @staticmethod
    def display_text(robot, text, duration, size, xoffset=15, yoffset=2):
        '''
        displays text on cozmo's screen
        '''

        font = None
        try:
            font = ImageFont.truetype("arial.ttf", size)
        except IOError:
            try:
                font = ImageFont.truetype("/Library/Fonts/Arial.ttf", size)
            except IOError:
                pass

        text_image = CustomDisplay.make_text_image(text, xoffset, yoffset, font)
        lcd_face_data = cozmo.oled_face.convert_image_to_screen_data(text_image)
        robot.display_oled_face_image(lcd_face_data, duration)

    @staticmethod
    def make_text_image(text_to_draw, x, y, font=None):
        '''Make a Pillow.Image with the current time printed on it

        Args:
            text_to_draw (string): the text to draw to the image
            x (int): x pixel location
            y (int): y pixel location
            font (PIL.ImageFont): the font to use

        Returns:
            :class:(`PIL.Image.Image`): a Pillow image with the text drawn on it
        '''

        # make a blank image for the text, initialized to opaque black
        txt = Image.new('RGBA', cozmo.oled_face.dimensions(), (0, 0, 0, 255))

        # get a drawing context
        dc = ImageDraw.Draw(txt)

        # draw the text
        dc.text((x, y), text_to_draw, fill=(255, 255, 255, 255), font=font)

        return txt
