using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WZData
{
    public static class Extensions
    {
        public static Func<A,K> Memoize<A,K>(this Func<A,K> that)
            where K : class
        {
            Dictionary<A, Tuple<EventWaitHandle, K>> history = new Dictionary<A, Tuple<EventWaitHandle, K>>();

            return (input) =>
            {
                if (history.ContainsKey(input))
                {
                    history[input].Item1.WaitOne();
                    return history[input].Item2;
                }
                EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);

                history.Add(input, new Tuple<EventWaitHandle, K>(wait, null));
                K result = that(input);
                history[input] = new Tuple<EventWaitHandle, K>(wait, result);

                wait.Set();
                return result;
            };
        }

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
