//#define REDUCE_PUBNUB_COROUTINES
using System;
using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestCoroutineRunSubscribeAbort: MonoBehaviour
    {
        public IEnumerator Start ()
        { 
            #if(REDUCE_PUBNUB_COROUTINES)
            CommonIntergrationTests common = new CommonIntergrationTests ();
            System.Random r = new System.Random ();
            string channel = "UnityIntegrationTestsTimeout_" + r.Next (100);
            string channel2 = "UnityIntegrationTestsTimeout_" + r.Next (100);

            string[] multiChannel = new string[2];
            multiChannel [0] = channel;

            Pubnub pubnub = new Pubnub (
                CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey,
                "",
                "",
                true
            );

            CurrentRequestType crt = CurrentRequestType.Subscribe;
            string expectedMessage = "Aborted";
            string expectedChannels = string.Join (",", multiChannel);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (DateTime.UtcNow);

            string url = string.Format ("http://pubsub.pubnub.com/v2/subscribe/{0}/{1}/0?uuid={3}&tt={2}&pnsdk={4}", CommonIntergrationTests.SubscribeKey, 
                expectedChannels, nanoSecondTime, pubnub.SessionUUID, pubnub.Version
            );
            multiChannel [1] = channel2;
            expectedChannels = string.Join (",", multiChannel);
            string url2 = string.Format ("http://pubsub.pubnub.com/v2/subscribe/{0}/{1}/0?uuid={3}&tt={2}&pnsdk={4}", CommonIntergrationTests.SubscribeKey, 
                expectedChannels, nanoSecondTime, pubnub.SessionUUID, pubnub.Version
            );
            ResponseType respType =  ResponseType.SubscribeV2;

            common.TestCoroutineRunSubscribeAbort(url, url2, 20, -1, multiChannel, false,
                false, this.name, expectedMessage, expectedChannels, false, false, false, 0, crt, respType);
            //yield return StartCoroutine(ienum);

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

