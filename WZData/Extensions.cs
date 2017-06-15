using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WZData
{
    public static class Extensions
    {
        public static Func<K> Memoize<K>(this Func<K> that)
            where K : class
        {
            K result = null;
            EventWaitHandle wait = null;
            bool running = false;

            return () =>
            {
                if (running) throw new Exception("No concurrent loading of memoized data");
                if (result != null) return result;
                running = true;
                result = that();
                return result;
            };
        }
    }
}
