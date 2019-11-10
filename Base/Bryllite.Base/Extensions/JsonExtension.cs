using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Extensions
{
    public static class JsonExtension
    {
        // jobject -> byte array
        public static byte[] ToByteArray(this JObject o)
        {
            return BsonConverter.ToByteArray(o);
        }

        // byte array -> jobject
        public static JObject ToJObject(this byte[] bytes)
        {
            return BsonConverter.FromByteArray<JObject>(bytes);
        }

        // get value
        public static T Get<T>(this JObject o, string key, T def)
        {
            try
            {
                if (!o.ContainsKey(key))
                    return def;

                return o[key].ToObject<T>();
            }
            catch
            {
                return def;
            }
        }

        public static T Get<T>(this JObject o, string key)
        {
            return Get(o, key, default(T));
        }

        // put value
        public static bool Put<T>(this JObject o, string key, T value)
        {
            try
            {
                if (ReferenceEquals(value, null)) o[key] = null;
                else o[key] = value is Hex hex ? hex.ToString(true) : JToken.FromObject(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
