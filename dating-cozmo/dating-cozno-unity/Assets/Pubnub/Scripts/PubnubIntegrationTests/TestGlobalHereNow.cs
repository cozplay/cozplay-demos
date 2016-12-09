using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestGlobalHereNow: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine (common.DoSubscribeThenDoGlobalHereNowAndParse (SslOn, this.name, !AsObject));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}

