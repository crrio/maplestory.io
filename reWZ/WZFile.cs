// reWZ is copyright angelsl, 2011 to 2013 inclusive.
// 
// This file (WZFile.cs) is part of reWZ.
// 
// reWZ is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// reWZ is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with reWZ. If not, see <http://www.gnu.org/licenses/>.
// 
// Linking reWZ statically or dynamically with other modules
// is making a combined work based on reWZ. Thus, the terms and
// conditions of the GNU General Public License cover the whole combination.
// 
// As a special exception, the copyright holders of reWZ give you
// permission to link reWZ with independent modules to produce an
// executable, regardless of the license terms of these independent modules,
// and to copy and distribute the resulting executable under terms of your
// choice, provided that you also meet, for each linked independent module,
// the terms and conditions of the license of that module. An independent
// module is a module which is not derived from or based on reWZ.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using reWZ.WZProperties;

namespace reWZ
{
    /// <summary>
    ///   A WZ file.
    /// </summary>
    public sealed class WZFile : IDisposable
    {
        byte[] underlyingData;
        internal WZAES _aes;
        internal readonly bool _encrypted;
        internal Stream _file;
        internal readonly WZReadSelection _flag;
        private WZBinaryReader _r;
        internal readonly WZVariant _variant;
        private readonly bool _disposeStream;
        internal uint _fstart;
        internal readonly object _lock = new object();
        private WZDirectory _maindir;

        /// <summary>
        ///   Creates and loads a WZ file from a path. The Stream created will be disposed when the WZ file is disposed.
        /// </summary>
        /// <param name="path"> The path where the WZ file is located. </param>
        /// <param name="variant"> The variant of this WZ file. </param>
        /// <param name="encrypted"> Whether the WZ file is encrypted outside a WZ image. </param>
        /// <param name="flag"> WZ parsing flags. </param>
        public WZFile(string path, WZVariant variant, bool encrypted, WZReadSelection flag = WZReadSelection.None)
            : this(File.ReadAllBytes(path), variant, encrypted, flag)
        {
            _disposeStream = true;
        }

        /// <summary>
        ///   Creates and loads a WZ file. The Stream passed will not be closed when the WZ file is disposed.
        /// </summary>
        /// <param name="input"> The stream containing the WZ file. </param>
        /// <param name="variant"> The variant of this WZ file. </param>
        /// <param name="encrypted"> Whether the WZ file is encrypted outside a WZ image. </param>
        /// <param name="flag"> WZ parsing flags. </param>
        public WZFile(byte[] input, WZVariant variant, bool encrypted, WZReadSelection flag = WZReadSelection.None)
        {
            underlyingData = input;
            _file = new MemoryStream(input);
            _variant = variant;
            _encrypted = encrypted;
            _flag = flag;
            _aes = new WZAES(_variant);
            _r = new WZBinaryReader(_file, _aes, 0);
            Parse();
        }

        /// <summary>
        /// Disposer.
        /// </summary>
        ~WZFile()
        {
            Dispose(false);
        }

        /// <summary>
        ///   The root directory of the WZ file.
        /// </summary>
        public WZDirectory MainDirectory
        {
            get
            {
                return _maindir;
            }
        }

        #region IDisposable Members

        /// <summary>
        ///   Disposes this WZ file.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true); 
        }

        private void Dispose(bool disposing)
        {
            _r.Close(disposing && _disposeStream);
            _maindir = null;
            _aes = null;
            _r = null;
            _file = null;
        }

        #endregion

        /// <summary>
        ///   Resolves a path in the form "/a/b/c/.././d/e/f/".
        /// </summary>
        /// <param name="path"> The path to resolve. </param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The path has an invalid node.</exception>
        public WZObject ResolvePath(string path)
        {
            path = path.Replace('\\', '/');
            return (path.StartsWith("/") ? path.Substring(1) : path).Split('/').Where(node => node != ".").Aggregate<string, WZObject>(_maindir, (current, node) => node == ".." ? current.Parent : current[node]);
        }

        private void Parse()
        {
            _r.Seek(0);
            lock (_lock) {
                if (_r.ReadASCIIString(4) != "PKG1") Die("WZ file has invalid header; file does not have magic \"PKG1\".");
                _r.Skip(8);
                _fstart = _r.ReadUInt32();
                _r.ReadASCIIZString();
                GuessVersion();
                _maindir = new WZDirectory("", null, this, _r, _fstart + 2);
            }
        }

        private void GuessVersion() {
            _r.Seek(_fstart);
            short ver = _r.ReadInt16();
            bool success;
            long offset = TryFindImageInDir(out success);
            if (success) {
                success = GuessVersionWithImageOffsetAt(ver, offset);
                _r.Seek(_fstart);
                if (success) return;
            }

            for (ushort v = 0; v < ushort.MaxValue; v++) {
                uint vHash = v.ToString(CultureInfo.InvariantCulture)
                              .Aggregate<char, uint>(0, (current, t) => (32*current) + t + 1);
                if ((0xFF ^ (vHash >> 24) ^ (vHash << 8 >> 24) ^ (vHash << 16 >> 24) ^ (vHash << 24 >> 24)) != ver) continue;
                _r.VersionHash = vHash;
                if (DepthFirstImageSearch(out offset)) break;
            }

            if (!GuessVersionWithImageOffsetAt(ver, offset)) Die("Unable to guess WZ version.");
            _r.Seek(_fstart);
        }

