using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ImageSharp;
using ImageSharp.PixelFormats;

namespace PKG1 {
    public class WZReader : BinaryReader {
        private static readonly byte[] AESKey = {0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00};
        private static readonly byte[] GMSIV = {0x4D, 0x23, 0xC7, 0x2B, 0x4D, 0x23, 0xC7, 0x2B, 0x4D, 0x23, 0xC7, 0x2B, 0x4D, 0x23, 0xC7, 0x2B};
        private static readonly byte[] KMSIV = {0xB9, 0x7D, 0x63, 0xE9, 0xB9, 0x7D, 0x63, 0xE9, 0xB9, 0x7D, 0x63, 0xE9, 0xB9, 0x7D, 0x63, 0xE9};

        private static byte[] GMSKey;
        public static byte[] KMSKey;

        public static void InitializeKeys()
            => Task.WaitAll(
                Task.Run(() => {
                    if (File.Exists("gms.aes")) GMSKey = File.ReadAllBytes("gms.aes");
                    else File.WriteAllBytes("gms.aes", GMSKey = GetWZKey(WZVariant.GMS));
                }),
                Task.Run(() => {
                    if (File.Exists("kms.aes")) KMSKey = File.ReadAllBytes("kms.aes");
                    else File.WriteAllBytes("kms.aes", KMSKey = GetWZKey(WZVariant.KMS));
                })
            );

        private static byte[] GetWZKey(WZVariant version)
        {
            switch ((int)version) {
                case 0:
                    return GenerateKey(KMSIV, AESKey);
                case 1:
                    return GenerateKey(GMSIV, AESKey);
                case 2:
                    return new byte[0x10000];
                default:
                    throw new ArgumentException("Invalid WZ variant passed.", "version");
            }
        }

