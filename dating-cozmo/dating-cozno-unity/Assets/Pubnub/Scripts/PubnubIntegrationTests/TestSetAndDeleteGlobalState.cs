using System;
using System.Collections;
using UnityEngine;

namespace PubNubMessaging.Tests
{
    [IntegrationTest.DynamicTestAttribute ("TestSetAndDeleteGlobalState")]
    public class TestSetAndDeleteGlobalState: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            string testName = "TestSetAndDeleteGlobalState";

            yield return StartCoroutine(common.SetAndDeleteStateAndParse(false, testName));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

        }

    }
}

