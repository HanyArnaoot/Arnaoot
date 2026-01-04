using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;
//
namespace Arnaoot.VectorGraphics.Platform.Skia
{
    public sealed partial class SkiaRenderTarget : IRenderTarget, IDisposable
    {
        #region Local variables

        private SKSurface _sceneCache;       // Cached scene render
        private SKSurface _workingSurface;   // Current frame being built
        private SKCanvas _canvas;

        private int _cacheWidth;
        private int _cacheHeight;
        private bool _usingCache;              // Tracks which Begin method was called

         private int _surfaceWidth;
        private int _surfaceHeight;
        private SKSurface? _surface;
         private SKSurface _ownedSurface; // Only if we create it from System.Drawing.Bitmap
        private object _targetGdiBitmap; // Target GDI+ bitmap to copy back to (stored as object to avoid type loading)

        // Internal platform-specific caching
        private readonly Dictionary<string, SKPaint> _paintCache = new Dictionary<string, SKPaint>();
        private readonly Dictionary<(string, float), SKTypeface> _typefaceCache = new Dictionary<(string, float), SKTypeface>();
        //
        #endregion
        #region constructor, dispose
        public SkiaRenderTarget()
        {
        }

        // Cleanup caches
        public void Dispose()
        {
            foreach (var paint in _paintCache.Values)
                paint.Dispose();
            _paintCache.Clear();

            foreach (var typeface in _typefaceCache.Values)
                typeface?.Dispose();
            _typefaceCache.Clear();

            _ownedSurface?.Dispose();
            _ownedSurface = null;
            _canvas = null;
            _surface?.Dispose();
            _sceneCache?.Dispose();  // ← Clean up cache

        }

        #endregion


        #region Surface
        public static class SkiaSurfaceFactory
        {
            // Use this everywhere (Win/Linux/mac) — safe, no dependencies
            public static SKSurface CreateSurface(int width, int height)
            {
                return SKSurface.Create(new SKImageInfo(
                    width, height,
                    SKImageInfo.PlatformColorType,  // ← uses optimal native format
                    SKAlphaType.Premul));
            }

            // Optional: get raw pixel data (e.g., for OpenGL texture upload, custom blit)
            public static byte[] SnapshotPixels(SKSurface surface, out int width, out int height)
            {
                using var image = surface.Snapshot();
                width = image.Width;
                height = image.Height;
                using var data = image.Encode(SKEncodedImageFormat.Bmp, 100); // or use .PeekPixels() for raw
                return data.ToArray();
            }
        }

