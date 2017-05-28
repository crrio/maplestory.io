// reWZ is copyright angelsl, 2011 to 2013 inclusive.
// 
// This file (WZBinaryReader.cs) is part of reWZ.
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace reWZ
{
    internal sealed class WZBinaryReader : BinaryReader
    {
        private readonly WZAES _aes;
        private uint _versionHash;

        internal WZBinaryReader(Stream inStream, WZAES aes, uint versionHash) : base(inStream, Encoding.ASCII)
        {
            _aes = aes;
            _versionHash = versionHash;
        }

        internal uint VersionHash
        {
            get { return _versionHash; }
            set { _versionHash = value; }
        }

        internal void Close(bool disposeStream)
        {
            Dispose(disposeStream);
        }

        /// <summary>
        ///   Sets the position within the backing stream to the specified value.
        /// </summary>
        /// <param name="offset"> The new position within the backing stream. This is relative to the <paramref name="loc" /> parameter, and can be positive or negative. </param>
        /// <param name="loc"> A value of type <see cref="T:System.IO.SeekOrigin" /> , which acts as the seek reference point. This defaults to <code>SeekOrigin.Begin</code> . </param>
        internal void Seek(long offset, SeekOrigin loc = SeekOrigin.Begin)
        {
            BaseStream.Seek(offset, loc);
        }

        /// <summary>
        ///   Advances the position within the backing stream by <paramref name="count" /> .
        /// </summary>
        /// <param name="count"> The amount of bytes to skip. </param>
        internal void Skip(long count)
        {
            BaseStream.Position += count;
        }

        /// <summary>
        ///   Executes a delegate of type <see cref="System.Action" /> , then sets the position of the backing stream back to the original value.
        /// </summary>
        /// <param name="result"> The delegate to execute. </param>
        internal void PeekFor(Action result)
        {
            long orig = BaseStream.Position;
            try {
                result();
            } finally {
                BaseStream.Position = orig;
            }
        }

        /// <summary>
        ///   Executes a delegate of type <see cref="System.Func{TResult}" /> , then sets the position of the backing stream back to the original value.
        /// </summary>
        /// <typeparam name="T"> The return type of the delegate. </typeparam>
        /// <param name="result"> The delegate to execute. </param>
        /// <returns> The object returned by the delegate. </returns>
        internal T PeekFor<T>(Func<T> result)
        {
            long orig = BaseStream.Position;
            try {
                return result();
            } finally {
                BaseStream.Position = orig;
            }
        }

        /// <summary>
        ///   Reads a string encoded in WZ format.
        /// </summary>
        /// <param name="encrypted"> Whether the string is encrypted. </param>
        /// <returns> The read string. </returns>
        internal string ReadWZString(bool encrypted = true)
        {
            int length = ReadSByte();
            if (length == 0) return "";
            if (length > 0) {
                length = length == 127 ? ReadInt32() : length;
                if (length == 0) return "";
                byte[] rbytes = ReadBytes(length*2);
                return _aes.DecryptUnicodeString(rbytes, encrypted);
            } // !(length >= 0), i think we can assume length < 0, but the compiler can't seem to see that
            length = length == -128 ? ReadInt32() : -length;
            if (length == 0) return "";
            return _aes.DecryptASCIIString(ReadBytes(length), encrypted);
        }

        /// <summary>
        ///   Reads a string encoded in WZ format at a specific offset, then returns the backing stream's position to its original value.
        /// </summary>
        /// <param name="offset"> The offset where the string is located. </param>
        /// <param name="encrypted"> Whether the string is encrypted. </param>
        /// <returns> The read string. </returns>
        private string ReadWZStringAtOffset(long offset, bool encrypted = true)
        {
            return PeekFor(() => {
                BaseStream.Position = offset;
                return ReadWZString(encrypted);
            });
        }

        /// <summary>
        ///   Reads a raw and unencrypted ASCII string.
        /// </summary>
        /// <param name="length"> The length of the string. </param>
        /// <returns> The read string. </returns>
        internal string ReadASCIIString(int length)
        {
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        /// <summary>
        ///   Reads a raw and unencrypted null-terminated ASCII string.
        /// </summary>
        /// <returns> The read string. </returns>
        internal string ReadASCIIZString()
        {
            StringBuilder sb = new StringBuilder();
            byte b;
            while ((b = ReadByte()) != 0)
                sb.Append((char)b);
            return sb.ToString();
        }

        internal string ReadWZStringBlock(bool encrypted)
        {
            switch (ReadByte()) {
                case 0:
                case 0x73:
                    return ReadWZString(encrypted);
                case 1:
                case 0x1B:
                    return ReadWZStringAtOffset(ReadInt32(), encrypted);
                default:
                    return WZFile.Die<String>("Unknown string type in string block!");
            }
        }

        internal void SkipWZStringBlock()
        {
            switch (ReadByte()) {
                case 0:
                case 0x73:
                    SkipWZString();
                    return;
                case 1:
                case 0x1B:
                    Skip(4);
                    return;
                default:
                    WZFile.Die("Unknown string type in string block!");
                    return;
            }
        }

        internal void SkipWZString()
        {
            int length = ReadSByte();
            Skip((length >= 0) ? (length == 127 ? ReadInt32() : length)*2 : length == -128 ? ReadInt32() : -length);
        }

        /// <summary>
        ///   Reads a WZ-compressed 32-bit integer.
        /// </summary>
        /// <returns> The read integer. </returns>
        internal int ReadWZInt()
        {
            sbyte s = ReadSByte();
            return s == -128 ? ReadInt32() : s;
        }

        internal long ReadWZInt64()
        {
            sbyte s = ReadSByte();
            return s == -128 ? ReadInt64() : s;
        }

        internal uint ReadWZOffset(uint fstart)
        {
            unchecked {
                uint ret = ((((uint)BaseStream.Position - fstart) ^ 0xFFFFFFFF)*_versionHash) - WZAES.OffsetKey;
                return (((ret << (int)ret) | (ret >> (int)(32 - ret))) ^ ReadUInt32()) + (fstart*2);
            }
        }

        internal static byte[] Inflate(Stream @in)
        {
            long length = 512*1024;
            try {
                length = Math.Max(@in.Length, length);
            } catch {}
            byte[] dec = new byte[length];
            using (DeflateStream dStr = new DeflateStream(@in, CompressionMode.Decompress))
            using (MemoryStream @out = new MemoryStream(dec.Length*2)) {
                int len;
                while ((len = dStr.Read(dec, 0, dec.Length)) > 0) @out.Write(dec, 0, len);
                return @out.ToArray();
            }
        }
    }

    internal sealed class Substream : Stream
    {
        private readonly Stream _backing;
        private readonly long _end; // end is exclusive
        private readonly long _length; // end is exclusive
        private readonly long _origin; // end is exclusive
        private long _posInBacking;

        internal Substream(Stream backing, long start, long length)
        {
            if (!backing.CanSeek) throw new ArgumentException("A Substream's backing stream must be seekable!", "backing");
            if (start >= backing.Length) throw new ArgumentOutOfRangeException("start", "The Substream falls outside the backing stream!");
            _backing = backing;
            _origin = start;
            _length = length;
            _end = start + length;
            if (_end > backing.Length) throw new ArgumentOutOfRangeException("length", "The Substream falls outside the backing stream!");
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _posInBacking - _origin; }
            set { _posInBacking = value + _origin; }
        }

        public override void Flush()
        {}

        public override long Seek(long offset, SeekOrigin origin)
        {
            long tPos;
            switch (origin) {
                case SeekOrigin.Begin:
                    tPos = _origin + offset;
                    break;
                case SeekOrigin.Current:
                    tPos = _posInBacking + offset;
                    break;
                case SeekOrigin.End:
                    tPos = _end + offset;
                    break;
                default:
                    throw new ArgumentException("Invalid SeekOrigin specified.", "origin");
            }

            if (tPos >= _end || tPos < _origin) throw new ArgumentOutOfRangeException("offset", "You cannot seek out of the substream!");
            return (_posInBacking = tPos);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("A Substream cannot be resized.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long origPos = _backing.Position;
            if (origPos != _posInBacking) _backing.Position = _posInBacking;
            count = (int)Math.Min(count, _end - _posInBacking);
            if (count == 0) return 0;
            count = _backing.Read(buffer, offset, count);
            _posInBacking += count;
            Debug.Assert(_posInBacking == _backing.Position);
            //_backing.Position = origPos;
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("A Substream is not writable.");
        }

        public override int ReadByte()
        {
            if (_posInBacking >= _end) return -1;
            long origPos = _backing.Position;
            if (origPos != _posInBacking) _backing.Position = _posInBacking;
            int r = _backing.ReadByte();
            ++_posInBacking;
            Debug.Assert(_posInBacking == _backing.Position);
            //_backing.Position = origPos;
            return r;
        }
    }
}