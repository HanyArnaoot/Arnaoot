using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Arnaoot.VectorGraphics.Abstractions
{
    public partial class Abstractions
    {
        public enum PixelLayout
        {
            Bgra32Premul,  // ← Skia default & Windows GDI+ compatible
            Rgba32Premul,  // ← common on Linux/macOS
        }
        public readonly struct PixelData
        {
            public readonly byte[] Bytes;    // ✅ works everywhere
            public readonly int Width;
            public readonly int Height;
            public readonly PixelLayout Layout;

            public PixelData(byte[] bytes, int width, int height, PixelLayout layout)
            {
                Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
                Width = width;
                Height = height;
                Layout = layout;
            }
        }

        public readonly struct PixelBuffer
        {
            public readonly byte[] Data;   // RGBA or BGRA — document it!
            public readonly int Width;
            public readonly int Height;
            public readonly PixelLayout Format; // e.g., Bgra32Premul

            public PixelBuffer(byte[] data, int width, int height, PixelLayout format)
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Width = width;
                Height = height;
                Format = format;
            }
        }
    }
}
