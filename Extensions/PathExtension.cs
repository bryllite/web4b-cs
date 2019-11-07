using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bryllite.Extensions
{
    public static class PathExtension
    {
        // path가 존재하지 않으면 생성한다
        public static void MakeSureDirectoryPathExists(this string filepath)
        {
            string path = Path.GetDirectoryName(filepath);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
