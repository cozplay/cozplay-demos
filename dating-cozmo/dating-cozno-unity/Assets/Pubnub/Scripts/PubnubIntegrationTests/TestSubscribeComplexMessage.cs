using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestSubscribeComplexMessage: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public IEnumerator Start ()
        {
            if (CommonIntergrationTests.TestingUsingMiniJSON) {
                UnityEngine.Debug.Log (string.Format ("{0}: Ignored for MiniJSON", this.name));
                IntegrationTest.Pass ();
            } else {
                object Message = new PubnubDemoObject ();
                CommonIntergrationTests common = new CommonIntergrationTests ();
                yield return StartCoroutine (common.DoSubscribeThenPublishAndParse (SslOn, this.name, AsObject, CipherOn, Message, "\"VersionID\":3.4", true));
                UnityEngine.Debug.Log (string.Format ("{0}: After StartCoroutine", this.name));
                yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
            }
        }
    }
}

