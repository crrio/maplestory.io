using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace WZData {
    public class DataFactory {
        Thread strongReferenceRemover;
        DataFactory() {
            strongReferenceRemover = new Thread(WatchStrongReferences);
            strongReferenceRemover.Start();
        }

        void WatchStrongReferences()
        {
            while (true) {
                Tuple<DateTime, object> head;
                if (strongReferences.TryPeek(out head) && (DateTime.Now - head.Item1).Minutes > 5)
                    strongReferences.TryDequeue(out head);
                Thread.Sleep(5000);
            }
        }

        ConcurrentQueue<Tuple<DateTime, object>> strongReferences = new ConcurrentQueue<Tuple<DateTime, object>>();
        ConcurrentDictionary<string, ConcurrentDictionary<string, WeakReference<object>>> memoized = new ConcurrentDictionary<string, ConcurrentDictionary<string, WeakReference<object>>>();
        public K Cache<K>(System.Linq.Expressions.Expression<Func<K>> toInvoke)
            where K : class
        {
            MethodCallExpression invoking = ((MethodCallExpression)toInvoke.Body);
            MethodInfo calling = invoking.Method;
            string callingHash = calling.ToString();
            IEnumerable<ConstantExpression> args = invoking.Arguments.Select(c => (ConstantExpression)c);
            string argsHash = string.Join("", args.Select(c => c.Value.ToString()));

            // Add the method to the list of memoized references
            // Prefer old values instead of new
            if (!memoized.ContainsKey(callingHash))
                memoized.AddOrUpdate(callingHash, new ConcurrentDictionary<string, WeakReference<object>>(), (a, old) => old);
            ConcurrentDictionary<string, WeakReference<object>> memoizedValues = memoized[callingHash];
            // Prefer old values instead of new
            if (!memoizedValues.ContainsKey(argsHash))
                memoizedValues.AddOrUpdate(argsHash, new WeakReference<object>(null), (a, old) => old);
            WeakReference<object> memoizedResult = memoizedValues[argsHash];

            // Try to pull out the value, otherwise get new value and cache it
            object result = null;
            if (!memoizedResult.TryGetTarget(out result) || result == null) {
                result = toInvoke.Compile()();
                memoizedResult.SetTarget(result);
                strongReferences.Enqueue(new Tuple<DateTime, object>(DateTime.Now, result));
            }

            return (K)result;
        }
    }
}