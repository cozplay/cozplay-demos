import unreal_engine as ue
import os
import asyncio
import threading
import functools
import cozmo
import PIL.Image
import numpy
import sys

class CozmoBridge:
    # Initialization
    def __init__(self, *a, **kw):
        ue.log('UECozmo __init__')
        #os.environ['COZMO_PROTOCOL_LOG_LEVEL'] = 'DEBUG'
        #os.environ['COZMO_LOG_LEVEL'] = 'DEBUG'
        self._use_logging = True
        self._cozmo_thread = None
        self._cozmo_loop = None
        self._cozmo_conn = None
        self._cozmo_robot = None
        self._cozmo_connected = False
        self._coroutine_future = None
        self._coroutine_done_uobj = None   # Uobject on which the below callback should be invoked
        self._coroutine_done_call = None   # A C++ function call of the form "FunctionName arg1 arg2 ..."
        self._start_pose = None            # Offset pose sent to unreal by Cozmo's initial pose
    
    # Starts Cozmo event loop in a new thread
    def start_cozmo(self):
        ue.log('UECozmo: Spinning off Cozmo thread from ' + str(threading.get_ident()))
        self._cozmo_thread = threading.Thread(target=self.cozmo_main, args=[])
        self._cozmo_thread.start()
    
    # Ends Cozmo event loop
    def stop_cozmo(self):
        self._cozmo_connected = False
        self._cozmo_thread.join()
        ue.log('UECozmo: Terminated Cozmo thread')
        self._cozmo_thread = None
        self._cozmo_loop = None
        self._cozmo_robot = None
        self._coroutine_future = None
        self._coroutine_done_uobj = None
        self._coroutine_done_call = None       
    
    # Called every frame for this component upon activation
    def tick(self, delta_time):
        # Check if we should invoke the callback of a coroutine that just finished
        if self._coroutine_future is not None and self._coroutine_future.done():
            self._coroutine_future = None
            if self._coroutine_done_uobj is not None and self._coroutine_done_call is not None:
                # Temp variable must be used in case callback schedules a new coroutine
                done_uobj = self._coroutine_done_uobj
                done_call = self._coroutine_done_call
                self._coroutine_done_uobj = None
                self._coroutine_done_call = None
                done_uobj.call(done_call)
    
    # coroutine_call is a string containing a call to an async python function wrapping Cozmo actions
    # C++ callback will be invoked on completion if both _coroutine_done_uobj and _coroutine_done_call are set
    # Remember that a callback can reschedule a coroutine
    # TODO: eval not the safest...
    def run_cozmo_coroutine(self, coroutine_call):
        self._coroutine_future = asyncio.run_coroutine_threadsafe(eval(coroutine_call), self._cozmo_loop)
    
    # Entry point for Cozmo thread
    def cozmo_main(self):
        ue.log('UECozmo: Cozmo thread is ' + str(threading.get_ident()))
        if self._use_logging:
            cozmo.setup_basic_logging()
        try:
            cozmo.connect(self.cozmo_run, connector=cozmo.run.FirstAvailableConnector())
            self._cozmo_conn.shutdown()
            self._cozmo_loop.stop()
            ue.log('!!!!Returned from connection')
        except cozmo.ConnectionError as e:
            ue.log('No Cozmo :(')
            sys.exit('No Cozmo :(')
    
    # Main loop for Cozmo thread, kept alive until joined by main
    async def cozmo_run(self, sdk_conn):
        asyncio.set_event_loop(sdk_conn._loop)
        self._cozmo_loop = sdk_conn._loop
        self._cozmo_conn = sdk_conn;
        self._cozmo_robot = await sdk_conn.wait_for_robot()
        self._cozmo_connected = True
        self._start_pose = self._cozmo_robot.pose
        await self.on_treasure_hunt_start()
        # self._cozmo_robot.camera.image_stream_enabled = True
        while self._cozmo_connected:
            await asyncio.sleep(0)
        self._cozmo_robot.abort_all_actions()
        self._cozmo_robot.stop_all_motors()
            
    def is_cozmo_ready(self) -> bool:
        return self._cozmo_robot is not None and self._cozmo_robot.is_ready
    
    def get_cozmo_pose(self):
        if self._cozmo_robot is None:
            return []
        # pose returned is relative to self._start_pose
        pose = self._cozmo_robot.pose
        
        qCur = (pose.rotation.q0, pose.rotation.q1, pose.rotation.q2, pose.rotation.q3)
        qStartInv = (self._start_pose.rotation.q0, -self._start_pose.rotation.q1, -self._start_pose.rotation.q2, -self._start_pose.rotation.q3)
        return [str(pose.origin_id),
                str(pose.position.x - self._start_pose.position.x),
                str(pose.position.y - self._start_pose.position.y),
                str(pose.position.z - self._start_pose.position.z),
                str(qCur[0]*qStartInv[0] - qCur[1]*qStartInv[1] - qCur[2]*qStartInv[2] - qCur[3]*qStartInv[3]),
                str(qCur[0]*qStartInv[1] + qCur[1]*qStartInv[0] + qCur[2]*qStartInv[3] + qCur[3]*qStartInv[2]),
                str(qCur[0]*qStartInv[2] + qCur[2]*qStartInv[0] + qCur[3]*qStartInv[1] + qCur[1]*qStartInv[3]),
                str(qCur[0]*qStartInv[3] + qCur[3]*qStartInv[0] + qCur[1]*qStartInv[2] + qCur[2]*qStartInv[1]),
                str(pose.rotation.angle_z.radians - self._start_pose.rotation.angle_z.radians),
                str(pose.rotation.angle_z.degrees - self._start_pose.rotation.angle_z.degrees)]
    
    async def force_go_to_position(self, x, y):
        ue.log("Force go to position: " + str(x) + " " + str(y))
        self._cozmo_robot.abort_all_actions()
        self._cozmo_robot.stop_all_motors()
        # x and y are passed relative to start pose
        await self._cozmo_robot.go_to_pose(cozmo.util.Pose(x + self._start_pose.position.x, y + self._start_pose.position.y, 0,
                                           angle_z=self._start_pose.rotation.angle_z), False).wait_for_completed()
        ue.log("Got to position")
    
    # Cozmo's behavior on game start
    async def on_treasure_hunt_start(self):
        await self._cozmo_robot.say_text("Help me find the treasure").wait_for_completed()
        self._cozmo_robot.start_behavior(cozmo.behavior.BehaviorTypes.LookAroundInPlace)
    
    # Called when Cozmo reaches treasure
    async def on_reach(self):
        await self._cozmo_robot.play_anim_trigger(cozmo.anim.Triggers.OnSpeedtapGameCozmoWinHighIntensity).wait_for_completed()
        self._cozmo_robot.start_behavior(cozmo.behavior.BehaviorTypes.LookAroundInPlace)
    
    @property
    def cozmo_thread(self):
        return self._cozmo_thread
        
    @property
    def cozmo_loop(self):
        return self._cozmo_loop
        
    @property
    def cozmo_robot(self):
        return self._cozmo_robot