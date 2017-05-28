// reWZ is copyright angelsl, 2011 to 2013 inclusive.
// 
// This file (WZProperty.cs) is part of reWZ.
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
using System.Collections.Generic;

namespace reWZ.WZProperties
{
    /// <summary>
    /// Ab abstract class representing a WZ property that contains a value of type <typeparamref name="T"/> and is lazy-loaded.
    /// </summary>
    /// <typeparam name="T">The type that this property contains.</typeparam>
    public abstract class WZDelayedProperty<T> : WZProperty<T>
    {
        private readonly long _offset;
        /// <summary>
        /// Whether the delayed property has been parsed.
        /// </summary>
        protected bool _parsed;

        internal WZDelayedProperty(string name, WZObject parent, WZImage container, bool children, WZObjectType type) : base(name, parent, default(T), container, children, type)
        {
            _offset = container._r.BaseStream.Position;
            lock (File._lock)
                _parsed = Parse(container._r, true, out _value);
        }

        internal abstract bool Parse(WZBinaryReader r, bool initial, out T result);

        /// <summary>
        ///   The value held by this WZ property.
        /// </summary>
        public override T Value
        {
            get
            {
                if (!_parsed)
                    lock (File._lock)
                        _parsed = Image._r.PeekFor(() =>
                                                   {
                                                       Image._r.Seek(_offset);
                                                       return Parse(Image._r, false, out _value);
                                                   });
                return _value;
            }
        }
    }

    /// <summary>
    ///   An abstract class representing a WZ property that contains a value of type <typeparamref name="T" /> .
    /// </summary>
    /// <typeparam name="T"> The type that this property contains. </typeparam>
    public abstract class WZProperty<T> : WZObject
    {
        private readonly WZImage _image;
        internal T _value;

        internal WZProperty(string name, WZObject parent, T value, WZImage container, bool children, WZObjectType type) : base(name, parent, container.File, children, type)
        {
            _value = value;
            _image = container;
        }

        /// <summary>
        ///   The value held by this WZ property.
        /// </summary>
        public virtual T Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        ///   The image that this property resides in.
        /// </summary>
        public WZImage Image
        {
            get { return _image; }
        }
        
    }

    internal static class WZExtendedParser
    {
        internal static List<WZObject> ParsePropertyList(WZBinaryReader r, WZObject parent, WZImage f, bool encrypted)
        {
            lock (f.File._lock) {
                int num = r.ReadWZInt();
                List<WZObject> ret = new List<WZObject>(num);
                for (int i = 0; i < num; ++i) {
                    string name = r.ReadWZStringBlock(encrypted);
                    switch (r.ReadByte()) {
                        case 0:
                            ret.Add(new WZNullProperty(name, parent, f));
                            break;
                        case 0x0B:
                        case 2:
                            ret.Add(new WZUInt16Property(name, parent, r, f));
                            break;
                        case 3:
                            ret.Add(new WZInt32Property(name, parent, r, f));
                            break;
                        case 4:
                            ret.Add(new WZSingleProperty(name, parent, r, f));
                            break;
                        case 5:
                            ret.Add(new WZDoubleProperty(name, parent, r, f));
                            break;
                        case 8:
                            ret.Add(new WZStringProperty(name, parent, f));
                            break;
                        case 9:
                            uint blockLen = r.ReadUInt32();
                            ret.Add(r.PeekFor(() => ParseExtendedProperty(name, r, parent, f, encrypted)));
                            r.Skip(blockLen);
                            break;
                        case 20:
                            ret.Add(new WZInt64Property(name, parent, r, f));
                            break;
                        default:
                            return WZFile.Die<List<WZObject>>("Unknown property type at ParsePropertyList");
                    }
                }
                return ret;
            }
        }

        internal static WZObject ParseExtendedProperty(string name, WZBinaryReader r, WZObject parent, WZImage f, bool encrypted)
        {
            lock (f.File._lock) {
                string type = r.ReadWZStringBlock(encrypted);
                switch (type) {
                    case "Property":
                        r.Skip(2);
                        return new WZSubProperty(name, parent, r, f);
                    case "Canvas":
                        return new WZCanvasProperty(name, parent, r, f);
                    case "Shape2D#Vector2D":
                        return new WZPointProperty(name, parent, r, f);
                    case "Shape2D#Convex2D":
                        return new WZConvexProperty(name, parent, r, f);
                    case "Sound_DX8":
                        return new WZAudioProperty(name, parent, f);
                    case "UOL":
                        r.Skip(1);
                        return new WZUOLProperty(name, parent, r, f);
                    default:
                        return WZFile.Die<WZObject>(String.Format("Unknown ExtendedProperty type \"{0}\"", type));
                }
            }
        }
    }
}