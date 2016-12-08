#if((!USE_JSONFX_UNITY_IOS) && (!USE_MiniJSON))
#define USE_JSONFX_UNITY_IOS
#endif

#if (USE_JSONFX_UNITY_IOS)
using Pathfinding.Serialization.JsonFx;
#elif (USE_MiniJSON)
using MiniJSON;
#endif

using System;
using System.Collections.Generic;
using System.Linq;

namespace PubNubMessaging.Core
{
    #region "Json Pluggable Library"
    public interface IJsonPluggableLibrary
    {
        bool IsArrayCompatible (string jsonString);

        bool IsDictionaryCompatible (string jsonString);

        string SerializeToJsonString (object objectToSerialize);

        List<object> DeserializeToListOfObject (string jsonString);

        object DeserializeToObject (string jsonString);

        Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString);
    }

    public static class JSONSerializer{
        private static IJsonPluggableLibrary jsonPluggableLibrary = null;
        public static IJsonPluggableLibrary JsonPluggableLibrary{
            get {
                #if (USE_MiniJSON)
                #if (ENABLE_PUBNUB_LOGGING)
                LoggingMethod.WriteToLog("JSON LIB: USE_MiniJSON", LoggingMethod.LevelInfo);
                #endif
                jsonPluggableLibrary = new MiniJSONObjectSerializer();
                #elif (USE_JSONFX_UNITY_IOS)
                #if (ENABLE_PUBNUB_LOGGING)
                LoggingMethod.WriteToLog ("JSON LIB: USE_JSONFX_UNITY_IOS", LoggingMethod.LevelInfo);
                #endif
                jsonPluggableLibrary = new JsonFxUnitySerializer ();
                #endif
                return jsonPluggableLibrary;
            }
        }

    }

    #if (USE_JSONFX_UNITY_IOS)
    public class JsonFxUnitySerializer : IJsonPluggableLibrary
    {
        public bool IsArrayCompatible (string jsonString)
        {
            return false;
        }

        public bool IsDictionaryCompatible (string jsonString)
        {
            return true;
        }

        public string SerializeToJsonString (object objectToSerialize)
        {
            string json = JsonWriter.Serialize (objectToSerialize); 
            return PubnubCryptoBase.ConvertHexToUnicodeChars (json);
        }

        public List<object> DeserializeToListOfObject (string jsonString)
        {
            #if (ENABLE_PUBNUB_LOGGING)
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, DeserializeToListOfObject: jsonString: {1}", 
                        DateTime.Now.ToString (), jsonString), 
                        LoggingMethod.LevelInfo);
            #endif
        
            var output = JsonReader.Deserialize<object[]> (jsonString) as object[];
            List<object> messageList = output.Cast<object> ().ToList ();
            return messageList;
        }

        public object DeserializeToObject (string jsonString)
        {
            #if (ENABLE_PUBNUB_LOGGING)
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, DeserializeToObject: jsonString: {1}", 
                        DateTime.Now.ToString (), jsonString), 
                        LoggingMethod.LevelInfo);
            #endif
        
            var output = JsonReader.Deserialize<object> (jsonString) as object;
            return output;
        }

        public T Deserialize<T> (string jsonString)
        {
            var output = JsonReader.Deserialize<T> (jsonString);
            return output;
        }

        public Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString)
        {
            object obj = DeserializeToObject (jsonString);
            Dictionary<string, object> stateDictionary = new Dictionary<string, object> ();
            Dictionary<string, object> message = (Dictionary<string, object>)obj;
            if (message != null) {
                foreach (KeyValuePair<String, object> kvp in message) {
                    stateDictionary.Add (kvp.Key, kvp.Value);
                }
            }
            return stateDictionary;
        }
    }
    #elif (USE_MiniJSON)
    public class MiniJSONObjectSerializer : IJsonPluggableLibrary
    {
        public bool IsArrayCompatible (string jsonString)
        {
            return jsonString.Trim().StartsWith("[");
        }

        public bool IsDictionaryCompatible (string jsonString)
        {
            return jsonString.Trim().StartsWith("{");
        }

        public string SerializeToJsonString (object objectToSerialize)
        {
            string json = Json.Serialize (objectToSerialize); 
            return PubnubCryptoBase.ConvertHexToUnicodeChars (json);
        }

        public List<object> DeserializeToListOfObject (string jsonString)
        {
            return Json.Deserialize (jsonString) as List<object>;
        }

        public object DeserializeToObject (string jsonString)
        {
            return Json.Deserialize (jsonString) as object;
        }

        public Dictionary<string, object> DeserializeToDictionaryOfObject (string jsonString)
        {
            return Json.Deserialize (jsonString) as Dictionary<string, object>;
        }
    }
    #endif
    #endregion
}

