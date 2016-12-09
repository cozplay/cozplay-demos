using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [IntegrationTest.DynamicTestAttribute ("TestNullAsEmptyOnPublish")]
    public class TestNullAsEmptyOnPublish: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string TestName = "TestNullAsEmptyOnPublish";
            Pubnub pubnub = new Pubnub (
                null,
                "demo",
                null,
                null,
                false
            );

            common.SetPubnub = pubnub;

            pubnub.Publish<object> ("testchannel", "testmessage", common.DisplayReturnMessage, common.DisplayReturnMessage);
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", TestName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

        }
    }
}




