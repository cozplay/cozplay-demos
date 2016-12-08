//#define REDUCE_PUBNUB_COROUTINES
using System;
using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    #if(REDUCE_PUBNUB_COROUTINES)
    [IntegrationTest.Ignore]
    #endif
    public class TestPHBTimeout: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            #if(!REDUCE_PUBNUB_COROUTINES)
            CommonIntergrationTests common = new CommonIntergrationTests ();

            System.Random r = new System.Random ();
            string channel = "UnityIntegrationTestsTimeout_" + r.Next (100);

            string[] multiChannel = new string[1];
            multiChannel [0] = channel;

            Pubnub pubnub = new Pubnub (
                CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey,
                "",
                "",
                true
            );

            CurrentRequestType crt = CurrentRequestType.PresenceHeartbeat;
            string expectedMessage = "Timed out";
            string expectedChannels = string.Join (",", multiChannel);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (DateTime.UtcNow);

            //Send a sub request (intentional) that waits for response
            string url = string.Format ("http://pubsub.pubnub.com/subscribe/{0}/{1}/0/{2}?uuid={3}&pnsdk={4}", CommonIntergrationTests.SubscribeKey, 
                expectedChannels, nanoSecondTime, pubnub.SessionUUID, pubnub.Version
            );
            ResponseType respType =  ResponseType.PresenceHeartbeat;

            common.TestCoroutineRun(url, 5, 0, multiChannel, false,
                false, this.name, expectedMessage, expectedChannels, true, true, false, 0, crt, respType);

            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
            #else
            yield return null;
            UnityEngine.Debug.Log (string.Format("{0}: Ignoring test", this.name));
            IntegrationTest.Pass();
            #endif

        }
    }
}

