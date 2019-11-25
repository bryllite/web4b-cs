using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.AppBase
{
    public class CommandLineParser : Dictionary<string, string>
    {
        // arguments
        private string[] args;

        public CommandLineParser() : base()
        {
        }

        public CommandLineParser(string[] args) : this()
        {
            this.args = args;

            foreach (var arg in args)
            {
                string[] kv = arg.Split('=');

                string key = kv[0].Trim(' ', '-');
                string value = kv.Length > 1 ? kv[1].Trim() : string.Empty;

                this[key] = value;    
            }
        }

        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }

        public override string ToString()
        {
            return string.Join(" ", args);
        }

        public bool Has(string key)
        {
            return ContainsKey(key);
        }

        public string Value(string key)
        {
            return Value<string>(key);
        }

        public T Value<T>(string key)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)Has(key);

            return TryGetValue(key, out var value) ? ((JToken)value).Value<T>() : default(T);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            try
            {
                value = Value<T>(key);
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }
    }
}
