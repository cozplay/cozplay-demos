using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class TestSubscribeWildcard: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public bool BothString = false;
        Pubnub pubnub;
        public IEnumerator Start ()
        {
            
            //CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine(DoTestSubscribeWildcard(this.name));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }

        public IEnumerator DoTestSubscribeWildcard ( string testName)
        {
            /*  ⁃   Add CH to CG
        ⁃   List CG
        ⁃   Get all CGs
        ⁃   
        ⁃   */
            pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey);

            System.Random r = new System.Random ();
            string ch = "UnityIntegrationTest_CH." + r.Next (100);
            string channel = "UnityIntegrationTest_CH." + r.Next (100);
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            string uuid = "UnityIntegrationTest_UUID";
            pubnub.ChangeUUID(uuid);

            /*Subscribe CG
            ⁃   Publish to CH
            ⁃   Read Message on CG*/

            bool bSubMessage = false;
            bool bSubMessage2 = false;
            bool bSubConnect = false;
            bool bSubWC = false;
            string pubMessage = "TestMessageWC";
            string chToSub = "UnityIntegrationTest_CH.*";
            pubnub.Subscribe<string>(chToSub, "", (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe {2}", DateTime.Now.ToString (), testName, retM));
                if(retM.Contains(pubMessage) && retM.Contains(channel)){
                    bSubMessage = true;
                }
                if(retM.Contains(pubMessage) && retM.Contains(ch)){
                    bSubMessage2 = true;
                }
            }, (string retConnect)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe Connected {2}", DateTime.Now.ToString (), testName, retConnect));
                bSubConnect = true;
            }, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe WC {2}", DateTime.Now.ToString (), testName, retM));
                if(retM.Contains("join") && retM.Contains(uuid) && retM.Contains(chToSub)){
                    bSubWC = true;
                }
            }, this.DisplayErrorMessage); 

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
            pubnub.Publish(channel, pubMessage, (string pub)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Published CH {2}", DateTime.Now.ToString (), testName, pub));
            },this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 

            /*pubnub.Publish(ch, pubMessage, (string pub)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Published CH {2}", DateTime.Now.ToString (), testName, pub));
            },this.DisplayErrorMessage);*/



            /*⁃   Unsub from CG*/

            bool bUnsub = true;
            /*pubnub.Unsubscribe<string>(chToSub, "", this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, (string retM)=> {
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Unsubscribe {2} {3}", 
                    DateTime.Now.ToString (), testName, retM, retM.Contains("Unsubscribed")));

                if(retM.Contains("Unsubscribed")){
                    bUnsub = true;
                }
            },  this.DisplayErrorMessage);*/

            //yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);

            string strLog2 = string.Format ("{0}: {1} After wait2   {2} {3} {4}", 
                DateTime.Now.ToString (), 
                testName, 
                bSubWC,
                bSubMessage,
                bUnsub

            );
            UnityEngine.Debug.Log (strLog2);

            if(bSubWC
                & bSubMessage
                & bUnsub

            ){
                IntegrationTest.Pass();
            }            
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

