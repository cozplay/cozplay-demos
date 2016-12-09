using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestDetailedHistoryParams: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public bool NoStore = false;

        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            object[] message = {"Test Detailed History 1","Test Detailed History 2","Test Detailed History 3","Test Detailed History 4",
                "Test Detailed History 5","Test Detailed History 6","Test Detailed History 7","Test Detailed History 8",
                "Test Detailed History 9","Test Detailed History 10"};

            yield return StartCoroutine(common.DoPublishThenDetailedHistoryAndParse(SslOn, this.name, message, AsObject, CipherOn, NoStore, message.Length, true));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

        }
    }
}
