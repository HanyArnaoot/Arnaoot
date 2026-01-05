
// SvgRasterizer.cs — place in new console project

using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Platform.Skia;
using Arnaoot.VectorGraphics.Rendering;
using Arnaoot.VectorGraphics.Scene;
using Arnaoot.VectorGraphics.View;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.CLI
{
    class Program
    {

        static void Main(string[] args)
        {
            // Configuration
            string appPath = AppContext.BaseDirectory;
            string inputSvg = "test.svg";      // ← Change this to your SVG path
            string outputJpg = "output.jpg";    // ← Change this to desired output
            int width = 1920;
            int height = 1080;
            //
            Console.WriteLine("=== Simple Headless SVG Renderer ===\n");
            //
            try
            {
                var cullSw = Stopwatch.StartNew();
                RenderSvgToJpg(inputSvg, outputJpg, width, height);
                cullSw.Stop();
                Console.WriteLine("\n✓ Done! Image saved to: " + outputJpg + " , render and save time is:" + cullSw.ElapsedMilliseconds.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void RenderSvgToJpg(string svgPath, string jpgPath, int width, int height)
        {
            // Step 1: Load SVG
            Console.WriteLine($"Loading SVG: {svgPath}");
            if (!File.Exists(svgPath))
            {
                throw new FileNotFoundException($"File not found: {svgPath}");
            }

            var layerManager = new LayerManager();
            var importer = new Arnaoot.VectorGraphics.Formats.Svg.SvgImporter();
            importer.LoadFromSvg(svgPath, layerManager);

            Console.WriteLine($"✓ Loaded {layerManager.Layers.Count} layers");
            Console.WriteLine($"✓ Elements: {layerManager.GetAllElements().Count()}");

            // Step 2: Setup viewport and zoom to fit
            var viewport = new Rect2(0, 0, width, height);
            IViewSettings viewSettings = new ViewSettings(
                viewport,
                new Arnaoot.Core.Vector3D(1, 1, 1),
                new Arnaoot.Core.Vector3D(),
                new Arnaoot.Core.Vector3D(),
                new Arnaoot.Core.Vector3D()
            );

            var zooming = new Zooming();
            viewSettings = zooming.ZoomExtents(viewSettings, layerManager, 5f);

            Console.WriteLine($"✓ Viewport: {width}x{height}");

            // Step 3: Render
            using var renderTarget = new SkiaRenderTarget();
            var renderManager = new RenderManager(renderTarget)
            {
                ShowGrid = false,
                ShowAxes = false,
                ShowScaleBar = false
            };

            var visibleElements = layerManager.GetVisibleElements()
                .Where(el => renderManager.IsBoundsVisible(el.GetBounds(), viewSettings))
                .ToList();

            Console.WriteLine($"✓ Rendering {visibleElements.Count} elements...");

            long renderTime = renderManager.RasterizeIntoBuffer(
                width, height,
                viewSettings,
                visibleElements,
                new Layer(),
                ArgbColor.White,
                InvalidationLevel.Full,
                out PixelData pixels,
                out bool success
            );

            if (!success)
            {
                throw new Exception("Rendering failed");
            }

            Console.WriteLine($"✓ Rendered in {renderTime}ms");

            // Step 4: Save to JPG
            SaveAsJpg(pixels, jpgPath);
            Console.WriteLine($"✓ Saved to: {jpgPath}");
        }

        static void SaveAsJpg(PixelData pixels, string path, int quality = 90)
        {
            var imageInfo = new SKImageInfo(
                pixels.Width,
                pixels.Height,
                SKColorType.Rgba8888,
                SKAlphaType.Premul
            );

            using var surface = SKSurface.Create(imageInfo);
            var pixmap = surface.PeekPixels();

            // Copy pixel data
            System.Runtime.InteropServices.Marshal.Copy(
                pixels.Bytes,
                0,
                pixmap.GetPixels(),
                pixels.Bytes.Length
            );

            // Encode as JPEG
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            using var fileStream = File.OpenWrite(path);
            data.SaveTo(fileStream);
        }
    }
}


