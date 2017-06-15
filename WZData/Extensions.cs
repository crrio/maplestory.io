using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WZData
{
    public static class Extensions
    {
        public static MemoizedThrowFunc<K> Memoize<K>(this Func<K> that)
            where K : class
        {
            return new MemoizedThrowFunc<K>(that);
        }
    }

    public class MemoizedThrowFunc<K>
        where K : class
    {
        Func<K> Callback;
        EventWaitHandle wait = null;
        K memoizedValue = default(K);
        bool running;

        public MemoizedThrowFunc(Func<K> that)
        {
            Callback = that;
        }

        public K Invoke()
        {
            if (running) throw new Exception("Wait for the previous to finish");
            else if (memoizedValue != default(K)) return memoizedValue;
            else
            {
                running = true;
                memoizedValue = Callback();
                return memoizedValue;
            }
        }
    }
}
