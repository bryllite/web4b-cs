using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Extensions
{
    public static class ArrayExtension
    {
        /// <summary>
        /// is null or empty array?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> array)
        {
            return ReferenceEquals(array, null) || array.Count() == 0;
        }

        /// <summary>
        /// reverse array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] Reverse<T>(this T[] array)
        {
            if (IsNullOrEmpty(array)) return array;

            var reverse = array.ToArray();
            Array.Reverse(reverse);
            return reverse;
        }

        /// <summary>
        /// get random shufled array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enums)
        {
            return IsNullOrEmpty(enums) ? enums : enums.OrderBy(x => SecureRandom.Next<int>()).ToArray();
        }

        /// <summary>
        /// get concated array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T[] Append<T>(this T[] array, T[] items)
        {
            T[] append = IsNullOrEmpty(items) ? new T[0] : items;
            return IsNullOrEmpty(array) ? append : array.Concat(append).ToArray();
        }

        public static T[] Append<T>(this T[] array, T item)
        {
            return ReferenceEquals(item, null) ? array : Append(array, new T[] { item });
        }

        /// <summary>
        /// 배열 객체를 반으로 자르고, 새로운 좌, 우 배열 객체를 리턴합니다.  
        /// 만약, 배열의 길이가 홀수이면, 우 배열이 좌 배열 보다 큽니다.  
        /// 예를 들어 byte[1].Divide().Left().Length == 0
        /// byte[1].Divide().Right().Length == 1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">반으로 나눌 배열 객체</param>
        /// <returns>반으로 나뉜 새로운 좌, 우 배열 객체의 튜플</returns>
        public static (T[] Left, T[] Right) Divide<T>(this T[] array)
        {
            if (array.IsNullOrEmpty()) return (new T[0], new T[0]);

            int half = array.Length / 2;
            return (array.Take(half).ToArray(), array.Skip(half).ToArray());
        }

        /// <summary>
        /// 배열 객체를 반으로 자르고, 왼쪽 절반을 새로운 객체로 리턴합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">원본 배열 객체</param>
        /// <returns>반으로 나뉜 새로운 좌 배열 객체</returns>
        public static T[] Left<T>(this T[] array)
        {
            return Divide(array).Left;
        }

        /// <summary>
        /// 배열 객체를 반으로 자르고, 오른쪽 절반을 새로운 객체로 리턴합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">원본 배열 객체</param>
        /// <returns>반으로 나뉜 새로운 우 배열 객체</returns>
        public static T[] Right<T>(this T[] array)
        {
            return Divide(array).Right;
        }


        /// <summary>
        /// 배열 객체를 offset 만큼 건너뛰고, length만큼 선택하여 새로운 배열 객체를 만듭니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">원본 배열 객체</param>
        /// <param name="offset">건너뛸 길이</param>
        /// <param name="length">선택할 길이</param>
        /// <returns>새로운 배열 객체</returns>
        public static T[] Slice<T>(this T[] array, ref int offset, int length)
        {
            try
            {
                return array.IsNullOrEmpty() ? new T[0] : array.Skip(offset).Take(length).ToArray();
            }
            finally
            {
                offset += length;
            }
        }

        /// <summary>
        /// 배열 객체를 offset 만큼 건너뛰고, length만큼 선택하여 새로운 배열 객체를 만듭니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">원본 배열 객체</param>
        /// <param name="offset">건너뛸 길이</param>
        /// <param name="length">선택할 길이</param>
        /// <returns>새로운 배열 객체</returns>
        public static T[] Slice<T>(this T[] array, int offset, int length)
        {
            return Slice(array, ref offset, length);
        }

        /// <summary>
        /// 배열 객체에서 offset 만큼 건너뛰고, 나머지 배열 객체를 새로 만들어 리턴합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">원본 배열 객체</param>
        /// <param name="offset">건너뛸 길이</param>
        /// <returns>새로운 배열 객체</returns>
        public static T[] Slice<T>(this T[] array, int offset)
        {
            return array.IsNullOrEmpty() ? new T[0] : array.Skip(offset).ToArray();
        }

    }
}
