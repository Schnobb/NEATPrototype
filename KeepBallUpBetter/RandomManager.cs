﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal static class RandomManager
    {
        public static int? Seed;
        private static Random _random;

        public static Random GetRandomInstance()
        {
            if (_random == null)
            {
                if (Seed.HasValue)
                    _random = new Random(Seed.Value);
                else
                    _random = new Random();
            }

            return _random;
        }

        public static float GetNextFloat()
        {
            return (float)GetRandomInstance().NextDouble();
        }

        public static int GetNextInt()
        {
            return GetRandomInstance().Next();
        }

        public static int GetNextInt(int maxValue)
        {
            return GetRandomInstance().Next(maxValue);
        }

        public static int GetNextInt(int minValue, int maxValue)
        {
            return GetRandomInstance().Next(minValue, maxValue);
        }

        public static double GetNextDouble()
        {
            return GetRandomInstance().NextDouble();
        }

        public static T Choose<T>(IReadOnlyList<T> choices)
        {
            return choices[GetNextInt(choices.Count)];
        }

        public static T Choose<T>(T[] choices)
        {
            return choices[GetNextInt(choices.Length)];
        }
    }
}
