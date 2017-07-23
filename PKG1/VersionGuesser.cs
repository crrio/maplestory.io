using System;
using System.IO;
using System.Linq;

namespace PKG1 {
    public class VersionGuesser {
        public static Action<string> Logging = (s) => { };
        public WZReader _r;
        public ushort VersionId;
        public uint VersionKey;
        public byte VersionHash;

        public VersionGuesser(WZReader reader) {
            _r = reader;
            GuessVersion();
        }

        private void GuessVersion() {
            _r.BaseStream.Seek(_r.ContentsStart, SeekOrigin.Begin);
            short ver = _r.ReadInt16();
            bool success;
            long offset = TryFindImageInDir(out success);
            Logging($"Found img at {offset} {success}");
            if (success) {
                success = GuessVersionWithImageOffsetAt(ver, offset);
                _r.BaseStream.Seek(_r.ContentsStart, SeekOrigin.Begin);
                if (success) return;
            }

            for (ushort v = 0; v < ushort.MaxValue; v++) {
                uint vHash = v.ToString()
                              .Aggregate<char, uint>(0, (current, t) => (32*current) + t + 1);
                if ((0xFF ^ (vHash >> 24) ^ (vHash << 8 >> 24) ^ (vHash << 16 >> 24) ^ (vHash << 24 >> 24)) != ver) continue;
                VersionKey = vHash;
                VersionId = v;
                if (DepthFirstImageSearch(out offset)) break;
            }

            if (!GuessVersionWithImageOffsetAt(ver, offset)) throw new Exception("Unable to guess WZ version.");
            _r.BaseStream.Seek(_r.ContentsStart, SeekOrigin.Begin);
        }
        private bool DepthFirstImageSearch(out long offset) {
            bool success = false;
            offset = -1;
            int count = _r.ReadWZInt();
            for (int i = 0; i < count; i++) {
                byte type = _r.ReadByte();
                Logging(type.ToString());
                switch (type) {
                    case 1:
                        _r.BaseStream.Seek(10, SeekOrigin.Current);
                        continue;
                    case 2:
                        int x = _r.ReadInt32();
                        type = _r.PeekFor(() => {
                                              _r.BaseStream.Seek(x + _r.ContentsStart, SeekOrigin.Begin);
                                              return _r.ReadByte();
                                          });
                        break;
                    case 3:
                    case 4:
                        _r.ReadWZStringBlock();
                        break;
                    default:
                        throw new Exception("Unknown object type in WzDirectory.");
                }

                _r.ReadWZInt();
                _r.ReadWZInt();
                offset = _r.BaseStream.Position;
                if (type == 4) {
                    success = true;
                    break;
                }

                if (type == 3) {
                    try {
                        offset = _r.PeekFor(() => {
                            _r.BaseStream.Seek(_r.ReadWZOffset(), SeekOrigin.Begin);
                            long o;
                            success = DepthFirstImageSearch(out o);
                            return o;
                        });
                        break;
                    } catch {}
                }
                _r.BaseStream.Seek(4, SeekOrigin.Current);
            }
            return success;
        }

        private long TryFindImageInDir(out bool success) {
            int count = _r.ReadWZInt();
            Logging($"Count: {count}");
            if (count == 0) throw new Exception("WZ file has no entries!");
            long offset = 0;
            offset = TryFindImageOffset(count, offset, out success);
            return offset;
        }

        private long TryFindImageOffset(int count, long offset, out bool success) {
            success = false;
            Logging($"Trying to find img at {offset}");
            for (int i = 0; i < count; i++) {
                byte type = _r.ReadByte();
                Logging($"Child type {type}");
                switch (type) {
                    case 1:
                        _r.BaseStream.Seek(10, SeekOrigin.Current);
                        continue;
                    case 2:
                        int x = _r.ReadInt32();
                        type = _r.PeekFor(() => {
                                              _r.BaseStream.Seek(x + _r.ContentsStart, SeekOrigin.Begin);
                                              return _r.ReadByte();
                                          });
                        break;
                    case 3:
                    case 4:
                        Logging(_r.ReadWZString());
                        break;
                    default:
                        throw new Exception("Unknown object type in WzDirectory.");
                }

                _r.ReadWZInt();
                _r.ReadWZInt();
                offset = _r.BaseStream.Position;
                _r.BaseStream.Seek(4, SeekOrigin.Current);
                if (type != 4) continue;

                success = true;
                break;
            }
            return offset;
        }

        private bool GuessVersionWithImageOffsetAt(short ver, long offset) {
            bool success = false;
            for (ushort v = 0; v < ushort.MaxValue; v++) {
                uint vHash = v.ToString()
                              .Aggregate<char, uint>(0, (current, t) => (32*current) + t + 1);
                if ((0xFF ^ (vHash >> 24) ^ (vHash << 8 >> 24) ^ (vHash << 16 >> 24) ^ (vHash << 24 >> 24)) != ver) continue;
                _r.BaseStream.Seek(offset, SeekOrigin.Begin);
                _r.VersionKey = vHash;

                VersionKey = vHash;
                VersionId = v;

                try {
                    _r.BaseStream.Seek(_r.ReadWZOffset(), SeekOrigin.Begin);
                    if (_r.ReadByte() != 0x73 ||
                        (_r.PeekFor(() => _r.ReadWZString()) != "Property" &&
                         _r.PeekFor(() => _r.ReadWZString(false)) != "Property")) continue;
                    success = true;
                    break;
                } catch {
                    success = false;
                }
            }
            return success;
        }
    }
}