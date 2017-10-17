using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PKG1 {
    public class WeakishReference<K> : IDisposable
        where K : class
    {
        static ConcurrentBag<Action> weakishChecks;
        public static Timer Watcher;
        //readonly WeakReference<K> weakReference;
        K strongReference;
        EventWaitHandle wait;
        readonly Func<K> refresh;
        DateTime lastAccess;
        int loading = 0;

        static WeakishReference() {
            weakishChecks = new ConcurrentBag<Action>();
            Watcher = new Timer((st) => {
                ConcurrentBag<Action> newChecks = new ConcurrentBag<Action>();
                ConcurrentBag<Action> oldChecks;
                do {
                    oldChecks = weakishChecks;
                } while(Interlocked.CompareExchange(ref weakishChecks, newChecks, weakishChecks) != weakishChecks);

                Parallel.ForEach(oldChecks, a => a());
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public WeakishReference(K initialValue, Func<K> refreshData)
        {
            //wait = new EventWaitHandle(false, EventResetMode.ManualReset);
            //weakReference = new WeakReference<K>(initialValue);
            refresh = refreshData;
        }

        void ResetWatcher() => weakishChecks.Add(ResetCallback);

        void ResetCallback()
        {
            if ((DateTime.Now - lastAccess) > TimeSpan.FromSeconds(5))
                strongReference = null;
            else weakishChecks.Add(ResetCallback);
        }

        public K GetValue() {
            lastAccess = DateTime.Now;
            K value = strongReference;
            if (value != null) return value;
            //weakReference.TryGetTarget(out value);
            if (value == null) {
                EventWaitHandle waiter = wait;
                if (waiter == null)
                {
                    EventWaitHandle result = Interlocked.CompareExchange(ref wait, waiter = new EventWaitHandle(false, EventResetMode.ManualReset), null);
                    if (result != null) waiter = result;
                }
                if (Interlocked.CompareExchange(ref loading, 1, 0) != 0) {
                    waiter.WaitOne(500);
                    return GetValue();
                }

                if (strongReference != null)
                {
                    loading = 0;
                    return strongReference;
                }

                try
                {
                    strongReference = value = refresh();
                    //weakReference.SetTarget(strongReference);
                    ResetWatcher();
                }
                catch (Exception)
                {
                    // Report Exception somewhere?
                }
                finally
                {
                    loading = 0;
                    waiter.Set();
                    wait = null;
                }
            }
            lastAccess = DateTime.Now;
            return value;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                strongReference = null;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}