// reWZ is copyright angelsl, 2011 to 2013 inclusive.
// 
// This file (WZCanvasProperty.cs) is part of reWZ.
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace reWZ.WZProperties
{
    /// <summary>
    ///   A bitmap property, containing an image, and children.
    ///   Please dispose any parsed Canvas properties once they are no longer needed, and before the containing WZ file is disposed.
    /// </summary>
    public sealed class WZCanvasProperty : WZDelayedProperty<Bitmap>, IDisposable
    {
        private GCHandle _gcH;

        internal WZCanvasProperty(string name, WZObject parent, WZBinaryReader br, WZImage container)
            : base(name, parent, container, true, WZObjectType.Canvas)
        {}

        /// <summary>
        /// Destructor.
        /// </summary>
        ~WZCanvasProperty()
        {
            Dispose();
        }

        internal override bool Parse(WZBinaryReader br, bool initial, out Bitmap result)
        {
            bool skip = (File._flag & WZReadSelection.NeverParseCanvas) == WZReadSelection.NeverParseCanvas, eager = (File._flag & WZReadSelection.EagerParseCanvas) == WZReadSelection.EagerParseCanvas;
            if (skip && eager) {
                result = null;
                return WZFile.Die<bool>("Both NeverParseCanvas and EagerParseCanvas are set.");
            }
            br.Skip(1);
            if (br.ReadByte() == 1) {
                br.Skip(2);
                List<WZObject> l = WZExtendedParser.ParsePropertyList(br, this, Image, Image._encrypted);
                if (ChildCount == 0) l.ForEach(Add);
            }
            int width = br.ReadWZInt(); // width
            int height = br.ReadWZInt(); // height
            int format1 = br.ReadWZInt(); // format 1
            int format2 = br.ReadByte(); // format 2
            br.Skip(4);
            int blockLen = br.ReadInt32();
            if ((initial || skip) && !eager) br.Skip(blockLen); // block Len & png data
            else {
                br.Skip(1);
                ushort header = br.PeekFor(() => br.ReadUInt16());
                byte[] pngData = br.ReadBytes(blockLen - 1);
                result = ParsePNG(width, height, format1 + format2, (header != 0x9C78 && header != 0xDA78) ? DecryptPNG(pngData) : pngData);
                return true;
            }
            result = null;
            return skip;
        }

        private byte[] DecryptPNG(byte[] @in)
        {
            using (MemoryStream @sIn = new MemoryStream(@in, false))
            using (BinaryReader @sBr = new BinaryReader(@sIn))
            using (MemoryStream @sOut = new MemoryStream(@in.Length)) {
                while (@sIn.Position < @sIn.Length) {
                    int blockLen = @sBr.ReadInt32();
                    @sOut.Write(File._aes.DecryptBytes(@sBr.ReadBytes(blockLen)), 0, blockLen);
                }
                return @sOut.ToArray();
            }
        }

        private unsafe Bitmap ParsePNG(int width, int height, int format, byte[] data)
        {
            byte[] dec;
            using (MemoryStream @in = new MemoryStream(data, 2, data.Length - 2))
                dec = WZBinaryReader.Inflate(@in);
            int decLen = dec.Length;
            switch (format) {
                case 1:
                    byte[] argb = new byte[width*height*4];
                    fixed (byte* r = argb, t = dec) {
                        byte* s = r, u = t;
                        for (int i = 0; i < decLen; i++) {
                            *(s++) = (byte)(((*u) & 0x0F)*0x11);
                            *(s++) = (byte)(((*(u++) & 0xF0) >> 4)*0x11);
                        }
                    }
                    decLen *= 2;
                    dec = argb;
                    goto case 2;
                case 2:
                    if (decLen != width * height * 4)
                    {
                        Debug.WriteLine("Warning; dec.Length != 4wh; 32BPP");
                        byte[] proper = new byte[width*height*4];
                        Buffer.BlockCopy(dec, 0, proper, 0, Math.Min(proper.Length, decLen));
                        dec = proper;
                    }
                    _gcH = GCHandle.Alloc(dec, GCHandleType.Pinned);
                    return new Bitmap(width, height, width << 2, PixelFormat.Format32bppArgb, _gcH.AddrOfPinnedObject());
                case 513:
                    if (decLen != width * height * 2)
                    {
                        Debug.WriteLine("Warning; dec.Length != 2wh; 16BPP");
                        byte[] proper = new byte[width*height*2];
                        Buffer.BlockCopy(dec, 0, proper, 0, Math.Min(proper.Length, decLen));
                        dec = proper;
                    }
                    _gcH = GCHandle.Alloc(dec, GCHandleType.Pinned);
                    return new Bitmap(width, height, width << 1, PixelFormat.Format16bppRgb565, _gcH.AddrOfPinnedObject());
                case 517:
                    width >>= 4;
                    height >>= 4;
                    goto case 513;
                case 1026:
                    throw new NotSupportedException("TODO: Re-implement DXT3 compressed images");
/*                    byte[] image = ManagedSquish.Squish.DecompressImage(dec, width, height, ManagedSquish.SquishFlags.Dxt3);
                    _gcH = GCHandle.Alloc(image, GCHandleType.Pinned);
                    return new Bitmap(width, height, width << 2, PixelFormat.Format32bppArgb, _gcH.AddrOfPinnedObject());*/
                default:
                    return WZFile.Die<Bitmap>(String.Format("Unknown bitmap format {0}.", format));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_parsed || _value == null) return;
            _value.Dispose();
            _gcH.Free();
            _parsed = false;
        }
    }
}