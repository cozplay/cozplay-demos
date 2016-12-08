import cozmo

'''
This is extended class of cozmo robot with custom properties for HorseShoe game
@class HorseShoeRobot
@author - Team Cozplay
'''


class HorseShoeRobot(cozmo.robot.Robot):
    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)
        self._id = None
        self._angle = 0
        self._slot = 1
        self._enabled = False
        self._light = None

    @property
    def enabled(self):
        return self._enabled

    @enabled.setter
    def enabled(self, value):
        self._enabled = value

    @property
    def id(self):
        return self._id

    @id.setter
    def id(self, value):
        self._id = value

    @property
    def light(self):
        return self._light

    @light.setter
    def light(self, value):
        self._light = value

    @property
    def angle(self):
        return self._angle

    @angle.setter
    def angle(self, value):
        self._angle = value

    @property
    def slot(self):
        return self._slot

    @slot.setter
    def slot(self, value):
        self._slot = value

    def set_light(self, light):
        self.light = light
        self.set_all_backpack_lights(self.light)

    def flash(self, flag=True):
        if self.light:
            if flag:
                self.set_all_backpack_lights(self.light.flash())
            else:
                self.set_all_backpack_lights(self.light)
