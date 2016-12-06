import pygame
import _thread

class CozGame:
    def __init__(self):
        pygame.mixer.init(frequency=44100, size=-16, channels=1, buffer=1024)
        pygame.init()

    @property
    def exit_flag(self):
        for event in pygame.event.get():
            if (event.type == pygame.KEYUP):
                if (event.key == pygame.K_ESCAPE):
                    print("escape")
                    return True
        return False

