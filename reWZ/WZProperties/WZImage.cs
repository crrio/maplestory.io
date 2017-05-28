// reWZ is copyright angelsl, 2011 to 2013 inclusive.
// 
// This file (WZImage.cs) is part of reWZ.
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

namespace reWZ.WZProperties
{
    /// <summary>
    ///   A WZ image in a WZ file.
    /// </summary>
    public sealed class WZImage : WZObject
    {
        internal bool _encrypted;
        private bool _parsed;
        internal WZBinaryReader _r;
        private Func<WZBinaryReader> _transform;

        internal WZImage(string name, WZObject parent, WZFile file, WZBinaryReader reader, Func<WZBinaryReader> trans = null) : base(name, parent, file, true, WZObjectType.Image)
        {
            _r = reader;
            _transform = trans;
            if ((file._flag & WZReadSelection.EagerParseImage) == WZReadSelection.EagerParseImage) Parse();
        }

        /// <summary>
        ///   Returns the child with the name <paramref name="childName" /> .
        /// </summary>
        /// <param name="childName"> The name of the child to return. </param>
        /// <returns> The retrieved child. </returns>
        public override WZObject this[string childName]
        {
            get
            {
                if(!_parsed) Parse();
                return base[childName];
            }
        }

        /// <summary>
        ///   Returns the number of children this property contains.
        /// </summary>
        public override int ChildCount
        {
            get
            {
                if (!_parsed) Parse();
                return base.ChildCount;
            }
        }

        public override System.Collections.Generic.IEnumerator<WZObject> GetEnumerator()
        {
            if (!_parsed) Parse();
            return base.GetEnumerator();
        }
        /// <summary>
        ///   Checks if this property has a child with name <paramref name="name" /> .
        /// </summary>
        /// <param name="name"> The name of the child to locate. </param>
        /// <returns> true if this property has such a child, false otherwise or if this property cannot contain children. </returns>
        public override bool HasChild(string name)
        {
            if (!_parsed) Parse();
            return base.HasChild(name);
        }

        private void Parse()
        {
            lock (File._lock) {
                if (!_parsed)
                {
                    _r.Seek(0);
                    if (_r.ReadByte() != 0x73) WZFile.Die("WZImage with invalid header (not beginning with 0x73!)");
                    if ((int)File._variant == 2) _encrypted = false;
                    else if (_r.PeekFor(() => _r.ReadWZString()) == "Property") _encrypted = true;
                    else if (_r.PeekFor(() => _r.ReadWZString(false)) == "Property") _encrypted = false;
                    else WZFile.Die("WZImage with invalid header (no Property string! check your WZVariant)");
                    if (_r.ReadWZString(_encrypted) != "Property") WZFile.Die("Failed to determine image encryption!");
                    if (_r.ReadUInt16() != 0) WZFile.Die("WZImage with invalid header (no zero UInt16!)");
                    WZExtendedParser.ParsePropertyList(_r, this, this, _encrypted).ForEach(Add);
                    _parsed = true;
                    if (_transform == null) return;
                    _r.Close(false);
                    _r = _transform();
                    _transform = null;
                }
            }
        }
    }
}