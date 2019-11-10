﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bryllite.Extensions
{
    public static class StringExtension
    {
        // path가 존재하지 않으면 생성한다
        public static void MakeSureDirectoryPathExists(this string filepath)
        {
            string path = Path.GetDirectoryName(filepath);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// is null or empty?
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// string is equal?
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="casesensitive"></param>
        /// <returns></returns>
        public static bool Equals(this string left, string right, bool casesensitive)
        {
            return string.Compare(left, right, !casesensitive) == 0;
        }

    }
}
