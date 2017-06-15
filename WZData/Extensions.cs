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

            return () =>
            {
                if (wait != null) wait.WaitOne();
                else wait = new EventWaitHandle(false, EventResetMode.ManualReset);
                if (result != null) return result;
                result = that();
                wait.Set();
                return result;
            };
        }
    }
}
