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

        // indexer
        public string this[int idx]
        {
            get
            {
                return args.Length > idx ? args[idx] : null;
            }
        }

        public CommandLineParser(bool casesensitive = true) : base(casesensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
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

        public bool Contains(string key)
        {
            return ContainsKey(key);
        }

        public T Value<T>(string key, T def = default(T))
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)Contains(key);

            return TryGetValue(key, out var value) ? ((JToken)value).Value<T>() : def;
        }

        public string Value(string key)
        {
            return Value<string>(key, default(string));
        }

        public string Value(string key, string def)
        {
            return Value<string>(key, def);
        }
    }
}