        static byte[] GenerateKey(byte[] iv, byte[] aesKey) {
            using (MemoryStream memStream = new MemoryStream(0x100000))
            using (Aes aem = Aes.Create())
            {
                aem.KeySize = 256;
                aem.Key = aesKey;
                aem.Mode = CipherMode.ECB;
                using (CryptoStream cStream = new CryptoStream(memStream, aem.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cStream.Write(iv, 0, 16);
                    for (int i = 0; i < (0x100000 - 16); i += 16)
                        cStream.Write(memStream.ToArray(), i, 16);
                    cStream.Flush();
                    return memStream.ToArray();
                }
            }
        }

        public uint VersionKey;
        public byte VersionHash;
        public uint ContentsStart;
        public long PositionFromStart { get => BaseStream.Position - ContentsStart; }
        Encoding _encoding;
        public Package Package;
        public WZProperty Container;
        public WZReader(Package package, WZProperty container, Stream str, uint versionKey, byte versionHash, uint contentsStart) : base(str) {
            this.VersionHash = versionHash;
            this.VersionKey = versionKey;
            this.ContentsStart = contentsStart;
            this.Package = package;
            this.Container = container;
        }

        public WZReader(Package package, WZProperty container, Stream str, Encoding encoding, uint versionKey, byte versionHash, uint contentsStart) : base(str, encoding) {
            _encoding = encoding;
            this.VersionHash = versionHash;
            this.VersionKey = versionKey;
            this.ContentsStart = contentsStart;
            this.Package = package;
            this.Container = container;
        }

        public string ReadString(int length) => new string(this.ReadChars(length));
        public string ReadNULTerminatedString(int maxLength)
        {
            StringBuilder builder = new StringBuilder();
            byte lastChar = 0;
            do {
                lastChar = this.ReadByte();
                if (lastChar != 0) builder.Append((char)lastChar);
                if (builder.Length == maxLength) break;
            } while(lastChar != 0);
            return builder.ToString();
        }

        public string ReadWZStringBlock(bool encrypted = false)
        {
            byte type = ReadByte();
            switch (type) {
                case 0:
                case 0x73:
                    return ReadWZString();
                case 1:
                case 0x1B:
                    return ReadDeDupedString();
                default:
                    throw new Exception($"Unknown type ({type}) of string block");
            }
        }
        public string ReadLuaScript(bool readByte = false) {
            if (readByte && this.ReadByte() == 0) return "";
            int length = this.ReadSByte();
            bool isUnicode = length > 0;
            if (isUnicode) {
                if (length == 127) length = this.ReadInt32();
                length = length * 2;
            } else {
                if (length == -128) length = this.ReadInt32();
                else length = length * -1;
            }

            IEnumerable<byte> textData = DecryptBytes(this.ReadBytes(length), KMSKey);

            if (!isUnicode) return Encoding.ASCII.GetString(textData.ToArray());
            return Encoding.Unicode.GetString(textData.ToArray());
        }
        public string ReadWZString(bool readByte = false) {
            if (readByte && this.ReadByte() == 0) return "";
            int length = this.ReadSByte();
            bool isUnicode = length > 0;
            if (isUnicode) {
                if (length == 127) length = this.ReadInt32();
                length = length * 2;
            } else {
                if (length == -128) length = this.ReadInt32();
                else length = length * -1;
            }
            byte[] textData = this.ReadBytes(length);
            byte asciiMask = 0xAA;
            ushort unicodeMask = 0xAAAA;

            if (!isUnicode) return Encoding.ASCII.GetString(textData.Select(c => (byte)(c ^ asciiMask++)).ToArray());

            for (int i = 0; i < length; i += 2) {
                textData[i] ^= (byte)(unicodeMask & 0xFF);
                textData[i + 1] ^= (byte)((unicodeMask >> 8) & 0xFF);
                unicodeMask++;
            }

            return Encoding.Unicode.GetString(textData);
        }

        public string ReadDeDupedString(bool readByte = false) => ReadWZStringAtOffset(ReadInt32(), readByte);

        public string ReadWZStringAtOffset(long offset, bool readByte = false) {
            long currentOffset = BaseStream.Position;

            BaseStream.Seek(Container.ContainerStartLocation + offset, SeekOrigin.Begin);
            string result = ReadWZString(readByte);
            BaseStream.Seek(currentOffset, SeekOrigin.Begin);

            return result;
        }

        public float ReadWZSingle()
        {
            byte t = ReadByte();
            if (t == 0x80)
                return ReadSingle();
            else if(t == 0)
                return t;
            else throw new Exception("Unknown single type");
        }

        public int ReadWZInt() {
            sbyte possible = this.ReadSByte();
            if (possible == -128) return this.ReadInt32();
            return possible;
        }
        public long ReadWZLong() {
            sbyte possible = this.ReadSByte();
            if (possible == -128) return this.ReadInt64();
            return possible;
        }

        /// This shit's fucked up, yo.
        public uint ReadWZOffset() {
            uint offset = (uint)((PositionFromStart) ^ 0xFFFFFFFF);
            offset *= VersionKey;
            offset -= 0x581C3F6D;
            offset = ROTL(offset, (byte)(offset & 0x1F));
            uint encryptedOffset = this.ReadUInt32();
            offset ^= encryptedOffset;
            offset += (uint)ContentsStart * 2;

            return offset;
        }

        IEnumerable<byte> DecryptBytes(IEnumerable<byte> bytes, byte[] wzKey = null)
        {
            // Assume GMS for now, need to pull out into global collection
            wzKey = wzKey ?? GMSKey;
            return bytes.Select((a,i) => (byte)(a ^ wzKey[i]));
        }

        internal IEnumerable<IEnumerable<byte>> DecryptPNGNested(Stream inData, uint length)
        {
            long expectedEndPosition = inData.Position + length;
            using (BinaryReader reader = new BinaryReader(inData)) {
                while (inData.Position < expectedEndPosition) {
                    int blockLen = reader.ReadInt32();
                    yield return DecryptBytes(reader.ReadBytes(blockLen));
                }
            }
        }

        IEnumerable<byte> DecryptPNG(Stream inData, uint length) => DecryptPNGNested(inData, length).SelectMany(c => c);

        byte[] Inflate(Stream data, uint dataLength)
        {
            long length = 512*1024;
            try {
                length = Math.Max(dataLength, length);
            } catch {}
            byte[] dec = new byte[length];
            using (DeflateStream deflator = new DeflateStream(data, CompressionMode.Decompress))
            using (MemoryStream @out = new MemoryStream(dec.Length*2)) {
                int len;
                while ((len = deflator.Read(dec, 0, dec.Length)) > 0) @out.Write(dec, 0, len);
                return @out.ToArray();
            }
        }

        internal Image<Rgba32> ParsePNG(int width, int height, int format, bool isEncrypted, uint imageLength)
        {
            byte[] sourceData = isEncrypted ? DecryptPNG(BaseStream, imageLength).ToArray() : ReadBytes((int)imageLength);
            Stream sourceDataFromStream = new MemoryStream(sourceData, 2, sourceData.Length - 2);
            sourceData = Inflate(sourceDataFromStream, imageLength);
            int sourceDataLength = sourceData.Length;

            switch (format) {
                case 1: // Transform
                    byte[] destinationData = new byte[width*height*4];
                    for (int i = 0; i < sourceDataLength; ++i)
                    {
                        destinationData[i * 2] = (byte)(((sourceData[i]) & 0x0F) * 0x11);
                        destinationData[(i * 2) + 1] = (byte)(((sourceData[i] & 0xF0) >> 4) * 0x11);
                    }
                    sourceDataLength *= 2;
                    sourceData = destinationData;
                    goto case 2;
                case 2:
                    if (sourceDataLength != width * height * 4)
                    {
                        byte[] proper = new byte[width*height*4];
                        Buffer.BlockCopy(sourceData, 0, proper, 0, Math.Min(proper.Length, sourceDataLength));
                        sourceData = proper;
                    }
                    return ImageSharp.Image.LoadPixelData<Argb32>(new Span<byte>(sourceData), width, height).To<Rgba32>();
                case 513:
                    if (sourceDataLength != width * height * 2)
                    {
                        byte[] proper = new byte[width*height*2];
                        Buffer.BlockCopy(sourceData, 0, proper, 0, Math.Min(proper.Length, sourceDataLength));
                        sourceData = proper;
                    }
                    return ImageSharp.Image.LoadPixelData<Rgb565>(new Span<byte>(sourceData), width, height).To<Rgba32>();
                case 517:
                    width >>= 4;
                    height >>= 4;
                    goto case 513;
                case 1026: //dxt3
                    destinationData = GetPixelDataDXT3(sourceData, width, height);
                    return ImageSharp.Image.LoadPixelData<Argb32>(new Span<byte>(destinationData), width, height).To<Rgba32>();
                case 2050:
                    destinationData = GetPixelDataDXT5(sourceData, width, height);
                    return ImageSharp.Image.LoadPixelData<Argb32>(new Span<byte>(destinationData), width, height).To<Rgba32>();
                default:
                    throw new InvalidOperationException("Unknown canvas format");
            }
        }
        public static byte[] GetPixelDataDXT3(byte[] rawData, int width, int height)
        {
            byte[] pixel = new byte[width * height * 4];

            Rgba32[] colorTable = new Rgba32[4];
            int[] colorIdxTable = new int[16];
            byte[] alphaTable = new byte[16];
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    int off = x * 4 + y * width;
                    ExpandAlphaTableDXT3(alphaTable, rawData, off);
                    ushort u0 = BitConverter.ToUInt16(rawData, off + 8);
                    ushort u1 = BitConverter.ToUInt16(rawData, off + 10);
                    ExpandColorTable(colorTable, u0, u1);
                    ExpandColorIndexTable(colorIdxTable, rawData, off + 12);

                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            SetPixel(pixel,
                                x + i,
                                y + j,
                                width,
                                colorTable[colorIdxTable[j * 4 + i]],
                                alphaTable[j * 4 + i]);
                        }
                    }
                }
            }

            return pixel;
        }
        public static byte[] GetPixelDataDXT5(byte[] rawData, int width, int height)
        {
            byte[] pixel = new byte[width * height * 4];

            Rgba32[] colorTable = new Rgba32[4];
            int[] colorIdxTable = new int[16];
            byte[] alphaTable = new byte[8];
            int[] alphaIdxTable = new int[16];
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    int off = x * 4 + y * width;
                    ExpandAlphaTableDXT5(alphaTable, rawData[off + 0], rawData[off + 1]);
                    ExpandAlphaIndexTableDXT5(alphaIdxTable, rawData, off + 2);
                    ushort u0 = BitConverter.ToUInt16(rawData, off + 8);
                    ushort u1 = BitConverter.ToUInt16(rawData, off + 10);
                    ExpandColorTable(colorTable, u0, u1);
                    ExpandColorIndexTable(colorIdxTable, rawData, off + 12);

                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            SetPixel(pixel,
                                x + i,
                                y + j,
                                width,
                                colorTable[colorIdxTable[j * 4 + i]],
                                alphaTable[alphaIdxTable[j * 4 + i]]);
                        }
                    }
                }
            }

            return pixel;
        }
        private static void SetPixel(byte[] pixelData, int x, int y, int width, Rgba32 color, byte alpha)
        {
            int offset = (y * width + x) * 4;
            pixelData[offset + 0] = color.B;
            pixelData[offset + 1] = color.G;
            pixelData[offset + 2] = color.R;
            pixelData[offset + 3] = alpha;
        }
        private static void ExpandColorTable(Rgba32[] color, ushort c0, ushort c1)
        {
            color[0] = RGB565ToColor(c0);
            color[1] = RGB565ToColor(c1);
            if (c0 > c1)
            {
                color[2] = new Rgba32((byte)((color[0].R * 2f + color[1].R + 1) / 3f), (byte)((color[0].G * 2f + color[1].G + 1) / 3f), (byte)((color[0].B * 2f + color[1].B + 1) / 3f), 255);
                color[3] = new Rgba32((byte)((color[0].R + color[1].R * 2f + 1) / 3f), (byte)((color[0].G + color[1].G * 2f + 1) / 3f), (byte)((color[0].B + color[1].B * 2f + 1) / 3f), 255);
            }
            else
            {
                color[2] = new Rgba32((byte)((color[0].R + color[1].R) / 2f), (byte)((color[0].G + color[1].G) / 2f), (byte)((color[0].B + color[1].B) / 2f), 255);
                color[3] = new Rgba32(0, 0, 0f);
            }
        }
        private static void ExpandColorIndexTable(int[] colorIndex, byte[] rawData, int offset)
        {
            for (int i = 0; i < 16; i += 4, offset++)
            {
                colorIndex[i + 0] = (rawData[offset] & 0x03);
                colorIndex[i + 1] = (rawData[offset] & 0x0c) >> 2;
                colorIndex[i + 2] = (rawData[offset] & 0x30) >> 4;
                colorIndex[i + 3] = (rawData[offset] & 0xc0) >> 6;
            }
        }
        private static void ExpandAlphaTableDXT3(byte[] alpha, byte[] rawData, int offset)
        {
            for (int i = 0; i < 16; i += 2, offset++)
            {
                alpha[i + 0] = (byte)(rawData[offset] & 0x0f);
                alpha[i + 1] = (byte)((rawData[offset] & 0xf0) >> 4);
            }
            for (int i = 0; i < 16; i++)
                alpha[i] = (byte)(alpha[i] | (alpha[i] << 4));
        }
        private static void ExpandAlphaTableDXT5(byte[] alpha, byte a0, byte a1)
        {
            alpha[0] = a0;
            alpha[1] = a1;
            if (a0 > a1)
                for (int i = 2; i < 8; i++)
                    alpha[i] = (byte)(((8 - i) * a0 + (i - 1) * a1 + 3) / 7);
            else
            {
                for (int i = 2; i < 6; i++)
                    alpha[i] = (byte)(((6 - i) * a0 + (i - 1) * a1 + 2) / 5);
                alpha[6] = 0;
                alpha[7] = 255;
            }
        }
        private static void ExpandAlphaIndexTableDXT5(int[] alphaIndex, byte[] rawData, int offset)
        {
            for (int i = 0; i < 16; i += 8, offset += 3)
            {
                int flags = rawData[offset]
                    | (rawData[offset + 1] << 8)
                    | (rawData[offset + 2] << 16);
                for (int j = 0; j < 8; j++)
                {
                    int mask = 0x07 << (3 * j);
                    alphaIndex[i + j] = (flags & mask) >> (3 * j);
                }
            }
        }
        private static Rgba32 RGB565ToColor(ushort val)
        {
            const int rgb565_mask_r = 0xf800;
            const int rgb565_mask_g = 0x07e0;
            const int rgb565_mask_b = 0x001f;
            int r = (val & rgb565_mask_r) >> 11;
            int g = (val & rgb565_mask_g) >> 5;
            int b = (val & rgb565_mask_b);
            var c = new Rgba32(
                (byte)((r << 3) | (r >> 2)),
                (byte)((g << 2) | (g >> 4)),
                (byte)((b << 3) | (b >> 2)));
            return c;
        }

        static uint ROTL(uint value, byte shiftTimes) {
            return (uint)((value << shiftTimes) | (value >> (32 - shiftTimes)));
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
        ///   This enum is used to specify the WZ key to be used.
        /// </summary>
        public enum WZVariant
        {
            /// <summary>
            ///   MapleStory SEA
            /// </summary>
            MSEA = 0,

            /// <summary>
            ///   Korea MapleStory
            /// </summary>
            KMS = 0,

            /// <summary>
            ///   Korea MapleStory (Tespia)
            /// </summary>
            KMST = 0,

            /// <summary>
            ///   Japan MapleStory
            /// </summary>
            JMS = 0,

            /// <summary>
            ///   Japan MapleStory (Tespia)
            /// </summary>
            JMST = 0,

            /// <summary>
            ///   Europe MapleStory
            /// </summary>
            EMS = 0,

            /// <summary>
            ///   Global MapleStory
            /// </summary>
            GMS = 1,

            /// <summary>
            ///   Global MapleStory (Tespia)
            /// </summary>
            GMST = 1,

            /// <summary>
            ///   Taiwan MapleStory
            /// </summary>
            TMS = 1,

            /// <summary>
            ///   Brazil MapleStory
            /// </summary>
            BMS = 2,

            /// <summary>
            ///   Classic MapleStory (Data.wz)
            /// </summary>
            Classic = 2
        }
    }
}