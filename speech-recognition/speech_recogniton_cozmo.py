import asyncio
import cozmo
import speech_recognition as sr
from os import system


'''
Speech Recognition using pc
@class SpeechRecognitionCozmo
@author - Team Cozplay
'''
class SpeechRecognitionCozmo:
    GAME_TIME = 10 * 60

    def __init__(self, *a, **kw):

        #init cozmo
        cozmo.setup_basic_logging()
        cozmo.connect(self.run)
        while True:
            self.take_input()

    def run(self, coz_conn):
        asyncio.set_event_loop(coz_conn._loop)
        self.coz = coz_conn.wait_for_robot()
        while True:
            self.take_input()

    def speak(self,text):
        system("say '"+text+"' -v Alex -r 200")

    def take_input(self):
        # Record Audio
        r = sr.Recognizer()
        with sr.Microphone(chunk_size=512) as source:
            print("Say something!")
            self.flash_backpack(True)
            self.coz.say_text(text="", play_excited_animation=True).wait_for_completed()
            audio = r.listen(source)

        # Speech recognition using Google Speech Recognition
        try:
            # for testing purposes, we're just using the default API key
            # to use another API key, use `r.recognize_google(audio, key="GOOGLE_SPEECH_RECOGNITION_API_KEY")`
            # instead of `r.recognize_google(audio)`
            #self.flash_backpack(False)
            print("You said: " + r.recognize_google(audio))
            #self.speak(r.recognize_google(audio))
            self.coz.say_text(r.recognize_google(audio)).wait_for_completed()

        except sr.UnknownValueError:
            print("Google Speech Recognition could not understand audio")

        except sr.RequestError as e:
            print("Could not request results from Google Speech Recognition service; {0}".format(e))

    def flash_backpack(self, flag):
        self.coz.set_all_backpack_lights(cozmo.lights.green_light.flash() if flag else cozmo.lights.off_light)


if __name__ == '__main__':
    SpeechRecognitionCozmo()
