using System;
using System.Threading;

namespace PKG1 {
    public class WeakishReference<K> : IDisposable
        where K : class
    {
        readonly WeakReference<K> weakReference;
        K strongReference;
        readonly EventWaitHandle wait;
        readonly Func<K> refresh;
        Timer resetStrongReference;
        DateTime lastAccess;
        int loading = 0;

        public WeakishReference(K initialValue, Func<K> refreshData)
        {
            wait = new EventWaitHandle(false, EventResetMode.ManualReset);
            weakReference = new WeakReference<K>(initialValue);
            refresh = refreshData;
        }

        void ResetWatcher() {
            if (resetStrongReference != null)
                return;

            Timer t = new Timer((state) => {
                if ((DateTime.Now - lastAccess) > TimeSpan.FromSeconds(60)) {
                    strongReference = null;
                    resetStrongReference?.Dispose();
                    resetStrongReference = null;
                }
            }, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));

            if (Interlocked.CompareExchange(ref resetStrongReference, t, null) != null && resetStrongReference != t)
                t.Dispose();
        }

        public K GetValue() {
            lastAccess = DateTime.Now;
            K value = strongReference;
            if (value != null) return value;
            weakReference.TryGetTarget(out value);
            if (value == null) {
                if (Interlocked.CompareExchange(ref loading, 1, 0) == 0) {
                    wait.WaitOne(500);
                    return GetValue();
                }

                if (strongReference != null) {
                    loading = 0;
                    return strongReference;
                }

                wait.Reset();

                try {
                    strongReference = value = refresh();
                    weakReference.SetTarget(strongReference);
                    ResetWatcher();
                } catch (Exception) {
                    loading = 0;
                } finally {
                    wait.Set();
                    loading = 0;
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
                if (disposing)
                {
                    wait.Dispose();
                    resetStrongReference?.Dispose();
                }

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