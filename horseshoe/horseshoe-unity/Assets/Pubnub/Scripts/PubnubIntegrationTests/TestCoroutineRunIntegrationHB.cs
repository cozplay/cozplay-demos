using System;
using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestCoroutineRunIntegrationHB: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string url = "https://pubsub.pubnub.com/time/0";
            string[] multiChannel = {"testChannel"};

            CurrentRequestType crt = CurrentRequestType.Heartbeat;
            string expectedMessage = "[14";
            string expectedChannels = string.Join (",", multiChannel);
            ResponseType respType =  ResponseType.Heartbeat;

            IEnumerator ienum = common.TestCoroutineRunProcessResponse(url, 20, -1, multiChannel, false,
                false, this.name, expectedMessage, expectedChannels, false, false, false, 0, crt, respType);
            yield return StartCoroutine(ienum);

            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}

