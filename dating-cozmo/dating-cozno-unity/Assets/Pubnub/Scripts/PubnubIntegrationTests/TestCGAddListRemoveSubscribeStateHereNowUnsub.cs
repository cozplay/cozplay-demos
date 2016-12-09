using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class TestCGAddListRemoveSubscribeStateHereNowUnsub: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public bool BothString = false;
        public IEnumerator Start ()
        {
            Dictionary<string, long> Message1 = new Dictionary<string, long>();
            Dictionary<string, string> Message2 = new Dictionary<string, string>();
            object Message = null;
            if (BothString) {
                Message2.Add("cat", "test");
                Message = Message2;
            } else {
                Message1.Add ("cat", 14255515120803306);
                Message = Message1;
            }

            string expectedMessage = "\"cat\":\"14255515120803306\"";
            if (BothString) {
                expectedMessage = "\"cat\":\"test\"";
            } else {
                if (CommonIntergrationTests.TestingUsingMiniJSON) {
                    expectedMessage = "\"cat\":14255515120803306";
                } 
            }
            CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine(common.DoCGAddListRemoveSubscribeStateHereNowUnsub(SslOn, this.name, AsObject, CipherOn, Message, expectedMessage, true));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}

