using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PKG1
{
    public static class PropertyResolvers
    {
        public static WZProperty Image(WZReader reader, WZProperty self)
        {
            if (self.Offset >= reader.BaseStream.Length) return self;
            self.ContainerStartLocation = self.Offset;
            reader.Container = self;
            byte imgType = reader.ReadByte();
            if (imgType == 1)
                return new WZPropertyWeak<string>(() =>
                {
                    using (reader = self.FileContainer.GetContentReader(null, self))
                    {
                        reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                        reader.ReadByte();
                        return reader.ReadLuaScript();
                    }
                }, self.NameWithoutExtension, self.Path, self.FileContainer, PropertyType.Lua, self.Parent, self.Size, self.Checksum, self.Offset);
            if (imgType != 0x73) throw new InvalidOperationException("Unknown image type, not supported!");
            long oldPosition = reader.BaseStream.Position;
            if (!reader.ReadWZString(false, self.Encrypted).Equals("Property"))
            {
                reader.BaseStream.Position = oldPosition;
                if (!reader.ReadWZStringExpecting(out self.Encrypted, "Property", false))
                    throw new InvalidOperationException("Unknown encryption method");
            }
            if (reader.ReadInt16() != 0) throw new InvalidOperationException("Image needs to have 0 as part of header");

            return self;
        }

        public static IEnumerable<WZProperty> ImageChildren(WZProperty self)
        {
            using (WZReader reader = self.FileContainer.GetContentReader(null, self))
            {
                if (self.Offset >= reader.BaseStream.Length) yield break;
                reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);

                if (reader.ReadByte() != 0x73) throw new InvalidOperationException("Image should have header of 0x73");
                long oldPosition = reader.BaseStream.Position;
                if (!reader.ReadWZString(false, self.Encrypted).Equals("Property"))
                {
                    reader.BaseStream.Position = oldPosition;
                    if (!reader.ReadWZStringExpecting(out self.Encrypted, "Property", false))
                        throw new InvalidOperationException("Unknown encryption method");
                }
                if (reader.ReadInt16() != 0) throw new InvalidOperationException("Image needs to have 0 as part of header");

                foreach (WZProperty prop in PropertyList(reader, self).ToArray()) yield return prop;
            }
        }

        public static IEnumerable<WZProperty> SubPropChildren(WZProperty self)
        {
            using (WZReader reader = self.FileContainer.GetContentReader(null, self))
            {
                reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                foreach (WZProperty prop in PropertyList(reader, self).ToArray()) yield return prop;
            }
        }

        public static IEnumerable<WZProperty> PropertyList(WZReader reader, WZProperty parent)
        {
            if (reader.BaseStream.Position + 4 >= reader.BaseStream.Length) return new WZProperty[0];
            return Enumerable.Range(0, reader.ReadWZInt()).Select(i =>
            {
                uint position = (uint)reader.BaseStream.Position;
                if (position >= reader.BaseStream.Length) return null;
                string name = reader.ReadWZStringBlock(parent.Encrypted | parent.Container.Encrypted);
                byte type = reader.ReadByte();
                switch (type)
                {
                    case 0:
                        return new WZProperty(name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Null, parent, 0, 0, position);
                    case 0x10:
                        return new WZPropertyVal<sbyte>(reader.ReadSByte(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.UInt16, parent, 1, 0, position);
                    case 0x11:
                        return new WZPropertyVal<byte>(reader.ReadByte(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.UInt16, parent, 1, 0, position);
                    case 0x0B:
                    case 2:
                    case 0x12:
                        return new WZPropertyVal<UInt16>(reader.ReadUInt16(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.UInt16, parent, 2, 0, position);
                    case 3:
                        return new WZPropertyVal<Int32>(reader.ReadWZInt(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Int32, parent, 4, 0, position);
                    case 19:
                        return new WZPropertyVal<Rgba32>(new Rgba32((uint)reader.ReadWZInt()), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Int32, parent, 4, 0, position);
                    case 4:
                        return new WZPropertyVal<Single>(reader.ReadWZSingle(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Single, parent, 4, 0, position);
                    case 5:
                        return new WZPropertyVal<Double>(reader.ReadDouble(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Double, parent, 8, 0, position);
                    case 8:
                        return new WZPropertyVal<string>(reader.ReadWZStringBlock(parent.Encrypted | parent.Container.Encrypted), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.String, parent, 0, 0, position);
                    case 9:
                        uint blockLen = reader.ReadUInt32();
                        WZProperty result = reader.PeekFor(() => ParseExtendedProperty(reader, parent, name));
                        reader.BaseStream.Seek(blockLen, SeekOrigin.Current);
                        return result;
                    case 20:
                        return new WZPropertyVal<long>(reader.ReadWZLong(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Int64, parent, 8, 0, position);
                    case 21:
                        return new WZPropertyVal<ulong>((ulong)reader.ReadWZLong(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Int64, parent, 8, 0, position);
                    default:
                        throw new Exception("Unknown property type at ParsePropertyList");
                }
            });
        }

        static byte[] calcAggregateHash(SHA1 sha = null, params byte[][] hashes)
        {
            hashes = hashes.Where(c => c.Length != 0).ToArray();
            if (hashes.Length == 0) return new byte[0];
            byte[] firstHash = hashes.First();
            byte[] allHashes = new byte[firstHash.Length * (hashes.Length + 1)];
            Buffer.BlockCopy(firstHash, 0, allHashes, 0, firstHash.Length);
            int i = 0;
            foreach (byte[] childHash in hashes) Buffer.BlockCopy(childHash, 0, allHashes, (++i) * firstHash.Length, firstHash.Length);
            return sha.ComputeHash(allHashes);
        }

        public static byte[] ResolveHash(WZProperty prop)
        {
            using (WZReader reader = prop.FileContainer.GetContentReader(null, prop))
            using (SHA1 sha = SHA1.Create())
            {
                reader.BaseStream.Position = prop.Offset;
                switch (prop.Type)
                {
                    case PropertyType.Image:
                        reader.BaseStream.Position = prop.ContainerStartLocation;
                        byte imgType = reader.ReadByte();
                        if (imgType == 1)
                        {
                            byte unk = reader.ReadByte();
                            return reader.GetLuaScriptBytes().ToArray();
                        }
                        else return new byte[0];
                    case PropertyType.Directory: return new byte[0];
                    case PropertyType.SubProperty: return new byte[0];
                    case PropertyType.Convex: return new byte[0];
                    case PropertyType.Vector2: return sha.ComputeHash(reader.GetWZIntBytes().Concat(reader.GetWZIntBytes()).ToArray());
                    case PropertyType.UOL: return sha.ComputeHash(reader.GetWZStringBlockBytes(prop.Encrypted | prop.Container.Encrypted));
                    case PropertyType.Audio:
                        using (Stream sub = new SubStream(reader.BaseStream, (int)reader.BaseStream.Position, prop.Size))
                            return sha.ComputeHash(sub);
                    case PropertyType.Canvas:
                        reader.BaseStream.Seek(1, SeekOrigin.Current);
                        byte[][] childHashes;
                        if (reader.ReadByte() == 1)
                        {
                            reader.BaseStream.Seek(2, SeekOrigin.Current);
                            childHashes = PropertyList(reader, prop).ToArray().Select(c => ResolveHash(c)).ToArray();
                        }
                        else
                            childHashes = new byte[0][];
                        int width = reader.ReadWZInt(); // width
                        int height = reader.ReadWZInt(); // height
                        int format1 = reader.ReadWZInt(); // format 1
                        byte format2 = reader.ReadByte(); // format 2
                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                        uint blockLen = (uint)reader.ReadInt32();
                        ushort header = reader.PeekFor(() => reader.ReadUInt16());
                        byte[] imageHash = new byte[0];
                        using (Stream sub = new SubStream(reader.BaseStream, reader.BaseStream.Position, blockLen - 1))
                            imageHash = sha.ComputeHash(sub);

                        if (childHashes.Length > 0)
                            return calcAggregateHash(sha, childHashes.Prepend(imageHash).ToArray());
                        else return imageHash;
                    default:
                        byte[] nameHash = sha.ComputeHash(reader.GetWZStringBlockBytes(prop.Encrypted | prop.Container.Encrypted));
                        byte type = reader.ReadByte();
                        switch (type)
                        {
                            case 0: return nameHash;
                            case 0x10: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.ReadBytes(1)));
                            case 0x11: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.ReadBytes(1)));
                            case 0x0B:
                            case 2:
                            case 0x12: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.ReadBytes(2)));
                            case 3: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.GetWZIntBytes()));
                            case 19: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.GetWZIntBytes()));
                            case 4: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.GetWZSingleBytes()));
                            case 5: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.ReadBytes(8)));
                            case 8: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.GetWZStringBlockBytes(prop.Container.Encrypted)));
                            case 9: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.ReadBytes(reader.ReadInt32())));
                            case 20: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.GetWZLongBytes()));
                            case 21: return calcAggregateHash(sha, nameHash, sha.ComputeHash(reader.GetWZLongBytes()));
                            default:
                                System.Diagnostics.Debugger.Break();
                                Console.WriteLine("Wow");
                                if (type > 100)
                                    throw new Exception("Unknown property type at ParsePropertyList");
                                else return new byte[0];
                        }
                }
            }
        }

        static WZProperty ParseExtendedProperty(WZReader reader, WZProperty parent, string name, bool maintainReader = false)
        {
            string type = reader.ReadWZStringBlock(parent.Encrypted | parent.Container.Encrypted);
            PropertyType propType;
            switch (type)
            {
                case "Property":
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    propType = PropertyType.SubProperty;
                    break;
                case "Canvas":
                    propType = PropertyType.Canvas;
                    break;
                case "Shape2D#Vector2D":
                    propType = PropertyType.Vector2;
                    break;
                case "Shape2D#Convex2D":
                    propType = PropertyType.Convex;
                    break;
                case "Sound_DX8":
                    propType = PropertyType.Audio;
                    break;
                case "UOL":
                    reader.BaseStream.Seek(1, SeekOrigin.Current);
                    propType = PropertyType.UOL;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown ExtendedProperty type {type}");
            }

            WZProperty result = new WZProperty(
                name,
                Path.Combine(parent.Path, name),
                parent.FileContainer,
                propType,
                parent,
                0,
                0,
                (uint)reader.BaseStream.Position
            );

            if (maintainReader)
                return Resolve(parent.FileContainer, result, reader);
            else
                return Resolve(parent.FileContainer, result);
        }

        public static IEnumerable<WZProperty> DirectoryChildren(WZProperty self)
        {
            using (WZReader reader = self.FileContainer.GetContentReader(null, self))
            {
                reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                int count = reader.ReadWZInt();
                WZProperty[] children = new WZProperty[count];
                string name = null;
                for (int i = 0; i < count; ++i)
                {
                    byte type = reader.ReadByte();
                    switch (type)
                    {
                        case 1:
                            reader.ReadBytes(10);
                            continue;
                        case 2:
                            int dedupedAt = (int)(reader.ReadInt32() + reader.ContentsStart);
                            reader.PeekFor(() => {
                                reader.BaseStream.Position = dedupedAt;
                                type = reader.ReadByte();
                                name = reader.ReadWZString(false, self.Encrypted | self.Container.Encrypted);
                            });
                            break;
                        case 3:
                        case 4:
                            name = reader.ReadWZString(false, self.Encrypted | self.Container.Encrypted);
                            break;
                        default:
                            throw new Exception("Unknown child type");
                    }
                    if (name == null) throw new InvalidOperationException("Found a property without a name, this shouldn't be possible.");

                    uint size = (uint)reader.ReadWZInt();
                    int checksum = reader.ReadWZInt();
                    uint offset = reader.ReadWZOffset();
                    WZProperty childProperty = new WZProperty(
                        name,
                        self != null ? Path.Combine(self.Path, name) : name,
                        reader.Package,
                        type == 3 ? PropertyType.Directory : type == 4 ? PropertyType.Image : throw new InvalidOperationException("Not sure what this is, but I don't handle it"),
                        self,
                        size,
                        checksum,
                        offset
                    );
                    // These can be lazy loaded
                    yield return Resolve(reader.Package, childProperty);
                }
            }
        }

        public static WZProperty Audio(WZReader reader, WZProperty self)
        {
            byte unk = reader.ReadByte();
            int length = reader.ReadWZInt();
            int duration = reader.ReadWZInt();

            WZProperty result = new WZPropertyWeak<byte[]>(
                () => {
                    Package.Logging($"{self.Path} (Audio) - {unk}");
                    using (reader = self.FileContainer.GetContentReader(null, self))
                    {
                        reader.BaseStream.Seek(self.Offset + 1, SeekOrigin.Begin);
                        if (length > sbyte.MaxValue || length <= sbyte.MinValue) reader.BaseStream.Seek(5, SeekOrigin.Current);
                        else reader.BaseStream.Seek(1, SeekOrigin.Current);
                        if (duration > sbyte.MaxValue || duration <= sbyte.MinValue) reader.BaseStream.Seek(5, SeekOrigin.Current);
                        else reader.BaseStream.Seek(1, SeekOrigin.Current);
                        return reader.ReadBytes(length);
                    }
                }, self
            );

            result.Size = (uint)length;
            result.Meta.Add("duration", duration);
            result.Meta.Add("unk", unk);

            return result;
        }

        public static IEnumerable<WZProperty> ConvexChildren(WZProperty self)
        {
            using (WZReader reader = self.FileContainer.GetContentReader(null, self))
            {
                reader.BaseStream.Position = self.Offset;
                int count = reader.ReadWZInt();
                for (int i = 0; i < count; ++i) yield return ParseExtendedProperty(reader, self, i.ToString(), true);
            }
        }

        public static WZProperty Vector(WZReader reader, WZProperty self)
            => new WZPropertyVal<Point>(new Point(reader.ReadWZInt(), reader.ReadWZInt()), self);

        public static WZProperty UOL(WZReader reader, WZProperty self)
            => new WZPropertyVal<string>(reader.ReadWZStringBlock(self.Encrypted | self.Container.Encrypted), self);

        public static WZProperty Resolve(Package container, WZProperty self)
        {
            // Determine lazy loading here
            using (WZReader reader = container.GetContentReader(null, self))
            {
                reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                return Resolve(container, self, reader);
            }
        }

        public static WZProperty Resolve(Package container, WZProperty self, WZReader reader)
        {
            switch (self.Type)
            {
                case PropertyType.Directory:
                    return self;
                case PropertyType.Image:
                    return Image(reader, self);
                case PropertyType.SubProperty:
                    return self;
                case PropertyType.Convex:
                    return self;
                case PropertyType.Vector2:
                    return Vector(reader, self);
                case PropertyType.Audio:
                    return Audio(reader, self);
                case PropertyType.Canvas:
                    return Canvas(reader, self);
                case PropertyType.UOL:
                    return UOL(reader, self);
                default:
                    return self;
            }
        }

        static WZProperty Canvas(WZReader reader, WZProperty self)
        {
            // Define the variables ahead of time that way we can come back to them
            int width = 0, height = 0, format1 = 0, format2 = 0;
            uint blockLen = 0, position = 0;

            // Define what well be doing once we come back
            WZProperty result = new WZPropertyWeak<Image<Rgba32>>(
                () => {
                    using (reader = self.FileContainer.GetContentReader(null, self))
                    {
                        reader.BaseStream.Seek(position + 1, SeekOrigin.Begin);
                        ushort header = reader.PeekFor(() => reader.ReadUInt16());
                        return reader.ParsePNG(
                            width,
                            height,
                            format1 + format2,
                            header != 0x9C78 && header != 0xDA78,
                            blockLen - 1
                        );
                    }
                }, self
            );

            reader.BaseStream.Seek(1, SeekOrigin.Current);
            if (reader.ReadByte() == 1) // Has children
            {
                reader.BaseStream.Seek(2, SeekOrigin.Current);
                result.Children = PropertyList(reader, result).ToArray();
            }
            else
            {
                result.Children = new WZProperty[0];
            }
            width = reader.ReadWZInt(); // width
            height = reader.ReadWZInt(); // height
            format1 = reader.ReadWZInt(); // format 1
            format2 = reader.ReadByte(); // format 2
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            blockLen = (uint)reader.ReadInt32();
            result.Size = (uint)blockLen;
            position = (uint)reader.BaseStream.Position;

            return result;
        }
    }
}