using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Platform.Skia;
using Arnaoot.VectorGraphics.Platform.WinForms;
using Arnaoot.VectorGraphics.Scene;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using static Arnaoot.VectorGraphics.Abstractions.Abstractions;


namespace Arnaoot.VectorGraphics.Rendering
{
    //Contains methods For drawing shapes And the main painting logic.
    public interface IRenderManager : IDisposable
    {
        // Flags
        bool ShowGrid { get; set; }
        bool ShowAxes { get; set; }
        bool ShowScaleBar { get; set; }
        float GridSpacingReal { get; set; }

        object BackgroundImage { get; set; }

        public static IRenderTarget CreateDefault()
        {
            return IsSkiaAvailable() ? (IRenderTarget)new SkiaRenderTarget() : new WinFormsRenderTarget();
        }

        public static bool IsSkiaAvailable()
        {
            try
            {
                // Force type load — fails if SkiaSharp.dll missing or incompatible
                var _ = typeof(SkiaSharp.SKSurface);
                return true;
            }
            catch
            {
                return false;
            }
            return false;
        }

        //void InvalidateSceneCache();
        // Main rasterization function
        long RasterizeIntoBuffer(
            int Width, int Height,
            IViewSettings view,
            IReadOnlyCollection<IDrawElement> drawElements,
            Layer tempLayer,
            ArgbColor backColor,
            InvalidationLevel invalidationLevel, 
            out PixelData pixelData,
            out bool success);

        // Visibility check
        bool IsBoundsVisible(
            BoundingBox3D worldBounds,
            IViewSettings view);

        // Overlays and helpers
        void DrawScaleBar(
            IViewSettings view,
            IRenderTarget renderTarget,
            int scaleBarLengthPixels);
    }

    public partial class RenderManager : IRenderManager, IDisposable
    {
        #region local variables
        private readonly IRenderTarget _renderTarget;
        //
        // Cache state tracking
       // private bool _sceneCacheDirty = true;
        private int _lastWidth = -1;
        private int _lastHeight = -1;
        //
        public bool ShowGrid { get; set; }
        public bool ShowAxes { get; set; }
        public bool ShowScaleBar { get; set; }
        public float GridSpacingReal { get; set; }
        public object BackgroundImage { get; set; }
        //
        // Cache fields
        private PixelData? _cachedScenePixels;
        private int _cachedWidth;
        private int _cachedHeight;
        // Configuration
        public int ScaleBarLengthPixels { get; set; }
        #endregion

        #region constructor and dispose
        public RenderManager(IRenderTarget renderTarget)
        {
            // Default values
            ScaleBarLengthPixels = 100;
            GridSpacingReal = 50;
            ShowGrid = false;
            ShowAxes = true;
            ShowScaleBar = true;

            _renderTarget = renderTarget ?? throw new ArgumentNullException(nameof(renderTarget));
        }
        public void Dispose()
        {
            // RenderManager doesn't own the render target
        }

        #endregion
        // Call this when scene changes (elements added/removed, view changed)
        //public void InvalidateSceneCache()
        //{
        //    _sceneCacheDirty = true;
        //}

        /// <summary>
        /// Render scene into the provided surface.
        /// Surface can be: System.Drawing.Bitmap, SKSurface, SKBitmap, or SKCanvas.
        /// </summary>
        public long RasterizeIntoBuffer(
                int Width, int Height,
                IViewSettings view,
                IReadOnlyCollection<IDrawElement> drawElements,
                Layer tempLayer,
                ArgbColor backColor,
                InvalidationLevel invalidationLevel,
                out PixelData pixelData,
                out bool success)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            Stopwatch sw = Stopwatch.StartNew();

            // ← NEW: Determine render path
            bool sizeChanged = _lastWidth != Width || _lastHeight != Height;
            bool needFullRender = (invalidationLevel> InvalidationLevel.Overlay) || sizeChanged;

            if (needFullRender)
            {
                // === FULL RENDER PATH ===
                Console.WriteLine($"FULL RENDER: {Width}x{Height}");

                _renderTarget.BeginFrame(Width, Height);

                // Clear with background
                _renderTarget.Clear(BackgroundImage != null ? ArgbColor.White : backColor);

                // Draw background image if present
                if (BackgroundImage != null)
                {
                    _renderTarget.DrawImage(BackgroundImage, 0, 0, Width, Height);
                }

                // Render scene elements
                Render_IDrawElement(drawElements, _renderTarget, view);

                // Update cache state
                //_sceneCacheDirty = false;
                _lastWidth = Width;
                _lastHeight = Height;
                _renderTarget.EndScene();    // ← NEW: signals "stop — now cache me"

            }
            else
            {
                // === FAST OVERLAY PATH ===
                Console.WriteLine("OVERLAY RENDER: Using cached scene");

                _renderTarget.BeginFrameFromCache();
                // Scene already rendered, just draw temp layer on top
            }

            // Always render temp layer (whether full or overlay)
            if (tempLayer != null)
            {
                foreach (var el in tempLayer.GetLayerElements())
                    el.EmitCommands(_renderTarget, view);
            }
            // Render overlays (grid/axes are part of scene)
            if (ShowScaleBar)
                DrawScaleBar(view, _renderTarget, ScaleBarLengthPixels);
            if (ShowGrid)
                DrawGrid(view, _renderTarget, GridSpacingReal);
            if (ShowAxes)
            {
                DrawAxes(view, _renderTarget);
            }
            _renderTarget.EndFrame();

            // Extract pixels
            success = _renderTarget.TryGetPixelData(out pixelData);

            sw.Stop();
            Console.WriteLine($"Rasterize time: {sw.ElapsedMilliseconds} ms | Pixels: {(success ? "OK" : "FAILED")}");

            return sw.ElapsedMilliseconds;
        }

        private void Render_IDrawElement(
            IEnumerable<IDrawElement> elements,
            IRenderTarget target,
            IViewSettings view)
        {
            foreach (var el in elements)
            {
                // Frustum culling
                if (!IsVisible(el.GetBounds(), view))
                    continue;
                el.EmitCommands(target, view);
            }
        }
    }
}