        private bool DepthFirstImageSearch(out long offset) {
            bool success = false;
            offset = -1;
            int count = _r.ReadWZInt();
            for (int i = 0; i < count; i++) {
                byte type = _r.ReadByte();
                switch (type) {
                    case 1:
                        _r.Skip(10);
                        continue;
                    case 2:
                        int x = _r.ReadInt32();
                        type = _r.PeekFor(() => {
                                              _r.Seek(x + _fstart);
                                              return _r.ReadByte();
                                          });
                        break;
                    case 3:
                    case 4:
                        _r.SkipWZString();
                        break;
                    default:
                        Die("Unknown object type in WzDirectory.");
                        break;
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
                                                _r.Seek(_r.ReadWZOffset(_fstart));
                                                long o;
                                                success = DepthFirstImageSearch(out o);
                                                return o;
                                            });
                        break;
                    } catch {}
                }
                _r.Skip(4);
            }
            return success;
        }

        private long TryFindImageInDir(out bool success) {
            int count = _r.ReadWZInt();
            if (count == 0) Die("WZ file has no entries!");
            long offset = 0;
            offset = TryFindImageOffset(count, offset, out success);
            return offset;
        }

        private long TryFindImageOffset(int count, long offset, out bool success) {
            success = false;
            for (int i = 0; i < count; i++) {
                byte type = _r.ReadByte();
                switch (type) {
                    case 1:
                        _r.Skip(10);
                        continue;
                    case 2:
                        int x = _r.ReadInt32();
                        type = _r.PeekFor(() => {
                                              _r.Seek(x + _fstart);
                                              return _r.ReadByte();
                                          });
                        break;
                    case 3:
                    case 4:
                        _r.SkipWZString();
                        break;
                    default:
                        Die("Unknown object type in WzDirectory.");
                        break;
                }

                _r.ReadWZInt();
                _r.ReadWZInt();
                offset = _r.BaseStream.Position;
                _r.Skip(4);
                if (type != 4) continue;

                success = true;
                break;
            }
            return offset;
        }

        private bool GuessVersionWithImageOffsetAt(short ver, long offset) {
            bool success = false;
            for (ushort v = 0; v < ushort.MaxValue; v++) {
                uint vHash = v.ToString(CultureInfo.InvariantCulture)
                              .Aggregate<char, uint>(0, (current, t) => (32*current) + t + 1);
                if ((0xFF ^ (vHash >> 24) ^ (vHash << 8 >> 24) ^ (vHash << 16 >> 24) ^ (vHash << 24 >> 24)) != ver) continue;
                _r.Seek(offset);
                _r.VersionHash = vHash;
                try {
                    _r.Seek(_r.ReadWZOffset(_fstart));
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

        internal Stream GetSubbytes(long offset, long length)
        {
            byte[] @out = new byte[length];
            long p = _file.Position;
            _file.Position = offset;
            _file.Read(@out, 0, (int)length);
            _file.Position = p;
            return new MemoryStream(@out, false);
        }

        internal Stream GetSubstream(long offset, long length)
        {
            return new Substream(_file, offset, length);
        }

        internal static T Die<T>(string cause)
        {
            throw new WZException(cause);
        }

        internal static void Die(string cause)
        {
            throw new WZException(cause);
        }
    }

    /// <summary>
    ///   WZ reading flags.
    /// </summary>
    [Flags]
    public enum WZReadSelection : byte
    {
        /// <summary>
        ///   No flags are enabled, that is, lazy loading of properties and WZ images is enabled.
        /// </summary>
        None = 0,

        /// <summary>
        ///   Set this flag to disable lazy loading of string properties.
        /// </summary>
        EagerParseStrings = 1,

        /// <summary>
        ///   Set this flag to disable lazy loading of Audio properties.
        /// </summary>
        EagerParseAudio = 2,

        /// <summary>
        ///   Set this flag to disable lazy loading of canvas properties.
        /// </summary>
        EagerParseCanvas = 4,

        /// <summary>
        ///   Set this flag to completely disable loading of canvas properties.
        /// </summary>
        NeverParseCanvas = 8,

        /// <summary>
        ///   Set this flag to disable lazy loading of string, Audio and canvas properties.
        /// </summary>
        EagerParseAll = EagerParseCanvas | EagerParseAudio | EagerParseStrings,

        /// <summary>
        ///   Set this flag to disable lazy loading of WZ images.
        /// </summary>
        EagerParseImage = 16,

        /// <summary>
        ///   Set this flag to disable reading entire WZ images into memory when any of the eager load flags are set.
        /// </summary>
        LowMemory = 32
    }
}