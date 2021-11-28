using System;
using System.Collections.Generic;

namespace FadingFlame.Leagues
{
    public static class ListExtensions
    {
        private static Random rng = new();

        public static List<T> Shuffle<T>(this IList<T> list)
        {
            var shuffledList = new List<T>();
            foreach (var t in list)
            {
                shuffledList.Add(t);
            }

            var n = list.Count;
            while (n > 1) {
                n--;
                var k = rng.Next(n + 1);
                (shuffledList[k], shuffledList[n]) = (list[n], list[k]);
            }

            return shuffledList;
        }
    }
}