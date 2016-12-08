using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class TestPublishWithMeta: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public bool BothString = false;
        Pubnub pubnub;
        public IEnumerator Start ()
        {

            //CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine(DoTestPresenceCG(this.name));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }

        public IEnumerator DoTestPresenceCG ( string testName)
        {
            pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey);

            System.Random r = new System.Random ();
            string ch = "UnityIntegrationTest_CH_" + r.Next (100);
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            string uuid = "UnityIntegrationTest_UUID";
            pubnub.ChangeUUID(uuid);

            bool bSubConnect = false;

            string pubMessage = "TestMessageMeta";
            pubnub.FilterExpression = "region=='east'";
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("region", "east");

            pubnub.Subscribe<string>(ch, "", (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe {2}", DateTime.Now.ToString (), testName, retM));
                if(retM.Contains(pubMessage)){
                    bSubConnect = true;
                }
            }, (string retConnect)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe Connected {2}", DateTime.Now.ToString (), testName, retConnect));

                pubnub.Publish<string>(ch, pubMessage, dict, this.DisplayReturnMessageDummy, 
                    this.DisplayErrorMessage);
            }, this.DisplayReturnMessageDummy, this.DisplayErrorMessage); 

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);

            /*⁃   Unsub from CG*/

            bool bUnsub = false;
            pubnub.Unsubscribe<string>(ch, "", this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, (string retM)=> {
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Unsubscribe {2} {3}", 
                    DateTime.Now.ToString (), testName, retM, retM.Contains("Unsubscribed")));

                if(retM.Contains("Unsubscribed")){
                    bUnsub = true;
                   
                }
            },  this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);

            string strLog2 = string.Format ("{0}: {1} After wait2   {2} {3}", 
                DateTime.Now.ToString (), 
                testName, 
                bSubConnect,
                bUnsub

            );
            UnityEngine.Debug.Log (strLog2);

            if (bSubConnect
                & bUnsub

            ){
                IntegrationTest.Pass();
            }    

            //pubnub.EndPendingRequests ();
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



