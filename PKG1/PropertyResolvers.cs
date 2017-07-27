using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using SixLabors.Primitives;
using ImageSharp;

namespace PKG1 {
    public class PropertyResolvers {
        private readonly PackageCollection _collection;

        public PropertyResolvers(PackageCollection collection) {
            _collection = collection;
        }

        public WZProperty Image(WZReader reader, WZProperty self) {
            self.ContainerStartLocation = self.Offset;
            reader.Container = self;
            byte imgType = reader.ReadByte();
            if(imgType == 1)
                return new WZPropertyWeak<string>(() => {
                    using (reader = self.FileContainer.GetContentReader(null, self)) {
                        reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                        reader.ReadByte();
                        return reader.ReadLuaScript();
                    }
                }, self.Name, self.Path, self.FileContainer, PropertyType.String, self.Parent, self.Size, self.Checksum, self.Offset);
            if(imgType != 0x73) throw new InvalidOperationException("Unknown image type, not supported!");
            if(!reader.ReadWZString().Equals("Property")) throw new InvalidOperationException("No supporting nested encryption yet");
            if(reader.ReadInt16() != 0) throw new InvalidOperationException("Image needs to have 0 as part of header");

            self.LoadChildren += () => {
                using (reader = self.FileContainer.GetContentReader(null, self)) {
                    reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);

                    if(reader.ReadByte() != 0x73) throw new InvalidOperationException("Image should have header of 0x73");
                    if(!reader.ReadWZString().Equals("Property")) throw new InvalidOperationException("No supporting nested encryption yet");
                    if(reader.ReadInt16() != 0) throw new InvalidOperationException("Image needs to have 0 as part of header");

                    return PropertyList(reader, self).ToDictionary(c => c.Name.Replace(".img", ""), c => c);
                }
            };

            return self;
        }

        public WZProperty SubProp(WZReader reader, WZProperty self) {
            self.LoadChildren += () => {
                using (reader = self.FileContainer.GetContentReader(null, self)) {
                    reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                    return PropertyList(reader, self).ToDictionary(c => c.Name.Replace(".img", ""), c => c);
                }
            };

            return self;
        }

        public IEnumerable<WZProperty> PropertyList(WZReader reader, WZProperty parent)
            => Enumerable.Range(0, reader.ReadWZInt()).Select(i => {
                uint position = (uint)reader.BaseStream.Position;
                string name = reader.ReadWZStringBlock();
                byte type = reader.ReadByte();
                switch (type) {
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
                        return new WZPropertyVal<string>(reader.ReadWZStringBlock(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Double, parent, 0, 0, position);
                    case 9:
                        uint blockLen = reader.ReadUInt32();
                        WZProperty result = reader.PeekFor(() => ParseExtendedProperty(reader, parent, name));
                        reader.BaseStream.Seek(blockLen, SeekOrigin.Current);
                        return result;
                    case 20:
                        return new WZPropertyVal<long>(reader.ReadWZLong(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Int64, parent, 4, 0, position);
                    case 21:
                        return new WZPropertyVal<ulong>((ulong)reader.ReadWZLong(), name, Path.Combine(parent.Path, name), parent.FileContainer, PropertyType.Int64, parent, 4, 0, position);
                    default:
                        throw new Exception("Unknown property type at ParsePropertyList");
                }
            }).ToArray();

        WZProperty ParseExtendedProperty(WZReader reader, WZProperty parent, string name, bool maintainReader = false) {
            string type = reader.ReadWZStringBlock();
            PropertyType propType;
            switch (type) {
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

        public WZProperty Directory(WZReader reader, WZProperty self) {
            self.LoadChildren += () => {
                using (reader = self.FileContainer.GetContentReader(null, self)) {
                    reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                    int count = reader.ReadWZInt();
                    WZProperty[] children = new WZProperty[count];
                    string name = null;
                    for(int i = 0; i < count; ++i){
                        byte type = reader.ReadByte();
                        switch (type) {
                            case 1:
                                reader.ReadBytes(10);
                                continue;
                            case 2:
                                int dedupedAt = (int)(reader.ReadInt32() + reader.ContentsStart);
                                reader.PeekFor(() => {
                                    reader.BaseStream.Position = dedupedAt;
                                    type = reader.ReadByte();
                                    name = reader.ReadWZString();
                                });
                                break;
                            case 3:
                            case 4:
                                name = reader.ReadWZString();
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
                            type == 3 ? PropertyType.Directory : type == 4 ? PropertyType.Image : throw new InvalidOperationException("Not sure how this happened, but it's not possilbe."),
                            self,
                            size,
                            checksum,
                            offset
                        );
                        // These can be lazy loaded
                        children[i] = Resolve(reader.Package, childProperty);
                    }
                    return children.ToDictionary(c => c.Name.Replace(".img", ""), c => c);
                }
            };

            return self;
        }

        public WZProperty Audio(WZReader reader, WZProperty self) {
            //reader.BaseStream.Seek(1, SeekOrigin.Current);
            byte unk = reader.ReadByte();
            int length = reader.ReadWZInt();
            int duration = reader.ReadWZInt();

            WZProperty result = new WZPropertyWeak<byte[]>(
                () => {
                    Package.Logging($"{self.Path} (Audio) - {unk}");
                    using(reader = self.FileContainer.GetContentReader(null, self)) {
                        reader.BaseStream.Seek(self.Offset + 9 + 84, SeekOrigin.Begin);
                        return reader.ReadBytes(length);
                    }
                }, self
            );

            result.Size = (uint)length;

            return result;
        }

        public WZProperty Convex(WZReader reader, WZProperty self) {
            int count = reader.ReadWZInt();
            self.Children = Enumerable.Range(0, count).Select(c => ParseExtendedProperty(reader, self, c.ToString(), true)).ToDictionary(c => c.Name.Replace(".img", ""), c => c);
            return self;
        }

        public WZProperty Vector(WZReader reader, WZProperty self)
            => new WZPropertyVal<Point>(new Point(reader.ReadWZInt(), reader.ReadWZInt()), self);

        public WZProperty UOL(WZReader reader, WZProperty self)
            => new WZPropertyVal<string>(reader.ReadWZStringBlock(), self);

        public WZProperty Resolve(Package container, WZProperty self) {
            // Determine lazy loading here
            using (WZReader reader = container.GetContentReader(null, self)) {
                reader.BaseStream.Seek(self.Offset, SeekOrigin.Begin);
                return Resolve(container, self, reader);
            }
        }

        public WZProperty Resolve(Package container, WZProperty self, WZReader reader) {
            switch(self.Type) {
                case PropertyType.Directory:
                    return Directory(reader, self);
                case PropertyType.Image:
                    return Image(reader, self);
                case PropertyType.SubProperty:
                    return SubProp(reader, self);
                case PropertyType.Convex:
                    return Convex(reader, self);
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

        WZProperty Canvas(WZReader reader, WZProperty self)
        {
            // Define the variables ahead of time that way we can come back to them
            int width = 0, height = 0, format1 = 0, format2 = 0;
            uint blockLen = 0, position = 0;

            // Define what well be doing once we come back
            WZProperty result = new WZPropertyWeak<Image<Rgba32>>(
                () => {
                    using(reader = self.FileContainer.GetContentReader(null, self)) {
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
            if (reader.ReadByte() == 1) {
                reader.BaseStream.Seek(2, SeekOrigin.Current);
                result.Children = PropertyList(reader, result).ToDictionary(c => c.Name.Replace(".img", ""), c => c);
            } else {
                result.Children = new Dictionary<string, WZProperty>();
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