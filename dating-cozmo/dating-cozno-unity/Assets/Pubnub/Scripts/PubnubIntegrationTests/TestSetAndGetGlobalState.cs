using System;
using System.Collections;
using UnityEngine;

namespace PubNubMessaging.Tests
{
    [IntegrationTest.DynamicTestAttribute ("TestSetAndGetGlobalState")]
    public class TestSetAndGetGlobalState: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string testName = "TestSetAndGetGlobalState";

            yield return StartCoroutine(common.SetAndGetStateAndParse(false, testName));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}

