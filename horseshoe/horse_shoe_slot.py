'''
Horse Shoe game slot class to store current state information
@class HorseShoeSlot
@author - Team Cozplay
'''


class HorseShoeSlot:
    def __init__(self, state=0, active=0):
        self._state = state
        self._active = active

    @property
    def state(self):
        return self._state

    @state.setter
    def state(self, value):
        self._state = value

    @property
    def active(self):
        return self._active

    @active.setter
    def active(self, value):
        self._active = value
