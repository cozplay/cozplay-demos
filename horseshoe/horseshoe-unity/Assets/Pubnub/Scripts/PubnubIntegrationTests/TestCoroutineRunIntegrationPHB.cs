using System;
using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class TestCoroutineRunIntegrationPHB: MonoBehaviour
    {
        public IEnumerator Start ()
        {
            CommonIntergrationTests common = new CommonIntergrationTests ();

            string[] multiChannel = {"testChannel"};

            Pubnub pubnub = new Pubnub (
                CommonIntergrationTests.PublishKey,
                CommonIntergrationTests.SubscribeKey,
                "",
                "",
                true
            );

            CurrentRequestType crt = CurrentRequestType.PresenceHeartbeat;
            string expectedMessage = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\"}";
            string expectedChannels = string.Join (",", multiChannel);
            string url = string.Format ("http://pubsub.pubnub.com/v2/presence/sub_key/{0}/channel/{1}/heartbeat?uuid={2}&heartbeat=62&pnsdk={3}", CommonIntergrationTests.SubscribeKey, 
                expectedChannels, pubnub.SessionUUID, pubnub.Version
            );
            ResponseType respType =  ResponseType.PresenceHeartbeat;

            IEnumerator ienum = common.TestCoroutineRunProcessResponse(url, 20, -1, multiChannel, false,
                false, this.name, expectedMessage, expectedChannels, false, false, false, 0, crt, respType);
            yield return StartCoroutine(ienum);

            UnityEngine.Debug.Log (string.Format("{0}: After StartCoroutine", this.name));
            yield return new WaitForSeconds (CommonIntergrationTests.WaitTimeBetweenCalls);
        }
    }
}

