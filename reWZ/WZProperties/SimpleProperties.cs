// reWZ is copyright angelsl, 2011 to 2013 inclusive.
// 
// This file (SimpleProperties.cs) is part of reWZ.
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
using System.Linq;
using System.Numerics;

namespace reWZ.WZProperties
{
    /// <summary>
    ///   A struct used to represent nothing.
    /// </summary>
    public struct WZNothing
    {}

    /// <summary>
    ///   Null.
    /// </summary>
    public sealed class WZNullProperty : WZProperty<WZNothing>
    {
        internal WZNullProperty(string name, WZObject parent, WZImage container) : base(name, parent, default(WZNothing), container, false, WZObjectType.Null)
        {}
    }

    /// <summary>
    ///   An unsigned 16-bit integer property.
    /// </summary>
    public sealed class WZUInt16Property : WZProperty<ushort>
    {
        internal WZUInt16Property(string name, WZObject parent, WZBinaryReader reader, WZImage container)
            : base(name, parent, reader.ReadUInt16(), container, false, WZObjectType.UInt16)
        {}
    }

    /// <summary>
    ///   A compressed signed 32-bit integer property.
    /// </summary>
    public sealed class WZInt32Property : WZProperty<int>
    {
        internal WZInt32Property(string name, WZObject parent, WZBinaryReader reader, WZImage container)
            : base(name, parent, reader.ReadWZInt(), container, false, WZObjectType.Int32)
        {}
    }

    public sealed class WZInt64Property : WZProperty<long>
    {
        internal WZInt64Property(string name, WZObject parent, WZBinaryReader reader, WZImage container)
            : base(name, parent, reader.ReadWZInt64(), container, false, WZObjectType.Int64)
        { }
    }

    /// <summary>
    ///   A floating Vector2 number with single precision property.
    /// </summary>
    public sealed class WZSingleProperty : WZProperty<Single>
    {
        internal WZSingleProperty(string name, WZObject parent, WZBinaryReader reader, WZImage container)
            : base(name, parent, ReadSingle(reader), container, false, WZObjectType.Single)
        {}

        private static Single ReadSingle(WZBinaryReader reader)
        {
            byte t = reader.ReadByte();
            return t == 0x80 ? reader.ReadSingle() : (t == 0 ? 0f : WZFile.Die<float>("Unknown byte while reading WZSingleProperty."));
        }
    }

    /// <summary>
    ///   A floating Vector2 number with double precision property.
    /// </summary>
    public sealed class WZDoubleProperty : WZProperty<Double>
    {
        internal WZDoubleProperty(string name, WZObject parent, WZBinaryReader reader, WZImage container)
            : base(name, parent, reader.ReadDouble(), container, false, WZObjectType.Double)
        {}
    }

    /// <summary>
    ///   A string property.
    /// </summary>
    public sealed class WZStringProperty : WZDelayedProperty<String>
    {
        internal WZStringProperty(string name, WZObject parent, WZImage container)
            : base(name, parent, container, false, WZObjectType.String)
        {}

        internal override bool Parse(WZBinaryReader r, bool initial, out string result)
        {
            if (!initial || (File._flag & WZReadSelection.EagerParseStrings) == WZReadSelection.EagerParseStrings) {
                result = r.ReadWZStringBlock(Image._encrypted);
                return true;
            }
            r.SkipWZStringBlock();
            result = null;
            return false;
        }
    }

    /// <summary>
    ///   A Vector2 property, containing an X and Y value pair.
    /// </summary>
    public sealed class WZVector2Property : WZProperty<Vector2>
    {
        internal WZVector2Property(string name, WZObject parent, WZBinaryReader wzbr, WZImage container)
            : base(name, parent, new Vector2(wzbr.ReadWZInt(), wzbr.ReadWZInt()), container, false, WZObjectType.Vector2)
        {}
    }

    /// <summary>
    ///   A link property, used to link to other properties in the WZ file.
    /// </summary>
    public sealed class WZUOLProperty : WZProperty<String>
    {
        internal WZUOLProperty(string name, WZObject parent, WZBinaryReader reader, WZImage container)
            : base(name, parent, reader.ReadWZStringBlock(container._encrypted), container, false, WZObjectType.UOL)
        {}

        /// <summary>
        ///   Resolves the link once.
        /// </summary>
        /// <returns> The WZ object that this link refers to. </returns>
        public WZObject Resolve()
        {
            return Value.Split('/').Where(node => node != ".").Aggregate(Parent, (current, node) => node == ".." ? current.Parent : current[node]);
        }

        /// <summary>
        ///   Resolves the link recursively, repeatedly resolving links until an object is reached.
        /// </summary>
        /// <returns> The non-link WZ object that this link refers to. </returns>
        public WZObject ResolveFully()
        {
            WZObject ret = this;
            while (ret is WZUOLProperty)
                ret = ((WZUOLProperty)ret).Resolve();
            return ret;
        }
    }

    /// <summary>
    ///   A sound property.
    /// </summary>
    public sealed class WZAudioProperty : WZDelayedProperty<byte[]>, IDisposable
    {
        internal WZAudioProperty(string name, WZObject parent, WZImage container)
            : base(name, parent, container, false, WZObjectType.Audio)
        {}

        internal override bool Parse(WZBinaryReader r, bool initial, out byte[] result)
        {
            r.Skip(1);
            int blockLen = r.ReadWZInt(); // sound data length
            r.ReadWZInt(); // sound duration
            //r.Skip(82); // header [82 bytes]
            if (!initial || (File._flag & WZReadSelection.EagerParseAudio) == WZReadSelection.EagerParseAudio) {
                result = r.ReadBytes(blockLen+82);
                return true; // sound data 
            }
            r.Skip(blockLen+82);
            result = null;
            return false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _parsed = false;
            _value = null;
        }
    }
}