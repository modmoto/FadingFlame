using System;
using System.Collections.Generic;

namespace FadingFlame.Leagues;

public static class ListExtensions
{
    private static Random rng = new();

    public static void Shuffle<T>(this IList<T> list)
    {
        var shuffledList = new List<T>();
        var listCount = list.Count;

        for (int i = 0; i < listCount; i++)
        {
            var maxValue = list.Count - 1;
            var randomIndex = rng.Next(maxValue);
            shuffledList.Add(list[randomIndex]);
            list.Remove(shuffledList[i]);
        }

        foreach (var item in shuffledList)
        {
            list.Add(item);
        }
    }
}