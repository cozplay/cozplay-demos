using System;
using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestBounceNonSub: MonoBehaviour
    {
        public IEnumerator Start ()
        {
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

            CurrentRequestType crt = CurrentRequestType.NonSubscribe;
            string expectedMessage = "Aborted";
            string expectedChannels = string.Join (",", multiChannel);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (DateTime.UtcNow);

            //Send a sub request (intentional) that waits for response
            string url = string.Format ("http://pubsub.pubnub.com/subscribe/{0}/{1}/0/{2}?uuid={3}&pnsdk={4}", CommonIntergrationTests.SubscribeKey, 
                expectedChannels, nanoSecondTime, pubnub.SessionUUID, pubnub.Version
            );
            ResponseType respType =  ResponseType.HereNow;

            common.TestCoroutineBounce(url, 5, 0, multiChannel, false,
                false, this.name, expectedMessage, expectedChannels, true, false, false, 0, crt, respType);

            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}

