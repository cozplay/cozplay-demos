using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class TestPublishNoStore: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public bool BothString = false;
        Pubnub pubnub;
        public IEnumerator Start ()
        {

            //CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine(DoPublishNoStore(this.name));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }

        public IEnumerator DoPublishNoStore ( string testName)
        {

            pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey);

            System.Random r = new System.Random ();
            string ch = "UnityIntegrationTest_CH_" + r.Next (100);
            string uuid = "UnityIntegrationTest_UUID";
            pubnub.ChangeUUID(uuid);

            string pubMessage = "TestMessageNoStore" + r.Next (100);

            pubnub.Publish(ch, pubMessage, false, (string retM)=>{
                pubnub.DetailedHistory(ch, 1, (string retM2)=>{
                    UnityEngine.Debug.Log (string.Format("retM2: {0}", retM2));
                    if(!retM2.Contains(pubMessage)){                        
                        IntegrationTest.Pass();
                    } else {
                        IntegrationTest.Fail();
                    }
                }, this.DisplayErrorMessage);
            }, this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);

            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        public void DisplayErrorMessage (PubnubClientError result)
        {
            //DeliveryStatus = true;
            UnityEngine.Debug.Log ("DisplayErrorMessage:" + result.ToString ());
        }

        public void DisplayReturnMessageDummy (object result)
        {
            //deliveryStatus = true;
            //Response = result;
            UnityEngine.Debug.Log ("DisplayReturnMessageDummy:" + result.ToString ());
        }

    }


}



