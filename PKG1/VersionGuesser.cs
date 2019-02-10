using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PKG1 {
    public class VersionGuesser {
        public static Action<string> Logging = (s) => { Console.WriteLine(s); };
        [JsonIgnore]
        public WZReader _r;
        public ushort VersionId;
        public uint VersionKey;
        public byte VersionHash;
        public EncryptionType IsEncrypted;
        public string PackagePath;

        public VersionGuesser() { }

        public VersionGuesser(WZReader reader, string path, ushort? hint = null) {
            _r = reader;
            PackagePath = path;
            GuessVersion(hint);
        }

        private void GuessVersion(ushort? hint = null) {
            _r.BaseStream.Seek(_r.ContentsStart, SeekOrigin.Begin);
            short ver = _r.ReadInt16();
            bool success;
            long oldPosition = _r.BaseStream.Position;
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
                _r.VersionKey = vHash;
                _r.BaseStream.Position = oldPosition;
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
                        _r.ReadWZString();
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
            for (ushort v = 0; v < ushort.MaxValue; v++)
            {
                uint vHash = v.ToString()
                              .Aggregate<char, uint>(0, (current, t) => (32 * current) + t + 1);
                if ((0xFF ^ (vHash >> 24) ^ (vHash << 8 >> 24) ^ (vHash << 16 >> 24) ^ (vHash << 24 >> 24)) != ver) continue;
                _r.BaseStream.Seek(offset, SeekOrigin.Begin);
                _r.VersionKey = vHash;

                VersionKey = vHash;
                VersionId = v;

                try
                {
                    long currentOffset = _r.BaseStream.Position;
                    _r.BaseStream.Seek(_r.ReadWZOffset(), SeekOrigin.Begin);
                    byte childType = _r.ReadByte();
                    if (childType == 0x73)
                    {
                        long oldPosition = _r.BaseStream.Position;
                        if (!_r.ReadWZStringExpecting(out IsEncrypted, "Property", false))
                        {
                            _r.BaseStream.Position = oldPosition;
                            continue;
                        }
                        success = true;
                        _r.BaseStream.Position = oldPosition;
                        break;
                    }
                    else if (childType == 0x01)
                    {
                        // Lua script, impossible to guarantee found valid version
                        string luaScript = _r.ReadLuaScript();
                        if (luaScript.StartsWith("!version"))
                        {
                            EncryptionType encTesting = EncryptionType.None;
                            bool wrongEncryption = false;
                            do
                            {
                                PackageCollection col = new PackageCollection();
                                using (Package p = new Package(col, PackagePath, v))
                                {
                                    p.MainDirectory.Encrypted = encTesting;
                                    IEnumerable<WZProperty> children = p.MainDirectory.Children;
                                    WZProperty firstImg;
                                    do
                                    {
                                        firstImg = children.FirstOrDefault(c => c.Type == PropertyType.Image);
                                        wrongEncryption = children.Any(c => c.Name.Contains("?"));
                                        if (firstImg == null && !wrongEncryption) children = children.Where(c => c != null).SelectMany(c => c.Children ?? new WZProperty[0]).Where(c => c != null).ToArray();
                                    } while (firstImg == null && children.Count() > 0 && !wrongEncryption);

                                    if (firstImg != null)
                                    {// No imgs???
                                        if (encTesting == EncryptionType.None)
                                            encTesting = IsEncrypted = firstImg.Encrypted;
                                        else
                                            IsEncrypted = encTesting;

                                        success = true;
                                        break;
                                    }
                                    else if (wrongEncryption) encTesting = (EncryptionType)(((int)encTesting) + 1);
                                    else break;
                                    if ((int)encTesting == 3) break;
                                }
                            } while (wrongEncryption);
                            if (!wrongEncryption && !success)
                            {
                                IsEncrypted = encTesting;
                                success = true;
                            }
                            break;
                        }
                    }
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        }
    }
}