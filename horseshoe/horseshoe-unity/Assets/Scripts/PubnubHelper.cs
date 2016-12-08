using UnityEngine;
using System.Collections;
using PubNubMessaging.Core;
using System.Collections.Generic;
using System;

public class PubnubHelper : MonoBehaviour {

	// Use this for initialization
	private Pubnub pubnub;
	private const string PUBLISH_CHANNEL = "unity_channel";
	private const string SUBSCRIBE_CHANNEL = "cozmo_channel";
	private const string PUBLISH_KEY = "demo";
	private const string SUBSCRIBE_KEY = "demo";
	public delegate void OnMessageReceived(String message);
	public OnMessageReceived onMessageReceived;


	void Start () {
		pubnub = new Pubnub( PUBLISH_KEY, SUBSCRIBE_KEY);
		pubnub.Subscribe<string>(
			SUBSCRIBE_CHANNEL, 
			DisplaySubscribeReturnMessage, 
			DisplaySubscribeConnectStatusMessage, 
			DisplayErrorMessage); 
		
	}

	void DisplaySubscribeConnectStatusMessage(string connectMessage)
	{
		print("PUBNUB UNITY : CONNECTED");

	}

	void DisplaySubscribeReturnMessage(string result) {
		//UnityEngine.Debug.Log(result);

		if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
		{
			List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
			if (deserializedMessage != null && deserializedMessage.Count > 0)
			{
				object subscribedObject = (object)deserializedMessage[0];
				if (subscribedObject != null)
				{
					//IF CUSTOM OBJECT IS EXCEPTED, YOU CAN CAST THIS OBJECT TO YOUR CUSTOM CLASS TYPE
					string resultActualMessage = pubnub.JsonPluggableLibrary.SerializeToJsonString(subscribedObject);
					if (this.onMessageReceived != null) {
						this.onMessageReceived(resultActualMessage);
					}
				}
			}
		}
	}

	void DisplayErrorMessage(PubnubClientError pubnubError)
	{
		UnityEngine.Debug.Log("DisplayErrorMessage: "+ pubnubError.StatusCode);
	}

	void DisplayReturnMessage(string result)
	{
		UnityEngine.Debug.Log("DisplayReturnMessage: "+result);

	}

	public void Publish(string message){
		pubnub.Publish<string>(
			PUBLISH_CHANNEL, 
			message, 
			DisplayReturnMessage, 
			DisplayErrorMessage); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
