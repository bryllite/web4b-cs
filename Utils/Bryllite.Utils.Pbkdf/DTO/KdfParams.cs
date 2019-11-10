using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Pbkdf.DTO
{
    public class KdfParams
    {
        public const int SALT_LENGTH = 32;
        public string salt = Hex.ToString(SecureRandom.GetNonZeroBytes(SALT_LENGTH));
        public int dklen = SALT_LENGTH;
    }
}
