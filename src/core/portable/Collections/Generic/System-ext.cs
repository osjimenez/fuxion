using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class Extensions
    {
        public static IEnumerable<T> TakeRandomly<T>(this IEnumerable<T> me, int count, Random ran = null, bool canRepeat = false)
        {
            if (ran == null) ran = new Random((int)DateTime.Now.Ticks);
            var list = me.ToList();
            if (!canRepeat && count > list.Count)
                throw new Exception("'count' cannot be higher than number of elements if 'canRepeat' is false");
            var used = new List<int>();
            for (int i = 0; i < count; i++)
            {
                var actual = ran.Next(0, list.Count - 1);
                while(used.Contains(actual))
                    actual = ran.Next(0, list.Count);
                used.Add(actual);
                yield return list[actual];
            }
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> me) => me == null || !me.Any();
    }
}
