using Bryllite.Extensions;
using Bryllite.Cryptography.Hash;
using Bryllite.Utils.NabiLog;
using Bryllite.Utils.Ntp;
using System;
using System.Linq;

namespace Bryllite.Utils.Auth
{
    public class BAuth
    {
        // access token byte length = 20
        // time + token + checksum
        public const int ACCESS_TOKEN_BYTES = TIME_BYTES + TOKEN_BYTES + CHECKSUM_BYTES;
        public const int TIME_BYTES = 4;
        public const int TOKEN_BYTES = 12;
        public const int CHECKSUM_BYTES = 4;

        // auth seed
        private byte[] seed;

        // auth initial vector
        private byte[] iv;

        // seed code
        private byte[] seedcode
        {
            get
            {
                return seed.Append(iv).Hash256();
            }
        }

        public BAuth(byte[] seed) : this(seed, null)
        {
        }

        public BAuth(byte[] seed, byte[] iv)
        {
            this.seed = seed;
            this.iv = iv;
        }

        public BAuth(string seed) : this(seed, null)
        {
        }

        public BAuth(string seed, string iv) : this(seed.ToByteArray(), iv?.ToByteArray())
        {
        }

        public byte[] GetAccessToken(byte[] time, byte[] salt)
        {
            // token
            byte[] token = CreateToken(time, salt);

            // token body : time + token
            byte[] tokenBody = time.Append(token);

            // add check sum : time + token + checksum
            byte[] accessToken = tokenBody.Append(tokenBody.Hash256().Slice(0, CHECKSUM_BYTES));

            return accessToken;
        }

        public byte[] GetAccessToken(byte[] salt)
        {
            return GetAccessToken(NetTime.UnixTime.ToByteArray(), salt);
        }

        public string GetAccessToken(string salt)
        {
            if (string.IsNullOrEmpty(salt) || salt.IsHexString())
                return Convert.ToBase64String(GetAccessToken(salt.IsHexString() ? salt.ToByteArray() : null));

            return string.Empty;
        }

        public bool Verify(byte[] accessToken, byte[] salt, int expire = 0)
        {
            // token 길이 확인
            if (accessToken.IsNullOrEmpty() || accessToken.Length != ACCESS_TOKEN_BYTES)
            {
                Log.Debug("invalid token length!");
                return false;
            }

            // time factor
            byte[] time = GetTime(accessToken);
            byte[] token = GetToken(accessToken);
            byte[] checksum = GetChecksum(accessToken);

            // 체크섬 확인
            if (!checksum.SequenceEqual(time.Append(token).Hash256().Slice(0, CHECKSUM_BYTES)))
            {
                Log.Debug("invalid token checksum!");
                return false;
            }

            // 토큰 유효기간 확인 : expire가 0이면 유효기간 확인하지 않음
            long timediff = NetTime.UnixTime - time.ToNumber<int>();
            if (expire > 0 && Math.Abs(timediff) > expire)
            {
                Log.Debug("token expired!");
                return false;
            }

            // 토큰 확인
            return token.SequenceEqual(CreateToken(time, salt));
        }

        public bool Verify(string accessToken, string salt, int expire = 0)
        {
            if (string.IsNullOrEmpty(salt) || salt.IsHexString())
                return Verify(Convert.FromBase64String(accessToken), salt.IsHexString() ? salt.ToByteArray() : null, expire);

            return false;
        }

        // time, salt 에 해당하는 토큰을 생성한다.
        private byte[] CreateToken(byte[] time, byte[] salt)
        {
            byte[] tokenBase = time.Append(!salt.IsNullOrEmpty() ? salt : new byte[0]);
            return seedcode.Append(tokenBase).Hash256().Slice(0, TOKEN_BYTES);
        }

        // get time from access token
        private byte[] GetTime(byte[] accessToken)
        {
            return accessToken.Slice(0, TIME_BYTES);
        }

        // get token from access token
        private byte[] GetToken(byte[] accessToken)
        {
            return accessToken.Slice(TIME_BYTES, TOKEN_BYTES);
        }

        // get checksum from access token
        private byte[] GetChecksum(byte[] accessToken)
        {
            return accessToken.Slice(TIME_BYTES + TOKEN_BYTES, CHECKSUM_BYTES);
        }
    }
}
