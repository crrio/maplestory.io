using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PKG1 {
    public class StreamFactory {
        Func<Stream> CreateNew;
        volatile ConcurrentBag<StreamContainer> containers;
        // Timer timedThread;
        public StreamFactory(Func<Stream> createNew) {
            CreateNew = createNew;
            containers = new ConcurrentBag<StreamContainer>();
            // timedThread = new Timer(ContainerWatcher, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        // void ContainerWatcher (object state) {
        //     if (containers.Count > 0) {
        //         DateTime now = DateTime.Now;
        //         IEnumerable<StreamContainer> disposing = containers.Where(c => (now - c.lastLock).Seconds > 10 && !c.isLocked).ToArray();

        //         if (disposing.Count() > 0) {
        //             containers = new ConcurrentBag<StreamContainer>(
        //                 containers.Where(c => !disposing.Contains(c))
        //             );
        //             Parallel.ForEach(disposing, c => {
        //                 c.Dispose();
        //                 c.underlying.Dispose();
        //             });
        //         }
        //     }
        // }

        public Stream GetStream() {
            StreamContainer res = containers.FirstOrDefault(c => c.Lock());

            if (res == null) {
                res = new StreamContainer(CreateNew());
                res.Lock();
                containers.Add(res);
            }

            return res;
        }

        class StreamContainer : Stream, IDisposable {
            internal Stream underlying;
            int locked = 0;
            public DateTime lastLock;
            public StreamContainer(Stream s) {
                underlying = s;
            }
            public override bool CanRead => underlying.CanRead;
            public override bool CanSeek => underlying.CanSeek;
            public override bool CanWrite => underlying.CanWrite;
            public override long Length => underlying.Length;
            public override long Position { get => underlying.Position; set { underlying.Position = value; } }

            public bool isLocked { get => locked == 0; }

            public override void Flush() => underlying.Flush();
            public bool Lock() {
                bool gotLock = Interlocked.CompareExchange(ref locked, 1, 0) == 0;
                if (gotLock) lastLock = DateTime.Now;
                return gotLock;
            }
            public override int Read(byte[] buffer, int offset, int count) => underlying.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => underlying.Seek(offset, origin);
            public override void SetLength(long value) => underlying.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => underlying.Write(buffer, offset, count);
            protected override void Dispose(bool disposing)
                => locked = 0;
        }
    }
}