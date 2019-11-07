using System;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace Bryllite.Utils.JsonRpc
{
    /// <summary>
    /// jsonrpc 2.0 class
    /// </summary>
    public class JsonRpc : JObject
    {
        public const string JSONRPC = "jsonrpc";
        public const string VERSION = "2.0";
        public const string ID = "id";
        public const string METHOD = "method";
        public const string PARAMS = "params";
        public const string RESULT = "result";
        public const string ERROR = "error";
        public const string CODE = "code";
        public const string MESSAGE = "message";
        public const string DATA = "data";

        // jsonrpc object type
        public enum ObjectType
        {
            Unknown,
            Request,
            Notification,
            Response,
            Error
        }

        // version
        public string Ver => Get<string>(JSONRPC);

        // id
        public int? Id => Get<int?>(ID);

        // method
        public string Method => Get<string>(METHOD);

        // params
        public JArray GetParams()
        {
            return Get<JArray>(PARAMS);
        }
        public int ParamsCount => GetParams()?.Count ?? 0;


        // result
        public JArray GetResult()
        {
            return Get<JArray>(RESULT);
        }
        public int ResultCount => GetResult()?.Count ?? 0;

        // error
        public bool HasError => ContainsKey(ERROR);
        public string ErrorMessage
        {
            get
            {
                var error = Get<JObject>(ERROR);
                return !ReferenceEquals(error, null) ? error.Value<int>(CODE) + ":" + error.Value<string>(MESSAGE) : string.Empty;
            }
        }

        public JsonRpc() : base()
        {
            // "jsonrpc" = "2.0"
            Put(JSONRPC, VERSION);
        }

        // request object
        public JsonRpc(string method, int? id) : this()
        {
            Put(ID, id);
            Put(METHOD, method);
        }

        // notification object
        public JsonRpc(string method) : this()
        {
            Put(METHOD, method);
        }

        // response object
        public JsonRpc(int? id) : this()
        {
            Put(ID, id);
        }

        // error object
        public JsonRpc(int? id, int code, string message, object data) : this()
        {
            Put(ID, id);

            // error
            Put(ERROR, new { code = code, message = message, data = JToken.FromObject(data) });
        }

        // error object
        public JsonRpc(int? id, int code, string message) : this()
        {
            Put(ID, id);
            Put(ERROR, new { code = code, message = message });
        }

        private JsonRpc(JObject other) : base(other)
        {
            if (Ver != VERSION) throw new Exception("incorrect jsonrpc version");
        }

        private void Put<T>(string key, T value)
        {
            this[key] = !ReferenceEquals(value, null) ? JToken.FromObject(value) : null;
        }

        private T Get<T>(string key, T def)
        {
            return ContainsKey(key) ? this[key].ToObject<T>() : def;
        }

        private T Get<T>(string key)
        {
            return Get(key, default(T));
        }

        // add params
        public JsonRpc AddParams(params object[] args)
        {
            if (!ReferenceEquals(args, null) && args.Length > 0)
            {
                var parameters = new JArray(args);
                var newParams = GetParams();
                if (ReferenceEquals(newParams, null)) newParams = parameters;
                else newParams.Merge(parameters);

                Put(PARAMS, newParams);
            }
            return this;
        }

        // get params
        public T Params<T>(int idx, T def)
        {
            try
            {
                return GetParams()[idx].Value<T>();
            }
            catch
            {
                return def;
            }
        }

        public T Params<T>(int idx)
        {
            return Params(idx, default(T));
        }

        // add result
        public JsonRpc AddResult(params object[] args)
        {
            var result = new JArray(args);
            var newResult = GetResult();
            if (ReferenceEquals(newResult, null)) newResult = result;
            else newResult.Merge(result);

            Put(RESULT, newResult);
            return this;
        }

        // get result
        public T Result<T>(int idx, T def)
        {
            try
            {
                return GetResult()[idx].Value<T>();
            }
            catch
            {
                return def;
            }
        }

        public T Result<T>(int idx)
        {
            return Result(idx, default(T));
        }


        public new static JsonRpc Parse(string json)
        {
            return new JsonRpc(JObject.Parse(json));
        }

        public static bool TryParse(string json, out JsonRpc jsonrpc)
        {
            try
            {
                jsonrpc = Parse(json);
                return true;
            }
            catch
            {
                jsonrpc = null;
                return false;
            }
        }

        public static JsonRpc Parse(byte[] bytes)
        {
            return new JsonRpc(BsonConverter.FromByteArray<JObject>(bytes));
        }

        public static bool TryParse(byte[] bytes, out JsonRpc jsonrpc)
        {
            try
            {
                jsonrpc = Parse(bytes);
                return true;
            }
            catch
            {
                jsonrpc = null;
                return false;
            }
        }


        public ObjectType GetObjectType()
        {
            // request / notification
            if (ContainsKey(METHOD)) return ContainsKey(ID) ? ObjectType.Request : ObjectType.Notification;

            // response
            if (ContainsKey(ID) && ContainsKey(RESULT)) return ObjectType.Response;

            // error
            if (HasError) return ObjectType.Error;

            return ObjectType.Unknown;
        }

        public byte[] ToByteArray()
        {
            return BsonConverter.ToByteArray<JObject>(this);
        }

        public static implicit operator byte[] (JsonRpc jsonrpc)
        {
            return jsonrpc.ToByteArray();
        }

        public static implicit operator string(JsonRpc jsonrpc)
        {
            return jsonrpc.ToString();
        }

        // request object
        public class Request : JsonRpc
        {
            public Request(string method, int id) : base(method, id)
            {
            }

            public Request(string method, int id, params object[] parameters) : this(method, id)
            {
                AddParams(parameters);
            }
        }

        // notification object
        public class Notification : JsonRpc
        {
            public Notification(string method) : base(method)
            {
            }

            public Notification(string method, params object[] parameters) : this(method)
            {
                AddParams(parameters);
            }
        }

        // response object
        public class Response : JsonRpc
        {
            public Response(int? id) : base(id)
            {
            }

            public Response(int? id, params object[] results) : this(id)
            {
                AddResult(results);
            }
        }

        // error object
        public class Error : JsonRpc
        {
            public Error(int? id, int code, string message, object data) : base(id, code, message, data)
            {
            }

            public Error(int? id, int code, string message) : base(id, code, message)
            {
            }
        }

        // batch object
        public class Batch : JArray
        {
            public Batch(params JsonRpc[] requests) : base()
            {
                foreach (var request in requests)
                    Add(request);
            }

            private Batch(JArray array) : base(array)
            {
            }

            public static implicit operator string(Batch batch)
            {
                return batch.ToString();
            }

            public new static Batch Parse(string json)
            {
                return new Batch(JArray.Parse(json));
            }

            public static bool TryParse(string json, out Batch batch)
            {
                try
                {
                    batch = Parse(json);
                    return true;
                }
                catch
                {
                    batch = null;
                    return false;
                }
            }
        }
    }
}
