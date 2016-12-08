using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;

namespace PubNubMessaging.Tests
{
    public class TestSubscribeInt: MonoBehaviour
    {
        public int Message = 1;
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public IEnumerator Start ()
        {
			#if !PUBNUB_PS_V2_RESPONSE
            CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine(common.DoSubscribeThenPublishAndParse(SslOn, this.name, AsObject, CipherOn, Message, "1", false));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
			#else
			yield return null;
			UnityEngine.Debug.Log (string.Format("{0}: Ignoring test", this.name));
			IntegrationTest.Pass();
			#endif

        }
    }
}

