using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class UtilityUnitTests
    {
        #if DEBUG  
        #if(UNITY_IOS)
        [Test]
        public void TestCheckTimeoutValue(){
            int v = Utility.CheckTimeoutValue(20);
            Assert.True (v.Equals (20));
        }

        [Test]
        public void TestCheckTimeoutValueGreaterThan60(){
            int v = Utility.CheckTimeoutValue(60);
            Assert.True (v.Equals (59));
        }
        #endif

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckPushTypeNone(){
            Utility.CheckPushType(PushTypeService.None);
        }

        [Test]
        public void TestCheckPushType(){
            Utility.CheckPushType(PushTypeService.APNS);
        }

        [Test]
        public void TestCheckAndAddNameSpaceEmpty(){
            List<string> ls = Utility.CheckAndAddNameSpace("");
            Assert.True(ls==null);
        }

        [Test]
        public void TestCheckAndAddNameSpace(){
            List<string> ls = Utility.CheckAndAddNameSpace("ns");
            Assert.True(ls.Contains("ns") && ls.Contains("namespace"));
        }

        [Test]
        public void TestCheckChannelGroupConvertToPres(){
            string s = Utility.CheckChannelGroup("cg", true);
            Assert.True(s.Contains("cg-pnpres"));
        }

        [Test]
        public void TestCheckChannelGroup(){
            string s = Utility.CheckChannelGroup("cg", false);
            Assert.True(s.Contains("cg") && !s.Contains("cg-pnpres"));
        }

        [Test]
        public void TestCheckChannelGroupConvertToPresMulti(){
            string s = Utility.CheckChannelGroup("cg, cg2", true);
            Assert.True(
                s.Contains("cg-pnpres")
                && s.Contains("cg2-pnpres")
            );
        }

        [Test]
        public void TestCheckChannelGroupMulti(){
            string s = Utility.CheckChannelGroup("cg, cg2", false);
            Assert.True(
                s.Contains("cg") 
                && !s.Contains("cg-pnpres")
                && s.Contains("cg2") 
                && !s.Contains("cg2-pnpres"));
        }

        [Test]
        [ExpectedException (typeof(MissingMemberException))]
        public void TestCheckChannelGroupMissingMemberException(){
            string s = Utility.CheckChannelGroup(",", false);
            Assert.True(
                s.Contains("cg") 
                && !s.Contains("cg-pnpres")
                && s.Contains("cg2") 
                && !s.Contains("cg2-pnpres"));
        }

        [Test]
        [ExpectedException (typeof(MissingMemberException))]
        public void TestCheckChannelGroupMissingMembeExcep(){
            string s = Utility.CheckChannelGroup("ch,  ", false);
            Assert.True(
                s.Contains("ch") 
                && !s.Contains("ch-pnpres")
                && s.Contains("ch2") 
                && !s.Contains("ch2-pnpres"));
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckChannelOrChannelGroupBothEmpty(){
            Utility.CheckChannelOrChannelGroup("", "");
        }

        [Test]
        public void TestCheckChannelOrChannelGroupCG(){
            Utility.CheckChannelOrChannelGroup("", "cg");
            //Assert.True(true);
        }

        [Test]
        public void TestCheckChannelOrChannelGroupCH(){
            Utility.CheckChannelOrChannelGroup("ch", "");
            Assert.True(true);
        }

        [Test]
        public void TestCheckChannelOrChannelGroupCHnCG(){
            Utility.CheckChannelOrChannelGroup("ch", "cg");
            Assert.True(true);
        }

        [Test]
        public void TestCheckKeyAndParseLong (){
            var dict = new Dictionary<string, object>(); 
            dict.Add("s", 2);
            long o = Utility.CheckKeyAndParseLong(dict, "seq", "s");
            Assert.True(o.Equals(2));
        }

        [Test]
        public void TestCheckKeyAndParseLongFalse (){
            var dict = new Dictionary<string, object>(); 
            dict.Add("s", "l");
            long o = Utility.CheckKeyAndParseLong(dict, "seq", "s");
            Assert.True(o.Equals(0));
        }

        [Test]
        public void TestValidateTimetoken (){
            long o = Utility.ValidateTimetoken("14685037252884276", false);
            Assert.True(o.Equals(14685037252884276));
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestValidateTimetokenRaiseError (){
            long o = Utility.ValidateTimetoken("a", true);
            Assert.True(o.Equals(0));
        }

        [Test]
        public void TestValidateTimetokenNoError (){
            long o = Utility.ValidateTimetoken("", false);
            Assert.True(o.Equals(0));
        }

        [Test]
        public void TestValidateTimetokenEmpty (){
            long o = Utility.ValidateTimetoken("", true);
            Assert.True(o.Equals(0));
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckChannelOrChannelGroupFalse ()
        {
            Utility.CheckChannelOrChannelGroup ("", "");
            Assert.True (true);
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckChannelFalse ()
        {
            Utility.CheckChannel ("");
            Assert.True (true);
        }

        [Test]
        public void TestCheckChannelOrChannelGroupTrueCHCG ()
        {
            Utility.CheckChannelOrChannelGroup ("ch", "cg");
            Assert.True (true);
        }

        [Test]
        public void TestCheckChannelOrChannelGroupTrueCH ()
        {
            Utility.CheckChannelOrChannelGroup ("ch", "");
            Assert.True (true);
        }

        [Test]
        public void TestCheckChannelOrChannelGroupTrueCG ()
        {
            Utility.CheckChannelOrChannelGroup ("", "cg");
            Assert.True (true);
        }

        [Test]
        public void TestCheckChannelsTrue ()
        {
            Utility.CheckChannels (new string[]{"ch"});
            Assert.True (true);
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckChannelsFalse ()
        {
            Utility.CheckChannels (new string[]{});
            Assert.True (true);
        }

        [Test]
        public void TestCheckChannelTrue ()
        {
            Utility.CheckChannel ("ch");
            Assert.True (true);
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckMessage ()
        {
            Utility.CheckMessage (null);
            Assert.True (true);
        }

        [Test]
        [ExpectedException (typeof(MissingMemberException))]
        public void TestCheckPublishKey ()
        {
            Utility.CheckPublishKey (null);
            Assert.True (true);
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckCallbackError ()
        {
            Utility.CheckCallback<PubnubClientError> (null, CallbackType.Error);
            Assert.True (true);
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckCallbackUser ()
        {
            Utility.CheckCallback<string> (null, CallbackType.Success);
            Assert.True (true);
        }

        [Test]
        public void TestCheckJSONPluggableLibrary ()
        {
            try{
                Utility.CheckJSONPluggableLibrary ();
                Assert.True (true);
            }catch{
                Assert.True (false);
            }
        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void TestCheckUserState ()
        {
            Utility.CheckUserState (null);
            Assert.True (true);
        }

        [Test]
        [ExpectedException (typeof(MissingMemberException))]
        public void TestCheckSecretKey ()
        {
            Utility.CheckSecretKey (null);
            Assert.True (true);
        }

        [Test]
        public void TestGenerateGuid ()
        {
            Assert.IsTrue(Utility.GenerateGuid ().ToString() != "");
        }

        [Test]
        public void CheckRequestTimeoutMessageInError ()
        {
            CustomEventArgs<string> cea = new CustomEventArgs<string> ();
            cea.CurrRequestType = CurrentRequestType.Subscribe;
            cea.IsError = true;
            cea.IsTimeout = false;
            cea.Message = "The request timed out.";
            cea.PubnubRequestState = null;
            Assert.IsTrue(Utility.CheckRequestTimeoutMessageInError<string> (cea));
        }

        [Test]
        public void TestIsPresenceChannelTrue ()
        {

            Assert.IsTrue(Utility.IsPresenceChannel ("my_channel-pnpres"));
        }

        [Test]
        public void TestIsPresenceChannelFalse ()
        {

            Assert.IsFalse(Utility.IsPresenceChannel ("my_channel"));
        }

        [Test]
        public void TestIsUnsafeWithComma ()
        {
            RunUnsafeTests (false);
        }

        [Test]
        public void TestIsUnsafe ()
        {
            RunUnsafeTests (true);
        }

        void RunUnsafeTests(bool ignoreComma)
        {
            char[] ch = {',', ' ','~','`','!','@','#','$','%','^','&','*','(',')','+','=','[',']','\\','{','}','|',';','\'',':','\"','/','<','>','?'};

            bool bPass = true;
            char currentChar = ' ';
            foreach (char c in ch) {
                currentChar = c;
                if (ignoreComma && c.Equals (',')) {
                    continue;
                }
                if (!Utility.IsUnsafe (c, ignoreComma)) {
                    bPass = false;
                    break;
                }
            }
            if (bPass) {
                Assert.True(bPass);
            } else {
                Assert.Fail(string.Format("failed for {0}", currentChar));
            }
        }

        [Test]
        public void TestEncodeUricomponent ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.SubscribeV2, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentDetailedHistoryIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.DetailedHistory, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentDetailedHistoryIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.DetailedHistory, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushGetIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushGet, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushGetIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushGet, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushRemoveIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushRemove, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushRemoveIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushRemove, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushRegisterIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushRegister, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushRegisterIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushRegister, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushUnregisterIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushUnregister, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPushUnregisterIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PushUnregister, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentHereNowIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";

            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.HereNow, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentHereNowIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.HereNow, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentLeaveIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.Leave, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentLeaveIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.Leave, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPresenceHeartbeatIgnorePercentFalse ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%252F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PresenceHeartbeat, true, false);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestEncodeUricomponentPresenceHeartbeatIgnorePercentTrue ()
        {
            //test unsafe surrogate and normal test
            string expected = "Text%20with%20\ud83d\ude1c%20emoji%20\ud83c\udf89.%20testencode%20%7E%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%5B%5D%5C%7B%7D%7C%3B%27%3A%22%2F%3C%3E%3F";
            string received = Utility.EncodeUricomponent("Text with 😜 emoji 🎉. testencode ~`!@#$%^&*()+=[]\\{}|;':\"/<>?", ResponseType.PresenceHeartbeat, true, true);
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestMd5 ()
        {
            //test unsafe surrogate and normal test
            string expected = "83a644046796c6a0d76bc161f73b75b4";
            string received = Utility.Md5("test md5");
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TestTranslateDateTimeToSeconds ()
        {
            //test unsafe surrogate and normal test
            long expected = 1449792000;
            long received = Utility.TranslateDateTimeToSeconds(DateTime.Parse("11 Dec 2015"));
            UnityEngine.Debug.Log (received);
            Assert.IsTrue(expected.Equals(received));
        }

        [Test]
        public void TranslateDateTimeToUnixTime ()
        {
            UnityEngine.Debug.Log ("Running TranslateDateTimeToUnixTime()");
            //Test for 26th June 2012 GMT
            DateTime dt = new DateTime (2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (dt);
            Assert.True ((13406688000000000).Equals (nanoSecondTime));
        }

        [Test]
        public void TranslateUnixTimeToDateTime ()
        {
            UnityEngine.Debug.Log ("Running TranslateUnixTimeToDateTime()");
            //Test for 26th June 2012 GMT
            DateTime expectedDate = new DateTime (2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateTime actualDate = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime (13406688000000000);
            Assert.True (expectedDate.ToString ().Equals (actualDate.ToString ()));
        }
        #endif
    }
}
