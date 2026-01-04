using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Rendering;
using Arnaoot.VectorGraphics.Scene;
using Arnaoot.VectorGraphics.View;
 using System.Drawing.Imaging;
 using System.Runtime.InteropServices;
 using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.UI
{
    public static class WinFormsImageExporter
    {
        public static void SaveRegionAsImage(
            string filePath,
            IRenderManager renderManager,
            IReadOnlyCollection<IDrawElement> drawElements,
            IViewSettings currentViewSettings,
            BoundingBox3D region,
            int pixelWidth,
            int pixelHeight,
            System.Drawing.Imaging.ImageFormat format = null,
            bool includeBackground = true,
            float padding = 0.05f)
        {
            if (!region.IsValid())
                throw new ArgumentException("Region must be valid.");

            var zooming = new Zooming();
            var regionView = zooming.GetRegionViewSettings(currentViewSettings, region, padding);

            long rasterTime = renderManager.RasterizeIntoBuffer(
                pixelWidth, pixelHeight,
                regionView,
                drawElements,
                new Layer(),
                includeBackground ? ArgbColor.White : ArgbColor.Transparent,
                InvalidationLevel.Full,
                out PixelData pixels,
                out bool success);

            if (!success)
                throw new InvalidOperationException("Rasterization failed.");

            var imageFormat = format ?? System.Drawing.Imaging.ImageFormat.Png;
            using var bmp = PixelsToBitmap(pixels);
            bmp.Save(filePath, imageFormat);
        }

        // Helper — same as before
        private static Bitmap PixelsToBitmap(PixelData pixels)
        {
            var bmp = new Bitmap(pixels.Width, pixels.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var rect = new Rectangle(0, 0, pixels.Width, pixels.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            try
            {
                if (bmpData.Stride == pixels.Width * 4)
                {
                    Marshal.Copy(pixels.Bytes, 0, bmpData.Scan0, pixels.Bytes.Length);
                }
                else
                {
                    for (int y = 0; y < pixels.Height; y++)
                    {
                        IntPtr dst = IntPtr.Add(bmpData.Scan0, y * bmpData.Stride);
                        int srcOffset = y * pixels.Width * 4;
                        Marshal.Copy(pixels.Bytes, srcOffset, dst, pixels.Width * 4);
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }
    }
}
