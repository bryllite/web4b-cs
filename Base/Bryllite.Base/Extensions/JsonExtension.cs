using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                else if (value is bool b) o[key] = b;
                else if (value is byte by) o[key] = by;
                else if (value is sbyte sb) o[key] = sb;
                else if (value is char c) o[key] = c;
                else if (value is short s) o[key] = s;
                else if (value is ushort us) o[key] = us;
                else if (value is int i) o[key] = i;
                else if (value is uint ui) o[key] = ui;
                else if (value is long l) o[key] = l;
                // BSON does not support unsigned long type!
                else if (value is ulong ul) o[key] = ul.ToString();
                else if (value is float f) o[key] = f;
                else if (value is double d) o[key] = d;
                else if (value is decimal de) o[key] = de;
                else if (value is string str) o[key] = str;
                else if (value is IEnumerable<byte> bytes) o[key] = bytes.ToArray();
                else o[key] = JToken.FromObject(value);

                return true;
            }
            catch
            {
                return false;
            }
        }


        public static JObject FromFile(string file)
        {
            try
            {
                return JObject.Parse(File.ReadAllText(file));
            }
            catch
            {
                return null;
            }
        }
    }
}
