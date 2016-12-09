using System;
using System.Collections;
using UnityEngine;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestPublishComplex: MonoBehaviour
    {
        public bool SslOn = false;
        public bool AsObject = false;
        public bool WithCipher = false;
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();

            object message = new PubnubDemoObject ();

            yield return StartCoroutine(common.DoPublishAndParse(SslOn, this.name, message, "Sent", AsObject, WithCipher));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

        }
    }
}



