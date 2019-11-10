using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bryllite
{
    public static class BsonConverter
    {
        public static byte[] ToByteArray<T>(T value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BsonDataWriter wr = new BsonDataWriter(ms))
                {
                    new JsonSerializer().Serialize(wr, value);
                    return ms.ToArray();
                }
            }
        }

        public static T FromByteArray<T>(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BsonDataReader rd = new BsonDataReader(ms))
                {
                    return new JsonSerializer().Deserialize<T>(rd);
                }
            }
        }
    }
}
