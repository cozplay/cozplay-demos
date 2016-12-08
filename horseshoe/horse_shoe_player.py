
'''
Horse Shoe Player class to store player slots
@class HorseShoePlayer
@author - Team Cozplay
'''

class HorseShoePlayer:
    def __init__(self):
        self._slots = []

    @property
    def slots(self):
        return self._slots

    @slots.setter
    def slots(self, value):
        self._slots = value
