using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [IntegrationTest.DynamicTestAttribute ("TestPublishKeyPresent")]
    public class TestPublishKeyPresent: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string TestName = "TestPublishKeyPresent";
            Pubnub pubnub = new Pubnub (
                "",
                "demo",
                "",
                "",
                false
            );

            pubnub.Publish<object> ("testchannel", "testmessage", common.DisplayReturnMessage, common.DisplayReturnMessage);
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", TestName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

        }
    }
}



