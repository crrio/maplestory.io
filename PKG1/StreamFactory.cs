using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PKG1 {
    public class StreamFactory {
        Func<Stream> CreateNew;
        volatile ConcurrentBag<StreamContainer> containers;
        Timer timedThread;
        int disposed;
        public StreamFactory(Func<Stream> createNew) {
            CreateNew = createNew;
            containers = new ConcurrentBag<StreamContainer>();
            timedThread = new Timer(ContainerWatcher, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        void ContainerWatcher (object state) {
            ConcurrentBag<StreamContainer> oldContainers = containers;

            if (oldContainers.Count > 0 && disposed == 0) {
                DateTime now = DateTime.Now;
                IEnumerable<StreamContainer> disposing = oldContainers.Where(c => disposed == 0 && (now - c.lastLock).Seconds > 10 && !c.isLocked && c.Lock(-1)).ToArray();

                if (disposed == 0 && disposing.Count() > 0) {
                    containers = new ConcurrentBag<StreamContainer>(
                        oldContainers.Where(c => !disposing.Contains(c))
                    );
                    Parallel.ForEach(disposing, c => {
                        c.Dispose();
                        c.underlying.Dispose();
                    });
                }

                while (oldContainers.TryTake(out StreamContainer blah)) ;
            }
        }

        public Stream GetStream() {
            if (disposed != 0) throw new ObjectDisposedException("StreamFactory");
            int threadId = Thread.CurrentThread.ManagedThreadId;
            
            StreamContainer res = containers.FirstOrDefault(c => c.Lock(threadId));

            if (res == null) {
                res = new StreamContainer(CreateNew());
                res.Lock(threadId);
                containers.Add(res);
            }

            return res;
        }

        internal void Dispose()
        {
            EventWaitHandle waitForDisposed = new EventWaitHandle(false, EventResetMode.ManualReset);
            timedThread.Dispose(waitForDisposed);
            waitForDisposed.WaitOne();

            disposed = 1;
            ConcurrentBag<StreamContainer> containersDisposing = Interlocked.CompareExchange(ref containers, null, containers);
            if (containersDisposing != null)
                foreach (StreamContainer container in containersDisposing.Where(c => c.Lock(-1)))
                    container.Dispose();
        }

        class StreamContainer : Stream, IDisposable {
            internal Stream underlying;
            volatile int locked = 0;
            public DateTime lastLock;
            public StreamContainer(Stream s) {
                underlying = s;
            }
            public override bool CanRead => underlying.CanRead;
            public override bool CanSeek => underlying.CanSeek;
            public override bool CanWrite => underlying.CanWrite;
            public override long Length => underlying.Length;
            public override long Position
            {
                get => underlying.Position;
                set
                {
                    if (locked != Thread.CurrentThread.ManagedThreadId) throw new InvalidOperationException("Modifying a stream container that the thread doesn't have control over");
                    underlying.Position = value;
                }
            }

            public bool isLocked { get => locked != 0; }

            public override void Flush() => underlying.Flush();
            public bool Lock(int threadId)
            {
                if (!underlying.CanRead) return false;
                int exchangedLock = Interlocked.CompareExchange(ref locked, threadId, 0);
                bool gotLock = exchangedLock == 0;
                if (gotLock) lastLock = DateTime.Now;
                return gotLock;
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (locked != Thread.CurrentThread.ManagedThreadId) throw new InvalidOperationException("Modifying a stream container that the thread doesn't have control over");
                return underlying.Read(buffer, offset, count);
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                if (locked != Thread.CurrentThread.ManagedThreadId) throw new InvalidOperationException("Modifying a stream container that the thread doesn't have control over");
                return underlying.Seek(offset, origin);
            }
            public override void SetLength(long value) => underlying.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => underlying.Write(buffer, offset, count);
            protected override void Dispose(bool disposing)
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int originalLock = locked;
                if (originalLock != Thread.CurrentThread.ManagedThreadId && originalLock != -1 && originalLock != 0) throw new InvalidOperationException("Unlocking a stream container that the thread doesn't have control over");
                locked = 0;
                if (disposing && originalLock == -1)
                    underlying.Dispose();
            }
        }
    }
}