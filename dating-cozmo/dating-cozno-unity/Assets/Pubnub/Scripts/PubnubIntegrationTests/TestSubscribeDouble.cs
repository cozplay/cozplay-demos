using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;

namespace PubNubMessaging.Tests
{
    public class TestSubscribeDouble: MonoBehaviour
    {
        public double Message = 1.1;
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine(common.DoSubscribeThenPublishAndParse(SslOn, this.name, AsObject, CipherOn, Message, "1.1", true));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}

