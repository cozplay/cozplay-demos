using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class TestPresenceCG: MonoBehaviour
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
            /*  ⁃   Add CH to CG
        ⁃   List CG
        ⁃   Get all CGs
        ⁃   
        ⁃   */
            pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey);

            System.Random r = new System.Random ();
            string cg = "UnityIntegrationTest_CG_" + r.Next (100);
            string ch = "UnityIntegrationTest_CH_" + r.Next (100);
            string channel = "UnityIntegrationTest_CH_" + r.Next (100);
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            string uuid = "UnityIntegrationTest_UUID";
            pubnub.ChangeUUID(uuid);

            /*Subscribe CG
            ⁃   Publish to CH
            ⁃   Read Message on CG*/

            bool bSubConnect = false;

            string pubMessage = "TestMessageWC";
            string chToSub = "UnityIntegrationTest_CH.*";

            bool bAddChannel = false;
            bool bGetChannel = false;

            pubnub.AddChannelsToChannelGroup<string>(new string[]{channel}, cg, (string result) =>{
                //[{"status":200,"message":"OK","service":"channel-registry","error":false}]
                UnityEngine.Debug.Log (string.Format ("{0}: {1} AddChannelsToChannelGroup {2}", DateTime.Now.ToString (), testName, result));
                if(result.Contains("OK") && result.Contains("\"error\":false")){
                    bAddChannel = true;
                    pubnub.GetChannelsForChannelGroup(cg, (string result2) =>{
                        //[{"status":200,"payload":{"channels":["UnityIntegrationTests_30","a","c","ch","tj"],"group":"cg"},"service":"channel-registry","error":false}] 

                        UnityEngine.Debug.Log (string.Format ("{0}: {1} GetChannelsOfChannelGroup {2}", DateTime.Now.ToString (), testName, result2));
                        if(result2.Contains(cg) && result2.Contains(channel)){
                            bGetChannel = true;
                        } else {
                            bGetChannel = false;
                        }
                    }, this.DisplayErrorMessage);
                }
            }, this.DisplayErrorMessage);
            UnityEngine.Debug.Log (string.Format ("{0}: {1} Waiting for response", DateTime.Now.ToString (), testName));

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);

            pubnub.Subscribe<string>("", cg+"-pnpres", (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe {2}", DateTime.Now.ToString (), testName, retM));
                if(retM.Contains("join")){
                    bSubConnect = true;
                }
            }, (string retConnect)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe Connected {2}", DateTime.Now.ToString (), testName, retConnect));

                pubnub.Subscribe<string>("", cg, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            }, this.DisplayReturnMessageDummy, this.DisplayErrorMessage); 

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);

            /*⁃   Unsub from CG*/

            bool bUnsub = false;
            pubnub.Unsubscribe<string>(chToSub, "", this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, (string retM)=> {
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Unsubscribe {2} {3}", 
                    DateTime.Now.ToString (), testName, retM, retM.Contains("Unsubscribed")));

                if(retM.Contains("Unsubscribed")){
                    bUnsub = true;
                    string strLog2 = string.Format ("{0}: {1} After wait2   {2} {3} {4} {5}", 
                        DateTime.Now.ToString (), 
                        testName, 
                        bGetChannel,
                        bAddChannel,
                        bSubConnect,
                        bUnsub

                    );
                    UnityEngine.Debug.Log (strLog2);

                    if(bAddChannel
                        & bGetChannel
                        & bSubConnect
                        & bUnsub

                    ){
                        IntegrationTest.Pass();
                    }    
                }
            },  this.DisplayErrorMessage);

            //yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);


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



