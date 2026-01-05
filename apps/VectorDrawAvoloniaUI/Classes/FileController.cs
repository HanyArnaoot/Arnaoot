using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Platform.Skia;
using Arnaoot.VectorGraphics.Rendering;
using Arnaoot.VectorGraphics.Scene;
using Arnaoot.VectorGraphics.View;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace VectorDrawAvoloniaUI.Classes
{
    /// <summary>
    /// Handles file operations (load SVG, save images, render to file)
    /// </summary>
    public class FileController
    {
        private readonly Window _parentWindow;
        private readonly DrawingController _drawingController;
        private readonly DialogService _dialogService;
        private readonly Canvas _drawCanvas;

        public FileController(Window parentWindow, DrawingController drawingController, Canvas drawCanvas)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
            _drawingController = drawingController ?? throw new ArgumentNullException(nameof(drawingController));
            _drawCanvas = drawCanvas ?? throw new ArgumentNullException(nameof(drawCanvas));
            _dialogService = new DialogService(parentWindow);
        }

        /// <summary>
        /// Load an SVG file and display it
        /// </summary>
        public async Task LoadSvgFileAsync()
        {
            try
            {
                string? filePath = await _dialogService.SelectFileToOpenAsync();

                if (filePath == null)
                {
                    Console.WriteLine("No file selected");
                    return;
                }

                var importer = new Arnaoot.VectorGraphics.Formats.Svg.SvgImporter();
                var layerManager = new LayerManager();
                importer.LoadFromSvg(filePath, layerManager);

                Console.WriteLine($"✓ Loaded SVG: {filePath}");
                Console.WriteLine($"  Layers: {layerManager.Layers.Count}");
                Console.WriteLine($"  Elements: {layerManager.GetAllElements().Count()}");

                _drawingController.LoadLayerManager(layerManager);

                // Zoom to fit content
                var zooming = new Zooming();
                var viewSettings = zooming.ZoomExtents(
                    _drawingController.ViewSettings,
                    layerManager,
                    5f
                );

                _drawingController.UpdateViewSettings(viewSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ File load error: {ex.Message}");
                await _dialogService.ShowMessageAsync("Error", $"Failed to load file:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Save the currently displayed canvas image
        /// </summary>
        public async Task SaveCanvasImageAsync()
        {
            try
            {
                string? filePath = await _dialogService.SelectFileToSaveAsync();

                if (filePath == null)
                {
                    return;
                }

                var image = GetDisplayedImageFromCanvas();

                if (image?.Source is Bitmap bitmap)
                {
                    bitmap.Save(filePath);
                    Console.WriteLine($"✓ Canvas image saved to: {filePath}");
                    await _dialogService.ShowMessageAsync("Success",
                        $"Image saved to:\n{Path.GetFileName(filePath)}");
                }
                else
                {
                    await _dialogService.ShowMessageAsync("Error",
                        "No image to save. Please load a file first.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Save error: {ex.Message}");
                await _dialogService.ShowMessageAsync("Error",
                    $"Failed to save image:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Render an SVG file directly to an image file (batch mode)
        /// </summary>
        public async Task RenderToImageFileAsync()
        {
            try
            {
                string? inputPath = await _dialogService.SelectFileToOpenAsync();
                string? outputPath = await _dialogService.SelectFileToSaveAsync();

                if (inputPath == null || outputPath == null)
                {
                    return;
                }

                await RenderFileToImageAsync(inputPath, outputPath, 800, 600);

                await _dialogService.ShowMessageAsync("Success",
                    $"Image rendered to:\n{Path.GetFileName(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Render error: {ex.Message}");
                await _dialogService.ShowMessageAsync("Error",
                    $"Failed to render image:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Render an SVG file to an image file without displaying it
        /// </summary>
        private async Task RenderFileToImageAsync(string filePath, string outputPath, int width, int height)
        {
            Console.WriteLine($"Loading SVG from: {filePath}");

            // Load SVG
            var layerManager = new LayerManager();
            var importer = new Arnaoot.VectorGraphics.Formats.Svg.SvgImporter();
            importer.LoadFromSvg(filePath, layerManager);

            Console.WriteLine($"Loaded {layerManager.Layers.Count} layers");

            // Setup view
            var viewport = new Rect2(0, 0, width, height);
            IViewSettings viewSettings = new ViewSettings(
                viewport,
                new Arnaoot.Core.Vector3D(1, 1, 1),
                new Arnaoot.Core.Vector3D(),
                new Arnaoot.Core.Vector3D(),
                new Arnaoot.Core.Vector3D()
            );

            // Zoom to extents
            var zooming = new Zooming();
            viewSettings = zooming.ZoomExtents(viewSettings, layerManager, 5f);

            // Render
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

            Console.WriteLine($"Rendering {visibleElements.Count} elements");

            renderManager.RasterizeIntoBuffer(
                width, height,
                viewSettings,
                visibleElements,
                new Layer(),
                ArgbColor.White,
             InvalidationLevel.Full,
            out PixelData pixels,
                out bool ok
            );

            if (ok)
            {
                SavePixelDataToFile(pixels, outputPath);
                Console.WriteLine($"✓ Saved to: {outputPath}");
            }
            else
            {
                throw new Exception("Rasterization failed");
            }
        }

        /// <summary>
        /// Save pixel data to an image file
        /// </summary>
        private void SavePixelDataToFile(PixelData pixels, string outputPath)
        {
            var imageInfo = new SKImageInfo(pixels.Width, pixels.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(imageInfo);

            var pixmap = surface.PeekPixels();
            System.Runtime.InteropServices.Marshal.Copy(pixels.Bytes, 0, pixmap.GetPixels(), pixels.Bytes.Length);

            using var image = surface.Snapshot();
            using var data = image.Encode(GetFormatFromPath(outputPath), 100);
            using var fileStream = File.OpenWrite(outputPath);
            data.SaveTo(fileStream);
        }

        /// <summary>
        /// Get the Image control from the canvas
        /// </summary>
        private Image? GetDisplayedImageFromCanvas()
        {
            foreach (var child in _drawCanvas.Children)
            {
                if (child is Image image)
                {
                    return image;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the image format from file extension
        /// </summary>
        private SKEncodedImageFormat GetFormatFromPath(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".png" => SKEncodedImageFormat.Png,
                ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
                ".webp" => SKEncodedImageFormat.Webp,
                ".bmp" => SKEncodedImageFormat.Bmp,
                _ => SKEncodedImageFormat.Png
            };
        }
    }
}
