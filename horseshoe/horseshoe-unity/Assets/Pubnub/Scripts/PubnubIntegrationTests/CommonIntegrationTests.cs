#define USE_JSONFX_UNITY_IOS
//#define REDUCE_PUBNUB_COROUTINES
//#define USE_MiniJSON
using System;
using PubNubMessaging.Core;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

#if (USE_JSONFX) || (USE_JSONFX_UNITY)
using JsonFx.Json;
#elif (USE_JSONFX_UNITY_IOS)
using Pathfinding.Serialization.JsonFx;
#elif (USE_DOTNET_SERIALIZATION)
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
#elif (USE_MiniJSON)
using MiniJSON;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
#endif


namespace PubNubMessaging.Tests
{
    class CustomClass
    {
        public string foo = "hi!";
        public int[] bar = { 1, 2, 3, 4, 5 };
    }

    [Serializable]
    class PubnubDemoObject
    {
        public double VersionID = 3.4;
        public long Timetoken = 13601488652764619;
        public string OperationName = "Publish";
        public string[] Channels = { "ch1" };
        public PubnubDemoMessage DemoMessage = new PubnubDemoMessage ();
        public PubnubDemoMessage CustomMessage = new PubnubDemoMessage ("This is a demo message");
        public XmlDocument SampleXml = new PubnubDemoMessage ().TryXmlDemo ();
    }

    [Serializable]
    class PubnubDemoMessage
    {
        public string DefaultMessage = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";
        //public string DefaultMessage = "\"";
        public PubnubDemoMessage ()
        {
        }

        public PubnubDemoMessage (string message)
        {
            DefaultMessage = message;
        }

        public XmlDocument TryXmlDemo ()
        {
            XmlDocument xmlDocument = new XmlDocument ();
            xmlDocument.LoadXml ("<DemoRoot><Person ID='ABCD123'><Name><First>John</First><Middle>P.</Middle><Last>Doe</Last></Name><Address><Street>123 Duck Street</Street><City>New City</City><State>New York</State><Country>United States</Country></Address></Person><Person ID='ABCD456'><Name><First>Peter</First><Middle>Z.</Middle><Last>Smith</Last></Name><Address><Street>12 Hollow Street</Street><City>Philadelphia</City><State>Pennsylvania</State><Country>United States</Country></Address></Person></DemoRoot>");

            return xmlDocument;
        }
    }

    public class CommonIntergrationTests
    {
        public static string PublishKey = "demo-36";
        public static string SubscribeKey = "demo-36";
        public static string SecretKey = "demo-36";
        public static float WaitTimeBetweenCalls = 3;
        public static float WaitTimeBetweenCallsLow = 2;
        public static float WaitTimeToReadResponse = 15;
        public static float WaitTime = 20;
        Pubnub pubnub;

        public bool TimedOut {
            get;
            set;
        }

        public Pubnub SetPubnub {
            set {
                pubnub = value;
            }
        }

        public static bool TestingUsingMiniJSON { 
            get {
                #if (USE_MiniJSON)
                return true;
                #else
                return false;
                #endif
            }
        }

        public object Response { get; set; }

        public string ResponseString{ get; set; }

        public string SubChannel{ get; set; }

        public object Name { get; set; }

        public string ErrorResponse { get; set; }

        public bool DeliveryStatus  { get; set; }

        public void DisplayErrorMessage (PubnubClientError result)
        {
            ErrorResponse = result.Description;
            //DeliveryStatus = true;
            UnityEngine.Debug.Log ("DisplayErrorMessage:" + result.ToString ());
        }

        public void DisplayReturnMessageDummy (object result)
        {
            //deliveryStatus = true;
            //Response = result;
            ErrorResponse = result.ToString ();
            UnityEngine.Debug.Log ("DisplayReturnMessageDummy:" + result.ToString ());
        }

        public void DisplayReturnMessage (object result)
        {
            UnityEngine.Debug.Log ("DisplayReturnMessageO:" + result.ToString ());
            Response = result;
            DeliveryStatus = true;
        }

        public void DisplayReturnMessagePresence (string result)
        {
            UnityEngine.Debug.Log ("DisplayReturnMessagePresence: " + Name + " " + result.ToString ());
            //Response = (object)result.ToString ();
            ResponseString += result.ToString ();
            DeliveryStatus = true;
        }

        public void OnSubConnectedUnsub (string result)
        {
            UnityEngine.Debug.Log ("OnSubConnectedUnsub: " + Name + " " + result.ToString ());
            pubnub.Unsubscribe<string> (SubChannel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
        }

        public void DisplayReturnMessage (string result)
        {
            UnityEngine.Debug.Log ("DisplayReturnMessageS: " + Name + " " + result.ToString ());
            Response = (object)result.ToString ();
            DeliveryStatus = true;
        }

        private string Init (string testName, bool ssl, bool withCipher, bool secretEmpty)
        {
            this.DeliveryStatus = false;
            this.TimedOut = false;
            this.Response = null;
            this.Name = testName;

            System.Random r = new System.Random ();
            string channel = "UnityIntegrationTests_" + r.Next (100);
            var cipher = "";
            if (withCipher) {
                cipher = "enigma";
            }
            if (!secretEmpty) {
                pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                    CommonIntergrationTests.SubscribeKey,
                    CommonIntergrationTests.SecretKey, 
                    cipher, 
                    ssl);
            } else {
                pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                    CommonIntergrationTests.SubscribeKey);
            }

            return channel;
        }

        private string Init (string testName, bool ssl, bool withCipher)
        {
            return Init (testName, ssl, withCipher, false);
        }

        private string Init (string testName, bool ssl)
        {
            return Init (testName, ssl, false);
        }

        public void GetTimeFromServerUsingNewPubNub (string testName, bool ssl)
        {
            Pubnub pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                                CommonIntergrationTests.SubscribeKey,
                                CommonIntergrationTests.SecretKey, 
                                "", 
                                ssl);
            pubnub.Time<object> (this.DisplayReturnMessage, this.DisplayErrorMessage);
        }

        public IEnumerator DoCGAddListRemoveSubscribeStateHereNowUnsub (bool ssl, string testName, bool asObject, bool withCipher, object message, string expectedStringResponse, bool matchExpectedStringResponse)
        {
        /*  ⁃   Add CH to CG
        ⁃   List CG
        ⁃   Get all CGs
        ⁃   
        ⁃   */

            string channel = Init (testName, ssl, withCipher);
            System.Random r = new System.Random ();
            string cg = "UnityIntegrationTest_CG_" + r.Next (100);
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
            
            /*Subscribe CG
            ⁃   Publish to CH
            ⁃   Read Message on CG*/

            bool bSubConnected = false;
            bool bSubMessage = false;
            string pubMessage = "TestMessage";
            pubnub.Subscribe<string>("", cg, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe {2}", DateTime.Now.ToString (), testName, retM));
                    if(retM.Contains(pubMessage) && retM.Contains(channel) && retM.Contains(cg)){
                        bSubMessage = true;
                    }
                }, (string retConnect)=>{
                    bSubConnected = true;
                    UnityEngine.Debug.Log (string.Format ("{0}: {1} Subscribe Connected {2}", DateTime.Now.ToString (), testName, retConnect));
                    pubnub.Publish(channel, pubMessage, (string pub)=>{
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} Published {2}", DateTime.Now.ToString (), testName, pub));
                    },this.DisplayErrorMessage);  
            },this.DisplayErrorMessage); 
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
            /*
            ⁃   Set State of CG
            ⁃   Get State of CG
            */
            bool bSetState = false;
            bool bGetState = false;
            string state = "{\"k\":\"v\"}";
            pubnub.SetUserState<string>("", cg, "", state, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} SetUserState {2} {3} {4}", 
                    DateTime.Now.ToString (), testName, retM, retM.Contains(state), retM.Contains(channel)));
                if(retM.Contains(state) && retM.Contains(cg)){
                    bSetState = true;
                    pubnub.GetUserState(channel, (string pub)=>{
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} GetUserState {2}", DateTime.Now.ToString (), testName, pub));
                        if(pub.Contains(state) && pub.Contains(cg)){
                            bGetState = true;
                        }
                    },this.DisplayErrorMessage);  
                }
            },this.DisplayErrorMessage);             
             
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
/*
            ⁃   Run Here Now CG with State
            ⁃   Run Here Now CG without State
*/
            bool bHereNow = false;
            pubnub.HereNow("", cg, true, false, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} HereNow {2}", 
                DateTime.Now.ToString (), testName, retM));
                if(retM.Contains(uuid)){
                    bHereNow = true;
                };    
            }, this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);

            bool bHereNowState = false;
            pubnub.HereNow("", cg, true, true, (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} HereNowWithState {2}", 
                DateTime.Now.ToString (), testName, retM));
                if(retM.Contains(uuid) && retM.Contains(state)){
                    bHereNowState = true;
                };    
            }, this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);
/*
            ⁃   Del State of CG

*/
            bool bSetUserState2 = false;
            bool bGetUserState2 = false;
            pubnub.SetUserState("", cg, uuid, new KeyValuePair<string,object>("k",""), (string retM)=>{
                UnityEngine.Debug.Log (string.Format ("{0}: {1} SetUserState2 {2} {3} {4}", 
                DateTime.Now.ToString (), testName, retM, retM.Contains(state), retM.Contains(channel)));
                if(!retM.Contains(state) && retM.Contains(cg)){
                    bSetUserState2 = true;
                    pubnub.GetUserState(channel, (string pub)=>{
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} GetUserState2 {2}", DateTime.Now.ToString (), testName, pub));
                        if(!pub.Contains(state) && pub.Contains(cg)){
                            bGetUserState2 = true;
                        }
                    },this.DisplayErrorMessage);  
                }

            }, this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);
            
