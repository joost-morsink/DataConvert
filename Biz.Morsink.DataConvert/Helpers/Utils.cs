using System;
using System.Collections.Generic;

namespace Biz.Morsink.DataConvert.Helpers
{
    static class Utils
    {
        public static IEnumerable<T> Iterate<T>(this T seed, Func<T, T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }
        }
    }
}
