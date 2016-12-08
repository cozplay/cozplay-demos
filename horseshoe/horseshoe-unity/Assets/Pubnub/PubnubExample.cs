using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using PubNubMessaging.Core;
using System;
using System.Reflection;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Collections.Generic;

public class PubnubExample : MonoBehaviour
{
    enum PubnubState
    {
        None,
        GrantSubscribe,
        AuditSubscribe,
        RevokeSubscribe,
        GrantPresence,
        AuditPresence,
        RevokePresence,
        AuthKey,
        Presence,
        Subscribe,
        Publish,
        DetailedHistory,
        HereNow,
        Time,
        Unsubscribe,
        PresenceUnsubscribe,
        DisconnectRetry,
        EnableNetwork,
        DisableNetwork,
        SetUserStateKeyValue,
        ViewUserState,
        DelUserState,
        GetUserState,
        SetUserStateJson,
        PresenceHeartbeat,
        PresenceInterval,
        WhereNow,
        GlobalHereNow,
        ChangeUUID,
        RemoveChannelGroup,
        AddChannelToChannelGroup,
        GetChannelsForChannelGroup,
        RemoveChannelFromChannelGroup,
        CGGrant,
        CGGrantPresence,
        CGRevoke,
        CGRevokePresence,
        CGAudit,
        CGAuditPresence,
        SetFilterExpression,
        GetFilterExpression
    }

    bool ssl = true;
    bool resumeOnReconnect = true;
    string cipherKey = "";
    string secretKey = "demo";
    string publishKey = "demo";
    string subscribeKey = "demo";
    string uuid = Guid.NewGuid ().ToString ();
    string subscribeTimeoutInSeconds = "310";
    string operationTimeoutInSeconds = "45";
    string networkMaxRetries = "50";
    string networkRetryIntervalInSeconds = "10";
    string heartbeatIntervalInSeconds = "10";
    static public bool showErrorMessageSegments = true;
    string channel = "";
    string publishedMessage = "";
    string publishedMetadataKey = "";
    string publishedMetadataValue = "";
    string pubChannel ="";
    string input = "";
    static Pubnub pubnub;
    PubnubState state;
    private static Queue<string> recordQueue = new Queue<string> ();
    Vector2 scrollPosition = Vector2.zero;
    string pubnubApiResult = "";

    #if(UNITY_IOS)
       bool requestInProcess = false;
    #endif
    bool showPublishPopupWindow = false;
    bool showGrantWindow = false;
    bool showAuthWindow = false;
    bool showTextWindow = false;
    bool showActionsPopupWindow = false;
    bool showCGPopupWindow = false;
    bool showPamPopupWindow = false;
    bool toggle1 = false;
    bool toggle2 = false;
    string valueToSet = "";
    string valueToSetSubs = "1440";
    string valueToSetAuthKey = "";
    string text1 = "";
    string text2 = "";
    string text3 = "";
    string text4 = "";
    bool storeInHistory = true;

    Rect publishWindowRect = new Rect (60, 365, 300, 250);
    Rect authWindowRect = new Rect (60, 365, 300, 200);
    Rect textWindowRect = new Rect (60, 365, 300, 300);
    Rect textWindowRect2 = new Rect (60, 365, 300, 350);
    bool allowUserSettingsChange = true;
    float fLeft = 20;
    float fLeftInit = 20;
    float fTop = 10;
    float fTopInit = 10;
    float fRowHeight = 45;
    float fHeight = 35;
    float fButtonHeight = 40;
    float fButtonWidth = 120;

    public void OnDisable(){
        InstantiatePubnub ();
        pubnub.Unsubscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
    }

