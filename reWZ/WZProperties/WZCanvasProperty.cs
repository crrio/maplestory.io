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
using ImageSharp;
using ImageSharp.ColorSpaces;
using ImageSharp.Drawing;
using ImageSharp.Formats;
using ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace reWZ.WZProperties
{
    /// <summary>
    ///   A Image<Rgba32> property, containing an image, and children.
    ///   Please dispose any parsed Canvas properties once they are no longer needed, and before the containing WZ file is disposed.
    /// </summary>
    public sealed class WZCanvasProperty : WZDelayedProperty<Image<Rgba32>>, IDisposable
    {
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

        internal override bool Parse(WZBinaryReader br, bool initial, out Image<Rgba32> result)
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

        private Image<Rgba32> ParsePNG(int width, int height, int format, byte[] data)
        {
            byte[] sourceData;
            using (MemoryStream @in = new MemoryStream(data, 2, data.Length - 2))
                sourceData = WZBinaryReader.Inflate(@in);
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
                        Debug.WriteLine("Warning; dec.Length != 4wh; 32BPP");
                        byte[] proper = new byte[width*height*4];
                        Buffer.BlockCopy(sourceData, 0, proper, 0, Math.Min(proper.Length, sourceDataLength));
                        sourceData = proper;
                    }
                    //                    _gcH = GCHandle.Alloc(sourceData, GCHandleType.Pinned);
                    return ImageSharp.Image.LoadPixelData<Argb32>(new Span<byte>(sourceData), width, height).To<Rgba32>();
//                    return new Image<Rgba32>(width, height, width << 2, PixelFormat.Format32bppArgb, _gcH.AddrOfPinnedObject());
                case 513:
                    if (sourceDataLength != width * height * 2)
                    {
                        Debug.WriteLine("Warning; dec.Length != 2wh; 16BPP");
                        byte[] proper = new byte[width*height*2];
                        Buffer.BlockCopy(sourceData, 0, proper, 0, Math.Min(proper.Length, sourceDataLength));
                        sourceData = proper;
                    }
                    //                    _gcH = GCHandle.Alloc(sourceData, GCHandleType.Pinned);
                    return ImageSharp.Image.LoadPixelData<Rgb565>(new Span<byte>(sourceData), width, height).To<Rgba32>();
//                    return new Image<Rgba32>(width, height, width << 1, PixelFormat.Format16bppRgb565, _gcH.AddrOfPinnedObject());
                case 517:
                    width >>= 4;
                    height >>= 4;
                    goto case 513;
                case 1026: //dxt3
                    destinationData = GetPixelDataDXT3(sourceData, width, height);
                    //                    Image<Rgba32> pngDecoded = new Image<Rgba32>(width, height, PixelFormat.Format32bppArgb);
                    //                    Image<Rgba32>Data bmpdata = pngDecoded.LockBits(new Rectangle(new Vector2(), pngDecoded.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    //                    Marshal.Copy(destinationData, 0, bmpdata.Scan0, destinationData.Length);
                    //                    pngDecoded.UnlockBits(bmpdata);
                    //                    return pngDecoded;
                    return ImageSharp.Image.LoadPixelData<Argb32>(new Span<byte>(destinationData), width, height).To<Rgba32>();
                case 2050:
                    destinationData = GetPixelDataDXT5(sourceData, width, height);
                    return ImageSharp.Image.LoadPixelData<Argb32>(new Span<byte>(destinationData), width, height).To<Rgba32>();
                default:
                    return WZFile.Die<Image<Rgba32>>(String.Format("Unknown Image<Rgba32> format {0}.", format));
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

        #region DXT1 Color
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
        #endregion

        #region DXT3/DXT5 Alpha
        private static void ExpandAlphaTableDXT3(byte[] alpha, byte[] rawData, int offset)
        {
            for (int i = 0; i < 16; i += 2, offset++)
            {
                alpha[i + 0] = (byte)(rawData[offset] & 0x0f);
                alpha[i + 1] = (byte)((rawData[offset] & 0xf0) >> 4);
            }
            for (int i = 0; i < 16; i++)
            {
                alpha[i] = (byte)(alpha[i] | (alpha[i] << 4));
            }
        }

        private static void ExpandAlphaTableDXT5(byte[] alpha, byte a0, byte a1)
        {
            alpha[0] = a0;
            alpha[1] = a1;
            if (a0 > a1)
            {
                for (int i = 2; i < 8; i++)
                {
                    alpha[i] = (byte)(((8 - i) * a0 + (i - 1) * a1 + 3) / 7);
                }
            }
            else
            {
                for (int i = 2; i < 6; i++)
                {
                    alpha[i] = (byte)(((6 - i) * a0 + (i - 1) * a1 + 2) / 5);
                }
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
        #endregion

        public static Rgba32 RGB565ToColor(ushort val)
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // There's no longer anything to dispose :^)
        }
    }
}