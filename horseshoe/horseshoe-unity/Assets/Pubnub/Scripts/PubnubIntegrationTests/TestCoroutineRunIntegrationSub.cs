using System;
using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestCoroutineRunIntegrationSub: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string[] multiChannel = {"testChannel"};
            //
            CurrentRequestType crt = CurrentRequestType.NonSubscribe;
            string expectedMessage = "[[]";
            string expectedChannels = string.Join (",", multiChannel);
            ResponseType respType =  ResponseType.Time;

            Pubnub pubnub = new Pubnub (
                CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey,
                "",
                "",
                true
            );

            long nanoSecondTime = 0;//Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (DateTime.UtcNow);

            string url = string.Format ("http://pubsub.pubnub.com/subscribe/{0}/{1}/0/{2}?uuid={3}&pnsdk={4}", CommonIntergrationTests.SubscribeKey, 
                expectedChannels, nanoSecondTime, pubnub.SessionUUID, pubnub.Version
            );

            IEnumerator ienum = common.TestCoroutineRunProcessResponse(url, 20, -1, multiChannel, false,
                false, this.name, expectedMessage, expectedChannels, false, false, false, 0, crt, respType);
            yield return StartCoroutine(ienum);

            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }

}

