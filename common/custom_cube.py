import cozmo

'''
Subclass of LightCube with color property.
@class CustomCube
@author - Team Cozplay
'''


class CustomCube(cozmo.objects.LightCube):

    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)
        self._color = None
        self._key = None

    @property
    def key(self):
        return self._key

    @key.setter
    def key(self, value):
        self._key = value

    @property
    def color(self):
        return self._color

    @color.setter
    def color(self, value):
        self._color = value

