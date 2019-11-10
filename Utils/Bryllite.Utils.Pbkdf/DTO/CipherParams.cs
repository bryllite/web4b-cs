using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Pbkdf.DTO
{
    public class CipherParams
    {
        public const int IV_LENGTH = 16;
        public string iv = Hex.ToString(SecureRandom.GetNonZeroBytes(IV_LENGTH));
    }
}
