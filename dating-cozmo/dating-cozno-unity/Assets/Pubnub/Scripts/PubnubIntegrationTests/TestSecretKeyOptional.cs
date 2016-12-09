using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [IntegrationTest.DynamicTestAttribute ("TestSecretKeyOptional")]
    public class TestSecretKeyOptional: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string TestName = "TestSecretKeyOptional";
            yield return StartCoroutine(common.DoPublishAndParse(true, TestName, "Simple message test", "Sent", false, true, true));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", TestName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

        }
    }
}