/*
⁃   Remove CH from CG
⁃   List CG
*/


            /*⁃   Unsub from CG*/

            bool bUnsub = false;
            pubnub.Unsubscribe<string>("", cg, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, (string retM)=> {
            UnityEngine.Debug.Log (string.Format ("{0}: {1} Unsubscribe {2} {3} {4}", 
                DateTime.Now.ToString (), testName, retM, retM.Contains("Unsubscribed"), retM.Contains(cg)));

            if(retM.Contains("Unsubscribed") && retM.Contains(cg)){
                    bUnsub = true;
                }
            },  this.DisplayErrorMessage);

            bool bRemoveCh = true;
            bool bGetChannel2 = true;

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);   


            string strLog2 = string.Format ("{0}: {1} After wait2   {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}", 
                DateTime.Now.ToString (), 
                testName, 
                bAddChannel, 
                bGetChannel,
                bGetAllCG,
                bSubMessage,
                bGetState,
                bSetState,
                bHereNowState,
                bHereNow,
                bUnsub,
                bSetUserState2,
                bGetUserState2,
                bRemoveCh,
                bGetChannel2
                );
            UnityEngine.Debug.Log (strLog2);

            if(bAddChannel 
                & bGetAllCG
                & bGetChannel
                & bGetState
                & bSetState
                & bSubMessage
                & bHereNowState
                & bHereNow
                & bUnsub
                & bSetUserState2
                & bGetUserState2
                & bRemoveCh
                & bGetChannel2
            
            ){
                IntegrationTest.Pass();
            }            
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        

        public IEnumerator DoSubscribeThenPublishAndParse (bool ssl, string testName, bool asObject, bool withCipher, object message, string expectedStringResponse, bool matchExpectedStringResponse)
        {
            string channel = Init (testName, ssl, withCipher);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            if (asObject) {
                pubnub.Subscribe<object> (channel, this.DisplayReturnMessage, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            } else {
                pubnub.Subscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            }

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            CommonIntergrationTests commonPublish = new CommonIntergrationTests ();
            commonPublish.DeliveryStatus = false;
            commonPublish.Response = null;
            commonPublish.Name = string.Format ("{0} Pubs", testName);

            UnityEngine.Debug.Log (string.Format ("{0}: {1} Publishing", DateTime.Now.ToString (), testName));
            if (asObject) {
                pubnub.Publish<object> (channel, message, true, commonPublish.DisplayReturnMessage, commonPublish.DisplayReturnMessage);
            } else {
                pubnub.Publish<string> (channel, message, true, commonPublish.DisplayReturnMessage, commonPublish.DisplayReturnMessage);
            }

            UnityEngine.Debug.Log (string.Format ("{0}: {1} Waiting for response", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            UnityEngine.Debug.Log (string.Format ("{0}: {1} After wait", DateTime.Now.ToString (), testName));

            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool passed = false;
                if (asObject) {
                    UnityEngine.Debug.Log (string.Format ("{0}: Response: {1}", DateTime.Now.ToString (), this.Response));
                    IList<object> fields = this.Response as IList<object>;
                    #if USE_MiniJSON
                    //mini json converts int to int64
                    if (fields != null) {
                        if(message.GetType().Equals(typeof(Int32))){
                            if (message.ToString().Equals (message.ToString())) {
                                passed = true;
                            }
                        } else {
                            if (message.Equals (message)) {
                                passed = true;
                            }
                        }
                    }
                    #else
                    if ((fields != null) && (message.Equals (fields [0]))) {
                        UnityEngine.Debug.Log (string.Format ("{0}: Response Type: {1} ", DateTime.Now.ToString (), fields [0].GetType ()));
                        passed = true;
                    }
                    #endif
                } else {
                    UnityEngine.Debug.Log (string.Format ("{0}: Response: {1} Expected : {2}", DateTime.Now.ToString (), this.Response, expectedStringResponse));

                    if (matchExpectedStringResponse) {
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} message: {2}, response: {3}", DateTime.Now.ToString (), testName, message, Response));
                        #if USE_MiniJSON
                        if((message.GetType().Equals(typeof(Int32))) || (message.GetType().Equals(typeof(Int64)))){
                            if (this.Response.ToString ().Contains (message.ToString())) {
                                passed = true;
                            }
                        } else if (message.GetType().Equals(typeof(Int64[]))){
                            Int64[] message1 = (Int64[])message;
                            foreach (Int64 int64 in message1){
                                if (this.Response.ToString ().Contains (int64.ToString())) {
                                    passed = true;
                                    break;
                                }
                            }
                        } else {
                            if (this.Response.ToString ().Contains (expectedStringResponse)) {
                                passed = true;
                            }
                        }
                        #else
                        if (this.Response.ToString ().Contains (expectedStringResponse)) {
                            passed = true;
                        }
                        #endif
                    } else {
                        UnityEngine.Debug.Log (string.Format ("{0}: !matchExpectedStringResponse {1}, expected : {2}", DateTime.Now.ToString (), this.Response, expectedStringResponse));
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} message: {2}, response: {3}", DateTime.Now.ToString (), testName, message, Response));
                        object[] deserializedMessage = Deserialize<object[]> (this.Response.ToString ());
                        if ((deserializedMessage != null) && (deserializedMessage.Length > 0)) {
                            foreach (var obj in deserializedMessage) {
                                UnityEngine.Debug.Log (string.Format ("{0}: obj: {1}", DateTime.Now.ToString (), obj.ToString ()));
                            }
                            UnityEngine.Debug.Log (string.Format ("{0}: deserializedMessage [0]: {1} {2}", DateTime.Now.ToString (), deserializedMessage [0], message));
                            UnityEngine.Debug.Log (string.Format ("{0}: deserializedMessage [0]: {1} {2}", DateTime.Now.ToString (), deserializedMessage [0].GetType(), message.GetType()));
                            #if USE_MiniJSON
                            //mini json converts int to int64
                            if(message.GetType().Equals(typeof(Int32))){
                                if (deserializedMessage [0].ToString().Equals (message.ToString())) {
                                    passed = true;
                                }
                            } else {
                                if (deserializedMessage [0].Equals (message)) {
                                    passed = true;
                                }
                            }
                            #else

                            if (deserializedMessage [0].Equals (message)) {
                                passed = true;
                            }
                            #endif
                        } else {
                            UnityEngine.Debug.Log (string.Format ("{0}: deserializedMessage null ", DateTime.Now.ToString ()));
                        }
                        //if (this.Response.ToString ().Contains (message.ToString())) {
                        //    passed = true;
                        //}
                        //object[] deserializedMessage = Deserialize<object[]> (this.Response.ToString ());
                        /*IList deserializedMessage = this.Response as IList;
                        UnityEngine.Debug.Log (string.Format ("{0}: !matchExpectedStringResponse {1}, expected : {2}", DateTime.Now.ToString (), this.Response, expectedStringResponse));
                        if ((deserializedMessage != null) && (message.Equals (deserializedMessage [0]))) {
                            UnityEngine.Debug.Log (string.Format ("{0}: Response Type: {1} ", DateTime.Now.ToString ()));
                            passed = true;
                        }*/
                    }
                }

                if (passed) {
                    IntegrationTest.Pass ();
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: Not found in {1}", testName, this.Response.ToString ())); 
                }
            }
            pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        public IEnumerator DoTimeAndParse (bool ssl, string testName, bool asObject)
        {
            Init (testName, ssl);
            this.DeliveryStatus = false;
            this.Response = null;

            if (asObject) {
                pubnub.Time<object> (this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.Time<string> (this.DisplayReturnMessage, this.DisplayErrorMessage);
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                UnityEngine.Debug.Log (string.Format ("{0}: {1} this.Response {2}", DateTime.Now.ToString (), testName, this.Response));
                bool passed = false;
                if (asObject) {
                    IList<object> fields = this.Response as IList<object>;

                    UnityEngine.Debug.Log (string.Format ("{0}: {1} Time", DateTime.Now.ToString (), fields [0].ToString ()));
                    if (!Convert.ToInt64 (fields [0].ToString ()).Equals (0)) {
                        passed = true;
                    }
                } else {
                    if (!this.Response.ToString ().Equals ("")
                        && !(this.Response.ToString ().Equals ("[]"))
                        && !(this.Response.ToString ().Equals ("[0]"))
                        && !(this.Response.ToString ().Equals ("0"))) {
                        passed = true;
                    }
                }
                if (passed) {
                    IntegrationTest.Pass ();
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: Not found in {1}", testName, this.Response.ToString ())); 
                }
            }
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        public IEnumerator DoPublishThenDetailedHistoryAndParse (bool ssl, string testName, object[] messages, bool asObject, bool withCipher, bool noStore, int numberOfMessages, bool isParamsTest)
        {

            string channel = Init (testName, ssl, withCipher);

            CommonIntergrationTests commonPublish = new CommonIntergrationTests ();
            commonPublish.DeliveryStatus = false;
            commonPublish.Response = null;
            commonPublish.Name = string.Format ("{0} Pubs", testName);

            UnityEngine.Debug.Log (string.Format ("{0}: {1} Publishing", DateTime.Now.ToString (), testName));

            bool storeInHistory = true;
            if (noStore) {
                storeInHistory = false;
            }

            //string starttime = GetTimeFromServerUsingNewPubNub(testName, ssl);
            this.DeliveryStatus = false;
            this.Response = null;

            GetTimeFromServerUsingNewPubNub (testName, ssl);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            UnityEngine.Debug.Log (string.Format ("{0}: {1} this.Response {2}", DateTime.Now.ToString (), testName, this.Response));
            IList<object> fields = this.Response as IList<object>;

            UnityEngine.Debug.Log (string.Format ("{0}: {1} Time", DateTime.Now.ToString (), fields [0].ToString ()));
            long starttime = Convert.ToInt64 (fields [0].ToString ());

            long midtime = starttime;
            int count = 0;
            UnityEngine.Debug.Log (string.Format ("{0}: {1} numberOfMessages: {2}", DateTime.Now.ToString (), testName, numberOfMessages));
            if (asObject) {
                foreach (object message in messages) {
                    pubnub.Publish<object> (channel, message, storeInHistory, commonPublish.DisplayReturnMessage, commonPublish.DisplayReturnMessage);
                    count++;
                    if (numberOfMessages / 2 == count) {
                        this.DeliveryStatus = false;
                        this.Response = null;
                        yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
                        GetTimeFromServerUsingNewPubNub (testName, ssl);
                        yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
                        fields = this.Response as IList<object>;
                        midtime = Convert.ToInt64 (fields [0].ToString ());
                    }

                    yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
                }
            } else {
                foreach (object message in messages) {
                    pubnub.Publish<string> (channel, message, storeInHistory, commonPublish.DisplayReturnMessage, commonPublish.DisplayReturnMessage);
                    count++;
                    if (numberOfMessages / 2 == count) {
                        this.DeliveryStatus = false;
                        this.Response = null;
                        yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
                        GetTimeFromServerUsingNewPubNub (testName, ssl);
                        yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
                        fields = this.Response as IList<object>;
                        midtime = Convert.ToInt64 (fields [0].ToString ());
                    }

                    yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow); 
                }
            }
            UnityEngine.Debug.Log (string.Format ("{0}: {1} After Publish", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            if (isParamsTest) {
                UnityEngine.Debug.Log (string.Format ("{0}: {1} starttime: {2}, midtime {3}", DateTime.Now.ToString (), testName, starttime, midtime));
                if (asObject) {
                    pubnub.DetailedHistory<object> (channel, starttime, midtime, numberOfMessages / 2, true, this.DisplayReturnMessage, this.DisplayReturnMessageDummy);
                } else {
                    pubnub.DetailedHistory<string> (channel, starttime, midtime, numberOfMessages / 2, true, this.DisplayReturnMessage, this.DisplayReturnMessageDummy);
                }
            } else {
                if (asObject) {
                    pubnub.DetailedHistory<object> (channel, numberOfMessages, this.DisplayReturnMessage, this.DisplayReturnMessageDummy);
                } else {
                    pubnub.DetailedHistory<string> (channel, numberOfMessages, this.DisplayReturnMessage, this.DisplayReturnMessageDummy);
                }
            }
            UnityEngine.Debug.Log (string.Format ("{0}: {1} Waiting for response", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            UnityEngine.Debug.Log (string.Format ("{0}: {1} After wait", DateTime.Now.ToString (), testName));

            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool passed = false;
                if (isParamsTest) {
                    passed = ParseDetailedHistoryResponse (messages, testName, asObject, 0, numberOfMessages / 2);

                    if (passed) {
                        this.DeliveryStatus = false;
                        this.Response = null;

                        if (asObject) {
                            UnityEngine.Debug.Log (string.Format ("{0}: {1} midtime: {2}", DateTime.Now.ToString (), testName, midtime));
                            pubnub.DetailedHistory<object> (channel, midtime, -1, numberOfMessages / 2, true, this.DisplayReturnMessage, this.DisplayReturnMessageDummy);
                        } else {
                            pubnub.DetailedHistory<string> (channel, midtime, -1, numberOfMessages / 2, true, this.DisplayReturnMessage, this.DisplayReturnMessageDummy);
                        }
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} Waiting for response2 ", DateTime.Now.ToString (), testName));
                        yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} After wait2 ", DateTime.Now.ToString (), testName));

                        if (this.Response == null) {
                            IntegrationTest.Fail (string.Format ("{0}: Null response 2", testName)); 
                        } else {
                            passed = ParseDetailedHistoryResponse (messages, testName, asObject, numberOfMessages / 2, numberOfMessages);
                        }
                    } else {
                        IntegrationTest.Fail (string.Format ("{0}: failed one", testName)); 
                    }
                } else {

                    UnityEngine.Debug.Log (string.Format ("{0}: {1} Response {2}", DateTime.Now.ToString (), testName, this.Response.ToString ()));

                    if (noStore) {
                        passed = ParseResponseNoStore (messages, testName);
                    } else {
                        passed = ParseDetailedHistoryResponse (messages, testName, asObject, 0, numberOfMessages);
                    }
                }
                if (passed) {
                    IntegrationTest.Pass ();
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: Not found in {1}", testName, this.Response.ToString ())); 
                }
            }
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        public bool ParseDetailedHistoryResponse (object[] messages, string testName, bool asObject, int messageStart, int messageEnd)
        {
            if (asObject) {
                IList<object> fields = this.Response as IList<object>;

                UnityEngine.Debug.Log (string.Format ("{0}: {1} fields.Count {2}", DateTime.Now.ToString (), testName, fields.Count));
                if (fields [0] != null) {
                    return ParseFields (fields, messages, testName, messageStart, messageEnd);
                } else {
                    return false;
                }
            } else {
                /*for (messageStart; messageStart <= messageEnd; messageStart++) {
                    if (this.Response.ToString ().Contains (message)) {
                    }
                }*/
                var found = false;
                var count = 0;
                string resp = this.Response.ToString ();
                //foreach (var message in messages) {
                for (int i = messageStart; i < messageEnd; i++) {
                    if (resp.Contains (messages [i].ToString ())) {
                        found = true;
                    } else {
                        found = false;
                    }
                    count++;
                }
                if (found) {
                    //IntegrationTest.Pass ();
                    return true;
                } else {
                    //IntegrationTest.Fail (string.Format("{0}: Not found in {1}", testName, this.Response.ToString ())); 
                    return false;
                }
            }
        }

        public bool ParseFields (IList<object> fields, object[] messages, string testName, int messageStart, int messageEnd)
        {
            string response = "";
            UnityEngine.Debug.Log (string.Format ("{0}: {1} messageStart: {2}, messageEnd: {3}", DateTime.Now.ToString (), testName, messageStart, messageEnd));
            var myObjectArray = (from item in fields
                                 select item as object).ToArray ();
            IList<object> enumerable = myObjectArray [0] as IList<object>;
            bool found = false;

            if ((enumerable != null) && (enumerable.Count > 0)) {
                //foreach (var message in messages) {
                //int count = 0;
                //foreach (var message in messages) {
                for (int i = messageStart; i < messageEnd; i++) {
                    /*if (count <= messageStart-1) {
                        continue;
                    }*/
                    var message = messages [i]; 
                    bool mfound = false;
                    foreach (object element in enumerable) {    
                        response = element.ToString ();
                        if (message.ToString ().Equals (response)) {
                            UnityEngine.Debug.Log (string.Format ("{0}: {1} message: {2}, response: {3}", DateTime.Now.ToString (), testName, message, response));
                            mfound = true;
                            break;
                        }
                    }
                    if (!mfound) {
                        UnityEngine.Debug.Log (string.Format ("{0}: {1} message: {2}", DateTime.Now.ToString (), testName, message));
                        found = false;
                        break;
                    } else {
                        found = true;
                    }
                    /*count++;
                    if (count >= messageEnd) {
                        break;
                    }*/
                    /*if (messageStart != messageEnd) {
                        Console.WriteLine (String.Format ("response :{0} :: j: {1}", response, j));
                        if (j < messageEnd) {
                            if(j.ToString ().Equals(response)){
                                found = true;
                                break;
                            }
                        }
                        j++;
                    } else if (!message.Equals ("")) {

                        Console.WriteLine ("Response:" + response);
                        if(message.Equals(response)){
                            found = true;
                            break;
                        }
                    } else {
                        Console.WriteLine ("Response:" + response);
                        //Assert.IsNotEmpty (response);
                        if(!string.IsNullOrEmpty(response)){
                            found = true;
                            break;
                        }
                    }*/
                }
                if (found) {
                    //IntegrationTest.Pass ();
                    return true;
                } else {
                    //IntegrationTest.Fail (string.Format("{0}: Not found in {1}", testName, this.Response.ToString ())); 
                    return false;
                }
            } else {
                //IntegrationTest.Fail (string.Format("{0}: {1}", testName, this.Response.ToString ())); 
                return false;
            }
        }

        public bool ParseResponseNoStore (object[] messages, string testName)
        {
            bool found = false;
            string resp = this.Response.ToString ();
            foreach (var message in messages) { 
                if (resp.Contains (message.ToString ())) {
                    found = true;
                } else {
                    found = false;
                }
            }
            if (!found) {
                //IntegrationTest.Pass (); 
                return true;
            } else {
                //IntegrationTest.Fail (string.Format ("{0}: {1}", testName, this.Response.ToString ())); 
                return false;
            }

        }

        /*public void ParseDetailedHistoryResponse (int messageStart, int messageEnd, string message, string testName, bool asObject)
        {
            if (asObject) {
                IList<object> fields = this.Response as IList<object>;

                UnityEngine.Debug.Log (string.Format ("{0}: {1} fields.Count {2}", DateTime.Now.ToString (), testName, fields.Count));
                if (fields [0] != null) {
                    ParseFields (fields, messageStart, messageEnd, message, testName);
                }
            } else {
                /*for (messageStart; messageStart <= messageEnd; messageStart++) {
                    if (this.Response.ToString ().Contains (message)) {
                    }
                }*/
        /*if (this.Response.ToString ().Contains (message)) {
                    IntegrationTest.Pass ();
                } else {
                    IntegrationTest.Fail (string.Format("{0}: Not found in {1}", testName, this.Response.ToString ())); 
                }
            }
        }

        public void ParseFields (IList<object> fields, int messageStart, int messageEnd, string message, string testName)
        {
            string response = "";

            var myObjectArray = (from item in fields
                select item as object).ToArray ();
            IList<object> enumerable = myObjectArray [0] as IList<object>;
            bool found = false;
            if ((enumerable != null) && (enumerable.Count > 0)) {
                int j = messageStart;
                foreach (object element in enumerable) {
                    response = element.ToString ();
                    if (messageStart != messageEnd) {
                        Console.WriteLine (String.Format ("response :{0} :: j: {1}", response, j));
                        if (j < messageEnd) {
                            if(j.ToString ().Equals(response)){
                                found = true;
                                break;
                            }
                        }
                        j++;
                    } else if (!message.Equals ("")) {
                        Console.WriteLine ("Response:" + response);
                        if(message.Equals(response)){
                            found = true;
                            break;
                        }
                    } else {
                        Console.WriteLine ("Response:" + response);
                        //Assert.IsNotEmpty (response);
                        if(!string.IsNullOrEmpty(response)){
                            found = true;
                            break;
                        }
                    }
                }
                if (found) {
                    IntegrationTest.Pass ();
                } else {
                    IntegrationTest.Fail (string.Format("{0}: Not found in {1}", testName, this.Response.ToString ())); 
                }
            } else {
                IntegrationTest.Fail (string.Format("{0}: {1}", testName, this.Response.ToString ())); 
            }
        }

        public void ParseResponseNoStore (string message, string testName)
        {
            if (!this.Response.ToString ().Contains (message)) {
                IntegrationTest.Pass (); 
            } else {
                IntegrationTest.Fail (string.Format("{0}: {1}", testName, this.Response.ToString ())); 
            }
        }*/


        public IEnumerator DoPublishAndParse (bool ssl, string testName, object message, string expected, bool asObject)
        {
            string channel = Init (testName, ssl);

            if (asObject) {
                pubnub.Publish<object> (channel, message, this.DisplayReturnMessage, this.DisplayReturnMessage);
            } else {
                pubnub.Publish<string> (channel, message, this.DisplayReturnMessage, this.DisplayReturnMessage);
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            ParsePublishResponse (testName, expected, asObject);
        }

        public IEnumerator DoPublishAndParse (bool ssl, string testName, object message, string expected, bool asObject, bool withCipher, bool noSecretKey)
        {
            string channel = "";
            if (noSecretKey) {
                channel = Init (testName, ssl, withCipher, true);
            } else {
                channel = Init (testName, ssl, withCipher);
            }

            if (asObject) {
                pubnub.Publish<object> (channel, message, this.DisplayReturnMessage, this.DisplayReturnMessage);
            } else {
                pubnub.Publish<string> (channel, message, this.DisplayReturnMessage, this.DisplayReturnMessage);
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            ParsePublishResponse (testName, expected, asObject);

        }

        public void ParsePublishResponse (string testName, string expected, bool asObject)
        {
            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool found = false;
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Response {2}", DateTime.Now.ToString (), testName, this.Response.ToString ()));
                if (asObject) {
                    IList<object> fields = this.Response as IList<object>;
                    string sent = fields [1].ToString ();
                    //string one = fields [0].ToString ();
                    if (sent.Equals (expected)) {
                        found = true;
                    }
                } else {
                    if (this.Response.ToString ().Contains (expected)) {
                        found = true;
                    }
                }

                if (found) {
                    IntegrationTest.Pass (); 
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: {1}", testName, this.Response.ToString ())); 
                }
            }

            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        public IEnumerator DoPublishAndParse (bool ssl, string testName, object message, string expected, bool asObject, bool withCipher)
        {
            string channel = Init (testName, ssl, withCipher);

            if (asObject) {
                pubnub.Publish<object> (channel, message, this.DisplayReturnMessage, this.DisplayReturnMessage);
            } else {
                pubnub.Publish<string> (channel, message, this.DisplayReturnMessage, this.DisplayReturnMessage);
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            ParsePublishResponse (testName, expected, asObject);    
        }

        private void SubscribeUsingSeparateCommon (string channel, string testName)
        {
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Running Subscribe ", DateTime.Now.ToString (), testName));
            CommonIntergrationTests commonSubscribe = new CommonIntergrationTests ();
            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;
            commonSubscribe.Name = string.Format ("{0} Subs", testName);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            try {
                pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            } catch (Exception ex) {
                UnityEngine.Debug.Log (string.Format ("{0} {1}: exception ", ex.ToString (), testName));
            }
        }

        private void SetState (string channel, string testName, string state)
        {
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Running Set State ", DateTime.Now.ToString (), testName));
            CommonIntergrationTests commonState = new CommonIntergrationTests ();
            commonState.DeliveryStatus = false;
            commonState.Response = null;
            commonState.Name = string.Format ("{0} State", testName);

            pubnub.SetUserState<string> (channel, state, commonState.DisplayReturnMessage, commonState.DisplayErrorMessage);
        }

        public IEnumerator SetAndDeleteStateAndParse (bool ssl, string testName)
        {
            string channel = Init (testName, ssl);

            KeyValuePair<string, object> kvp = new KeyValuePair<string, object> ("k", "v");

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Running Set State ", DateTime.Now.ToString (), testName));
            CommonIntergrationTests commonState = new CommonIntergrationTests ();
            commonState.DeliveryStatus = false;
            commonState.Response = null;
            commonState.Name = string.Format ("{0} State", testName);
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Set State k1 ", DateTime.Now.ToString (), testName));
            
            pubnub.SetUserState<string> (channel, kvp, commonState.DisplayReturnMessage, commonState.DisplayErrorMessage);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

            KeyValuePair<string, object> kvp2 = new KeyValuePair<string, object> ("k2", "v2");
            commonState.DeliveryStatus = false;
            commonState.Response = null;

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Set State k2 ", DateTime.Now.ToString (), testName));            
            pubnub.SetUserState<string> (channel, kvp2, commonState.DisplayReturnMessage, commonState.DisplayErrorMessage);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            pubnub.GetUserState<string> (channel, this.DisplayReturnMessage, this.DisplayErrorMessage);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Set State 3 null ", DateTime.Now.ToString (), testName));
            pubnub.SetUserState<string> (channel, new KeyValuePair<string, object> ("k2", null), commonState.DisplayReturnMessage, commonState.DisplayErrorMessage);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            pubnub.GetUserState<string> (channel, this.DisplayReturnMessage, this.DisplayErrorMessage);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool found = false;
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Response {2}", DateTime.Now.ToString (), testName, this.Response.ToString ()));
                if (this.Response.ToString ().Contains ("{\"k\":\"v\"}")) {
                    found = true;
                }
                if (found) {
                    IntegrationTest.Pass (); 
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: {1}", testName, this.Response.ToString ())); 
                }
            }

            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        public IEnumerator SetAndGetStateAndParse (bool ssl, string testName)
        {
            string channel = Init (testName, ssl);

            string state = "{\"testkey\":\"testval\"}";
            SetState (channel, testName, state);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            pubnub.GetUserState<string> (channel, this.DisplayReturnMessage, this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool found = false;
                UnityEngine.Debug.Log (string.Format ("{0}: {1} Response {2}", DateTime.Now.ToString (), testName, this.Response.ToString ()));
                if (this.Response.ToString ().Contains (state)) {
                    found = true;
                }
                if (found) {
                    IntegrationTest.Pass (); 
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: {1}", testName, this.Response.ToString ())); 
                }
            }

            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        public IEnumerator DoSubscribeThenDoGlobalHereNowAndParse (bool ssl, string testName, bool parseAsString)
        {
            string channel = Init (testName, ssl);

            SubscribeUsingSeparateCommon (channel, testName);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            if (parseAsString) {
                pubnub.GlobalHereNow<string> (true, true, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.GlobalHereNow<object> (true, true, this.DisplayReturnMessage, this.DisplayErrorMessage);
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool found = false;
                if (parseAsString) {
                    if (this.Response.ToString ().Contains (pubnub.SessionUUID)
                        && this.Response.ToString ().Contains (channel)) {
                        found = true;
                    }
                } else {
                    //TODO: refactor
                    IList responseFields = this.Response as IList;
                    UnityEngine.Debug.Log (string.Format ("{0}: responseFields: {1}", testName, responseFields.ToString ())); 
                    if (responseFields.Count >= 1) {
                        var item = responseFields [0];
                        UnityEngine.Debug.Log (string.Format ("{0}: item: {1}", testName, item.ToString ())); 
                        if (item is Dictionary<string, object>) {
                            Dictionary<string, object> message = (Dictionary<string, object>)item;
                            foreach (KeyValuePair<string, object> k in message) {
                                //UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, k.Value));
                                if (k.Key.Equals ("payload")) {
                                    //UnityEngine.Debug.Log (string.Format ("in objs:{0} {1}", k.Key, k.Value));
                                    Dictionary<string, object> message2 = (Dictionary<string, object>)k.Value;
                                    //UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, message2 ["channels"]));
                                    Dictionary<string, object> message3 = (Dictionary<string, object>)message2 ["channels"];
                                    Dictionary<string, object> message4 = (Dictionary<string, object>)message3 [channel];
                                    if (message4 != null) {
                                        foreach (KeyValuePair<string, object> k2 in message4) {
                                            UnityEngine.Debug.Log (string.Format ("objs2:{0} {1}", k2.Key, k2.Value));
                                            if (k2.Key.Equals ("uuids")) {
                                                UnityEngine.Debug.Log (string.Format ("in objs2:{0} {1}", k2.Key, k2.Value));
                                                #if USE_MiniJSON
                                                IList message5 = k2.Value as IList;
                                                UnityEngine.Debug.Log (string.Format ("in message5:{0}", message5));
                                                foreach (object k4 in message5) {
                                                    //UnityEngine.Debug.Log (string.Format ("objs3:{0} {1}", k3.Key, k3.Value));
                                                    IList<object> message6 = this.Response as IList<object>;
                                                    UnityEngine.Debug.Log (string.Format ("in message6:{0}", message6));
                                                    foreach (object k3 in message6) {
                                                        UnityEngine.Debug.Log (string.Format ("objs3:{0} {1}", k3.ToString (), pubnub.SessionUUID));
                                                        Dictionary<string, object> message7 = (Dictionary<string, object>)k3;
                                                        foreach (KeyValuePair<string, object> k5 in message7) {
                                                            //UnityEngine.Debug.Log (string.Format ("objs5:{0} {1}", k5.Key, k5.Value));
                                                            if (k5.Key.Equals ("payload")) {
                                                                //UnityEngine.Debug.Log (string.Format ("objs7:{0} {1}", k5.Key, k5.Value));
                                                                Dictionary<string, object> message8 = (Dictionary<string, object>)k5.Value;
                                                                Dictionary<string, object> message9 = (Dictionary<string, object>)message8 ["channels"];
                                                                foreach (KeyValuePair<string, object> k6 in message9) {
                                                                    if (k6.Key.Equals (channel)) {
                                                                        //UnityEngine.Debug.Log (string.Format ("objs8:{0} {1}", k6.Key, k6.Value));
                                                                        Dictionary<string, object> message10 = (Dictionary<string, object>)k6.Value;
                                                                        if (message10.ContainsKey ("uuids")) {
                                                                            //UnityEngine.Debug.Log (string.Format ("objs9:{0}", message10 ["uuids"]));
                                                                            IList<object> message11 = message10 ["uuids"] as IList<object>;
                                                                            foreach (object k7 in message11) {
                                                                                //UnityEngine.Debug.Log (string.Format ("objs7:{0} {1}", k7, pubnub.SessionUUID));
                                                                                Dictionary<string, object> message12 = (Dictionary<string, object>)k7;
                                                                                foreach (KeyValuePair<string, object> k8 in message12) {
                                                                                    UnityEngine.Debug.Log (string.Format ("objs10:{0} {1} {2}", k8.Key, k8.Value, pubnub.SessionUUID));
                                                                                    if (k8.Value.ToString ().Equals (pubnub.SessionUUID)) {
                                                                                        found = true;
                                                                                        break;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                #else
                                                Dictionary<string, object>[] message5 = (Dictionary<string, object>[])k2.Value;
                                                var arr = message5 [0] as Dictionary<string, object>;
                                                foreach (KeyValuePair<string, object> k3 in arr) {
                                                    UnityEngine.Debug.Log (string.Format ("objs3:{0} {1}", k3.Key, k3.Value));
                                                    if (k3.Value.Equals (pubnub.SessionUUID)) {
                                                        found = true;
                                                        break;
                                                    }
                                                }
                                                #endif
                                            }
                                        }
                                    } else {
                                        UnityEngine.Debug.Log ("msg4 null");
                                    }
                                }
                            }
                        }
                    }
                }
                if (found) {
                    IntegrationTest.Pass (); 
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: {1}", testName, this.Response.ToString ())); 
                }
            }

            pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }
    


        public IEnumerator DoSubscribeThenDoWhereNowAndParse (bool ssl, string testName, bool parseAsString)
        {
            string channel = Init (testName, ssl);

            SubscribeUsingSeparateCommon (channel, testName);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            if (parseAsString) {
                pubnub.WhereNow<string> (pubnub.SessionUUID, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.WhereNow<object> (pubnub.SessionUUID, this.DisplayReturnMessage, this.DisplayErrorMessage);
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool found = false;
                if (parseAsString) {
                    //if (this.Response.ToString ().Contains (pubnub.SessionUUID)
                        //&& this.Response.ToString ().Contains (channel)) {
                    if(this.Response.ToString ().Contains (channel)) {
                        found = true;
                    }
                } else {
                    //TODO: refactor
                    IList responseFields = this.Response as IList;
                    UnityEngine.Debug.Log (string.Format ("{0}: responseFields: {1}", testName, responseFields.ToString ())); 
                    if (responseFields.Count >= 2) {
                        var item = responseFields [0];
                        UnityEngine.Debug.Log (string.Format ("{0}: item: {1}", testName, item.ToString ())); 
                        if (item is Dictionary<string, object>) {
                            Dictionary<string, object> message = (Dictionary<string, object>)item;
                            foreach (KeyValuePair<string, object> k in message) {
                                UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, k.Value));
                                if (k.Key.Equals ("payload")) {
                                    UnityEngine.Debug.Log (string.Format ("in objs:{0} {1}", k.Key, k.Value));
                                    Dictionary<string, object> message2 = (Dictionary<string, object>)k.Value;
                                    
                                    #if USE_MiniJSON
                                    if (message2.ContainsKey ("channels")) {
                                        UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, message2 ["channels"]));
                                        IList<object> message11 = message2 ["channels"] as IList<object>;

                                        foreach (object k7 in message11) {
                                            UnityEngine.Debug.Log (string.Format ("objs7:{0}", k7));
                                            if(k7.Equals(channel)){
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                    #else
                                    UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, message2 ["channels"]));
                                    var arr = message2 ["channels"] as string[];
                                    foreach (string ch in arr) {
                                        if (ch.Equals (channel)) {
                                            found = true;
                                            break;
                                        }
                                    }
                                    #endif
                                }
                            }
                        }
                    }
                }
                if (found) {
                    IntegrationTest.Pass (); 
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: {1}", testName, this.Response.ToString ())); 
                }
            }

            pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        /*public IEnumerator DoSubscribeThenHereNowAsStringAndParse (bool ssl, string testName, bool parseAsString)
        {
            this.DeliveryStatus = false;
            this.TimedOut = false;
            this.Response = null;
            this.Name = testName;

            System.Random r = new System.Random ();
            string channel = "hello_world_hn" + r.Next (100);

            pubnub = new Pubnub (CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey,
                CommonIntergrationTests.SecretKey, 
                "", 
                ssl);

            UnityEngine.Debug.Log (string.Format("{0} {1}: Running Subscribe ", DateTime.Now.ToString (), testName));
            CommonIntergrationTests commonSubscribe = new CommonIntergrationTests ();
            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;
            commonSubscribe.Name = string.Format("{0} Subs", testName);

            UnityEngine.Debug.Log (string.Format("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            try {
                pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            } catch (Exception ex) {
                UnityEngine.Debug.Log (string.Format("{0} {1}: exception ", ex.ToString (), testName));
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            UnityEngine.Debug.Log (string.Format("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            pubnub.HereNow<string> (channel, this.DisplayReturnMessage, this.DisplayErrorMessage);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

            UnityEngine.Debug.Log (string.Format("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.Response == null) {
                IntegrationTest.Fail (string.Format("{0}: Null response", testName)); 
            } else {
                if (this.Response.ToString().Contains(pubnub.SessionUUID)) {
                    IntegrationTest.Pass (); 
                } else {
                    IntegrationTest.Fail (string.Format("{0}: {1}", testName, this.Response.ToString ())); 
                }
            }

            pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            pubnub.EndPendingRequests ();
        }*/

        public IEnumerator DoSubscribeThenHereNowAndParse (bool ssl, string testName, bool parseAsString, bool doWithState, string customUUID)
        {
            string channel = Init (testName, ssl);

            string matchUUID = pubnub.SessionUUID;
            if (!customUUID.Equals ("")) {
                pubnub.SessionUUID = customUUID;
                matchUUID = customUUID;
            }

            SubscribeUsingSeparateCommon (channel, testName);

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            string state = "{\"testkey\":\"testval\"}";
            if (doWithState) {
                SetState (channel, testName, state);
                yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
                pubnub.HereNow<string> (channel, true, true, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else if (parseAsString) {
                pubnub.HereNow<string> (channel, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.HereNow<object> (channel, this.DisplayReturnMessage, this.DisplayErrorMessage);
            }

            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                bool found = false;
                if (doWithState) {
                    if (this.Response.ToString ().Contains (matchUUID) && this.Response.ToString ().Contains (state)) {
                        found = true;
                    } 
                } else if (parseAsString) {
                    if (this.Response.ToString ().Contains (matchUUID)) {
                        found = true;
                    } 
                } else {

                    IList responseFields = this.Response as IList;

                    UnityEngine.Debug.Log (string.Format ("{0}: responseFields: {1}", testName, responseFields.ToString ())); 
                    foreach (object item in responseFields) {
                        UnityEngine.Debug.Log (string.Format ("{0}: item: {1}", testName, item.ToString ())); 
                        if (item is Dictionary<string, object>) {
                            Dictionary<string, object> message = (Dictionary<string, object>)item;
                            #if (USE_MiniJSON)
                            foreach (KeyValuePair<string, object> k in message) {
                                UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, k.Value));
                                if (k.Key.Equals ("uuids")) {
                                    found = ParseDict (matchUUID, k.Value);
                                }
                            }
                            #else
                            if (message.ContainsKey ("uuids")) {
                                object uuids = message ["uuids"];
                                found = ParseDict (matchUUID, uuids);
                            }
                            #endif
                        }
                    }
                }
                if (found) {
                    IntegrationTest.Pass (); 
                } else {
                    IntegrationTest.Fail (string.Format ("{0}: {1}", testName, this.Response.ToString ())); 
                }
                /*                                foreach (object item in responseFields) {
                                        response = item.ToString ();
                                        Console.WriteLine ("Response:" + response);
                                        Assert.NotNull (response);
                                }
                                Dictionary<string, object> message = (Dictionary<string, object>)responseFields [0];
                                foreach (KeyValuePair<String, object> entry in message) {
                                        Console.WriteLine ("value:" + entry.Value + "  " + "key:" + entry.Key);
                                }*/

                /*object[] objUuid = (object[])message["uuids"];
                foreach (object obj in objUuid)
                {
                    Console.WriteLine(obj.ToString()); 
                }*/
                //Assert.AreNotEqual(0, message["occupancy"]);
            }

            pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        bool ParseDict (string matchUUID, object uuids)
        {
            object[] objUuid = null;
            UnityEngine.Debug.Log ("uuids:" + uuids);
            Type valueType = uuids.GetType ();
            var expectedType = typeof(string[]);
            var expectedType2 = typeof(object[]);

            if (expectedType.IsAssignableFrom (valueType)) {
                objUuid = uuids as string[];
            } else if (expectedType2.IsAssignableFrom (valueType)) {
                objUuid = uuids as object[];
            } else if (uuids is IList && uuids.GetType ().IsGenericType) {
                objUuid = ((IEnumerable)uuids).Cast<object> ().ToArray ();
            } else {
                objUuid = CommonIntergrationTests.Deserialize<object[]> (uuids.ToString ());
            }
            foreach (object obj in objUuid) {
                UnityEngine.Debug.Log ("session:" + obj.ToString ()); 
                if (obj.Equals (matchUUID)) {
                    return true;
                }
            }
            return false;
        }

        public static T Deserialize<T> (string message)
        {
            object retMessage;
            #if (USE_JSONFX) || (USE_JSONFX_UNITY)
            var reader = new JsonFx.Json.JsonReader ();
            retMessage = reader.Read<T> (message);
            #elif (USE_JSONFX_UNITY_IOS)
            UnityEngine.Debug.Log ("message: " + message);
            retMessage = JsonReader.Deserialize<T> (message);
            #elif (USE_MiniJSON)
            UnityEngine.Debug.Log ("message: " + message);
            object retMessage1 = Json.Deserialize (message) as object;
            Type type = typeof(T);
            var expectedType2 = typeof(object[]);
            if (expectedType2.IsAssignableFrom (type)) {
                retMessage = ((System.Collections.IEnumerable)retMessage1).Cast<object> ().ToArray ();
            } else {
                retMessage    = retMessage1;
            }
            #else
            retMessage = JsonConvert.DeserializeObject<T> (message);
            #endif
            return (T)retMessage;
        }

        public IEnumerator DoSubscribe (Pubnub pn, string channel, string testName)
        {
            CommonIntergrationTests commonSubscribe = new CommonIntergrationTests ();
            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;
            commonSubscribe.Name = string.Format ("{0} Subs", testName);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            try {
                pn.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            } catch (Exception ex) {
                UnityEngine.Debug.Log (string.Format ("{0} {1}: exception ", ex.ToString (), testName));
            }

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
        }

        public IEnumerator DoConnectedTest (bool ssl, string testName, bool asObject, bool isPresence)
        {
            string channel = Init (testName, ssl);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            if (asObject) {
                if (isPresence) {
                    pubnub.Presence<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            } else {
                if (isPresence) {
                    pubnub.Presence<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            }
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                var found = false;
                UnityEngine.Debug.Log (string.Format ("{0}: Response: {1}", testName, this.Response));
                if (this.Response.ToString ().Contains ("Connected")) {
                    found = true;
                }

                if (found) {
                    UnityEngine.Debug.Log (string.Format ("{0}: Pass", testName));
                    IntegrationTest.Pass (); 
                } else {
                    UnityEngine.Debug.Log (string.Format ("{0}: Fail", testName));    
                    IntegrationTest.Fail (string.Format ("{0}: Channel not found", testName)); 
                }
            }
            if (isPresence) {
                pubnub.PresenceUnsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            }
        }

        public IEnumerator DoNotSubscribedTest (bool ssl, string testName, bool asObject, bool isPresence)
        {
            string channel = Init (testName, ssl);

            if (asObject) {
                if (isPresence) {
                    pubnub.PresenceUnsubscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                } else {
                    pubnub.Unsubscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                }
            } else {
                if (isPresence) {
                    pubnub.PresenceUnsubscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                } else {
                    pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                }
            }

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.ErrorResponse == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                var found = false;
                UnityEngine.Debug.Log (string.Format ("{0}: Response: {1}", testName, this.ErrorResponse));
                if (isPresence) {
                    if (this.ErrorResponse.ToString ().Contains ("Channel not subscribed for presence")) {
                        found = true;
                    }
                } else {
                    if (this.ErrorResponse.ToString ().Contains ("Channel not subscribed")) {
                        found = true;
                    }
                }

                if (found) {
                    UnityEngine.Debug.Log (string.Format ("{0}: Pass", testName));
                    IntegrationTest.Pass (); 
                } else {
                    UnityEngine.Debug.Log (string.Format ("{0}: Fail", testName));    
                    IntegrationTest.Fail (string.Format ("{0}: Channel not found", testName)); 
                }
            }
            if (isPresence) {
                pubnub.PresenceUnsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            }
        }

        public IEnumerator DoUnsubscribeTest (bool ssl, string testName, bool asObject, bool isPresence)
        {
            string channel = Init (testName, ssl);

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            if (asObject) {
                if (isPresence) {
                    pubnub.Presence<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                }
            } else {
                if (isPresence) {
                    pubnub.Presence<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
                }
            }
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
            this.DeliveryStatus = false;
            this.TimedOut = false;
            this.ErrorResponse = null;

            if (asObject) {
                if (isPresence) {
                    pubnub.PresenceUnsubscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Unsubscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            } else {
                if (isPresence) {
                    pubnub.PresenceUnsubscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            }

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.Response == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                var found = false;
                UnityEngine.Debug.Log (string.Format ("{0}: Response: {1}", testName, this.Response));
                if (isPresence) {
                    if (this.Response.ToString ().Contains ("Presence Unsubscribed")) {
                        found = true;
                    }
                } else {
                    if (this.Response.ToString ().Contains ("Unsubscribed")) {
                        found = true;
                    }
                }

                if (found) {
                    UnityEngine.Debug.Log (string.Format ("{0}: Pass", testName));
                    IntegrationTest.Pass (); 
                } else {
                    UnityEngine.Debug.Log (string.Format ("{0}: Fail", testName));    
                    IntegrationTest.Fail (string.Format ("{0}: Channel not found", testName)); 
                }
            }
            if (isPresence) {
                pubnub.PresenceUnsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            }
        }

        public IEnumerator DoAlreadySubscribeTest (bool ssl, string testName, bool asObject, bool isPresence)
        {
            string channel = Init (testName, ssl);
            channel = "hw_1";
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            if (asObject) {
                if (isPresence) {
                    pubnub.Presence<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            } else {
                if (isPresence) {
                    pubnub.Presence<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            }
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
            this.DeliveryStatus = false;
            this.TimedOut = false;
            this.ErrorResponse = null;

            if (asObject) {
                if (isPresence) {
                    pubnub.Presence<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            } else {
                if (isPresence) {
                    pubnub.Presence<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                } else {
                    pubnub.Subscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
                }
            }

            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.ErrorResponse == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                var found = false;
                UnityEngine.Debug.Log (string.Format ("{0}: Response: {1}", testName, this.ErrorResponse));
                if (isPresence) {
                    if (this.ErrorResponse.ToString ().Contains ("Already Presence-Subscribed")) {
                        found = true;
                    }
                } else {
                    if (this.ErrorResponse.ToString ().Contains ("Already Subscribed")) {
                        found = true;
                    }
                }

                if (found) {
                    UnityEngine.Debug.Log (string.Format ("{0}: Pass", testName));
                    IntegrationTest.Pass (); 
                } else {
                    UnityEngine.Debug.Log (string.Format ("{0}: Fail", testName));    
                    IntegrationTest.Fail (string.Format ("{0}: Channel not found", testName)); 
                }
            }
            if (isPresence) {
                pubnub.PresenceUnsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } else {
                pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            }
        }

        private void SubscribeCall (string channel, string testName)
        {
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Running Subscribe ", DateTime.Now.ToString (), testName));
            try {
                //pubnub.Subscribe<string> (channel, this.DisplayReturnMessage, this.OnSubConnectedUnsub, this.DisplayErrorMessage);
                pubnub.Subscribe<string> (channel, this.DisplayReturnMessage, this.DisplayReturnMessage, this.DisplayErrorMessage);
            } catch (Exception ex) {
                UnityEngine.Debug.Log (string.Format ("{0} {1}: exception ", ex.ToString (), testName));
            }
        }

        public IEnumerator DoPresenceThenSubscribeAndParse (bool ssl, string testName, bool asObject)
        {
            string channel = Init (testName, ssl);
            ResponseString = "";
            SubChannel = channel;
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Start coroutine ", DateTime.Now.ToString (), testName));
            /*if (asObject) {
                pubnub.Presence<object> (channel, this.DisplayReturnMessagePresence, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            } else {*/
            pubnub.Presence<string> (channel, this.DisplayReturnMessagePresence, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            //}
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            SubscribeCall (channel, testName);
            /*CommonIntergrationTests commonPublish = new CommonIntergrationTests ();
            commonPublish.DeliveryStatus = false;
            commonPublish.Response = null;
            commonPublish.Name = string.Format ("{0} Pubs", testName);*/

            /*UnityEngine.Debug.Log (string.Format ("{0} {1}: Running Subscribe ", DateTime.Now.ToString (), testName));
            //try {
            if (asObject) {
                pubnub.Subscribe<object> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            } else {
                pubnub.Subscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            }*/

            //while (!DeliveryStatus) {
            UnityEngine.Debug.Log (string.Format ("{0} {1}: Waiting ", DateTime.Now.ToString (), testName));
            //yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 
            //pubnub.Unsubscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
            //}
            /*else {
                UnityEngine.Debug.Log (string.Format ("{0} {1}:DeliveryStatus ", DateTime.Now.ToString (), testName));
            }*/
            //pubnub.Subscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayErrorMessage);
            //} catch (Exception ex) {
            //    UnityEngine.Debug.Log (string.Format ("{0} {1}: exception ", ex.ToString (), testName));
            //}
            
            /*CommonIntergrationTests commonPublish = new CommonIntergrationTests ();
            commonPublish.DeliveryStatus = false;
            commonPublish.Response = null;
            commonPublish.Name = string.Format ("{0} Pubs", testName);

            UnityEngine.Debug.Log (string.Format ("{0}: {1} Publishing", DateTime.Now.ToString (), testName));
            if (asObject) {
                pubnub.Publish<object> (channel, "test", true, commonPublish.DisplayReturnMessage, commonPublish.DisplayReturnMessage);
            } else {
                pubnub.Publish<string> (channel, "test", true, commonPublish.DisplayReturnMessage, commonPublish.DisplayReturnMessage);
            }*/

            
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls); 

            UnityEngine.Debug.Log (string.Format ("{0} {1}: After Wait ", DateTime.Now.ToString (), testName));
            if (this.ResponseString == null) {
                IntegrationTest.Fail (string.Format ("{0}: Null response", testName)); 
            } else {
                UnityEngine.Debug.Log (string.Format ("{0}: Response: {1}", testName, this.ResponseString));
                var found = false;
                if (asObject) {
                    IList responseFields = this.Response as IList;
                    UnityEngine.Debug.Log (string.Format ("{0}: responseFields: {1}", testName, responseFields.ToString ())); 
                    if (responseFields.Count >= 2) {
                        var item = responseFields [0];
                        UnityEngine.Debug.Log (string.Format ("{0}: item: {1}", testName, item.ToString ())); 
                        if (item is Dictionary<string, object>) {
                            Dictionary<string, object> message = (Dictionary<string, object>)item;
                            foreach (KeyValuePair<string, object> k in message) {
                                UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, k.Value));
                            }
                        }
                    }
                } else {
                    
                    object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject (this.ResponseString.ToString ()).ToArray ();
                    UnityEngine.Debug.Log (string.Format ("{0}: serializedMessage [2].ToString (): {1}", testName, serializedMessage [2].ToString ())); 
                    UnityEngine.Debug.Log (string.Format ("{0}: serializedMessage [0].ToString (): {1}", testName, serializedMessage [0].ToString ())); 
                    if (channel.Equals (serializedMessage [2])) {
                        found = true;    
                    }
                }
                if (found) {
                    UnityEngine.Debug.Log (string.Format ("{0}: Pass", testName));
                    IntegrationTest.Pass (); 
                } else {
                    UnityEngine.Debug.Log (string.Format ("{0}: Fail", testName));    
                    IntegrationTest.Fail (string.Format ("{0}: Channel not found", testName)); 
                }

            }

            pubnub.PresenceUnsubscribe<string> (channel, this.DisplayReturnMessageDummy, this.DisplayReturnMessageDummy, this.DisplayReturnMessage, this.DisplayErrorMessage);
            pubnub.EndPendingRequests ();
            pubnub.CleanUp();
        }

        string ExpectedMessage = "";
        string ExpectedChannels = "";
        bool IsError = false;
        bool IsTimeout = false;
        CurrentRequestType Crt;
        ResponseType RespType;

        public void TestCoroutineRun (string url, int timeout, int pause, string[] channels,
            bool resumeOnReconnect,bool ssl, string testName, string expectedMessage, string expectedChannels,
            bool isError, bool isTimeout, bool asObject, long timetoken, CurrentRequestType crt, ResponseType respType
        ){
            ExpectedMessage = expectedMessage;
            ExpectedChannels = string.Join (",", channels);
            IsError = isError;
            IsTimeout = isTimeout;
            Crt = crt;
            RespType = respType;

            if (asObject) {
                TestRun<object> (url, timeout, pause, channels, false, RespType, Crt, 
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,

                    isTimeout, isError, timetoken);
            } else {
                TestRun<string> (url, timeout, pause, channels, false, RespType, Crt,
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,
                    isTimeout, isError, timetoken);

            }
        }

        public void TestRun<T>(string url, int timeout, int pause, string[] channels, bool resumeOnReconnect, 
            ResponseType respType, CurrentRequestType crt, 
            Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback,
            bool isTimeout, bool isError, long timetoken
        ){
            List<ChannelEntity> channelEntities = Helpers.CreateChannelEntity<T>(channels, 
                    true, false, null, userCallback, connectCallback, errorCallback, null, null);  

            RequestState<T> pubnubRequestState = BuildRequests.BuildRequestState<T> (channelEntities, respType, 
                resumeOnReconnect, 0, isTimeout, timetoken, typeof(T), "", userCallback, 
                errorCallback);

            GameObject go = new GameObject ("PubnubIntegrationTestCoroutine");
            CoroutineClass cc = go.AddComponent<CoroutineClass> ();

            if(crt.Equals(CurrentRequestType.Subscribe)){
                cc.SubCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                cc.NonSubCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                cc.PresenceHeartbeatCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                cc.HeartbeatCoroutineComplete += CcCoroutineComplete<T>;
            }
            cc.Run<T>(url, pubnubRequestState, timeout, pause);
            
            DateTime dt = DateTime.Now;
            while (dt.AddSeconds(6) > DateTime.Now) {
                //UnityEngine.Debug.Log ("waiting");
            }
            cc.CheckComplete (crt);
            /*bool failTest = false;
            if(crt.Equals(CurrentRequestType.Subscribe)){
                if (!cc.isSubscribeComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                if (!cc.isNonSubscribeComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                if (!cc.isPresenceHeartbeatComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                if (!cc.isHearbeatComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            }      
            if (failTest) {
                IntegrationTest.Fail ("Coroutine running");
            }*/
        }

        public IEnumerator TestCoroutineRunProcessResponse (string url, int timeout, int pause, string[] channels,
            bool resumeOnReconnect,bool ssl, string testName, string expectedMessage, string expectedChannels,
            bool isError, bool isTimeout, bool asObject, long timetoken, CurrentRequestType crt, ResponseType respType
        ){
            UnityEngine.Debug.Log (string.Format ("url: {0}", url));
            WWW www = new WWW (url);
            yield return www;
            ExpectedMessage = expectedMessage;
            ExpectedChannels = string.Join (",", channels);
            IsError = isError;
            IsTimeout = isTimeout;
            Crt = crt;
            RespType = respType;

            if (asObject) {
                TestProcessResponse<object> (www, url, timeout, pause, channels, false, RespType, Crt, 
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,

                    isTimeout, isError, timetoken);
            } else {
                TestProcessResponse<string> (www, url, timeout, pause, channels, false, RespType, Crt,
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,
                    isTimeout, isError, timetoken);
            }
        }

        public void TestProcessResponse<T>(WWW www, string url, int timeout, int pause, string[] channels, bool resumeOnReconnect, 
            ResponseType respType, CurrentRequestType crt, 
            Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback,
            bool isTimeout, bool isError, long timetoken
        ){
            List<ChannelEntity> channelEntities = Helpers.CreateChannelEntity<T>(channels, 
                true, false, null, userCallback, connectCallback, errorCallback, null, null);  

            RequestState<T> pubnubRequestState = BuildRequests.BuildRequestState<T> (channelEntities, respType, 
                resumeOnReconnect, 0, isTimeout, timetoken, typeof(T), "", userCallback, 
                errorCallback);

            CoroutineParams<T> cp = new CoroutineParams<T> (url, timeout, pause, crt, typeof(T), pubnubRequestState);
            GameObject go = new GameObject ("PubnubIntegrationTestCoroutine");
            CoroutineClass cc = go.AddComponent<CoroutineClass> ();

            if(crt.Equals(CurrentRequestType.Subscribe)){
                cc.SubCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                cc.NonSubCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                cc.PresenceHeartbeatCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                cc.HeartbeatCoroutineComplete += CcCoroutineComplete<T>;
            }
            cc.ProcessResponse(www, cp);

        }

        void ErrorCallbackCommonExceptionHandler (PubnubClientError result)
        {
            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
        }

        void CcCoroutineComplete<T> (object sender, EventArgs ea)
        {
            CustomEventArgs<T> cea = ea as CustomEventArgs<T>;
            if (cea != null && cea.PubnubRequestState != null){
            
                UnityEngine.Debug.Log (string.Format("ExpectedChannels: {0}, " +
                    "IsError: {1}, IsTimeout: {2}, Crt: {3}, RespType: {4}, ExpectedMessage: {5}", 
                    ExpectedChannels, IsError, IsTimeout, Crt, RespType, ExpectedMessage));


                UnityEngine.Debug.Log (string.Format("cea.PubnubRequestState.Channels: {0}, " +
                    "cea.IsError: {1}, cea.IsTimeout: {2}, cea.Crt: {3}, cea.RespType: {4}, cea.Message: {5}", 
                    Helpers.GetNamesFromChannelEntities(cea.PubnubRequestState.ChannelEntities, false), cea.IsError, cea.IsTimeout, cea.CurrRequestType
                    ,cea.PubnubRequestState.RespType, cea.Message));

                if(Helpers.GetNamesFromChannelEntities(cea.PubnubRequestState.ChannelEntities, false).Equals (ExpectedChannels)
                    && cea.IsError.Equals (IsError)
                    && cea.IsTimeout.Equals (IsTimeout)
                    && cea.CurrRequestType.Equals (Crt)
                    && cea.PubnubRequestState.RespType.Equals (RespType)
                    && cea.Message.Contains(ExpectedMessage)
                ){
                    IntegrationTest.Pass();
                } else {
                    IntegrationTest.Fail ("Cea value not matching");
                }
            } else {
                IntegrationTest.Fail (string.Format("cea {0}, cea req state ?", (cea==null)?"null":"not null"));
            }
        }

        /*public IEnumerator TestProcessTimeout<T>(string url, int timeout, int pause, string[] channels, bool resumeOnReconnect, 
            ResponseType respType, CurrentRequestType crt,             
            bool isTimeout, bool isError, long timetoken, string expectedMessage
        ){
            UnityEngine.Debug.Log (string.Format ("url: {0}", url));
            WWW www = new WWW (url);
            yield return www;

            ExpectedMessage = expectedMessage;
            ExpectedChannels = string.Join (",", channels);
            IsError = isError;
            IsTimeout = isTimeout;
            Crt = crt;
            RespType = respType;

            RequestState<T> pubnubRequestState = BuildRequests.BuildRequestState<T> (channels, respType, 
                resumeOnReconnect, UserCallbackCommonExceptionHandler, 
                ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler, 0, isTimeout, timetoken, typeof(T));
            CoroutineParams<T> cp = new CoroutineParams<T> (url, timeout, pause, crt, typeof(T), pubnubRequestState);
            CoroutineClass cc = new CoroutineClass ();
            if(crt.Equals(CurrentRequestType.Subscribe)){
                cc.SubCoroutineComplete += CcCoroutineCompleteError<T>;
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                cc.NonSubCoroutineComplete += CcCoroutineCompleteError<T>;
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                cc.PresenceHeartbeatCoroutineComplete += CcCoroutineCompleteError<T>;
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                cc.HeartbeatCoroutineComplete += CcCoroutineCompleteError<T>;
            }
            cc.ProcessTimeout (cp);

        }*/

        public IEnumerator TestCoroutineRunError (string url, int timeout, int pause, string[] channels,
            bool resumeOnReconnect,bool ssl, string testName, string expectedMessage, string expectedChannels,
            bool isError, bool isTimeout, bool asObject, long timetoken, CurrentRequestType crt, ResponseType respType
        ){
            UnityEngine.Debug.Log (string.Format ("url: {0}", url));
            WWW www = new WWW (url);
            yield return www;
            ExpectedMessage = expectedMessage;
            ExpectedChannels = string.Join (",", channels);
            IsError = isError;
            IsTimeout = isTimeout;
            Crt = crt;
            RespType = respType;

            if (asObject) {
                TestProcessResponseError<object> (www, url, timeout, pause, channels, false, RespType, Crt, 
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,

                    isTimeout, isError, timetoken);
            } else {
                TestProcessResponseError<string> (www, url, timeout, pause, channels, false, RespType, Crt,
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,
                    isTimeout, isError, timetoken);

            }
        }

        public void TestProcessResponseError<T>(WWW www, string url, int timeout, int pause, string[] channels, bool resumeOnReconnect, 
            ResponseType respType, CurrentRequestType crt, 
            Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback,
            bool isTimeout, bool isError, long timetoken
        ){
            List<ChannelEntity> channelEntities = Helpers.CreateChannelEntity<T>(channels, 
               true, false, null, userCallback, connectCallback, errorCallback, null, null);  

            RequestState<T> pubnubRequestState = BuildRequests.BuildRequestState<T> (channelEntities, respType, 
                resumeOnReconnect, 0, isTimeout, timetoken, typeof(T), "", userCallback, 
                errorCallback);

            CoroutineParams<T> cp = new CoroutineParams<T> (url, timeout, pause, crt, typeof(T), pubnubRequestState);

            GameObject go = new GameObject ("PubnubIntegrationTestCoroutine");
            CoroutineClass cc = go.AddComponent<CoroutineClass> ();

            if (!isTimeout) {
                if (crt.Equals (CurrentRequestType.Subscribe)) {
                    cc.SubCoroutineComplete += CcCoroutineCompleteError<T>;
                } else if (crt.Equals (CurrentRequestType.NonSubscribe)) {
                    cc.NonSubCoroutineComplete += CcCoroutineCompleteError<T>;
                } else if (crt.Equals (CurrentRequestType.PresenceHeartbeat)) {
                    cc.PresenceHeartbeatCoroutineComplete += CcCoroutineCompleteError<T>;
                } else if (crt.Equals (CurrentRequestType.Heartbeat)) {
                    cc.HeartbeatCoroutineComplete += CcCoroutineCompleteError<T>;
                }
            }
            cc.ProcessResponse(www, cp);

        }

        void ErrorCallbackCommonExceptionHandlerError (PubnubClientError result)
        {
            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
        }

        void CcCoroutineCompleteError<T> (object sender, EventArgs ea)
        {
            CustomEventArgs<T> cea = ea as CustomEventArgs<T>;
            if (cea != null && cea.PubnubRequestState != null){
                /*UnityEngine.Debug.Log (":"+ExpectedChannels);
                UnityEngine.Debug.Log ("IsError:"+IsError);
                UnityEngine.Debug.Log ("IsTimeout:"+IsTimeout);
                UnityEngine.Debug.Log ("Crt:"+Crt);
                UnityEngine.Debug.Log ("RespType:"+RespType);
                UnityEngine.Debug.Log ("ExpectedMessage:"+ExpectedMessage);

                UnityEngine.Debug.Log ("cea.PubnubRequestState.Channels:" + Helpers.GetNamesFromChannelEntities(cea.PubnubRequestState.ChannelEntities, false));
                UnityEngine.Debug.Log ("cea.IsError:"+cea.IsError);
                UnityEngine.Debug.Log ("cea.IsTimeout:"+cea.IsTimeout);
                UnityEngine.Debug.Log ("cea.CurrRequestType:"+cea.CurrRequestType);
                UnityEngine.Debug.Log ("cea.PubnubRequestState.RespType:"+cea.PubnubRequestState.RespType);
                UnityEngine.Debug.Log ("cea.Message:"+cea.Message);*/
                UnityEngine.Debug.Log (string.Format("ExpectedChannels: {0}, " +
                "IsError: {1}, IsTimeout: {2}, Crt: {3}, RespType: {4}, ExpectedMessage: {5}", 
                ExpectedChannels, IsError, IsTimeout, Crt, RespType, ExpectedMessage));

                UnityEngine.Debug.Log (string.Format("cea.PubnubRequestState.Channels: {0}, " +
                "cea.IsError: {1}, cea.IsTimeout: {2}, cea.Crt: {3}, cea.RespType: {4}, cea.Message: {5}", 
                Helpers.GetNamesFromChannelEntities(cea.PubnubRequestState.ChannelEntities, false), cea.IsError, cea.IsTimeout, cea.CurrRequestType
                ,cea.PubnubRequestState.RespType, cea.Message));

                
                if(Helpers.GetNamesFromChannelEntities(cea.PubnubRequestState.ChannelEntities, false).Equals (ExpectedChannels)
                    && cea.IsError.Equals (IsError)
                    && cea.IsTimeout.Equals (IsTimeout)
                    && cea.CurrRequestType.Equals (Crt)
                    && cea.PubnubRequestState.RespType.Equals (RespType)
                    && cea.Message.Contains(ExpectedMessage)
                ){
                    IntegrationTest.Pass();
                } else {
                    IntegrationTest.Fail ("Cea value not matching" + cea.Message.Contains(ExpectedMessage));
                }
            } else {
                IntegrationTest.Fail (string.Format("cea {0}, cea req state ?", (cea==null)?"null":"not null"));
            }
        }

        void UserCallbackCommonExceptionHandler (string result)
        {
            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
        }

        void UserCallbackCommonExceptionHandler (object result)
        {
            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result.ToString()));
        }

        void DisconnectCallbackCommonExceptionHandler (string result)
        {
            UnityEngine.Debug.Log (string.Format ("Disconnect CALLBACK LOG: {0}", result));
        }

        void ConnectCallbackCommonExceptionHandler (string result)
        {
            UnityEngine.Debug.Log (string.Format ("CONNECT CALLBACK LOG: {0}", result));
        }

        void ConnectCallbackCommonExceptionHandler (object result)
        {
            UnityEngine.Debug.Log (string.Format ("CONNECT CALLBACK LOG: {0}", result.ToString()));
        }

        public void TestCoroutineBounce (string url, int timeout, int pause, string[] channels,
            bool resumeOnReconnect,bool ssl, string testName, string expectedMessage, string expectedChannels,
            bool isError, bool isTimeout, bool asObject, long timetoken, CurrentRequestType crt, ResponseType respType
        ){
            ExpectedMessage = expectedMessage;
            ExpectedChannels = string.Join (",", channels);
            IsError = isError;
            IsTimeout = isTimeout;
            Crt = crt;
            RespType = respType;

            if (asObject) {
                TestBounce<object> (url, timeout, pause, channels, false, RespType, Crt, 
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,

                    isTimeout, isError, timetoken);
            } else {
                TestBounce<string> (url, timeout, pause, channels, false, RespType, Crt,
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,
                    isTimeout, isError, timetoken);

            }
        }

        public void TestBounce<T>(string url, int timeout, int pause, string[] channels, bool resumeOnReconnect, 
            ResponseType respType, CurrentRequestType crt, 
            Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback,
            bool isTimeout, bool isError, long timetoken
        ){
            List<ChannelEntity> channelEntities = Helpers.CreateChannelEntity<T>(channels, 
                true, false, null, userCallback, connectCallback, errorCallback, null, null);  

            RequestState<T> pubnubRequestState = BuildRequests.BuildRequestState<T> (channelEntities, respType, 
                resumeOnReconnect, 0, isTimeout, timetoken, typeof(T), "", userCallback, 
                errorCallback);

            GameObject go = new GameObject ("PubnubIntegrationTestCoroutine");
            CoroutineClass cc = go.AddComponent<CoroutineClass> ();

            if(crt.Equals(CurrentRequestType.Subscribe)){
                cc.SubCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                cc.NonSubCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                cc.PresenceHeartbeatCoroutineComplete += CcCoroutineComplete<T>;
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                cc.HeartbeatCoroutineComplete += CcCoroutineComplete<T>;
            }
            cc.Run<T>(url, pubnubRequestState, timeout, pause);
            DateTime dt = DateTime.Now;
            /*while (dt.AddSeconds (6) > DateTime.Now) {
            }*/
            cc.BounceRequest<T>(crt, pubnubRequestState, true);
            UnityEngine.Debug.Log ("Bouncing request");
            
            /*while (dt.AddSeconds(6) > DateTime.Now) {
                //UnityEngine.Debug.Log ("waiting");
            }*/
            cc.CheckComplete (crt);
            UnityEngine.Debug.Log ("After check Complete");
            bool failTest = false;
            if(crt.Equals(CurrentRequestType.Subscribe)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isSubscribeComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone)  && !cc.isNonSubscribeComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isPresenceHeartbeatComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isHearbeatComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            }            
            if (failTest) {
                UnityEngine.Debug.Log ("www not null or done");
                IntegrationTest.Fail ("www not null and done");
            }
        }

        #if(REDUCE_PUBNUB_COROUTINES)
        public IEnumerator TestCoroutineRunSubscribeMultiple(string url, string url2, int timeout, int pause, string[] channels,
            bool resumeOnReconnect,bool ssl, string testName, string expectedMessage, string expectedChannels,
            bool isError, bool isTimeout, bool asObject, long timetoken, CurrentRequestType crt, ResponseType respType
        ){
            ExpectedMessage = expectedMessage;
            ExpectedChannels = string.Join (",", channels);
            IsError = isError;
            IsTimeout = isTimeout;
            Crt = crt;
            RespType = respType;

            List<ChannelEntity> channelEntities = Helpers.CreateChannelEntity<string>(channels, 
                true, false, null, UserCallbackCommonExceptionHandler, ConnectCallbackCommonExceptionHandler, 
                ErrorCallbackCommonExceptionHandler, null, null);  

            RequestState<string> pubnubRequestState = BuildRequests.BuildRequestState<string> (channelEntities, respType, 
                resumeOnReconnect, 0, isTimeout, timetoken, (asObject)?typeof(object):typeof(string), "", UserCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler);

            GameObject go = new GameObject ("PubnubIntegrationTestCoroutine");
            CoroutineClass cc = go.AddComponent<CoroutineClass> ();
            
            //cc.SubCoroutineComplete += CcCoroutineComplete<string>;
            cc.Run<string>(url, pubnubRequestState, timeout, pause);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);
            cc.Run<string>(url2, pubnubRequestState, timeout, pause);

            DateTime dt = DateTime.Now;
            //cc.BounceRequest<string>(crt, pubnubRequestState, false);

            //cc.CheckComplete (crt);
            bool failTest = false;
            if(crt.Equals(CurrentRequestType.Subscribe)){
                if(cc.subscribeTimer.Equals(0)){
                    failTest = true;
                    UnityEngine.Debug.Log ("subscribeTimer 0");
                } else if(!cc.runSubscribeTimer){
                    failTest = true;
                    UnityEngine.Debug.Log ("runSubscribeTimer false");
                } else {
                    UnityEngine.Debug.Log ("www not null or done");
                }
            } 
                       
            if (failTest) {
                IntegrationTest.Fail ("www not null and done");
            } else {
                IntegrationTest.Pass ();
            }
        }

        public void TestCoroutineRunSubscribeAbort (string url, string url2, int timeout, int pause, string[] channels,
            bool resumeOnReconnect,bool ssl, string testName, string expectedMessage, string expectedChannels,
            bool isError, bool isTimeout, bool asObject, long timetoken, CurrentRequestType crt, ResponseType respType
        ){
            ExpectedMessage = expectedMessage;
            ExpectedChannels = string.Join (",", channels);
            IsError = isError;
            IsTimeout = isTimeout;
            Crt = crt;
            RespType = respType;

            if (asObject) {
                TestSubscribeAbort<object> (url, timeout, pause, channels, false, RespType, Crt, 
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,

                    isTimeout, isError, timetoken);
            } else {
                TestSubscribeAbort<string> (url, timeout, pause, channels, false, RespType, Crt,
                    UserCallbackCommonExceptionHandler, 
                    ConnectCallbackCommonExceptionHandler, ErrorCallbackCommonExceptionHandler,
                    isTimeout, isError, timetoken);
            }
        }

        public void TestSubscribeAbort<T>(string url, int timeout, int pause, string[] channels, bool resumeOnReconnect, 
                ResponseType respType, CurrentRequestType crt, 
                Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback,
                bool isTimeout, bool isError, long timetoken
        ){
            List<ChannelEntity> channelEntities = Helpers.CreateChannelEntity<T>(channels, 
                true, false, null, userCallback, 
                connectCallback, errorCallback, null, null);  

            RequestState<T> pubnubRequestState = BuildRequests.BuildRequestState<T> (channelEntities, respType, 
                resumeOnReconnect, 0, isTimeout, timetoken, typeof(T), "", userCallback, 
                errorCallback);

            GameObject go = new GameObject ("PubnubIntegrationTestCoroutine");
            CoroutineClass cc = go.AddComponent<CoroutineClass> ();

            cc.Run<T>(url, pubnubRequestState, timeout, pause);
            DateTime dt = DateTime.Now;
            cc.BounceRequest<T>(crt, pubnubRequestState, false);

            //cc.CheckComplete (crt);
            bool failTest = false;
            if(crt.Equals(CurrentRequestType.Subscribe)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isSubscribeComplete) {
                    failTest = true;
                } else if(cc.subscribeTimer != 0){
                    failTest = true;
                    UnityEngine.Debug.Log ("subscribeTimer not 0");

                } else if(cc.runSubscribeTimer){
                    failTest = true;
                    UnityEngine.Debug.Log ("runSubscribeTimer true");
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone)  && !cc.isNonSubscribeComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isPresenceHeartbeatComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isHearbeatComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            }            
            if (failTest) {
                IntegrationTest.Fail ("www not null and done");
            } else {
                IntegrationTest.Pass ();
            }
        }

        public IEnumerator TestSubscribeMultiple<T>(string url, string url2, int timeout, int pause, string[] channels, bool resumeOnReconnect, 
            ResponseType respType, CurrentRequestType crt, 
            Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback,
            bool isTimeout, bool isError, long timetoken
        ){
            List<ChannelEntity> channelEntities = Helpers.CreateChannelEntity<T>(channels, 
                true, false, null, userCallback, 
                connectCallback, errorCallback, null, null);  

            RequestState<T> pubnubRequestState = BuildRequests.BuildRequestState<T> (channelEntities, respType, 
                resumeOnReconnect, 0, isTimeout, timetoken, typeof(T), "", userCallback, 
                errorCallback);

            GameObject go = new GameObject ("PubnubIntegrationTestCoroutine");
            CoroutineClass cc = go.AddComponent<CoroutineClass> ();

            //check runSubscribeTimer
            //check subscribeTimer
            //subCompleteOrTimeoutEvent
            //subscribeWww is done

            //cc.SubCompleteOrTimeoutEvent += CcCoroutineComplete2<T>;
            cc.Run<T>(url, pubnubRequestState, timeout, pause);
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCallsLow);
            cc.Run<T>(url2, pubnubRequestState, timeout, pause);

            DateTime dt = DateTime.Now;
            cc.BounceRequest<T>(crt, pubnubRequestState, false);

            //cc.CheckComplete (crt);
            bool failTest = false;
            if(crt.Equals(CurrentRequestType.Subscribe)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isSubscribeComplete) {
                    failTest = true;
                } else if(cc.subscribeTimer != 0){
                    failTest = true;
                    UnityEngine.Debug.Log ("subscribeTimer not 0");
                } else if(cc.runSubscribeTimer){
                    failTest = true;
                    UnityEngine.Debug.Log ("runSubscribeTimer true");
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.NonSubscribe)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone)  && !cc.isNonSubscribeComplete) {
                   failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.PresenceHeartbeat)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isPresenceHeartbeatComplete) {
                  failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            } else if (crt.Equals(CurrentRequestType.Heartbeat)){
                if ((cc.subscribeWww != null) && (!cc.subscribeWww.isDone) && !cc.isHearbeatComplete) {
                    failTest = true;
                } else {
                    UnityEngine.Debug.Log ("www null or done");
                }
            }            
            if (failTest) {
                IntegrationTest.Fail ("www not null and done");
            } 
        }

        /*void CcCoroutineComplete2<T> (object sender, EventArgs e)
        {
            //responseHandled = true;
            UnityEngine.Debug.Log ("Event handler fired");
            IntegrationTest.Pass();
        }*/
        #endif
    }
}