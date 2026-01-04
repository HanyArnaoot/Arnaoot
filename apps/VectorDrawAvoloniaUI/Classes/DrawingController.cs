using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Platform.Skia;
using Arnaoot.VectorGraphics.Rendering;
using Arnaoot.VectorGraphics.Scene;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;
using Vector3D = Arnaoot.Core.Vector3D;

namespace VectorDrawAvoloniaUI.Classes
{
    /// <summary>
    /// Core drawing controller - manages rendering pipeline
    /// </summary>
    public class DrawingController : IDisposable
    {
        private readonly Canvas _canvas;
        private LayerManager _layerManager;
        private IViewSettings _viewSettings;
        private WriteableBitmap? _currentBitmap;

        public IViewSettings ViewSettings => _viewSettings;
        public LayerManager LayerManager => _layerManager;
        public long LastRenderTime { get; private set; }

        public DrawingController(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _layerManager = new LayerManager();
            _viewSettings = CreateDefaultViewSettings();
        }

        public void LoadLayerManager(LayerManager layerManager)
        {
            _layerManager = layerManager ?? throw new ArgumentNullException(nameof(layerManager));
        }

        public void UpdateViewSettings(IViewSettings viewSettings)
        {
            _viewSettings = viewSettings ?? throw new ArgumentNullException(nameof(viewSettings));
            RedrawCanvas();
        }

        public Vector3D ScreenToWorld(Point screenPoint)
        {
            return _viewSettings.PictToReal(
                new Vector2D((float)screenPoint.X, (float)screenPoint.Y)
            );
        }

        public void RedrawCanvas()
        {
            try
            {
                Console.WriteLine("=== RedrawCanvas Started ===");

                int width = (int)_canvas.Bounds.Width;
                int height = (int)_canvas.Bounds.Height;

                if (width <= 0 || height <= 0)
                {
                    Console.WriteLine("⚠ Canvas has invalid dimensions");
                    return;
                }

                // Update viewport
                _viewSettings = new ViewSettings(
                    GetCanvasViewport(),
                    _viewSettings.ZoomFactor,
                    _viewSettings.ShiftWorld,
                    _viewSettings.RotationAngle,
                    _viewSettings.RotateAroundPoint
                );

                var allElements = _layerManager.GetVisibleElements();
                Console.WriteLine($"Total visible elements: {allElements.Count()}");

                if (!allElements.Any())
                {
                    Console.WriteLine("⚠ No visible elements to render");
                    DrawDebugPlaceholder("No visible elements");
                    return;
                }

                // Render
                var renderResult = RenderScene(width, height, allElements);

                if (renderResult.Success)
                {
                    LastRenderTime = renderResult.RenderTime;
                    DisplayPixelData(renderResult.Pixels);
                    Console.WriteLine("✓ Frame displayed");
                }
                else
                {
                    Console.WriteLine("⚠ Rasterization failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RedrawCanvas error: {ex.Message}");
                DrawDebugPlaceholder($"Error: {ex.Message}");
            }
        }

        private RenderResult RenderScene(int width, int height, IEnumerable<IDrawElement> allElements)
        {
            using var renderTarget = new SkiaRenderTarget();
            var renderManager = new RenderManager(renderTarget)
            {
                ShowGrid = false,
                ShowAxes = false,
                ShowScaleBar = false
            };

            var visibleElements = allElements
                .Where(el => renderManager.IsBoundsVisible(el.GetBounds(), _viewSettings))
                .ToList();

            Console.WriteLine($"Rendering {visibleElements.Count} visible elements");

            if (visibleElements.Count == 0)
            {
                return RenderResult.Failed();
            }

            long renderTime = renderManager.RasterizeIntoBuffer(
                width, height,
                _viewSettings,
                visibleElements,
                new Layer(),
                ArgbColor.White,
                InvalidationLevel.Full,
                out PixelData pixels,
                out bool ok
            );

            return new RenderResult(ok, pixels, renderTime);
        }

        private void DisplayPixelData(PixelData pixels)
        {
            try
            {
                _currentBitmap?.Dispose();

                var pixelSize = new PixelSize(pixels.Width, pixels.Height);
                _currentBitmap = new WriteableBitmap(
                    pixelSize,
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Premul
                );

                using (var locked = _currentBitmap.Lock())
                {
                    if (locked.RowBytes == pixels.Width * 4)
                    {
                        Marshal.Copy(pixels.Bytes, 0, locked.Address, pixels.Bytes.Length);
                    }
                    else
                    {
                        for (int y = 0; y < pixels.Height; y++)
                        {
                            IntPtr dst = IntPtr.Add(locked.Address, y * locked.RowBytes);
                            Marshal.Copy(pixels.Bytes, y * pixels.Width * 4, dst, pixels.Width * 4);
                        }
                    }
                }

                _canvas.Children.Clear();
                _canvas.Children.Add(new Image { Source = _currentBitmap });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Display error: {ex}");
                _currentBitmap?.Dispose();
                _currentBitmap = null;
            }
        }

        private void DrawDebugPlaceholder(string message)
        {
            try
            {
                _canvas.Children.Clear();

                var border = new Border
                {
                    Background = new SolidColorBrush(Colors.LightYellow),
                    BorderBrush = new SolidColorBrush(Colors.Red),
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(10),
                    [Canvas.LeftProperty] = 10,
                    [Canvas.TopProperty] = 10
                };

                border.Child = new TextBlock
                {
                    Text = $"DEBUG: {message}\n" +
                           $"Canvas: {_canvas.Bounds.Width}x{_canvas.Bounds.Height}\n" +
                           $"Layers: {_layerManager.Layers.Count}\n" +
                           $"Zoom: {_viewSettings.ZoomFactorAverage:F2}x",
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontWeight = FontWeight.Bold
                };

                _canvas.Children.Add(border);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Debug placeholder error: {ex.Message}");
            }
        }

        private Rect2 GetCanvasViewport()
        {
            int width = (int)_canvas.Bounds.Width;
            int height = (int)_canvas.Bounds.Height;
            return (width > 0 && height > 0)
                ? new Rect2(0, 0, width, height)
                : new Rect2(0, 0, 600, 800);
        }

        private IViewSettings CreateDefaultViewSettings()
        {
            return new ViewSettings(
                new Rect2(0, 0, 600, 800),
                new Vector3D(1, 1, 1),
                new Vector3D(),
                new Vector3D(),
                new Vector3D()
            );
        }

        public void Dispose()
        {
            _currentBitmap?.Dispose();
        }

        private class RenderResult
        {
            public bool Success { get; }
            public PixelData Pixels { get; }
            public long RenderTime { get; }

            public RenderResult(bool success, PixelData pixels, long renderTime)
            {
                Success = success;
                Pixels = pixels;
                RenderTime = renderTime;
            }

            public static RenderResult Failed() => new RenderResult(false, new PixelData (), 0);
        }
    }
}
