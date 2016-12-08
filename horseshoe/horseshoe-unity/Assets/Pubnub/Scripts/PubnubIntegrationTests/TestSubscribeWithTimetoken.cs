using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class TestSubscribeWithTimetoken: MonoBehaviour
    {
        public bool SslOn = false;
        public bool CipherOn = false;
        public bool AsObject = false;
        public bool BothString = false;
        Pubnub pubnub;
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
            //CommonIntergrationTests common = new CommonIntergrationTests ();
            yield return StartCoroutine(DoTestSubscribeWithTimetoken(SslOn, this.name, AsObject, CipherOn, Message, expectedMessage, true));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }

        public IEnumerator DoTestSubscribeWithTimetoken (bool ssl, string testName, bool asObject, bool withCipher, object message, string expectedStringResponse, bool matchExpectedStringResponse)
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
            bool bAddChannel = false;
            bool bGetChannel = false;
            bool bGetAllCG = true;
            string uuid = "UnityIntegrationTest_UUID";
            pubnub.ChangeUUID(uuid);
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

            string strLog = string.Format ("{0}: {1} After wait {2} {3}", 
                DateTime.Now.ToString (), 
                testName, 
                bAddChannel, 
                bGetChannel);
            UnityEngine.Debug.Log (strLog);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 

            string tt = "";
            pubnub.Time<string>((string str) => {
                UnityEngine.Debug.Log (string.Format ("{0}: {1}Time {2}", DateTime.Now.ToString (), testName, str));
                object[] obj = pubnub.JsonPluggableLibrary.DeserializeToObject(str) as object[];
                tt =obj[0].ToString();
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Time tt {2}", DateTime.Now.ToString (), testName, tt));
            }, this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
            /*Subscribe CG
            ⁃   Publish to CH
            ⁃   Read Message on CG*/

            bool bSubMessage = false;
            string pubMessage = "TestMessageTT";

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
            pubnub.Publish(channel, pubMessage, (string pub)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Published CH {2}", DateTime.Now.ToString (), testName, pub));
            },this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 

            pubnub.Subscribe<string>(ch, cg, tt, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe {2}", DateTime.Now.ToString (), testName, retM));
                if(retM.Contains(pubMessage) && retM.Contains(channel) && retM.Contains(cg)){
                    bSubMessage = true;
                }
            }, (string retConnect)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe Connected {2}", DateTime.Now.ToString (), testName, retConnect));

            }, this.DisplayReturnMessageDummy, this.DisplayErrorMessage); 

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);  

            /*⁃   Unsub from CG*/

            bool bUnsub = false;
            pubnub.Unsubscribe<string>(ch, cg, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, (string retM)=> {
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Unsubscribe {2} {3} {4}", 
                    DateTime.Now.ToString (), testName, retM, retM.Contains("Unsubscribed"), retM.Contains(cg)));

                if(retM.Contains("Unsubscribed") && retM.Contains(cg)){
                    bUnsub = true;
                }
            },  this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);
            bool bRemoveCh = true;
            /*pubnub.RemoveChannelsFromChannelGroup<string>(new string[]{channel}, cg, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} RemoveChannelsFromChannelGroup {2}", 
                    DateTime.Now.ToString (), testName, retM));
                if(retM.Contains("OK") && retM.Contains("\"error\":false")){
                    bRemoveCh = true;
                }
            },  this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);  */  

            string strLog2 = string.Format ("{0}: {1} After wait2   {2} {3} {4} {5} {6} {7}", 
                DateTime.Now.ToString (), 
                testName, 
                bAddChannel, 
                bGetChannel,
                bGetAllCG,
                bSubMessage,
                bUnsub,
                bRemoveCh
            );
            UnityEngine.Debug.Log (strLog2);

            if(bAddChannel 
                & bGetAllCG
                & bGetChannel
                & bSubMessage
                & bUnsub
                & bRemoveCh
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

