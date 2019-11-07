using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Pbkdf.DTO
{
    public class ScryptParams : KdfParams
    {
        public int n = 262144;
        public int p = 1;
        public int r = 8;

        public ScryptParams()
        {
        }

        public ScryptParams(int n, int p, int r) : this()
        {
            this.n = n;
            this.p = p;
            this.r = r;
        }

        public static ScryptParams Default
        {
            get { return new ScryptParams(); }
        }

        public static ScryptParams New(int n, int p = 1, int r = 8)
        {
            return new ScryptParams(n, p, r);
        }
    }
}
