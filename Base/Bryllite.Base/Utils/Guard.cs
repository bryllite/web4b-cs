using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite
{
    public static class Guard
    {
        public static void Assert(bool condition, string message = "Guard.Assert() failed")
        {
            if (!condition)
                throw new Exception(message);
        }

        public static T NotNull<T>(T value)
        {
            if (ReferenceEquals(value, null))
                throw new ArgumentNullException();

            return value;
        }

        public static T NotNull<T>(T value, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(name);

            if (ReferenceEquals(value, null))
                throw new ArgumentNullException(name);

            return value;
        }

        public static string NotEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException();

            return value;
        }

        public static string NotEmpty(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(name);

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(name);

            return value;
        }

        // value가 null인 경우, 기본값으로 대체한다.
        public static T IfNull<T>(T value, T defaultValue)
        {
            return ReferenceEquals(value, null) ? defaultValue : value;
        }
    }
}
