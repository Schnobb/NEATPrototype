using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    internal static class Util
    {
        public static T Choose<T>(Random random, params T[] values)
        {
            return values[random.Next(values.Length)];
        }
    }
}
