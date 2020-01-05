using System;
using System.Reflection;

namespace Bryllite
{
    public class Version
    {
        // bryllite version
        public static readonly int Major = 0;
        public static readonly int Minor = 6;
        public static readonly int Revision = 0;

        // version string
        public static readonly string Ver;

        static Version()
        {
            //Ver = Revision > 0 ? $"{Major}.{Minor}.{Revision}" : $"{Major}.{Minor}";
            Ver = $"{Major}.{Minor}.{Revision}";
        }

        public static string GetVersion()
        {
            return Ver;
        }

        // get module file version
        public static string GetFileVersion(string file)
        {
            return Assembly.LoadFile(file).GetName().Version.ToString();
        }

        // get entry version
        public static string GetEntryVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}