        private void CopySkiaSurfaceToGdiBitmap(SKSurface skSurface, object gdiBitmap)
        {
            // This method only works when System.Drawing is available
            // Use reflection to avoid hard dependency
            try
            {
                var bitmapType = gdiBitmap.GetType();
                var widthProp = bitmapType.GetProperty("Width");
                var heightProp = bitmapType.GetProperty("Height");

                if (widthProp == null || heightProp == null)
                    return;

                int width = (int)widthProp.GetValue(gdiBitmap);
                int height = (int)heightProp.GetValue(gdiBitmap);

                // Get pixel data from SkiaSharp surface
                using (var image = skSurface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = new System.IO.MemoryStream())
                {
                    data.SaveTo(stream);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);

                    // Load into temporary GDI+ bitmap using reflection
                    var bitmapCtor = bitmapType.GetConstructor(new[] { typeof(System.IO.Stream) });
                    if (bitmapCtor != null)
                    {
                        var tempBitmap = bitmapCtor.Invoke(new object[] { stream });

                        // Get Graphics.FromImage method
                        var graphicsType = bitmapType.Assembly.GetType("System.Drawing.Graphics");
                        var fromImageMethod = graphicsType?.GetMethod("FromImage", new[] { bitmapType });

                        if (fromImageMethod != null)
                        {
                            var graphics = fromImageMethod.Invoke(null, new[] { gdiBitmap });
                            var drawImageMethod = graphicsType.GetMethod("DrawImage", new[] { bitmapType, typeof(int), typeof(int), typeof(int), typeof(int) });

                            if (drawImageMethod != null)
                            {
                                drawImageMethod.Invoke(graphics, new object[] { tempBitmap, 0, 0, width, height });
                            }

                            // Dispose graphics
                            ((IDisposable)graphics).Dispose();
                        }

                        // Dispose temp bitmap
                        ((IDisposable)tempBitmap).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to copy to GDI bitmap: {ex.Message}");
            }
        }

        #endregion
        public void EndScene()
        {
            // Only cache if we were in full-render mode (not using cached base)
            if (!_usingCache && _surface != null)
            {
                // Cache *current canvas contents* = scene only
                _sceneCache?.Dispose();
                var info = new SKImageInfo(_surfaceWidth, _surfaceHeight );
                _sceneCache = SKSurface.Create(info);

                using (var snapshot = _surface.Snapshot())
                {
                    _sceneCache.Canvas.DrawImage(snapshot, 0, 0);
                    _sceneCache.Canvas.Flush();
                }

              }
        }
        #region Frame Methods
        public void BeginFrame(int width, int height)
        {
            // Check if cache size changed
            if (_cacheWidth != width || _cacheHeight != height)
            {
                _sceneCache?.Dispose();
                _sceneCache = null;
                _cacheWidth = width;
                _cacheHeight = height;
            }

            // Reuse only if size matches
            if (_surface == null || _surfaceWidth != width || _surfaceHeight != height)
            {
                _surface?.Dispose();
                var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                _surface = SKSurface.Create(info);
                _surfaceWidth = width;
                _surfaceHeight = height;
            }

            _canvas = _surface?.Canvas;
            _canvas?.Clear(SKColors.Transparent);

            _usingCache = false;  // ← This is a full render
        }

         public void BeginFrameFromCache()
        {
            if (_sceneCache == null)
            {
                throw new InvalidOperationException(
                    "Cannot begin frame from cache - no cache exists. Call BeginFrame() first.");
                // Instead of throwing, fall back gracefully
                // i want it that way to test it , i real operation i will do the next two lines
                //BeginFrame(_cacheWidth, _cacheHeight);  // or current size
                //return;
            }

            // Recreate surface if needed
            if (_surface == null || _surfaceWidth != _cacheWidth || _surfaceHeight != _cacheHeight)
            {
                _surface?.Dispose();
                var info = new SKImageInfo(_cacheWidth, _cacheHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                _surface = SKSurface.Create(info);
                _surfaceWidth = _cacheWidth;
                _surfaceHeight = _cacheHeight;
            }

            _canvas = _surface?.Canvas;
            _canvas?.Clear(SKColors.Transparent);

            // Draw cached scene as base
            using (var cachedImage = _sceneCache.Snapshot())
            {
                _canvas?.DrawImage(cachedImage, 0, 0);
            }

            _usingCache = true;  // ← This is an overlay render
        }

        public void EndFrame()
        {
            _canvas?.Flush();

            // If full render, update cache
            //if (!_usingCache && _surface != null)
            //{
            //    _sceneCache?.Dispose();
            //    var info = new SKImageInfo(_surfaceWidth, _surfaceHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            //    _sceneCache = SKSurface.Create(info);

            //    using (var snapshot = _surface.Snapshot())
            //    {
            //        _sceneCache.Canvas.DrawImage(snapshot, 0, 0);
            //        _sceneCache.Canvas.Flush();
            //    }
            //}
        }
        #endregion


        //
        // Pure cross-platform: no GDI+, no Bitmap
        public bool TryGetPixelData(out PixelData data)
        {
            if (_surface == null)
            {
                data = default;
                return false;
            }

            using var image = _surface.Snapshot();
            using var pixelData = image.PeekPixels();

            if (pixelData == null)
            {
                data = default;
                return false;
            }

            var w = pixelData.Width;
            var h = pixelData.Height;
            var size = pixelData.RowBytes * h;
            var bytes = new byte[size];

            var span = pixelData.GetPixelSpan();
            span.CopyTo(bytes);

            var layout = pixelData.ColorType switch
            {
                SKColorType.Bgra8888 => PixelLayout.Bgra32Premul,
                SKColorType.Rgba8888 => PixelLayout.Rgba32Premul,
                _ => PixelLayout.Bgra32Premul
            };

            data = new PixelData(bytes, w, h, layout);
            return true;
        }


 

        public void DrawImage(object image, float x, float y, float width, float height)
        {
            SKImage skImage = null;
            SKBitmap skBitmap = null;

            try
            {
                if (image is SKImage img)
                {
                    skImage = img;
                }
                else if (image is SKBitmap bmp)
                {
                    skBitmap = bmp;
                    skImage = SKImage.FromBitmap(bmp);
                }
                else
                {
                    throw new ArgumentException($"SkiaRenderTarget expects SKImage or SKBitmap, got {image?.GetType()}");
                }

                var destRect = SKRect.Create(x, y, width, height);
                _canvas.DrawImage(skImage, destRect);
            }
            finally
            {
                // Only dispose if we created it from bitmap
                if (image is SKBitmap && skImage != null)
                {
                    skImage.Dispose();
                }
            }
        }

  

        #region Internal Cache Methods
        private SKPaint GetCachedPaint(ArgbColor color, float width, bool isStroke, bool isSelected = false, SKPaintStyle? forceStyle = null)
        {
            string key = $"{color.GetHashCode()}-{width}-{isStroke}-{isSelected}-{forceStyle}";
            if (!_paintCache.TryGetValue(key, out SKPaint paint))
            {
                paint = new SKPaint
                {
                    Color = new SKColor(color.R, color.G, color.B, color.A),
                    IsAntialias = true,
                    Style = forceStyle ?? (isStroke ? SKPaintStyle.Stroke : SKPaintStyle.Fill),
                    StrokeWidth = width
                };

                if (isSelected && isStroke)
                {
                    paint.StrokeWidth = width + 1;
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                }

                _paintCache[key] = paint;
            }
            return paint;
        }

        private SKTypeface GetCachedTypeface(string family)
        {
            var key = (family, 0f); // Size doesn't matter for typeface
            if (!_typefaceCache.TryGetValue(key, out SKTypeface typeface))
            {
                typeface = SKTypeface.FromFamilyName(family, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                _typefaceCache[key] = typeface;
            }
            return typeface;
        }
        #endregion

    }

}

