using System;
using UnityEngine;

namespace Utility
{
    public static class Extensions
    {
        public static void ForEach<T>(this T[,] array, Action<int, int> action)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    action(x, y);
                }
            }
        }
    }
}
