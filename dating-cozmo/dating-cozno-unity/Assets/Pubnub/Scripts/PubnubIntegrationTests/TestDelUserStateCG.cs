using System;
using UnityTest;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using PubNubMessaging.Core;
using System.Collections;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class TestDelUserStateCG: MonoBehaviour
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
            yield return StartCoroutine(DoSubscribeSetStateDelStateCG(SslOn, this.name, AsObject, CipherOn, Message, expectedMessage, true));
            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }

        public IEnumerator DoSubscribeSetStateDelStateCG (bool ssl, string testName, bool asObject, bool withCipher, object message, string expectedStringResponse, bool matchExpectedStringResponse)
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
            string uuid = "UnityIntegrationTest_UUID";
            pubnub.ChangeUUID(uuid);
            pubnub.AddChannelsToChannelGroup<string>(new string[]{channel, ch}, cg, (string result) =>{
                //[{"status":200,"message":"OK","service":"channel-registry","error":false}]
                UnityEngine.Debug.Log (string.Format ("{0}: {1} AddChannelsToChannelGroup {2}", DateTime.Now.ToString (), testName, result));
                if(result.Contains("OK") && result.Contains("\"error\":false")){
                    bAddChannel = true;
                }
            }, this.DisplayErrorMessage);
            UnityEngine.Debug.Log (string.Format ("{0}: {1} Waiting for response", DateTime.Now.ToString (), testName));

            string strLog = string.Format ("{0}: {1} After wait {2}", 
                DateTime.Now.ToString (), 
                testName, 
                bAddChannel
                );
            UnityEngine.Debug.Log (strLog);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 


            bool bSubConnected = false;
            bool bSetState = false;
            bool bGetState = true;
            string state = "{\"k\":\"v\"}";
            pubnub.Subscribe<string>(ch, cg, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe {2}", DateTime.Now.ToString (), testName, retM));
            }, (string retConnect)=>{
                bSubConnected = true;
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe Connected {2}", DateTime.Now.ToString (), testName, retConnect));

                pubnub.SetUserState<string>(ch, cg, "", state, (string retM)=>{
                    UnityEngine.Debug.Log (string.Format ("{0}: {1} SetUserState {2} {3} {4}", 
                        DateTime.Now.ToString (), testName, retM, retM.Contains(state), retM.Contains(ch)));
                    if(retM.Contains(state) && retM.Contains(cg)){
                        bSetState = true;

                    }
                },this.DisplayErrorMessage);             

            },this.DisplayErrorMessage); 


            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);  

            bool bSetUserState2 = true;
            bool bGetUserState2 = true;
            pubnub.SetUserState(ch, cg, uuid, new KeyValuePair<string,object>("k",""), (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} SetUserState2 {2} {3} {4}", 
                DateTime.Now.ToString (), testName, retM, retM.Contains(state), retM.Contains(channel)));
                if(!retM.Contains(state) && retM.Contains(cg)){
                    bSetUserState2 = true;
                    pubnub.GetUserState(ch, cg, (string pub)=>{
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} GetUserState2 {2}", DateTime.Now.ToString (), testName, pub));
                        if(!pub.Contains(state) && pub.Contains(cg)){
                            bGetUserState2 = true;
                        }
                    },this.DisplayErrorMessage);  
                }
                },this.DisplayErrorMessage);


            bool bRemoveAll = false;
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);  

            pubnub.RemoveChannelGroup(cg, (string retM) =>{
                //[{"status":200,"payload":{"channels":["UnityIntegrationTests_30","a","c","ch","tj"],"group":"cg"},"service":"channel-registry","error":false}] 

                UnityEngine.Debug.Log (string.Format ("{0}: {1} RemoveChannelGroup {2}", DateTime.Now.ToString (), testName, retM));
                if(retM.Contains("OK") && retM.Contains("\"error\":false")){
                    bRemoveAll = true;
                } else {
                    bRemoveAll = false;
                }
            }, this.DisplayErrorMessage);


            string strLog2 = string.Format ("{0}: {1} After wait2   {2} {3} {4} {5} {6}", 
                DateTime.Now.ToString (), 
                testName, 
                bAddChannel, 
                bSetUserState2,
                bGetUserState2,
                bRemoveAll,
                    bSetState
            );
            UnityEngine.Debug.Log (strLog2);

            if(bAddChannel 
                & bRemoveAll
                    & bSetState
                    & bSetUserState2
                    & bGetUserState2
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

