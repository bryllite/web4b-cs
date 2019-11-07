using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Bryllite.Utils.Pbkdf.DTO
{
    public class KeyStoreV3<TKdfParams> where TKdfParams : KdfParams, new()
    {
        public class Crypto
        {
            public string ciphertext;
            public CipherParams cipherparams = new CipherParams();
            public string cipher;
            public string kdf;
            public TKdfParams kdfparams = new TKdfParams();
            public string mac;
        }

        public int version;
        public string id;
        public string address;
        public Crypto crypto = new Crypto();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
