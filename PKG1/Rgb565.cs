using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;


namespace PKG1
{
    // <copyright file="Rgb565.cs" company="James Jackson-South">
    // Copyright (c) James Jackson-South and contributors.
    // Licensed under the Apache License, Version 2.0.
    // </copyright>

    /// <summary>
    /// Packed pixel type containing unsigned normalized values ranging from 0 to 1. The x and z components use 5 bits, and the y component uses 6 bits.
    /// <para>
    /// Ranges from &lt;0, 0, 0, 1&gt; to &lt;1, 1, 1, 1&gt; in vector form.
    /// </para>
    /// </summary>
    public struct Rgb565 : IPixel<Rgb565>, IPackedVector<ushort>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rgb565"/> struct.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        /// <param name="z">The z-component</param>
        public Rgb565(float x, float y, float z)
        {
            this.PackedValue = Pack(x, y, z);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgb565"/> struct.
        /// </summary>
        /// <param name="vector">
        /// The vector containing the components for the packed value.
        /// </param>
        public Rgb565(Vector3 vector)
        {
            this.PackedValue = Pack(vector.X, vector.Y, vector.Z);
        }

        /// <inheritdoc/>
        public ushort PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="Rgb565"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgb565"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgb565"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rgb565 left, Rgb565 right)
        {
            return left.PackedValue == right.PackedValue;
        }

        /// <summary>
        /// Compares two <see cref="Rgb565"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgb565"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgb565"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rgb565 left, Rgb565 right)
        {
            return left.PackedValue != right.PackedValue;
        }

        /// <inheritdoc />
        public PixelOperations<Rgb565> CreatePixelOperations() => new PixelOperations<Rgb565>();

        /// <summary>
        /// Expands the packed representation into a <see cref="Vector3"/>.
        /// The vector components are typically expanded in least to greatest significance order.
        /// </summary>
        /// <returns>The <see cref="Vector3"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3()
        {
            return new Vector3(
                        (this.PackedValue & 0x1F) * (1F / 31F),
                        ((this.PackedValue >> 5) & 0x3F) * (1F / 63F),
                        ((this.PackedValue >> 11) & 0x1F) * (1F / 31F));
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackFromVector4(Vector4 vector)
        {
            this.PackedValue = Pack(vector.X, vector.Y, vector.Z);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4()
        {
            return new Vector4(this.ToVector3(), 1F);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackFromRgba32(Rgba32 source)
        {
            this.PackFromVector4(source.ToVector4());
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgb24(ref Rgb24 dest)
        {
            Vector4 vector = this.ToVector4() * 255F;
            dest.R = (byte)MathF.Round(vector.X);
            dest.G = (byte)MathF.Round(vector.Y);
            dest.B = (byte)MathF.Round(vector.Z);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            Vector4 vector = this.ToVector4() * 255F;
            dest.R = (byte)MathF.Round(vector.X);
            dest.G = (byte)MathF.Round(vector.Y);
            dest.B = (byte)MathF.Round(vector.Z);
            dest.A = (byte)MathF.Round(vector.W);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToBgr24(ref Bgr24 dest)
        {
            Vector4 vector = this.ToVector4() * 255F;
            dest.R = (byte)MathF.Round(vector.X);
            dest.G = (byte)MathF.Round(vector.Y);
            dest.B = (byte)MathF.Round(vector.Z);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToBgra32(ref Bgra32 dest)
        {
            Vector4 vector = this.ToVector4() * 255F;
            dest.R = (byte)MathF.Round(vector.X);
            dest.G = (byte)MathF.Round(vector.Y);
            dest.B = (byte)MathF.Round(vector.Z);
            dest.A = (byte)MathF.Round(vector.W);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj is Rgb565) && this.Equals((Rgb565)obj);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rgb565 other)
        {
            return this.PackedValue == other.PackedValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToVector3().ToString();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return this.PackedValue.GetHashCode();
        }

        /// <summary>
        /// Packs the <see cref="float"/> components into a <see cref="ushort"/>.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        /// <param name="z">The z-component</param>
        /// <returns>The <see cref="ushort"/> containing the packed values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort Pack(float x, float y, float z)
        {
            return (ushort)(((int)Math.Round(z.Clamp(0, 1) * 31F) & 0x1F) |
                    (((int)Math.Round(y.Clamp(0, 1) * 63F) & 0x3F) << 5) |
                    (((int)Math.Round(x.Clamp(0, 1) * 31F) & 0x1F) << 11));
        }

        public void PackFromScaledVector4(Vector4 vector) => Pack(vector.X, vector.Y, vector.Z);

        public Vector4 ToScaledVector4() => this.ToVector4();
        public void PackFromArgb32(Argb32 source)
            => Pack(source.R, source.G, source.B);

        public void PackFromBgra32(Bgra32 source)
            => Pack(source.R, source.G, source.B);

        public void ToArgb32(ref Argb32 dest)
        {
            Vector4 vector = this.ToVector4() * 255F;
            dest.R = (byte)MathF.Round(vector.X);
            dest.G = (byte)MathF.Round(vector.Y);
            dest.B = (byte)MathF.Round(vector.Z);
            dest.A = (byte)MathF.Round(vector.W);
        }
    }
}
