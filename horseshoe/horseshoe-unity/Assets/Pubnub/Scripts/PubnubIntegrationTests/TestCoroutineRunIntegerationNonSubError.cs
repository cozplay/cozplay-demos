using System;
using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestCoroutineRunIntegerationNonSubError: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string url = "http://pubsub.pubnub.com";
            string[] multiChannel = {"testChannel"};

            //Inducing Error by setting wrong request type
            CurrentRequestType crt = CurrentRequestType.NonSubscribe;
            string expectedMessage = "404 Nothing";
            string expectedChannels = string.Join (",", multiChannel);
            ResponseType respType =  ResponseType.Time;

            IEnumerator ienum = common.TestCoroutineRunError(url, 20, -1, multiChannel, false,
                false, this.name, expectedMessage, expectedChannels, true, false, false, 0, crt, respType);
            yield return StartCoroutine(ienum);

            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}


