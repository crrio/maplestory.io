using MoreLinq;
using SharpCompress.Compressors.Deflate;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace PKG1
{
    public class WZSerializer
    {
        Queue<Tuple<long, WZProperty>> pending = new Queue<Tuple<long, WZProperty>>();
        Dictionary<string, long> dedupedStrings = new Dictionary<string, long>();
        Dictionary<WZProperty, uint> imgLocations = new Dictionary<WZProperty, uint>();
        byte[] encryptionKey;

        uint VersionKey;
        byte VersionHash;
        ushort Version;

        public WZSerializer(ushort version)
        {
            this.Version = version;
            VersionHash = CalcVersionHash(version, out VersionKey);
        }

        byte CalcVersionHash(ushort version, out uint versionKey)
        {
            uint key = version.ToString().Select(c => (uint)c).Aggregate((uint)0, (result, next) => {
                result <<= 5;
                result += (next + 1);
                return result;
            });
            versionKey = key;
            return BitConverter.GetBytes(key).Aggregate((byte)0xFF, (result, next) => (byte)(result ^ next));
        }

        public void Image(BinaryWriter writer, WZProperty self)
        {
            if (!imgLocations.ContainsKey(self))
                imgLocations.Add(self, (uint)writer.BaseStream.Position);
            else
                imgLocations[self] = (uint)writer.BaseStream.Position; // Pretty sure this should never happen

            using (WZReader reader = self.FileContainer.GetContentReader(null, self))
            {
                reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                writer.Write(reader.ReadBytes((int)self.Size));
            }

            //writer.Write((byte)0x73); // Regular img
            //GetWZStringBytes(writer, "Property");
            //writer.Write((short)0);
            //PropertyList(writer, self.Children);
        }

        public void Lua(BinaryWriter writer, WZProperty self)
        {
            writer.Write((byte)0x01);
            string s = ((IWZPropertyVal<string>)self).Value;
            bool isASCII = s.All(c => c <= byte.MaxValue);
            int length = s.Length * (isASCII ? -1 : 1);

            IEnumerable<byte> lengthBytes;
            if (length >= 127)
                lengthBytes = new byte[] { 127 }.Concat(BitConverter.GetBytes(length));
            else if (length <= -128)
                lengthBytes = new byte[] { 0x80 }.Concat(BitConverter.GetBytes(length * -1));
            else
                lengthBytes = new byte[] { (byte)length };
            writer.Write(lengthBytes.ToArray());

            if (isASCII) writer.Write(Encoding.ASCII.GetBytes(s).Select((c, i) => (byte)(c ^ WZReader.KMSKey[i])).ToArray());
            else writer.Write(Encoding.Unicode.GetBytes(s).Select((c, i) => (byte)(c ^ WZReader.KMSKey[i])).ToArray());
        }

        void GetStringToBlock(BinaryWriter writer, string s) {
            // TODO: Implement string de-duping
            writer.Write((byte)0); // Not de-duped
            GetWZStringBytes(writer, s);
        }

        void GetWZStringBytes(BinaryWriter writer, string s)
        {
            bool isASCII = s.All(c => c <= byte.MaxValue);
            int length = s.Length * (isASCII ? -1 : 1);

            IEnumerable<byte> lengthBytes;
            if (length >= 127)
            {
                lengthBytes = new byte[] { 127 }.Concat(BitConverter.GetBytes(length));
            }
            else if (length <= -128)
            {
                lengthBytes = new byte[] { 0x80 }.Concat(BitConverter.GetBytes(length * -1));
            }
            else lengthBytes = new byte[] { (byte)length };
            writer.Write(lengthBytes.ToArray());
            lengthBytes = null;

            byte[] textData;
            if (isASCII)
            {
                textData = Encoding.ASCII.GetBytes(s);
                byte asciiMask = 0xAA;
                for (int i = 0; i < textData.Length; ++i) textData[i] = (byte)((textData[i] ^ asciiMask++) ^ (encryptionKey != null ? encryptionKey[i] : 0));
            }
            else
            {
                textData = Encoding.Unicode.GetBytes(s);
                ushort unicodeMask = 0xAAAA;
                for (int i = 0; i < textData.Length; i += 2)
                {
                    textData[i] ^= (byte)((unicodeMask & 0xFF) ^ (encryptionKey != null ? encryptionKey[i] : 0));
                    textData[i + 1] ^= (byte)(((unicodeMask >> 8) & 0xFF) ^ (encryptionKey != null ? encryptionKey[i + 1] : 0));

                    unicodeMask++;
                }
            }
            writer.Write(textData);
        }

        public void PropertyList(BinaryWriter writer, IEnumerable<WZProperty> childEnum)
        {
            WZProperty[] children = childEnum.ToArray();

            WZIntToByte(writer, children.Length);
            foreach (WZProperty c in childEnum)
            {
                IEnumerable<byte> val = null;
                GetStringToBlock(writer, c.NameWithoutExtension);
                if (c is WZPropertyVal<sbyte>)
                    writer.Write((byte)0x10);
                else if (c is WZPropertyVal<byte>)
                    writer.Write((byte)0x11);
                else if (c is WZPropertyVal<UInt16>)
                {
                    writer.Write((byte)0x12);
                    writer.Write(BitConverter.GetBytes(c.ResolveFor<UInt16>().Value));
                }
                else if (c is WZPropertyVal<Int32>)
                {
                    writer.Write((byte)3);
                    WZIntToByte(writer, c.ResolveFor<Int32>().Value);
                }
                else if (c is WZPropertyVal<Rgba32>)
                {
                    writer.Write((byte)19);
                    WZIntToByte(writer, (int)c.ResolveFor<Rgba32>().Value.Rgba);
                }
                else if (c is WZPropertyVal<Single>)
                {
                    writer.Write((byte)4);
                    WZSingleToByte(writer, c.ResolveFor<Single>().Value);
                }
                else if (c is WZPropertyVal<Double>)
                {
                    writer.Write((byte)5);
                    writer.Write(BitConverter.GetBytes(c.ResolveFor<Double>().Value));
                }
                else if (c is WZPropertyVal<string> && c.Type != PropertyType.Lua && c.Type != PropertyType.UOL)
                {
                    writer.Write((byte)8);
                    GetStringToBlock(writer, c.ResolveForOrNull<string>() ?? "");
                }
                else if (c is WZPropertyVal<long>)
                {
                    writer.Write((byte)20);
                    WZLongToByte(writer, c.ResolveFor<long>().Value);
                }
                else if (c is WZPropertyVal<ulong>)
                {
                    writer.Write((byte)21);
                    WZLongToByte(writer, (long)c.ResolveFor<ulong>().Value);
                }
                else if (c.Type == PropertyType.Null)
                    writer.Write((byte)0);
                else
                {
                    writer.Write((byte)9);
                    writer.Write((int)0); // BlockLen
                    long position = writer.BaseStream.Position;
                    Resolve(writer, c);
                    uint length = (uint)(writer.BaseStream.Position - position);
                    long newPosition = writer.BaseStream.Position;
                    writer.BaseStream.Position = position - 4;
                    writer.Write(length);
                    writer.BaseStream.Position = newPosition;
                }
            }
        }

        void SubProperty(BinaryWriter writer, WZProperty self) {
            writer.Write((short)0);
            PropertyList(writer, self.GetChildren());
        }

        public void DirectoryChildren(BinaryWriter writer, WZProperty self)
        {
            WZProperty[] children = self.GetChildren().ToArray();
            WZIntToByte(writer, children.Length);

            foreach (WZProperty child in children)
            {
                if (child.Type == PropertyType.Directory)
                    writer.Write((byte)3);
                else if (child.Type == PropertyType.Image || child.Type == PropertyType.Lua)
                    writer.Write((byte)4);
                else throw new InvalidOperationException("Only directories, img, and lua can be directly below a directory object");
                GetWZStringBytes(writer, child.NameWithoutExtension);
                long position = writer.BaseStream.Position;
                pending.Enqueue(new Tuple<long, WZProperty>(position, child));
                WZIntToByte(writer, 123456789);
                WZIntToByte(writer, 123456789);
                writer.Write((uint)0); // Offset
            }
        }

        public void Audio(BinaryWriter writer, WZProperty self)
        {
            byte unk = (byte)(self.HasDefinedMeta && self.Meta.ContainsKey("unk") ? self.Meta["unk"] : 0);
            int duration = (int)(self.HasDefinedMeta && self.Meta.ContainsKey("duration") ? self.Meta["duration"] : 0);
            int length = (int)self.Size;

            writer.Write(unk);
            WZIntToByte(writer, length);
            WZIntToByte(writer, duration);
            writer.Write(self.ResolveForOrNull<byte[]>());
        }

        public void ConvexChildren(BinaryWriter writer, WZProperty self)
        {
            WZProperty[] children = self.GetChildren().ToArray();
            WZIntToByte(writer, children.Length);

            foreach (WZProperty child in children) Resolve(writer, child);
        }

        void WZIntToByte(BinaryWriter writer, int val)
        {
            if (val > sbyte.MaxValue || val <= sbyte.MinValue)
                writer.Write(new byte[] { 0x80 }.Concat(BitConverter.GetBytes(val)).ToArray());
            else writer.Write((byte)val);
        }
        void WZSingleToByte(BinaryWriter writer, float val)
        {
            if (val > sbyte.MaxValue || val <= sbyte.MinValue || Math.Round(val, 0) != val)
                writer.Write(new byte[] { 0x80 }.Concat(BitConverter.GetBytes(val)).ToArray());
            else writer.Write((byte)val);
        }
        void WZLongToByte(BinaryWriter writer, long val)
        {
            if (val > sbyte.MaxValue || val <= sbyte.MinValue)
                writer.Write(new byte[] { 0x80 }.Concat(BitConverter.GetBytes(val)).ToArray());
            else writer.Write((byte)val);
        }

        public void Vector(BinaryWriter writer, WZProperty self)
        {
            Point pos = self.ResolveFor<Point>() ?? Point.Empty;
            WZIntToByte(writer, pos.X);
            WZIntToByte(writer, pos.Y);
        }

        public void UOL(BinaryWriter writer, WZPropertyVal<string> self)
            => GetStringToBlock(writer, self.Value);

        public void Resolve(BinaryWriter writer, WZProperty parent)
        {
            switch (parent.Type)
            {
                case PropertyType.Directory:
                    DirectoryChildren(writer, parent);
                    break;
                case PropertyType.Image:
                    Image(writer, parent);
                    break;
                case PropertyType.Lua:
                    Lua(writer, parent);
                    break;
                case PropertyType.SubProperty:
                    GetStringToBlock(writer, "Property");
                    SubProperty(writer, parent);
                    break;
                case PropertyType.Canvas:
                    GetStringToBlock(writer, "Canvas");
                    Canvas(writer, parent);
                    break;
                case PropertyType.Vector2:
                    GetStringToBlock(writer, "Shape2D#Vector2D");
                    Vector(writer, parent);
                    break;
                case PropertyType.Convex:
                    GetStringToBlock(writer, "Shape2D#Convex2D");
                    ConvexChildren(writer, parent);
                    break;
                case PropertyType.Audio:
                    GetStringToBlock(writer, "Sound_DX8");
                    Audio(writer, parent);
                    break;
                case PropertyType.UOL:
                    GetStringToBlock(writer, "UOL");
                    writer.Write((byte)0);
                    UOL(writer, (WZPropertyVal<string>)parent);
                    break;
            }
        }

        void Canvas(BinaryWriter writer, WZProperty self)
        {
            Image<Rgba32> img = self.ResolveForOrNull<Image<Rgba32>>() ?? new Image<Rgba32>(1, 1);
            WZProperty[] children = self.GetChildren().ToArray();
            byte[] deflated = Deflate(img.CloneAs<Argb32>().SavePixelData<Argb32>());
            writer.Write((byte)0);
            if (children.Length > 0)
            {
                writer.Write(new byte[] { 1, 0, 0 });
                PropertyList(writer, children);
            } else writer.Write((byte)0);
            WZIntToByte(writer, img.Width);
            WZIntToByte(writer, img.Height);
            writer.Write(new byte[] { 1, 1 }); // ARGB32 format, because I'm not about to add logic for the other formats :^)
            writer.Write(new byte[4]); // Unk
            writer.Write(BitConverter.GetBytes(deflated.Length));
            writer.Write((byte)0);
            writer.Write(deflated);
        }

        public static byte[] Deflate(byte[] data)
        {
            using (MemoryStream str = new MemoryStream(data))
            using (MemoryStream result = new MemoryStream())
            using (ZlibStream inflate = new ZlibStream(str, SharpCompress.Compressors.CompressionMode.Compress, SharpCompress.Compressors.Deflate.CompressionLevel.BestCompression))
            {
                inflate.CopyTo(result);

                result.Position = 0;
                return result.ToArray();
            }
        }

        public uint GetWZOffset(uint PositionFromStart, uint ContentsStart, uint unencryptedOffset)
        {
            uint offset = (uint)((PositionFromStart) ^ 0xFFFFFFFF);
            offset *= VersionKey;
            offset -= 0x581C3F6D;
            offset = ROTL(offset, (byte)(offset & 0x1F));
            offset ^= (unencryptedOffset - (ContentsStart * 2));

            return offset;
        }

        static uint ROTL(uint value, byte shiftTimes)
        {
            return (uint)((value << shiftTimes) | (value >> (32 - shiftTimes)));
        }

        public void Serialize(BinaryWriter writer, WZProperty mainDirectory)
        {
            imgLocations.Add(mainDirectory, (uint)writer.BaseStream.Position);
            writer.Write((ushort)VersionHash);
            Resolve(writer, mainDirectory);

            Tuple<long, WZProperty> pendingNode = null;

            while (pending.TryDequeue(out pendingNode))
            {
                uint position = (uint)writer.BaseStream.Position; // If it's over 4gb, you need to break that up. Sorry fam.
                Resolve(writer, pendingNode.Item2);
                uint size = (uint)writer.BaseStream.Position - position;
                writer.BaseStream.Position = pendingNode.Item1 + 1;
                writer.Write(size);
                writer.BaseStream.Position = pendingNode.Item1 + 10;
                WZProperty container = pendingNode.Item2.Container;
                uint contentsStart = imgLocations[container ?? mainDirectory];
                uint positionFromStart = (uint)(writer.BaseStream.Position - contentsStart);
                writer.Write(GetWZOffset(positionFromStart, contentsStart, position)); // Is position supposed to be relative to contentsStart?
                writer.BaseStream.Position = position + size;
            }
        }

        public void Serialize(string file, WZProperty mainDirectory)
        {
            encryptionKey = mainDirectory.Encrypted == EncryptionType.GMS ? WZReader.GMSKey : (mainDirectory.Encrypted == EncryptionType.KMS ? WZReader.KMSKey : null);
            using (FileStream dest = File.OpenWrite(file))
            using(BinaryWriter writer = new BinaryWriter(dest))
            {
                writer.Write(Encoding.ASCII.GetBytes("PKG1"));
                // File size
                writer.Write((long)0);
                // ContentsStartLocation
                writer.Write((int)0);
                byte[] desc = Encoding.ASCII.GetBytes("Minimized WZ / MapleStory.IO");
                writer.Write(desc);
                writer.Write((byte)0);
                uint contentsStartLocation = (uint)dest.Position;
                dest.Position = 12;
                writer.Write(contentsStartLocation);
                dest.Position = contentsStartLocation;

                Serialize(writer, mainDirectory);
            }
        }
    }
}
