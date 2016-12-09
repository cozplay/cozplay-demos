using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestDetailedHistory: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public bool NoStore = false;

        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            object[] message = {"Test Detailed History"};

            yield return StartCoroutine(common.DoPublishThenDetailedHistoryAndParse(SslOn, this.name, message, AsObject, CipherOn, NoStore, message.Length, false));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

        }
    }
}