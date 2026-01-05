using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
 using System.Runtime.InteropServices;
 using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Platform.WinForms
{
    public sealed partial class WinFormsRenderTarget : IRenderTarget
    {
        #region Local Variables
        private Bitmap _sceneCache;          // Cached scene render
        private Bitmap _workingBuffer;       // Current frame being built
        private Graphics _g;

        private int _cacheWidth;
        private int _cacheHeight;
        private bool _useCache;              // Tracks which Begin method was called

          //
        // Internal platform-specific caching
        private readonly Dictionary<string, Pen> _penCache = new Dictionary<string, Pen>();
        private readonly Dictionary<ArgbColor, SolidBrush> _brushCache = new Dictionary<ArgbColor, SolidBrush>();
        private readonly Dictionary<(string, float), Font> _fontCache = new Dictionary<(string, float), Font>();
        #endregion
        #region Constructor
        public WinFormsRenderTarget()
        {
        }
        // Cleanup caches
        public void Dispose()
        {
            foreach (var pen in _penCache.Values)
                pen.Dispose();
            _penCache.Clear();

            foreach (var brush in _brushCache.Values)
                brush.Dispose();
            _brushCache.Clear();

            foreach (var font in _fontCache.Values)
                font.Dispose();
            _fontCache.Clear();

            _g?.Dispose();
            _workingBuffer?.Dispose();
            _sceneCache?.Dispose();

        }
        #endregion
        // Add helper to access the result
        public bool TryGetPixelData(out PixelData data)
        {
            if (_workingBuffer == null)
            {
                data = default;
                return false;
            }

            var rect = new Rectangle(0, 0, _workingBuffer.Width, _workingBuffer.Height);
            var bmpData = _workingBuffer.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            try
            {
                int byteCount = Math.Abs(bmpData.Stride) * _workingBuffer.Height;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);

                data = new PixelData(pixels, _workingBuffer.Width, _workingBuffer.Height, PixelLayout.Bgra32Premul);
                return true;
            }
            finally
            {
                _workingBuffer.UnlockBits(bmpData);
            }
        }

        public void EndScene()
        {
            // If this was a full render (not using cache), update the cache
            if (!_useCache && _workingBuffer != null)
            {
                _sceneCache?.Dispose();
                _sceneCache = (Bitmap)_workingBuffer.Clone();
            }
        }

        //
        #region Frame methods
        public void BeginFrame(int width, int height)
        {
            // Guard against invalid size
            if (width <= 0 || height <= 0)
            {
                _workingBuffer?.Dispose();
                _workingBuffer = null;
                _g?.Dispose();
                _g = null;
                return;
            }

            // Check if cache size changed
            if (_cacheWidth != width || _cacheHeight != height)
            {
                // Invalidate cache on size change
                _sceneCache?.Dispose();
                _sceneCache = null;
                _cacheWidth = width;
                _cacheHeight = height;
            }

            // Create fresh working buffer
            _workingBuffer?.Dispose();
            _workingBuffer = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

            _g?.Dispose();
            _g = Graphics.FromImage(_workingBuffer);
            _g.SmoothingMode = SmoothingMode.AntiAlias;
            _g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            _useCache = false;  // This is a full render
        }
 
        public void BeginFrameFromCache()
        {
            // If no cache exists, can't use it
            if (_sceneCache == null)
            {
                throw new InvalidOperationException(
                    "Cannot begin frame from cache - no cache exists. Call BeginFrame() first.");
            }

            // Clone the cached scene as starting point
            _workingBuffer?.Dispose();
            _workingBuffer = (Bitmap)_sceneCache.Clone();

            _g?.Dispose();
            _g = Graphics.FromImage(_workingBuffer);
            _g.SmoothingMode = SmoothingMode.AntiAlias;
            _g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            _useCache = true;  // This is an overlay render
        }

        public void EndFrame()
        {
            _g?.Dispose();
            _g = null;

        }
        #endregion 
            public Bitmap? GetBitmap() => _workingBuffer;
    public void Clear(ArgbColor color)
        {
            if (_g == null) return;
            _g.Clear(Color.FromArgb(color.A, color.R, color.G, color.B));
        }
        public void DrawImage(object image, float x, float y, float width, float height)
        {
            if (image is Image gdiImage)
            {
                _g.DrawImage(gdiImage, x, y, width, height);
            }
            else
            {
                throw new ArgumentException($"WinFormsRenderTarget expects System.Drawing.Image, got {image?.GetType()}");
            }
        }
       

        #region Internal Cache Methods
        private Pen GetCachedPen(ArgbColor color, int width, bool isSelected)
        {
            string key = $"{color.GetHashCode()}-{width}-{isSelected}";
            if (!_penCache.TryGetValue(key, out Pen pen))
            {
                pen = new Pen(color, width);
                if (isSelected)
                {
                    pen.Width = width + 1;
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                _penCache[key] = pen;
            }
            return (Pen)pen.Clone();
        }

        private SolidBrush GetCachedBrush(ArgbColor color)
        {
            if (!_brushCache.TryGetValue(color, out SolidBrush brush))
            {
                brush = new SolidBrush(color);
                _brushCache[color] = brush;
            }
            return brush;
        }

        private Font GetCachedFont(string family, float size)
        {
            var key = (family, size);
            if (!_fontCache.TryGetValue(key, out Font font))
            {
                font = new Font(family, size, FontStyle.Regular, GraphicsUnit.Pixel);
                _fontCache[key] = font;
            }
            return font;
        }
        #endregion

    }

}
