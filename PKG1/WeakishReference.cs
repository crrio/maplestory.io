using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace PKG1
{
    public class WeakishReference<K> : IDisposable
        where K : class
    {
        static ConcurrentBag<WeakishReference<K>> weakishChecks;
        public static Timer Watcher;
        //readonly WeakReference<K> weakReference;
        volatile K strongReference;
        EventWaitHandle wait;
        readonly Func<K> refresh;
        DateTime lastAccess;
        int loading = 0;
        int lastAccessTS = 0;

        static Dictionary<Type, Predicate<object>> IsDisposedChecks = new Dictionary<Type, Predicate<object>>();

        static WeakishReference() {
            weakishChecks = new ConcurrentBag<WeakishReference<K>>();
            Watcher = new Timer((st) => {
                ConcurrentBag<WeakishReference<K>> newChecks = new ConcurrentBag<WeakishReference<K>>();
                ConcurrentBag<WeakishReference<K>> oldChecks = Interlocked.CompareExchange(ref weakishChecks, newChecks, weakishChecks);

                // Ensure that the concurrent bag has the items taken out to prevent memory leak
                while (oldChecks.TryTake(out WeakishReference<K> weak)) weak.ResetCallback();
            }, null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
        }

        public WeakishReference(K initialValue, Func<K> refreshData)
        {
            //wait = new EventWaitHandle(false, EventResetMode.ManualReset);
            //weakReference = new WeakReference<K>(initialValue);
            refresh = refreshData;
        }

        void ResetWatcher() => weakishChecks.Add(this);

        void ResetCallback()
        {
            K iStrong = strongReference;
            int iLastAccessTS = lastAccessTS;
            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if ((unixTimestamp - iLastAccessTS) > 15 && Interlocked.CompareExchange(ref lastAccessTS, -1, iLastAccessTS) == iLastAccessTS)
            {
                strongReference = null;
                lastAccessTS = 0;
                if (iStrong is IDisposable)
                {
                    if (iStrong is Image<Rgba32> && ((Image<Rgba32>)(object)iStrong).Frames.Count > 0)
                        ((IDisposable)iStrong).Dispose();
                    else
                        ((IDisposable)iStrong).Dispose();
                }
            }
            else weakishChecks.Add(this);
        }

        public K GetValue() {
            lastAccess = DateTime.Now;
            int iLastAccessTS = lastAccessTS;
            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            K value = strongReference;

            if (value != null && typeof(K).Equals(typeof(Image<Rgba32>)))
            {
                Image<Rgba32> actual = (Image<Rgba32>)(object)value;
                if (actual.Frames.Count == 0) strongReference = value = null;
            }

            while (iLastAccessTS == -1)
            {
                Thread.Sleep(1);
                iLastAccessTS = lastAccessTS;
                value = null;
            }

            int exchRes = Interlocked.CompareExchange(ref lastAccessTS, unixTimestamp, iLastAccessTS);

            if (exchRes != 0 && value != null)
                return value;
            else
            {
                EventWaitHandle waiter = wait;
                if (waiter == null)
                {
                    EventWaitHandle result = Interlocked.CompareExchange(ref wait, waiter = new EventWaitHandle(false, EventResetMode.ManualReset), null);
                    if (result != null) waiter = result;
                }
                if (Interlocked.CompareExchange(ref loading, 1, 0) != 0)
                {
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
            K stronger = this.strongReference;
            if (stronger != null && stronger is IDisposable)
                ((IDisposable)stronger).Dispose();
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}