using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;

namespace PubNubMessaging.Tests
{
    public class TestSubscribeJoin: MonoBehaviour
    {
        CommonIntergrationTests common = new CommonIntergrationTests ();

        public bool SslOn = false;
        public bool AsObject = false;
        public IEnumerator Start ()
        {
            yield return StartCoroutine(common.DoPresenceThenSubscribeAndParse(SslOn, this.name, AsObject));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }

    }
}