    public void OnGUI ()
    {
        GUI.enabled = !allowUserSettingsChange;

        GUIStyle customStyle = new GUIStyle (GUI.skin.button);

        customStyle.fontSize = 10;
        customStyle.hover.textColor = Color.yellow;
        customStyle.fontStyle = FontStyle.Italic;

        fLeft = fLeftInit;
        fTop = fTopInit + 0 * fRowHeight;
        if (GUI.Button (new Rect (fLeft, fTop, 120, 40), "Reset Settings", customStyle)) {
            allowUserSettingsChange = true;
            ResetPubnubInstance ();
            pubnubApiResult = "";
        }

        GUI.enabled = true;

        fLeft = fLeftInit + 150;
        if (GUI.Button (new Rect (fLeft, fTop, 90, fButtonHeight), "Actions")) {
            showPamPopupWindow = false;
            showCGPopupWindow = false;
            showActionsPopupWindow = !showActionsPopupWindow;
        }
        if (showActionsPopupWindow) {
            ShowActions (fLeft, fTop, fButtonHeight);
        }

        fLeft = fLeft + 100;
        if (GUI.Button (new Rect (fLeft, fTop, 50, fButtonHeight), "PAM")) {
            showActionsPopupWindow = false;
            showCGPopupWindow = false;
            showPamPopupWindow = !showPamPopupWindow;
        }
        if (showPamPopupWindow) {
            ShowPamActions (fLeft + 50, fTop, fButtonHeight);
        }

        fLeft = fLeft + 60;
        if (GUI.Button (new Rect (fLeft, fTop, 90, fButtonHeight), "CG & More")) {
            showActionsPopupWindow = false;
            showPamPopupWindow = false;
            showCGPopupWindow = !showCGPopupWindow;
        }
        if (showCGPopupWindow) {
            ShowCGActions (fLeft + 50, fTop, fButtonHeight);
        }



        GUI.enabled = allowUserSettingsChange;

        fTop = fTopInit + 1 * fRowHeight;
        fLeft = fLeftInit;
        ssl = GUI.Toggle (new Rect (fLeft, fTop, 60, fButtonHeight), ssl, " SSL ");

        fLeft = fLeft + 50 + 10;
        resumeOnReconnect = GUI.Toggle (new Rect (fLeft, fTop, 200, fButtonHeight), resumeOnReconnect, " Resume On Reconnect ");

        fTop = fTopInit + 2 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Cipher Key");

        fLeft = fLeft + 75;
        cipherKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), cipherKey);

        fLeft = fLeft + 145;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "UUID");

        fLeft = fLeft + 45;
        uuid = GUI.TextField (new Rect (fLeft, fTop, 170, fHeight), uuid);

        fTop = fTopInit + 3 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Subscribe Key");

        fLeft = fLeft + 75;
        subscribeKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), subscribeKey);

        fLeft = fLeft + 145;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Publish Key");

        fLeft = fLeft + 85;
        publishKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), publishKey);


        fTop = fTopInit + 4 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Secret Key");
        fLeft = fLeft + 75;
        secretKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), secretKey);

        fLeft = fLeft + 145;
        GUI.Label (new Rect (fLeft, fTop, 160, fHeight), "Subscribe Timeout (secs)");

        fLeft = fLeft + 185;
        subscribeTimeoutInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), subscribeTimeoutInSeconds, 6);
        subscribeTimeoutInSeconds = Regex.Replace (subscribeTimeoutInSeconds, "[^0-9]", "");

        fTop = fTopInit + 5 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 160, fHeight), "MAX retries");

        fLeft = fLeft + 175;
        networkMaxRetries = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), networkMaxRetries, 6);
        networkMaxRetries = Regex.Replace (networkMaxRetries, "[^0-9]", "");

        fLeft = fLeft + 45;
        GUI.Label (new Rect (fLeft, fTop, 180, fHeight), "Non Subscribe Timeout (secs)");

        fLeft = fLeft + 185;
        operationTimeoutInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), operationTimeoutInSeconds, 6);
        operationTimeoutInSeconds = Regex.Replace (operationTimeoutInSeconds, "[^0-9]", "");

        fTop = fTopInit + 6 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 160, fHeight), "Retry Interval (secs)");

        fLeft = fLeft + 175;
        networkRetryIntervalInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), networkRetryIntervalInSeconds, 6);
        networkRetryIntervalInSeconds = Regex.Replace (networkRetryIntervalInSeconds, "[^0-9]", "");

        fLeft = fLeft + 45;
        GUI.Label (new Rect (fLeft, fTop, 180, fHeight), "Heartbeat Interval (secs)");
        fLeft = fLeft + 185;
        heartbeatIntervalInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), heartbeatIntervalInSeconds, 6);
        heartbeatIntervalInSeconds = Regex.Replace (heartbeatIntervalInSeconds, "[^0-9]", "");

        GUI.enabled = true;

        if (showPublishPopupWindow) {
            GUI.backgroundColor = Color.black;
            publishWindowRect = GUI.ModalWindow (0, publishWindowRect, DoPublishWindow, "Message Publish");
            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }
        if (showAuthWindow) {
            GUI.backgroundColor = Color.black;
            if (state == PubnubState.AuthKey) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoAuthWindow, "Enter Auth Key");
            } else if (state == PubnubState.ChangeUUID) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoChangeUuidWindow, "Change UUID");
            } else if (state == PubnubState.WhereNow) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoWhereNowWindow, "Where Now");
            } else if (state == PubnubState.PresenceHeartbeat) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoPresenceHeartbeatWindow, "Presence Heartbeat");
            } else if (state == PubnubState.PresenceInterval) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoPresenceIntervalWindow, "Presence Interval");
            } else if (state == PubnubState.DetailedHistory) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoDetailedHistory, "Detailed History");
            } else if (state == PubnubState.RemoveChannelGroup) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoRemoveChannelGroup, "Remove Channel Group");
            } else if (state == PubnubState.GetChannelsForChannelGroup) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoListAllChannelsOfChannelGroups, "List Channels of Channel Group");
            } else if (state == PubnubState.SetFilterExpression) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoSetFilterExpression, "SetFilterExpression");
            }
                
            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }        
        if (showGrantWindow) {
            GUI.backgroundColor = Color.black;
            string title = "";
            if (state == PubnubState.GrantPresence) {
                title = "Presence Grant";
            } else if (state == PubnubState.GrantSubscribe) {
                title = "Subscribe Grant";
            } else if (state == PubnubState.CGGrant) {
                title = "CG Subscribe Grant";
            } else if (state == PubnubState.CGGrantPresence) {
                title = "CG Presence Grant";
            } else if (state == PubnubState.HereNow) {
                title = "Here now";
            } else if (state == PubnubState.GlobalHereNow) {
                title = "Global here now";
            }
            authWindowRect = GUI.ModalWindow (0, authWindowRect, DoGrantWindow, title);
            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }
        if (showTextWindow) {
            GUI.backgroundColor = Color.black;
            string title = "";
            if (state == PubnubState.GetUserState) {
                title = "Get User State";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.DelUserState) {
                title = "Delete User Statue";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.SetUserStateJson) {
                title = "Set User State using Json";
                textWindowRect = GUI.ModalWindow (0, textWindowRect, DoTextWindow, title);
            } else if (state == PubnubState.SetUserStateKeyValue) {
                title = "Set User State using Key-Value pair";
                textWindowRect = GUI.ModalWindow (0, textWindowRect, DoTextWindow, title);
            } else if (state == PubnubState.Subscribe) {
                title = "Subscribe";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.Presence) {
                title = "Presence";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.Unsubscribe) {
                title = "Unsubscribe";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.PresenceUnsubscribe) {
                title = "PresenceUnsubscribe";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.AddChannelToChannelGroup) {
                title = "AddChannelToChannelGroup";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.RemoveChannelFromChannelGroup) {
                title = "RemoveChannelFromChannelGroup";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.AuditPresence) {
                title = "AuditPresence";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.AuditSubscribe) {
                title = "AuditSubscribe";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.RevokePresence) {
                title = "RevokePresence";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.RevokeSubscribe) {
                title = "RevokeSubscribe";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.CGAuditPresence) {
                title = "CGAuditPresence";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.CGRevokePresence) {
                title = "CGRevokePresence";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.CGAudit) {
                title = "CGAudit";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.CGRevoke) {
                title = "CGRevoke";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            }

            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }

        fTop = fTopInit + 7 * fRowHeight;
        fLeft = fLeftInit;
        scrollPosition = GUI.BeginScrollView (new Rect (fLeft, fTop, 430, 420), scrollPosition, new Rect (fLeft, fTop, 430, 420), false, true);
        GUI.enabled = false;
        pubnubApiResult = GUI.TextArea (new Rect (fLeft, fTop, 430, 420), pubnubApiResult);            
        GUI.enabled = true;
        GUI.EndScrollView ();
    }

    void ShowPamActions (float fLeft, float fTop, float fButtonHeight)
    {
        Rect windowRect = new Rect (fLeft - 160, fTop + fButtonHeight, 160, 650);
        GUI.backgroundColor = Color.black;
        windowRect = GUI.Window (0, windowRect, DoPamActionWindow, "");
        GUI.backgroundColor = new Color (1, 1, 1, 1);
    }

    void ShowCGActions (float fLeft, float fTop, float fButtonHeight)
    {
        Rect windowRect = new Rect (fLeft - 160, fTop + fButtonHeight, 160, 650);
        GUI.backgroundColor = Color.black;
        windowRect = GUI.Window (0, windowRect, DoCGActionWindow, "");
        GUI.backgroundColor = new Color (1, 1, 1, 1);
    }

    void ShowActions (float fLeft, float fTop, float fButtonHeight)
    {
        Rect windowRect = new Rect (fLeft, fTop + fButtonHeight, 140, 650);
        GUI.backgroundColor = Color.black;
        windowRect = GUI.Window (0, windowRect, DoActionWindow, "");
        GUI.backgroundColor = new Color (1, 1, 1, 1);
    }

    void DoTextWindow (int windowID)
    {
        string title = "";
        string buttonTitle = "";
        string label1 = "";
        string label2 = "";
        string label3 = "";
        string label4 = "";

        if (state == PubnubState.GetUserState) {
            title = "Get User State";
            label1 = "Channel";
            label2 = "Channel Group";
            label3 = "UUID";
            buttonTitle = "Get";
        } else if (state == PubnubState.DelUserState) {
            title = "Delete User State";
            label1 = "Channel";
            label2 = "Channel Group";
            label3 = "Key";
            buttonTitle = "Delete";
        } else if (state == PubnubState.SetUserStateJson) {
            title = "Set User State";
            label1 = "Channel";
            label2 = "Channel Group";
            label3 = "UUID";
            label4 = "Enter Json";

            buttonTitle = "Set";
        } else if (state == PubnubState.SetUserStateKeyValue) {
            title = "Set User State";
            label1 = "Channel";
            label2 = "Channel Group";
            label3 = "Key";
            label4 = "Value";

            buttonTitle = "Set";
        } else if (state.Equals(PubnubState.Subscribe)){
            title = "Subscribe";
            label1 = "Channel";
            label2 = "Channel Group";
            buttonTitle = "Subscribe";
        } else if (state.Equals(PubnubState.Presence)){
            title = "Presence";
            label1 = "Channel";
            label2 = "Channel Group";
            buttonTitle = "Presence";
        } else if (state.Equals(PubnubState.Unsubscribe)){
            title = "SubscribeUnsubscribe";
            label1 = "Channel";
            label2 = "Channel Group";
            buttonTitle = "Unsubscribe";
        } else if (state.Equals(PubnubState.PresenceUnsubscribe)){
            title = "PresenceUnsubscribe";
            label1 = "Channel";
            label2 = "Channel Group";
            buttonTitle = "PresenceUnsubscribe";
        } else if (state.Equals(PubnubState.AddChannelToChannelGroup)){
            title = "Add Channel To Channel Group";
            label1 = "Channel";
            label2 = "Channel Group";
            buttonTitle = "Add Channel";
        } else if (state.Equals(PubnubState.RemoveChannelFromChannelGroup)){
            title = "Remove Channel From Channel Group";
            label1 = "Channel";
            label2 = "Channel Group";
            buttonTitle = "Remove Channel";
        } else if (state == PubnubState.AuditPresence) {
            title = "AuditPresence";
            label1 = "Channel";
            label2 = "Auth key";
            buttonTitle = "Audit";
        } else if (state == PubnubState.AuditSubscribe) {
            title = "AuditSubscribe";
            label1 = "Channel";
            label2 = "Auth key";
            buttonTitle = "Audit";
        } else if (state == PubnubState.RevokePresence) {
            title = "RevokePresence";
            label1 = "Channel";
            label2 = "Auth key";
            buttonTitle = "Revoke";
        } else if (state == PubnubState.RevokeSubscribe) {
            title = "RevokeSubscribe";
            label1 = "Channel";
            label2 = "Auth key";
            buttonTitle = "Revoke";
        } else if (state == PubnubState.CGAuditPresence) {
            title = "CGAuditPresence";
            label1 = "Channel Group";
            label2 = "Auth key";
            buttonTitle = "Audit";

        } else if (state == PubnubState.CGRevokePresence) {
            title = "CGRevokePresence";
            label1 = "Channel Group";
            label2 = "Auth key";
            buttonTitle = "Revoke";

        } else if (state == PubnubState.CGAudit) {
            title = "CGAudit";
            label1 = "Channel Group";
            label2 = "Auth key";
            buttonTitle = "CGAudit";

        } else if (state == PubnubState.CGRevoke) {
            title = "CGRevoke";
            label1 = "Channel Group";
            label2 = "Auth key";
            buttonTitle = "Revoke";
        }

        fLeft = fLeftInit;
        fTop = 20;
        GUI.Label (new Rect (fLeft, fTop, 100, fHeight), label1);
        fLeft = fLeftInit + 100;

        text1 = GUI.TextField (new Rect (fLeft, fTop, 90, fButtonHeight), text1);

        fLeft = fLeftInit;
        fTop = fTop + fButtonHeight;
        GUI.Label (new Rect (fLeft, fTop, 100, fHeight), label2);
        fLeft = fLeftInit + 100;

        text2 = GUI.TextField (new Rect (fLeft, fTop, 90, fButtonHeight), text2);

        if ((state == PubnubState.SetUserStateJson) || (state == PubnubState.SetUserStateKeyValue)) {
            fLeft = fLeftInit;
            fTop = fTop + 2 * fHeight - 30;
            GUI.Label (new Rect (fLeft, fTop, 100, fHeight + 30), label3);
            fLeft = fLeftInit + 100;

            text3 = GUI.TextField (new Rect (fLeft, fTop, 90, fButtonHeight), text3);
            fLeft = fLeftInit;
            fTop = fTop + 3 * fHeight - 60;

            GUI.Label (new Rect (fLeft, fTop, 100, 90), label4);
            fLeft = fLeftInit + 100;

            text4 = GUI.TextField (new Rect (fLeft, fTop, 90, fButtonHeight), text4);
            fLeft = fLeftInit;
            fTop = fTop + 4 * fHeight - 90;
        } else if ((state == PubnubState.GetUserState) || (state == PubnubState.DelUserState)){
            fLeft = fLeftInit;
            fTop = fTop + 2 * fHeight - 30;
            GUI.Label (new Rect (fLeft, fTop, 100, 90), label3);
            fLeft = fLeftInit + 100;

            text3 = GUI.TextField (new Rect (fLeft, fTop, 90, fButtonHeight), text3);
            fLeft = fLeftInit;
            fTop = fTop + 3 * fHeight - 60;

        } else {
            fLeft = fLeftInit;
            fTop = fTop + 2 * fButtonHeight;
        }
        if (GUI.Button (new Rect (fLeft, fTop, 100, fButtonHeight), buttonTitle)) {
            string currentChannel = text1;
            try{
                if (state == PubnubState.GetUserState) {
                    AddToPubnubResultContainer ("Running get user state");
                    UnityEngine.Debug.Log (string.Format("text2 {0}, text3 {1}", text2, text3));
                    pubnub.GetUserState<string> (text1, text2, text3, DisplayReturnMessage, DisplayErrorMessage);
                } else if (state == PubnubState.DelUserState) {
                    AddToPubnubResultContainer ("Running delete user state");
                    string stateKey = text3;
                    pubnub.SetUserState<string> (currentChannel, text2, "", new KeyValuePair<string, object> (stateKey, null), DisplayReturnMessage, DisplayErrorMessage);

                } else if (state == PubnubState.SetUserStateJson) {
                    AddToPubnubResultContainer ("Running Set User State Json");
                    string currentUuid = text3;
                    string jsonUserState = "";
                    UnityEngine.Debug.Log (string.Format("text4 {0}, text3 {1}", text4, text3));
                    if (string.IsNullOrEmpty (text4)) {
                        //jsonUserState = pubnub.GetLocalUserState (text1);
                    } else {
                        jsonUserState = text4;
                    }
                    pubnub.SetUserState<string> (currentChannel, text2, currentUuid, jsonUserState, DisplayReturnMessage, DisplayErrorMessage);

                } else if (state == PubnubState.SetUserStateKeyValue) {
                    AddToPubnubResultContainer ("Running Set User State Key Value");
                    int valueInt;
                    double valueDouble;
                    string stateKey = text3;
                    UnityEngine.Debug.Log (string.Format("text4 {0}, text3 {1}", text4, text3));
                    if (Int32.TryParse (text4, out valueInt)) {
                        pubnub.SetUserState<string> (currentChannel, text2,  "", new KeyValuePair<string, object> (stateKey, valueInt), DisplayReturnMessage, DisplayErrorMessage);
                    } else if (Double.TryParse (text4, out valueDouble)) {
                        pubnub.SetUserState<string> (currentChannel, text2, "", new KeyValuePair<string, object> (stateKey, valueDouble), DisplayReturnMessage, DisplayErrorMessage);
                    } else {
                        string val = text4;
                        pubnub.SetUserState<string> (currentChannel, text2, "", new KeyValuePair<string, object> (stateKey, val), DisplayReturnMessage, DisplayErrorMessage);
                    }
                } else if (state.Equals(PubnubState.Subscribe)){
                    string channelGroup = text2;
                    AddToPubnubResultContainer ("Running Subscribe");

                    #if PUBNUB_PS_V2_RESPONSE
                    pubnub.Subscribe<string> (currentChannel, channelGroup, 
                        (string returnMessage) => {
                            object obj = pubnub.JsonPluggableLibrary.DeserializeToObject(returnMessage);
                            Dictionary<string, object> dict = obj as Dictionary<string, object>;
                            UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString Object: result:{0}\nobj:{1}\nCount:{2}\n", returnMessage,
                                obj.ToString(), dict.Count));

                            foreach(var pair in dict){
                                UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString pair.Key: {0}, pair.Value:{1}", 
                                    pair.Key, pair.Value));
                                AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeString:- {0}:{1}", pair.Key, pair.Value));
                            }

                            //PNMessageResult pnMessageResult = (PNMessageResult)Convert.ChangeType(obj, typeof(PNMessageResult));
                            //UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString: {0}", pnMessageResult.Payload));
                            //AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeString: {0}", pnMessageResult.Payload));                            
                        }, 
                        (string connectStatus) => {
                            UnityEngine.Debug.Log (string.Format ("DisplayConnectMessage: {0}", connectStatus));
                            AddToPubnubResultContainer (string.Format ("DisplayConnectMessage: {0}", connectStatus));

                        }, 
                        (string wildcardMessage) => {
                            UnityEngine.Debug.Log (string.Format ("Wildcard CALLBACK LOG: {0}", wildcardMessage));
                            AddToPubnubResultContainer (string.Format ("Wildcard CALLBACK: {0}", wildcardMessage));

                        },
                        (PubnubClientError result) => {
                            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
                            AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));

                        });

                    /*pubnub.Subscribe<object> (currentChannel, channelGroup, 
                        (object returnMessage) => {
                            PNMessageResult pnMessageResult = returnMessage as PNMessageResult;

                            UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0} {1} {2} {3} {4} {5}", 
                                pnMessageResult.Payload, 
                                pnMessageResult.Channel, 
                                pnMessageResult.Subscription,
                                pnMessageResult.OriginatingTimetoken,
                                   pnMessageResult.Timetoken,
                                pnMessageResult.UserMetadata.ToString()));


                            AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0} {1} {2} {3} {4} {5}", 
                                pnMessageResult.Payload, 
                                pnMessageResult.Channel, 
                                pnMessageResult.Subscription, 
                                pnMessageResult.OriginatingTimetoken,
                                pnMessageResult.Timetoken,
                                pnMessageResult.UserMetadata.ToString())); 
                        }, 
                        (object connectStatus) => {
                            var myList = connectStatus as List<object>;
                            var stringList = myList.OfType<string>();
                            string result2 = string.Join(",", stringList.ToArray());

                            UnityEngine.Debug.Log (string.Format ("DisplayConnectMessage: {0}", result2));
                            AddToPubnubResultContainer (string.Format ("DisplayConnectMessage: {0}", result2));

                        }, 
                        (object wildcardMessage) => {
                            var myList = wildcardMessage as List<object>;
                            var stringList = myList.OfType<string>();
                            string result2 = string.Join(",", stringList.ToArray());

                            UnityEngine.Debug.Log (string.Format ("Wildcard CALLBACK LOG: {0}", result2));
                            AddToPubnubResultContainer (string.Format ("Wildcard CALLBACK: {0}", result2));

                        },
                        (PubnubClientError result) => {
                            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
                            AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));

                        });*/
                      #else
                      pubnub.Subscribe<string> (currentChannel, channelGroup, DisplayReturnMessage, DisplayConnectStatusMessage, 
                        DisplayWildcardReturnMessage, DisplayErrorMessage);
                      #endif
                } else if (state.Equals(PubnubState.Presence)){
                    string channelGroup = text2;
                    AddToPubnubResultContainer ("Running Presence");
                    #if PUBNUB_PS_V2_RESPONSE
                    pubnub.Presence<string> (currentChannel, channelGroup, 
                        (string returnMessage) => {
                            object obj = pubnub.JsonPluggableLibrary.DeserializeToObject(returnMessage);
                            Dictionary<string, object> dict = obj as Dictionary<string, object>;
                            UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString Object: result:{0}\nobj:{1}\nCount:{2}\n", returnMessage,
                                obj.ToString(), dict.Count));

                            foreach(var pair in dict){
                                UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString pair.Key: {0}, pair.Value:{1}", 
                                    pair.Key, pair.Value));
                                AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeString:- {0}:{1}", pair.Key, pair.Value));
                            }

                            //PNMessageResult pnMessageResult = (PNMessageResult)Convert.ChangeType(obj, typeof(PNMessageResult));
                            //UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString: {0}", pnMessageResult.Payload));
                            //AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeString: {0}", pnMessageResult.Payload));                            
                        }, 
                        (string connectStatus) => {
                            UnityEngine.Debug.Log (string.Format ("DisplayConnectMessage: {0}", connectStatus));
                            AddToPubnubResultContainer (string.Format ("DisplayConnectMessage: {0}", connectStatus));

                        }, 
                        (PubnubClientError result) => {
                            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
                            AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));

                        });
                    
                    /*pubnub.Presence<object> (currentChannel, channelGroup, 
                        (object returnMessage) => {
                            PNPresenceEventResult pnMessageResult = returnMessage as PNPresenceEventResult;

                            UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0}  {1} {2} {3} {4} {5} {6}", pnMessageResult.Event,
                                pnMessageResult.Channel,
                                pnMessageResult.Subscription,
                                pnMessageResult.Occupancy,
                                pnMessageResult.Timetoken, 
                                pnMessageResult.UUID,
                                pnMessageResult.Timestamp));


                            AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0} {1} {2} {3} {4} ", pnMessageResult.Event,
                             pnMessageResult.Channel, pnMessageResult.Subscription, pnMessageResult.Occupancy, pnMessageResult.Timetoken));

                        }, 
                        (object connectStatus) => {
                            var myList = connectStatus as List<object>;
                            var stringList = myList.OfType<string>();
                            string result2 = string.Join(",", stringList.ToArray());

                            UnityEngine.Debug.Log (string.Format ("DisplayConnectMessage: {0}", result2));
                            AddToPubnubResultContainer (string.Format ("DisplayConnectMessage: {0}", result2));

                        }, 
                        (PubnubClientError result) => {
                            UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
                            AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));

                        });*/
                    #else
                    pubnub.Presence<string> (currentChannel, channelGroup, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
                    #endif
                } else if (state.Equals(PubnubState.Unsubscribe)){
                    string channelGroup = text2;
                    AddToPubnubResultContainer ("Running Unsubscribe");
                    pubnub.Unsubscribe<string> (currentChannel, channelGroup, DisplayDisconnectReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
                } else if (state.Equals(PubnubState.PresenceUnsubscribe)){
                    string channelGroup = text2;
                    AddToPubnubResultContainer ("Running PresenceUnsubscribe");

                    pubnub.PresenceUnsubscribe<string> (currentChannel, channelGroup, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
                } else if (state.Equals(PubnubState.AddChannelToChannelGroup)){
                    string channelGroup = text2;
                    AddToPubnubResultContainer ("Running AddChannelToChannelGroup");
                    string[] channels = currentChannel.Split(',');
                    pubnub.AddChannelsToChannelGroup<string> (channels, channelGroup, DisplayReturnMessage, DisplayErrorMessage);

                } else if (state.Equals(PubnubState.RemoveChannelFromChannelGroup)){
                    string channelGroup = text2;
                    AddToPubnubResultContainer ("Running RemoveChannelFromChannelGroup");
                    string[] channels = currentChannel.Split(',');
                    pubnub.RemoveChannelsFromChannelGroup<string> (channels, channelGroup, DisplayReturnMessage, DisplayErrorMessage);
                } else if (state == PubnubState.AuditPresence) {
                    AddToPubnubResultContainer ("Running Audit Presence");
                    string auth = text2;
                    pubnub.AuditPresenceAccess<string> (currentChannel, auth, DisplayReturnMessage, DisplayErrorMessage);
                } else if (state == PubnubState.AuditSubscribe) {
                    string auth = text2;
                    AddToPubnubResultContainer ("Running Audit Subscribe");
                    pubnub.AuditAccess<string> (currentChannel, auth, DisplayReturnMessage, DisplayErrorMessage);
                } else if (state == PubnubState.RevokePresence) {
                    string auth = text2;
                    AddToPubnubResultContainer ("Running Revoke Presence");
                    pubnub.GrantPresenceAccess<string> (currentChannel, auth, false, false, DisplayReturnMessage, DisplayErrorMessage);
                } else if (state == PubnubState.RevokeSubscribe) {
                    string auth = text2;
                    AddToPubnubResultContainer ("Running Revoke Subscribe");
                    pubnub.GrantAccess<string> (currentChannel, auth, false, false, DisplayReturnMessage, DisplayErrorMessage);
                } else if (state == PubnubState.CGAuditPresence) {
                    string auth = text2;
                    AddToPubnubResultContainer ("Running CG Audit Presence");
                    pubnub.ChannelGroupAuditPresenceAccess<string> (currentChannel, auth, DisplayReturnMessage, DisplayErrorMessage);

                } else if (state == PubnubState.CGRevokePresence) {
                    string auth = text2;
                    AddToPubnubResultContainer ("Running CG Revoke Presence");
                    pubnub.ChannelGroupGrantPresenceAccess<string> (currentChannel, auth, false, false, DisplayReturnMessage, DisplayErrorMessage);

                } else if (state == PubnubState.CGAudit) {
                    string auth = text2;
                    AddToPubnubResultContainer ("Running CG Audit Subscribe");
                    pubnub.ChannelGroupAuditAccess<string> (currentChannel, auth, DisplayReturnMessage, DisplayErrorMessage);

                } else if (state == PubnubState.CGRevoke) {
                    string auth = text2;
                    AddToPubnubResultContainer ("Running CG Revoke Subscribe");
                    pubnub.ChannelGroupGrantAccess<string> (currentChannel, auth, false, false, DisplayReturnMessage, DisplayErrorMessage);
                }

            }catch (Exception ex){
                AddToPubnubResultContainer (ex.Message);
                UnityEngine.Debug.Log (ex.ToString());
            }

            text1 = "";
            text2 = "";
            if ((state == PubnubState.SetUserStateJson) || (state == PubnubState.SetUserStateKeyValue)) {
                text3 = "";
                text4 = "";
            } else if (state == PubnubState.GetUserState){
                text3 = "";
            }
            showTextWindow = false;
            showPamPopupWindow = false;
        }

        fLeft = fLeftInit + 100;
        if (GUI.Button (new Rect (fLeft, fTop, 100, fButtonHeight), "Cancel")) {
            text1 = "";
            text2 = "";
            if ((state == PubnubState.SetUserStateJson) || (state == PubnubState.SetUserStateKeyValue)) {
                text3 = "";
            }
            showTextWindow = false;
            showPamPopupWindow = false;
        }
        GUI.DragWindow (new Rect (0, 0, 800, 400));
    }

    void DoAuditPresenceWindow (int windowID)
    {
        ShowWindow (PubnubState.AuditPresence);
    }

    void DoAuditSubscribeWindow (int windowID)
    {
        ShowWindow (PubnubState.AuditSubscribe);
    }

    void DoRevokePresenceWindow (int windowID)
    {
        ShowWindow (PubnubState.RevokePresence);
    }

    void DoRevokeSubscribeWindow (int windowID)
    {
        ShowWindow (PubnubState.RevokeSubscribe);
    }

    void DoAuthWindow (int windowID)
    {
        ShowWindow (PubnubState.AuthKey);
    }

    void DoChangeUuidWindow (int windowID)
    {
        ShowWindow (PubnubState.ChangeUUID);
    }

    void DoWhereNowWindow (int windowID)
    {
        ShowWindow (PubnubState.WhereNow);
    }

    void DoPresenceHeartbeatWindow (int windowID)
    {
        ShowWindow (PubnubState.PresenceHeartbeat);
    }

    void DoPresenceIntervalWindow (int windowID)
    {
        ShowWindow (PubnubState.PresenceInterval);
    }

    void DoDetailedHistory (int windowID)
    {
        ShowWindow (PubnubState.DetailedHistory);
    }

    void DoRemoveChannelGroup (int windowID)
    {
        ShowWindow (PubnubState.RemoveChannelGroup);
    }

    void DoListAllChannelsOfChannelGroups (int windowID)
    {
        ShowWindow (PubnubState.GetChannelsForChannelGroup);
    }
    void DoSetFilterExpression (int windowID)
    {
        ShowWindow (PubnubState.SetFilterExpression);
    }

    void ShowGuiButton (string buttonTitle, PubnubState state)
    {
        if (GUI.Button (new Rect (30, 80, 100, fButtonHeight), buttonTitle)) {
            if (state == PubnubState.AuthKey) {
                pubnub.AuthenticationKey = input;
            } else if (state == PubnubState.ChangeUUID) {
                pubnub.ChangeUUID (input);
            } else if (state == PubnubState.WhereNow) {
                pubnub.WhereNow<string> (input, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.PresenceHeartbeat) {
                int iInterval;

                Int32.TryParse (input, out iInterval);
                if (iInterval < 0) {
                    AddToPubnubResultContainer ("ALERT: Please enter an integer value.");
                } else {
                    pubnub.PresenceHeartbeat = int.Parse (input);
                    AddToPubnubResultContainer (string.Format ("Presence Heartbeat is set to {0}", pubnub.PresenceHeartbeat));
                }
            } else if (state == PubnubState.PresenceInterval) {
                int iInterval;

                Int32.TryParse (input, out iInterval);
                if (iInterval == 0) {
                    AddToPubnubResultContainer ("ALERT: Please enter an integer value.");
                } else {
                    pubnub.PresenceHeartbeatInterval = int.Parse (input);
                    AddToPubnubResultContainer (string.Format ("Presence Heartbeat Interval is set to {0}", pubnub.PresenceHeartbeatInterval));
                }
            } else if (state == PubnubState.DetailedHistory) {
                AddToPubnubResultContainer ("Running Detailed History");
                pubnub.DetailedHistory<string> (input, 100, DisplayReturnMessageHistory, DisplayErrorMessage);
            } else if (state == PubnubState.RemoveChannelGroup) {
                AddToPubnubResultContainer ("Running RemoveChannelGroup");
                pubnub.RemoveChannelGroup<string> (input, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.GetChannelsForChannelGroup) {
                AddToPubnubResultContainer ("Running GetChannelsForChannelGroup");
                pubnub.GetChannelsForChannelGroup<string> (input, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.SetFilterExpression) {
                AddToPubnubResultContainer ("Running SetFilterExpression");
                pubnub.FilterExpression = input;//"(aoi_x >= 0 && aoi_x <= 2) && (aoi_y >= 0 && aoi_y <= 2)";//input;
            }

            input = "";
            showAuthWindow = false;
            showPamPopupWindow = false;
            showCGPopupWindow = false;
        }
    }

    void ShowWindow (PubnubState state)
    {
        string title = "";
        string buttonTitle = "";
        if (state == PubnubState.AuthKey) {
            title = "Enter Auth Key";
            buttonTitle = "Set";
        } else if (state == PubnubState.ChangeUUID) {
            title = "Enter UUID";
            buttonTitle = "Change";
        } else if (state == PubnubState.WhereNow) {
            title = "Enter UUID";
            buttonTitle = "Run";
        } else if (state == PubnubState.PresenceHeartbeat) {
            title = "Enter Presence Heartbeat";
            buttonTitle = "Set";
        } else if (state == PubnubState.PresenceInterval) {
            title = "Enter Presence Interval";
            buttonTitle = "Set";
        } else if (state == PubnubState.AuditPresence) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Run Audit";
        } else if (state == PubnubState.AuditSubscribe) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Run Audit";
        } else if (state == PubnubState.RevokePresence) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Revoke";
        } else if (state == PubnubState.RevokeSubscribe) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Revoke";
        } else if (state == PubnubState.DetailedHistory) {
            title = "Enter Channel";
            buttonTitle = "Detailed History";
        } else if (state == PubnubState.RemoveChannelGroup) {
            title = "Channel Group";
            buttonTitle = "Remove";
        } else if (state == PubnubState.GetChannelsForChannelGroup) {
            title = "Channel Group";
            buttonTitle = "List";
        } else if (state == PubnubState.SetFilterExpression) {
            title = "Filter Expression";
            buttonTitle = "Set";
        }
        GUI.Label (new Rect (10, 30, 100, fHeight), title);
        input = GUI.TextField (new Rect (110, 30, 150, fHeight), input);
        ShowGuiButton (buttonTitle, state);

        if (GUI.Button (new Rect (150, 80, 100, fButtonHeight), "Cancel")) {
            showAuthWindow = false;
            showPamPopupWindow = false;
        }
        GUI.DragWindow (new Rect (0, 0, 800, 400));
    }

    void DoGrantWindow (int windowID)
    {
        string title = "";
        string buttonTitle = "";
        string toggleTitle1 = "";
        string toggleTitle2 = "";
        string labelTitle = "";
        string labelTitle2 = "";
        string labelTitle0 = "";
        int fill = 0;
        if ((state == PubnubState.GrantPresence) || (state == PubnubState.GrantSubscribe)
            ||  (state == PubnubState.CGGrant) ||  (state == PubnubState.CGGrantPresence)
        ) {
            title = "Enter Auth Key";
            toggleTitle1 = " Can Read ";
            toggleTitle2 = " Can Write ";
            labelTitle = "TTL";
            buttonTitle = "Grant";
            labelTitle2 = "Auth Key (optional)";
            labelTitle0 = "Channel";
            if ((state == PubnubState.CGGrant) 
                ||  (state == PubnubState.CGGrantPresence))
            {
                labelTitle0 = "Channel Group";
                toggleTitle2 = " Can Manage ";
            }
        } else if (state == PubnubState.HereNow) {
            title = "Here Now";
            toggleTitle1 = " show uuid ";
            toggle1 = true;
            toggleTitle2 = " include state ";
            labelTitle = "Channel";
            buttonTitle = "Run";
            labelTitle2 = "Channel Grp";
            //valueToSetSubs = "";
        } else if (state == PubnubState.GlobalHereNow) {
            title = "Global Here Now";
            toggleTitle1 = " show uuid ";
            toggle1 = true;
            toggleTitle2 = " include state ";
            buttonTitle = "Run";
        }

        fLeft = fLeftInit;
        fTop = 20;
        toggle1 = GUI.Toggle (new Rect (fLeft, fTop, 95, fButtonHeight), toggle1, toggleTitle1);

        fLeft = fLeftInit + 100;
        toggle2 = GUI.Toggle (new Rect (fLeft, fTop, 95, fButtonHeight), toggle2, toggleTitle2);

        if ((state == PubnubState.GrantPresence) || (state == PubnubState.GrantSubscribe)
            || (state == PubnubState.CGGrant)|| (state == PubnubState.CGGrantPresence)
        ) {
            GUI.Label (new Rect (30, 45, 100, fHeight), labelTitle0);

            pubChannel = GUI.TextField (new Rect (110, 45, 100, fHeight), pubChannel);
            GUI.Label (new Rect (30, 90, 100, fHeight), labelTitle);

            valueToSetSubs = GUI.TextField (new Rect (110, 90, 100, fHeight), valueToSetSubs);
            GUI.Label (new Rect (30, 135, 100, fHeight), labelTitle2);

            valueToSetAuthKey = GUI.TextField (new Rect (110, 135, 100, fHeight), valueToSetAuthKey);
            fill = 90;
        } else if (state == PubnubState.HereNow) {
            GUI.Label (new Rect (30, 45, 100, fHeight), labelTitle);

            valueToSet = GUI.TextField (new Rect (110, 45, 100, fHeight), valueToSet);

            valueToSetSubs = GUI.TextField (new Rect (110, 90, 100, fHeight), valueToSetSubs);
            GUI.Label (new Rect (30, 90, 100, fHeight), labelTitle2);
            fill = 40;
        } else if (state == PubnubState.GlobalHereNow) {
            //no text needed
        }

        if (GUI.Button (new Rect (30, 90 + fill, 100, fButtonHeight), buttonTitle)) {
            if (state == PubnubState.GrantSubscribe) {
                int iTtl;
                Int32.TryParse (valueToSetSubs, out iTtl);
                if (iTtl < 0)
                    iTtl = 1440;
                try {
                    pubnub.GrantAccess<string> (pubChannel, valueToSetAuthKey, toggle1, toggle2, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                } catch (Exception ex) {
                    DisplayErrorMessage (ex.ToString ());
                }
                pubChannel = "";
            } else if (state == PubnubState.GrantPresence) {
                int iTtl;
                Int32.TryParse (valueToSetSubs, out iTtl);
                if (iTtl < 0)
                    iTtl = 1440;
                try {
                    pubnub.GrantPresenceAccess<string> (pubChannel, valueToSetAuthKey, toggle1, toggle2, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                } catch (Exception ex) {
                    DisplayErrorMessage (ex.ToString ());
                }
            } else if (state == PubnubState.CGGrant) {
                int iTtl;
                Int32.TryParse (valueToSetSubs, out iTtl);
                if (iTtl < 0)
                    iTtl = 1440;
                try {
                    pubnub.ChannelGroupGrantAccess<string> (pubChannel, valueToSetAuthKey, toggle1, toggle2, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                } catch (Exception ex) {
                    DisplayErrorMessage (ex.ToString ());
                }
            } else if (state == PubnubState.CGGrantPresence) {
                int iTtl;
                Int32.TryParse (valueToSetSubs, out iTtl);
                if (iTtl < 0)
                    iTtl = 1440;
                try {
                    pubnub.ChannelGroupGrantPresenceAccess<string> (pubChannel, valueToSetAuthKey, toggle1, toggle2, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                } catch (Exception ex) {
                    DisplayErrorMessage (ex.ToString ());
                }
            } else if (state == PubnubState.HereNow) {
                allowUserSettingsChange = false;
                AddToPubnubResultContainer ("Running Here now: " + valueToSet + ":" + valueToSetSubs);
                pubnub.HereNow<string> (valueToSet, valueToSetSubs, toggle1, toggle2, 
                    DisplayReturnMessage, DisplayErrorMessage);

            } else if (state == PubnubState.GlobalHereNow) {
                pubnub.GlobalHereNow<string> (toggle1, toggle2, DisplayReturnMessage, DisplayErrorMessage);
            }

            valueToSet = "";
            valueToSetSubs = "";
            valueToSetAuthKey = "";
            showGrantWindow = false;
            toggle1 = false;
            toggle2 = false;
        }

        if (GUI.Button (new Rect (150, 90 + fill, 100, fButtonHeight), "Cancel")) {
            showGrantWindow = false;
            toggle1 = false;
            toggle2 = false;
        }

        GUI.DragWindow (new Rect (0, 0, 800, 600));
    }

    void DoCGActionWindow (int windowID)
    {
        fLeft = fLeftInit - 10;
        fTop = fTopInit + 10;
        float fButtonWidth = 140;

        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Add Channel to CG")) {
            InstantiatePubnub ();
            showCGPopupWindow = false;
            showTextWindow = true;
            state = PubnubState.AddChannelToChannelGroup;
        }

        fTop = fTopInit + 1 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Remove Ch from CG")) {
            InstantiatePubnub ();
            showCGPopupWindow = false;
            showTextWindow = true;
            state = PubnubState.RemoveChannelFromChannelGroup;
        }

        fTop = fTopInit + 2 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Remove CG")) {
            InstantiatePubnub ();
            state = PubnubState.RemoveChannelGroup;
            showCGPopupWindow = false;
            showAuthWindow = true;
        }

        fTop = fTopInit + 3 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "List channels of CG")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            showAuthWindow = true;
            state = PubnubState.GetChannelsForChannelGroup;
        }

        fTop = fTopInit + 4 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Auth Key")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.AuthKey);
            showAuthWindow = true;
            showCGPopupWindow = false;
            state = PubnubState.AuthKey;
        }
        fTop = fTopInit + 5 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Add/Edit User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.SetUserStateKeyValue);
            showCGPopupWindow = false;
            state = PubnubState.SetUserStateKeyValue;
            showTextWindow = true;
        }
        fTop = fTopInit + 6 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Del User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.DelUserState);
            showCGPopupWindow = false;
            state = PubnubState.DelUserState;
            showTextWindow = true;
        }
        fTop = fTopInit + 7 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Set User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.SetUserStateJson);
            showCGPopupWindow = false;
            state = PubnubState.SetUserStateJson;
            showTextWindow = true;
        }
        fTop = fTopInit + 8 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Get User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GetUserState);
            showCGPopupWindow = false;
            state = PubnubState.GetUserState;
            showTextWindow = true;
        }
        fTop = fTopInit + 9 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence Heartbeat")) {
            InstantiatePubnub ();
            state = PubnubState.PresenceHeartbeat;
            DoAction (state);
            showAuthWindow = true;
            showCGPopupWindow = false;
        }
        fTop = fTopInit + 10 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence Interval")) {
            InstantiatePubnub ();
            state = PubnubState.PresenceInterval;
            DoAction (state);
            showAuthWindow = true;
            showCGPopupWindow = false;
        }
        fTop = fTopInit + 11 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Set Filter Expression")) {
            InstantiatePubnub ();
            state = PubnubState.SetFilterExpression;
            DoAction (state);
            showAuthWindow = true;
            showCGPopupWindow = false;
        }
        fTop = fTopInit + 12 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Get Filter Expression")) {
            InstantiatePubnub ();
            state = PubnubState.GetFilterExpression;
            DoAction (state);
            showCGPopupWindow = false;
        }
    }

    void DoPamActionWindow (int windowID)
    {
        fLeft = fLeftInit - 10;
        fTop = fTopInit + 10;
        float fButtonWidth = 140;

        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Grant Subscribe")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GrantSubscribe);
            state = PubnubState.GrantSubscribe;
            showPamPopupWindow = false;
            showGrantWindow = true;
        }
        fTop = fTopInit + 1 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Audit Subscribe")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            showTextWindow = true;
            state = PubnubState.AuditSubscribe;
        }

        fTop = fTopInit + 2 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke Subscribe")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            showTextWindow = true;
            state = PubnubState.RevokeSubscribe;
        }

        fTop = fTopInit + 3 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Grant Presence")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GrantPresence);
            state = PubnubState.GrantPresence;
            showPamPopupWindow = false;
            showGrantWindow = true;
        }

        fTop = fTopInit + 4 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Audit Presence")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            showTextWindow = true;
            state = PubnubState.AuditPresence;
        }

        fTop = fTopInit + 5 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke Presence")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.RevokePresence);
            showPamPopupWindow = false;
            showTextWindow = true;
            state = PubnubState.RevokePresence;
        }

        fTop = fTopInit + 6 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Grant CG")) {
            InstantiatePubnub ();
            showGrantWindow = true;
            showPamPopupWindow = false;
            state = PubnubState.CGGrant;
        }
        fTop = fTopInit + 7 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Audit CG")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            state = PubnubState.CGAudit;
            showTextWindow = true;
        }
        fTop = fTopInit + 8 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke CG")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            state = PubnubState.CGRevoke;
            showTextWindow = true;
        }

        fTop = fTopInit + 9 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Grant CG Presence")) {
            InstantiatePubnub ();
            showGrantWindow = true;
            showPamPopupWindow = false;
            state = PubnubState.CGGrantPresence;
        }
        fTop = fTopInit + 10 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Audit CG Presence")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            state = PubnubState.CGAuditPresence;
            showTextWindow = true;
        }
        fTop = fTopInit + 11 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke CG Presence")) {
            InstantiatePubnub ();
            showPamPopupWindow = false;
            state = PubnubState.CGRevokePresence;
            showTextWindow = true;
        }
        fTop = fTopInit + 12 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "DH Tests")) {
            InstantiatePubnub ();
            string[] chArr = {"hello_world", "hello_world2", "hello_world3"};
            showPamPopupWindow = false;
            RunDetailedHistoryForMultipleChannels(chArr, 0);
        }
        fTop = fTopInit + 13 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Sub TT Tests")) {
            InstantiatePubnub ();
            pubnub.Subscribe<string>("ch2, ch3", "cg5", "14641779000775299", DisplayReturnMessage, DisplayConnectStatusMessage, null, DisplayErrorMessage);
            showCGPopupWindow = false;
            showPamPopupWindow = false;
        }


    }

    string currentRTT = "RTT";
    public IEnumerator PublishMultiple(){
        for(int i =0; i<=100; i++){
            pubnub.Publish<string>(currentRTT, DateTime.Now.Ticks, (string o) => {print(o);}, DisplayErrorMessage);
            yield return new WaitForSeconds (0.2f);  
        }
        yield return null;
    }

    void RunDetailedHistoryForMultipleChannels(string[] chArr, int pos){
        UnityEngine.Debug.Log (string.Format ("Running DH for channel: {0}", chArr[pos]));

        /*Dictionary<string, string> message = new Dictionary<string, string>();
        message.Add("From", "me");
        message.Add("To", "you");
        message.Add("Message", "the message");
        pubnub.Publish<string>("tc", message, DisplayReturnMessage,DisplayErrorMessage);

        pubnub.DetailedHistory<string> ("tc", 100, 
            (string result) => { 
                UnityEngine.Debug.Log (string.Format ("DisplayHistoryMessage CALLBACK LOG: {0}", result));
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
                {
                    List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                    if (deserializedMessage != null && deserializedMessage.Count > 0)
                    {
                        object[] message2 = (from item in deserializedMessage select item as object).ToArray ();
                        if ((message2 != null)&&(message2.Length >= 0))
                        {
                            IList<object> enumerable = message2 [0] as IList<object>;
                            foreach (object item in enumerable)
                            {
                                UnityEngine.Debug.Log (string.Format ("Message2: {0}", item.ToString()));
                                //IF CUSTOM OBJECT IS EXCEPTED, YOU CAN CAST THIS OBJECT TO YOUR CUSTOM CLASS TYPE
                                try{
                                    Dictionary<string, object> retmessage = (Dictionary<string, object>)item;
                                    UnityEngine.Debug.Log (string.Format ("retmessage count: {0}", retmessage.Count()));
                                UnityEngine.Debug.Log (string.Format ("retmessage 0: {0}", retmessage["From"]));
                                UnityEngine.Debug.Log (string.Format ("retmessage 1: {0}", retmessage["To"]));
                                UnityEngine.Debug.Log (string.Format ("retmessage 2: {0}", retmessage["Message"]));
                                }catch (Exception ex){
                                    UnityEngine.Debug.Log (string.Format ("Exception {0}", ex.ToString()));
                                }

                            }
                        }
                    }
                }
            }, 
            DisplayErrorMessage);*/

        pubnub.Subscribe<string> (currentRTT, "", 
            (string s) => {
                var result = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s);
                long ticks;
                if(long.TryParse(result[0].ToString(), out ticks)){
                    long timetaken = (DateTime.Now.Ticks - ticks)/TimeSpan.TicksPerMillisecond;
                    print(timetaken.ToString());
                }
            }
            , 
            (string o) => {
                StartCoroutine(PublishMultiple());
            }
            , 
            DisplayWildcardReturnMessage, DisplayErrorMessage);
        
        /*pubnub.DetailedHistory<string> (chArr[pos], 100, 
            (string o) => { 
                UnityEngine.Debug.Log (string.Format ("DisplayHistoryMessage CALLBACK LOG: {0}", o));
                AddToPubnubResultContainer (string.Format ("DisplayHistoryMessage CALLBACK: {0}", o));
                if(pos < chArr.Count()-1){
                    pos++;
                    UnityEngine.Debug.Log (string.Format ("Calling pos: {0}", pos));
                    RunDetailedHistoryForMultipleChannels(chArr, pos);
                }
            }, 
            DisplayErrorMessage);*/
    }

    void DoActionWindow (int windowID)
    {
        fLeft = fLeftInit - 10;
        fTop = fTopInit + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Disconnect/Retry")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.DisconnectRetry);
            showActionsPopupWindow = false;
        }
        fTop = fTopInit + 1 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence")) {
            InstantiatePubnub ();
            state = PubnubState.Presence;
            DoAction (PubnubState.Presence);
            showTextWindow = true;
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 2 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Subscribe")) {
            InstantiatePubnub ();
            state = PubnubState.Subscribe;
            DoAction (PubnubState.Subscribe);
            showTextWindow = true;
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 3 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Detailed History")) {
            InstantiatePubnub ();
            showActionsPopupWindow = false;
            showAuthWindow = true;
            state = PubnubState.DetailedHistory;
        }

        fTop = fTopInit + 4 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Publish")) {
            InstantiatePubnub ();
            allowUserSettingsChange = false;
            showActionsPopupWindow = false;
            showPublishPopupWindow = true;
        }

        fTop = fTopInit + 5 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Unsubscribe")) {
            InstantiatePubnub ();
            state = PubnubState.Unsubscribe;
            DoAction (PubnubState.Unsubscribe);
            showTextWindow = true;
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 6 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence-Unsub")) {
            InstantiatePubnub ();
            state = PubnubState.PresenceUnsubscribe;
            DoAction (PubnubState.PresenceUnsubscribe);
            showTextWindow = true;
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 7 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Here Now")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.HereNow);
            state = PubnubState.HereNow;
            showGrantWindow = true;
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 8 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Time")) {
            InstantiatePubnub ();
            DoAction (PubnubState.Time);

            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 9 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Where Now")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.WhereNow);
            showActionsPopupWindow = false;
            state = PubnubState.WhereNow;
            showAuthWindow = true;
        }
        fTop = fTopInit + 10 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Global Here Now")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GlobalHereNow);
            showActionsPopupWindow = false;
            state = PubnubState.GlobalHereNow;
            showGrantWindow = true;
        }

        fTop = fTopInit + 11 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Change UUID")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.ChangeUUID);
            showActionsPopupWindow = false;
            state = PubnubState.ChangeUUID;
            showAuthWindow = true;
        }
        #if(UNITY_IOS || UNITY_ANDROID)
                GUI.enabled = false;
        #endif

        fTop = fTopInit + 12 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Disable Network")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.DisableNetwork);
            showActionsPopupWindow = false;
        }
        fTop = fTopInit + 13 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Enable Network")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.EnableNetwork);
            showActionsPopupWindow = false;
        }

        #if(UNITY_IOS || UNITY_ANDROID)
                GUI.enabled = true;
        #endif

    }

    /// <summary>
    /// Determines whether to send an asynchronous or synchronous call on the button click
    /// Async calls on button click when used in iOS results in random crashes thus sync calls 
    /// are preferred in case iOS
    /// </summary>
    /// <param name="pubnubState">Pubnub state.</param>
    void AsyncOrNonAsyncCall (PubnubState pubnubState)
    {
        DoAction(pubnubState);
    }

    public IEnumerator RunCoroutine (){
        AddToPubnubResultContainer ("Running Subscribe");
        allowUserSettingsChange = false;
        pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
        yield return null;
    }

    void Awake ()
    {
        Application.RegisterLogCallback (new Application.LogCallback (CaptureLogs));
    }

    void CaptureLogs (string condition, string stacktrace, LogType logType)
    {
        StringBuilder sb = new StringBuilder ();
        sb.AppendLine ("Type");
        sb.AppendLine (logType.ToString ());
        sb.AppendLine ("Condition");
        sb.AppendLine (condition);
        sb.AppendLine ("stacktrace");
        sb.AppendLine (stacktrace);
    }

    private void DoAction (object pubnubState)
    {
        try {
            switch((PubnubState)pubnubState){
            case PubnubState.Presence:
            case PubnubState.Subscribe:
            case PubnubState.DetailedHistory:
            case PubnubState.Unsubscribe:
            case PubnubState.PresenceUnsubscribe:
                allowUserSettingsChange = false;
                break;
            case PubnubState.HereNow:
                AddToPubnubResultContainer ("Running Here Now");
                break;    
            case PubnubState.Time:
                AddToPubnubResultContainer ("Running Time");
                allowUserSettingsChange = false;
                pubnub.Time<string> (DisplayReturnMessage, DisplayErrorMessage);
                break;
            case PubnubState.GetFilterExpression:
                AddToPubnubResultContainer ("Running Get Filter Expression");
                allowUserSettingsChange = false;
                AddToPubnubResultContainer (pubnub.FilterExpression.ToString());
                break;
            case PubnubState.GrantSubscribe:
                AddToPubnubResultContainer ("Running Grant Subscribe");
                break;
            case PubnubState.GrantPresence:
                AddToPubnubResultContainer ("Running Grant Presence");
                break;
            case PubnubState.AuthKey:
                AddToPubnubResultContainer ("Running Auth Key");
                break;
            case PubnubState.ChangeUUID:
                AddToPubnubResultContainer ("Changing UUID");
                break;
            case PubnubState.WhereNow:
                AddToPubnubResultContainer ("Running Where Now");
                break;
            case PubnubState.GlobalHereNow:
                AddToPubnubResultContainer ("Running Global Here Now");
                break;
            case PubnubState.PresenceHeartbeat:
                AddToPubnubResultContainer ("Running Presence Heartbeat");
                break;
            case PubnubState.PresenceInterval:
                AddToPubnubResultContainer ("Running Presence Interval");
                break;
            case PubnubState.DisconnectRetry:
                AddToPubnubResultContainer ("Running Disconnect Retry");
                pubnub.TerminateCurrentSubscriberRequest ();
                #if(UNITY_IOS)
                requestInProcess = false;
                #endif
                break;
            case PubnubState.SetUserStateJson:
                break;
            case PubnubState.GetUserState:
                break;
            case PubnubState.GetChannelsForChannelGroup:
                AddToPubnubResultContainer ("List All Channel Groups");
                break;
            default:
                //PubnubState.AuditSubscribe
                //PubnubState.RevokeSubscribe
                break;
            }
        } catch (Exception ex) {
            UnityEngine.Debug.Log ("DoAction exception:" + ex.ToString ());
        }
    }

    void InstantiatePubnub ()
    {
        if (pubnub == null) {
            pubnub = new Pubnub (publishKey, subscribeKey, secretKey, cipherKey, ssl);
            pubnub.SessionUUID = uuid;
            pubnub.SubscribeTimeout = int.Parse (subscribeTimeoutInSeconds);
            pubnub.NonSubscribeTimeout = int.Parse (operationTimeoutInSeconds);
            pubnub.NetworkCheckMaxRetries = int.Parse (networkMaxRetries);
            pubnub.NetworkCheckRetryInterval = int.Parse (networkRetryIntervalInSeconds);
            pubnub.LocalClientHeartbeatInterval = int.Parse (heartbeatIntervalInSeconds);
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
        }
    }

    void DoPublishWindow (int windowID)
    {
        GUI.Label (new Rect (10, 25, 100, 25), "Channel");
        pubChannel = GUI.TextField (new Rect (110, 25, 150, 25), pubChannel);

        GUI.Label (new Rect (10, 60, 100, 25), "Message");

        publishedMessage = GUI.TextField (new Rect (110, 60, 150, 30), publishedMessage);
        GUI.Label (new Rect (10, 95, 100, 25), "Metadata Key");
        publishedMetadataKey = GUI.TextField (new Rect (110, 95, 150, 30), publishedMetadataKey);

        GUI.Label (new Rect (10, 130, 100, 25), "Metadata Value");
        publishedMetadataValue = GUI.TextField (new Rect (110, 130, 150, 30), publishedMetadataValue);

        storeInHistory = GUI.Toggle (new Rect (10, 165, 150, 25), storeInHistory, "Store in History");

        string stringMessage = publishedMessage;
        if (GUI.Button (new Rect (30, 185, 100, 30), "Publish")) {
            //stringMessage = "Text with 😜 emoji 🎉.";
            //stringMessage = "{'operation':'ReturnData','channel':'Mobile1','sequenceNumber':0,'data':['ping 1.0.0.1']}";
            Dictionary<string, string> metadataDict = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(publishedMetadataKey) && (!string.IsNullOrEmpty(publishedMetadataValue))){
                metadataDict.Add (publishedMetadataKey, publishedMetadataValue);
            }
            pubnub.Publish<string> (pubChannel, stringMessage, storeInHistory, metadataDict, DisplayReturnMessage, DisplayErrorMessage);

            publishedMessage = "";
            showPublishPopupWindow = false;
            pubChannel = "";
            publishedMetadataKey = "";
            publishedMetadataValue = "";
        }

        if (GUI.Button (new Rect (150, 185, 100, 30), "Cancel")) {
            showPublishPopupWindow = false;
        }
        GUI.DragWindow (new Rect (0, 0, 800, 610));
    }

    void Start ()
    {
        //System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
    }

    private static bool ValidateServerCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0) {
            if (chain != null && chain.ChainStatus != null) {
                X509Certificate2 cert2 = new X509Certificate2 (certificate);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                //chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                //chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(1000);
                //chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                //chain.ChainPolicy.VerificationTime = DateTime.Now;
                chain.Build (cert2);

                foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus) {
                    if ((certificate.Subject == certificate.Issuer) &&
                        (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot)) {
                        // Self-signed certificates with an untrusted root are valid. 
                        continue;
                    } else {
                        if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError) {
                            // If there are any other errors in the certificate chain, the certificate is invalid,
                            // so the method returns false.
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Do not allow this client to communicate with unauthenticated servers. 
        return false;
    }

    void Update ()
    {
        try {
            System.Text.StringBuilder sbResult = new System.Text.StringBuilder ();

            int existingLen = pubnubApiResult.Length;
            int newRecordLen = 0;
            sbResult.Append (pubnubApiResult);

            if(recordQueue.Count > 0){
                string currentRecord = "";
                lock(recordQueue){
                    do
                    {
                        currentRecord = recordQueue.Dequeue();
                        sbResult.AppendLine (currentRecord);
                    } while (recordQueue.Count != 0);
                }

                pubnubApiResult = sbResult.ToString ();

                newRecordLen = pubnubApiResult.Length - existingLen;
                int windowLength = 600;

                if (pubnubApiResult.Length > windowLength) {
                    bool trimmed = false;
                    if (pubnubApiResult.Length > windowLength) {
                        trimmed = true;
                        int lengthToTrim = (((pubnubApiResult.Length - windowLength) < pubnubApiResult.Length - newRecordLen) ? pubnubApiResult.Length - windowLength : pubnubApiResult.Length - newRecordLen);
                        pubnubApiResult = pubnubApiResult.Substring (lengthToTrim);
                    }
                    if (trimmed) {
                        string prefix = "Output trimmed...\n";

                        pubnubApiResult = prefix + pubnubApiResult;
                    }
                }
            } 
        } catch (Exception ex) {
            Debug.Log ("Update exception:" + ex.ToString ());
        }
    }

    void OnApplicationQuit ()
    {
        if (pubnub != null) {
            pubnub.CleanUp ();
        }
    }

    void ResetPubnubInstance ()
    {
        if (pubnub != null) {
            pubnub.EndPendingRequests ();
            pubnub.CleanUp ();
            pubnub = null;
        }
    }

    void  DisplayReturnMessageHistory (string result)
    {
        UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));
        if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
        {
            List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
            if (deserializedMessage != null && deserializedMessage.Count > 0)
            {
                object[] message = (from item in deserializedMessage select item as object).ToArray ();
                if ((message != null)&&(message.Length >= 0)){
                    {
                        IList<object> enumerable = message [0] as IList<object>;
                        foreach (object item in enumerable)
                        {
                            UnityEngine.Debug.Log (string.Format ("Message: {0}", item.ToString()));
                            //IF CUSTOM OBJECT IS EXCEPTED, YOU CAN CAST THIS OBJECT TO YOUR CUSTOM CLASS TYPE
                        }
                    }
                }
            }
        }
    }

    void DisplayReturnMessageSubscribe (object result){
        PNMessageResult pnMessageResult = result as PNMessageResult;

        UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Payload));
        UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Channel));
        UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Subscription));
        UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.OriginatingTimetoken));
        UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Timetoken));
        UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.UserMetadata.ToString()));


        AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Payload));
        AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Channel));
        AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Subscription));
        AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.OriginatingTimetoken));

        AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.Timetoken));
        AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeObject: {0}", pnMessageResult.UserMetadata.ToString()));

    }

    void DisplayReturnMessageSubscribe (string result){
        object obj = pubnub.JsonPluggableLibrary.DeserializeToObject(result);
        Dictionary<string, object> dict = obj as Dictionary<string, object>;
        UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString Object: result:{0}\nobj:{1}\nCount:{2}\n", result,
            obj.ToString(), dict.Count));

        foreach(var pair in dict){
            UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString pair.Key: {0}, pair.Value:{1}", 
                pair.Key, pair.Value));
            AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeString:- {0}:{1}", pair.Key, pair.Value));
        }

        //PNMessageResult pnMessageResult = (PNMessageResult)Convert.ChangeType(obj, typeof(PNMessageResult));
        //UnityEngine.Debug.Log (string.Format ("DisplayReturnMessageSubscribeString: {0}", pnMessageResult.Payload));
        //AddToPubnubResultContainer (string.Format ("DisplayReturnMessageSubscribeString: {0}", pnMessageResult.Payload));
    }

    void DisplayConnectMessage (string result){
        UnityEngine.Debug.Log (string.Format ("DisplayConnectMessage: {0}", result));
        AddToPubnubResultContainer (string.Format ("DisplayConnectMessage: {0}", result));
    }

    void DisplayConnectMessage (object result){
        UnityEngine.Debug.Log (string.Format ("DisplayConnectMessage: {0}", result.ToString()));
        AddToPubnubResultContainer (string.Format ("DisplayConnectMessage: {0}", result.ToString()));
    }

    void DisplayReturnMessage (string result)
    {
        UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));
    }

    void DisplayReturnMessage (object result)
    {
        UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));
    }

    void DisplayWildcardReturnMessage (string result)
    {
        UnityEngine.Debug.Log (string.Format ("Wildcard CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("Wildcard CALLBACK: {0}", result));
    }

    void DisplayWildcardReturnMessage (object result)
    {
        var myList = result as List<object>;
        var stringList = myList.OfType<string>();
        string result2 = string.Join(",", stringList.ToArray());

        UnityEngine.Debug.Log (string.Format ("Wildcard CALLBACK LOG: {0}", result2));
        AddToPubnubResultContainer (string.Format ("Wildcard CALLBACK: {0}", result2));
    }

    void DisplayDisconnectReturnMessage (string result)
    {
        UnityEngine.Debug.Log (string.Format ("Disconnect CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("Disconnect CALLBACK: {0}", result));
        //pubnub.EndPendingRequests ();
    }

    void DisplayReturnMessageObj (object result)
    {
        //print(result);
        var myList = result as List<object>;
        var stringList = myList.OfType<string>();
        string result2 = string.Join(",", stringList.ToArray());
        UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result2));
        AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result2));
    }

    void DisplayConnectStatusMessage (string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format ("CONNECT CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("CONNECT CALLBACK: {0}", result));
        //pubnub.HereNow<string> ("hello_world2", true, true, DisplayReturnMessage, DisplayErrorMessage);
    }

    void DisplayConnectStatusMessageObj (object result)
    {
        var myList = result as List<object>;
        var stringList = myList.OfType<string>();
        //var stringList = (from o in result 
        //      select o.ToString()).ToList();
        //string result2 = string.Join(",", stringList.ToArray());
        string result2 = string.Join(",", stringList.ToArray());//myList.OfType<string>();

        UnityEngine.Debug.Log (string.Format ("CONNECT CALLBACK LOG: {0}", result2));
        AddToPubnubResultContainer (string.Format ("CONNECT CALLBACK: {0}", result2));
    }

    void DisplayDisconnectStatusMessage (string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format ("DISCONNECT CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("DISCONNECT CALLBACK: {0}", result));
        //pubnub.EndPendingRequests ();
    }

    void DisplayErrorMessage (string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format ("ERROR CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("ERROR CALLBACK: {0}", result));
    }

    /// <summary>
    /// Callback method for error messages
    /// </summary>
    /// <param name="result"></param>
    void DisplayErrorMessage (PubnubClientError result)
    {
        UnityEngine.Debug.Log (result.Description);

        switch (result.StatusCode) {
        case 103:
            //Warning: Verify origin host name and internet connectivity
            break;
        case 104:
            //Critical: Verify your cipher key
            break;
        case 106:
            //Warning: Check network/internet connection
            break;
        case 108:
            //Warning: Check network/internet connection
            break;
        case 109:
            //Warning: No network/internet connection. Please check network/internet connection
            break;
        case 110:
            //Informational: Network/internet connection is back. Active subscriber/presence channels will be restored.
            break;
        case 111:
            //Informational: Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing.
            break;
        case 112:
            //Informational: Channel Already Subscribed/Presence Subscribed. Duplicate channel subscription not allowed
            break;
        case 113:
            //Informational: Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed
            break;
        case 114:
            //Warning: Please verify your cipher key
            break;
        case 115:
            //Warning: Protocol Error. Please contact PubNub with error details.
            break;
        case 116:
            //Warning: ServerProtocolViolation. Please contact PubNub with error details.
            break;
        case 117:
            //Informational: Input contains invalid channel name
            break;
        case 118:
            //Informational: Channel not subscribed yet
            break;
        case 119:
            //Informational: Channel not subscribed for presence yet
            break;
        case 120:
            //Informational: Incomplete unsubscribe. Try again for unsubscribe.
            break;
        case 121:
            //Informational: Incomplete presence-unsubscribe. Try again for presence-unsubscribe.
            break;
        case 122:
            //Informational: Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.
            break;
        case 123:
            //Informational: During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.
            break;
        case 124:
            //Informational: During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.
            break;
        case 125:
            //Informational: Publish operation timeout occured.
            break;
        case 126:
            //Informational: HereNow operation timeout occured
            break;
        case 127:
            //Informational: Detailed History operation timeout occured
            break;
        case 128:
            //Informational: Time operation timeout occured
            break;
        case 4000:
            //Warning: Message too large. Your message was not sent. Try to send this again smaller sized
            break;
        case 4001:
            //Warning: Bad Request. Please check the entered inputs or web request URL
            break;
        case 4002:
            //Warning: Invalid Key. Please verify the publish key
            break;
        case 4010:
            //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
            break;
        case 4020:
            // PAM is not enabled. Please contact PubNub support
            break;
        case 4030:
            //Warning: Not authorized. Check the permimissions on the channel. Also verify authentication key, to check access.
            break;
        case 4031:
            //Warning: Incorrect public key or secret key.
            break;
        case 4140:
            //Warning: Length of the URL is too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list
            break;
        case 5000:
            //Critical: Internal Server Error. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
            break;
        case 5020:
            //Critical: Bad Gateway. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
            break;
        case 5030:
            //"Service Unavailable. Please try again. If the issue continues, please contact PubNub support"
            break;
        case 5040:
            //Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
            break;
        case 0:
            //Undocumented error. Please contact PubNub support with full error object details for further investigation
            break;
        default:
            break;
        }

        if (showErrorMessageSegments) {
            DisplayErrorMessageSegments (result);
        }
    }

    void DisplayErrorMessageSegments (PubnubClientError pubnubError)
    {
        StringBuilder errorMessageBuilder = new StringBuilder ();
        errorMessageBuilder.AppendFormat("<STATUS CODE>: {0}\n", pubnubError.StatusCode); // Unique ID of Error

        errorMessageBuilder.AppendFormat("<MESSAGE>: {0}\n", pubnubError.Message); // Message received from server/clent or from .NET exception
        AddToPubnubResultContainer (string.Format ("Error: {0}\n", pubnubError.Message));
        errorMessageBuilder.AppendFormat("<SEVERITY>: {0}\n", pubnubError.Severity); // Info can be ignored, Warning and Error should be handled

        if (pubnubError.DetailedDotNetException != null) {
            //Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
            errorMessageBuilder.AppendFormat("<DETAILED DOT.NET EXCEPTION>: {0}\n", pubnubError.DetailedDotNetException.ToString ()); // Full Details of .NET exception
        }
        errorMessageBuilder.AppendFormat("<MESSAGE SOURCE>: {0}\n", pubnubError.MessageSource); // Did this originate from Server or Client-side logic
        if (pubnubError.PubnubWebRequest != null) {
            //Captured Web Request details
            errorMessageBuilder.AppendFormat("<HTTP WEB REQUEST>: {0}\n", pubnubError.PubnubWebRequest.RequestUri.ToString ()); 
            errorMessageBuilder.AppendFormat("<HTTP WEB REQUEST - HEADERS>: {0}\n", pubnubError.PubnubWebRequest.Headers.ToString ()); 
        }
        if (pubnubError.PubnubWebResponse != null) {
            //Captured Web Response details
            errorMessageBuilder.AppendFormat("<HTTP WEB RESPONSE - HEADERS>: {0}\n", pubnubError.PubnubWebResponse.Headers.ToString ());
        }
        errorMessageBuilder.AppendFormat("<DESCRIPTION>: {0}\n", pubnubError.Description); // Useful for logging and troubleshooting and support
        AddToPubnubResultContainer (string.Format ("DESCRIPTION: {0}\n", pubnubError.Description));
        errorMessageBuilder.AppendFormat("<CHANNEL>: {0}\n", pubnubError.Channel); //Channel name(s) at the time of error
        errorMessageBuilder.AppendFormat("<DATETIME>: {0}\n", pubnubError.ErrorDateTimeGMT); //GMT time of error
        UnityEngine.Debug.Log (errorMessageBuilder.ToString());
    }

    void AddToPubnubResultContainer (string result)
    {
        lock (recordQueue) {
            recordQueue.Enqueue (result);
        }
    }
}


