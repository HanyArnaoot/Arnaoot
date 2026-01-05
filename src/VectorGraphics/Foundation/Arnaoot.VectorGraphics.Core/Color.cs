using System;
using System.Drawing;
namespace Arnaoot.VectorGraphics.Abstractions
{
 

        public readonly struct ArgbColor : IEquatable<ArgbColor>
        {
            public readonly byte A, R, G, B;

            public ArgbColor(byte a, byte r, byte g, byte b) => (A, R, G, B) = (a, r, g, b);
            public ArgbColor(Color c) : this(c.A, c.R, c.G, c.B) { }

            // Safe GDI+ bridge
            public static implicit operator Color(ArgbColor c) => Color.FromArgb(c.A, c.R, c.G, c.B);
            public static implicit operator ArgbColor(Color c) => new ArgbColor(c.A, c.R, c.G, c.B);

            // Equality & hash
            public bool Equals(ArgbColor other) => A == other.A && R == other.R && G == other.G && B == other.B;
            public override bool Equals(object obj) => obj is ArgbColor c && Equals(c);
            public override int GetHashCode() => (A << 24) | (R << 16) | (G << 8) | B;

            public static ArgbColor FromArgb(int argb) =>
                new ArgbColor((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
            public int ToArgb() => (A << 24) | (R << 16) | (G << 8) | B;
        public static bool operator ==(ArgbColor left, ArgbColor right)
    => left.Equals(right);

        public static bool operator !=(ArgbColor left, ArgbColor right)
            => !left.Equals(right);
        // Common colors
        public static ArgbColor Black => new ArgbColor(255, 0, 0, 0);
            public static ArgbColor White => new ArgbColor(255, 255, 255, 255);
            public static ArgbColor Red => new ArgbColor(255, 255, 0, 0);
            public static ArgbColor Green => new ArgbColor(255, 0, 255, 0);
            public static ArgbColor Blue => new ArgbColor(255, 0, 0, 255);
            public static ArgbColor LightGray => new ArgbColor(255, 211, 211, 211);
            public static ArgbColor Transparent => new ArgbColor(0, 0, 0, 0);
            public static ArgbColor Yellow => new ArgbColor(255, 255, 255, 0);
        }
    }

 