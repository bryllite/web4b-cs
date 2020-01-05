using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Utils.Pbkdf.Crypto;
using Bryllite.Utils.Pbkdf.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bryllite.Utils.Pbkdf
{
    /// <summary>
    /// key store service
    /// </summary>
    public class KeyStoreService
    {
        public static readonly int Version = 3;
        public static readonly string CIPHER = "aes-128-ctr";

        // kdf type
        public enum KdfType
        {
            pbkdf2,
            scrypt
        }

        public KeyStoreService()
        {
        }

        // json file 에서 kdf type을 얻는다.
        public static KdfType GetKdfTypeFromJson(string json)
        {
            try
            {
                var doc = JObject.Parse(json);
                JObject crypto = (JObject)doc.GetValue("crypto", StringComparison.OrdinalIgnoreCase);
                string kdf = crypto.GetValue("kdf", StringComparison.OrdinalIgnoreCase).ToString();

                foreach (KdfType type in Enum.GetValues(typeof(KdfType)))
                    if (type.ToString().Equals(kdf, StringComparison.OrdinalIgnoreCase)) return type;
            }
            catch (Exception ex)
            {
                throw new FormatException("json format exception", ex);
            }

            throw new FormatException("unknown kdf");
        }

        // json file에서 address를 얻는다.
        public static string GetAddressFromJson(string json)
        {
            try
            {
                var doc = JObject.Parse(json);
                return doc.GetValue("address", StringComparison.OrdinalIgnoreCase).ToString();
            }
            catch (Exception ex)
            {
                throw new FormatException("address not parsable", ex);
            }
        }

        public static string GetKeyStoreFileNameFor(string address, string ext)
        {
            if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address));
            return "UTC--" + DateTime.Now.ToString("s").Replace(":", "-") + "--" + address + ext;
        }


        public static string EncryptKeyStoreV3(PrivateKey key, string password)
        {
            return EncryptKeyStoreV3(key, password, ScryptParams.Default);
        }

        public static string EncryptKeyStoreV3(PrivateKey key, string password, ScryptParams kdfParams)
        {
            return EncryptKey(key.ToByteArray(), key.Address, password, kdfParams).ToJson();
        }

        public static string EncryptKeyStoreV3(PrivateKey key, string password, Pbkdf2Params kdfParams)
        {
            return EncryptKey(key.ToByteArray(), key.Address, password, kdfParams).ToJson();
        }


        public static byte[] DecryptKeyStoreV3(string json, string password)
        {
            KdfType kdf;
            try
            {
                kdf = GetKdfTypeFromJson(json);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("kdf not parsable", ex);
            }

            switch (kdf)
            {
                case KdfType.scrypt: return DecryptKey(JsonConvert.DeserializeObject<KeyStoreV3<ScryptParams>>(json), password);
                case KdfType.pbkdf2: return DecryptKey(JsonConvert.DeserializeObject<KeyStoreV3<Pbkdf2Params>>(json), password);
                default: break;
            }

            throw new Exception("unsupported kdf");
        }


        public static byte[] DecryptKeyStoreV3FromFile(string file, string password)
        {
            return DecryptKeyStoreV3(File.ReadAllText(file), password);
        }


        private static KeyStoreV3<ScryptParams> EncryptKey(byte[] key, string address, string password, ScryptParams kdfParams)
        {
            if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));
            if (password.IsNullOrEmpty()) throw new ArgumentNullException(nameof(password));

            // random values ( salt, iv )
            var salt = kdfParams.salt;
            var cipherParams = new CipherParams();

            // derivedKey -> cipherKey -> cipherText -> mac
            var derivedKey = PbkdfCrypt.GenerateDerivedScryptKey(password, salt.ToByteArray(), kdfParams.n, kdfParams.r, kdfParams.p, kdfParams.dklen);
            var cipherKey = PbkdfCrypt.GenerateCipherKey(derivedKey);
            var cipherText = PbkdfCrypt.GenerateAesCtrCipher(cipherParams.iv.ToByteArray(), cipherKey, key);
            var mac = PbkdfCrypt.GenerateMac(derivedKey, cipherText);

            return new KeyStoreV3<ScryptParams>()
            {
                version = Version,
                id = Guid.NewGuid().ToString(),
                address = address,
                crypto = {
                    ciphertext = cipherText.ToHexString(),
                    cipherparams = cipherParams,
                    cipher = CIPHER,
                    kdf = KdfType.scrypt.ToString(),
                    kdfparams = kdfParams,
                    mac = mac.ToHexString()
                }
            };
        }

        private static KeyStoreV3<Pbkdf2Params> EncryptKey(byte[] key, string address, string password, Pbkdf2Params kdfParams)
        {
            if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));
            if (password.IsNullOrEmpty()) throw new ArgumentNullException(nameof(password));
            // unsupported prf
            if (kdfParams.prf != Pbkdf2Params.HMACSHA256) throw new ArgumentException("unsupported kdfparams.prf");

            // random values ( salt, iv )
            var salt = kdfParams.salt;
            var cipherParams = new CipherParams();

            // derivedKey -> cipherKey -> cipherText -> mac
            var derivedKey = PbkdfCrypt.GeneratePbkdf2Sha256DerivedKey(password, salt.ToByteArray(), kdfParams.c, kdfParams.dklen);
            var cipherKey = PbkdfCrypt.GenerateCipherKey(derivedKey);
            var cipherText = PbkdfCrypt.GenerateAesCtrCipher(cipherParams.iv.ToByteArray(), cipherKey, key);
            var mac = PbkdfCrypt.GenerateMac(derivedKey, cipherText);

            return new KeyStoreV3<Pbkdf2Params>()
            {
                version = Version,
                id = Guid.NewGuid().ToString(),
                address = address,
                crypto = {
                    ciphertext = cipherText.ToHexString(),
                    cipherparams = cipherParams,
                    cipher = CIPHER,
                    kdf = KdfType.pbkdf2.ToString(),
                    kdfparams = kdfParams,
                    mac = mac.ToHexString()
                }
            };
        }

        private static byte[] DecryptKey(KeyStoreV3<ScryptParams> keystore, string password)
        {
            if (ReferenceEquals(keystore, null)) throw new ArgumentNullException(nameof(keystore));
            if (password.IsNullOrEmpty()) throw new ArgumentNullException(nameof(password));

            var crypto = keystore.crypto;
            var kdfparams = crypto.kdfparams;

            // unsupported cipher
            if (crypto.cipher != CIPHER) throw new ArgumentException("unsupported cipher");

            // decrypt
            return PbkdfCrypt.DecryptScrypt(password, HexToByteArray(crypto.mac),
                HexToByteArray(crypto.cipherparams.iv),
                HexToByteArray(crypto.ciphertext),
                kdfparams.n, kdfparams.p, kdfparams.r,
                HexToByteArray(kdfparams.salt), kdfparams.dklen);
        }

        private static byte[] DecryptKey(KeyStoreV3<Pbkdf2Params> keystore, string password)
        {
            if (ReferenceEquals(keystore, null)) throw new ArgumentNullException(nameof(keystore));
            if (password.IsNullOrEmpty()) throw new ArgumentNullException(nameof(password));

            var crypto = keystore.crypto;
            var kdfparams = crypto.kdfparams;

            // unsupported cipher
            if (crypto.cipher != CIPHER) throw new ArgumentException("unsupported cipher");

            // unsupported prf
            if (kdfparams.prf != Pbkdf2Params.HMACSHA256) throw new ArgumentException("unsupported kdfparams.prf");

            return PbkdfCrypt.DecryptPbkdf2Sha256(password, Hex.ToByteArray(crypto.mac),
                HexToByteArray(crypto.cipherparams.iv),
                HexToByteArray(crypto.ciphertext),
                kdfparams.c, kdfparams.salt.ToByteArray(), kdfparams.dklen);
        }

        private static byte[] HexToByteArray(string hex)
        {
            hex = Hex.StripPrefix(hex);
            if (!hex.All(Hex.Chars.Contains)) throw new ArgumentException("not hex string", hex);

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < hex.Length; i += 2)
                bytes.Add(Convert.ToByte(hex.Substring(i, 2), 16));

            return bytes.ToArray();
        }

    }
}
