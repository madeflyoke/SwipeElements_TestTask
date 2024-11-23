using System;
using System.Collections.Generic;
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
        
        public static void ForEach<T>(this T[,] array, Action<T,int, int> action)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    T element = array[x, y];
                    action(element, x, y);
                }
            }
        }

        public static T Find<T>(this T[,] array, Func<T, bool> condition) where T: class
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    T element = array[x, y];
                    if (condition(element))
                    {
                        return element;
                    }
                }
            }
            
            return null;
        }
        
        public static List<T> ToList<T>(this T[,] array)
        {
            var list = new List<T>();
            array.ForEach((e, x, y) =>
            {
                list.Add(e);
            });
            return list;
        }
        
        public static bool IsInBounds<T>(this T[,] array, int x, int y)
        {
            return x >= 0 && x < array.GetLength(0) &&
                   y >= 0 && y < array.GetLength(1);
        }
    }
}
