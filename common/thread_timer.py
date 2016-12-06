from threading import Thread, Event

'''
Thread timer class which uses threading
@class ThreadTimer
@author - Team Cozplay
'''


class ThreadTimer(Thread):

    """ Added 'event' variable (Type - threading.Event)
    # ThreadTimer is now initialized with this variable:
    #
    # Example:
    # stop_flag = Event()
    # thread = ThreadTimer(time, update_function, stop_flag)
    # thread.start()
    #
    # To stop the thread:
    # stop_flag.set()
    """
    def __init__(self, time_interval, update_callback, event):
        Thread.__init__(self)
        self.time_interval = time_interval
        self.update_callback = update_callback
        self.stopped = event

    def run(self):
        while not self.stopped.wait(self.time_interval):
            self.update_callback()
