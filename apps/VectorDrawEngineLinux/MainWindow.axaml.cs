using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Platform.Skia;
using Arnaoot.VectorGraphics.Rendering;
using Arnaoot.VectorGraphics.Scene;
using Arnaoot.VectorGraphics.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;
//using System.Runtime.InteropServices;
namespace VectorDrawEngineLinux
{
    public partial class MainWindow : Window
    {
        #region Variable Declare
        //  These are your core engine components
        private LayerManager _layerManager;
        private IViewSettings _viewSettings;
        private Zooming _zooming = new Zooming();
        private WriteableBitmap? _currentBitmap;
        long LastRenderTime;

        #endregion
        #region Window /winodw Initialize
        public MainWindow()
        {
            InitializeComponent();
            InitializeDrawingEngine();
            SetupEventHandlers();
            UpdateInfoText();
            //
        }
        private void Window_Activated(object? sender, EventArgs e)
        {
        }

        private void SetupEventHandlers()
        {
            // Pan buttons
            PanXPlus.Click += OnPanButtonClick;
            PanXMinus.Click += OnPanButtonClick;
            PanYPlus.Click += OnPanButtonClick;
            PanYMinus.Click += OnPanButtonClick;

            // Zoom buttons
            ZoomIn.Click += OnZoomIn;
            ZoomOut.Click += OnZoomOut;
            ZoomFit.Click += OnZoomFit;

            //
            // File load button
            LoadFileButton.Click += LoadFileButton_Click;
            SaveImageButton.Click += SaveImageButton_Click;
            RenderToImage.Click += RenderToImage_Click;

            // NEW: Mouse move events
            DrawCanvas.PointerMoved += DrawCanvas_PointerMoved;
            DrawCanvas.PointerEntered += DrawCanvas_PointerEntered;
            DrawCanvas.PointerExited += DrawCanvas_PointerExited;

            // Redraw when canvas size changes
            DrawCanvas.SizeChanged += (s, e) => RedrawCanvas();
            //
            // Connect slider events
            RotationXSlider.ValueChanged += OnRotationSliderChanged;
            RotationYSlider.ValueChanged += OnRotationSliderChanged;
            RotationZSlider.ValueChanged += OnRotationSliderChanged;

            // Connect reset button event
            ResetRotationButton.Click += OnResetRotationClicked;
        }

        //  Initialize your drawing engine
        private void InitializeDrawingEngine()
        {
            try
            {
                // Initialize your layer manager and load SVG
                _layerManager = new LayerManager();
                //   Setup initial view settings

                 _viewSettings = new ViewSettings(
                    GetUsableViewportwithControls(),
                    new Arnaoot.Core.Vector3D(1, 1, 1),  // zoom
                    new Arnaoot.Core.Vector3D(),         // pan
                    new Arnaoot.Core.Vector3D(),         // rotation
                    new Arnaoot.Core.Vector3D()          // center
                );
                // Update   local transform state from your view settings
                UpdateTransformFromViewSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialization error: {ex.Message}");
                // Fallback: Create empty layer manager
                _layerManager = new LayerManager();
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            _currentBitmap?.Dispose();
            base.OnClosed(e);
        }
        #endregion 
        #region load file 
        private async void LoadSvgFile()
        {
            try
            {

                string? filePath = await SelectFileToOpen();

                if (filePath != null)

                {
                    var importer = new Arnaoot.VectorGraphics.Formats.Svg.SvgImporter();
                    _layerManager = new LayerManager() ;
                    importer.LoadFromSvg(filePath, _layerManager);
                    Console.WriteLine($"✓ Loaded SVG: {filePath}");

                    // Reset view and zoom to fit
                    _viewSettings = _zooming.ZoomExtents(_viewSettings, _layerManager, 5f);
                    UpdateTransformFromViewSettings();
                    RedrawCanvas();
                }
                else
                {
                    Console.WriteLine("No file selected, creating demo content");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File load error: {ex.Message}");
            }
        }

        //static void RenderDemoSimple(int width, int height, string outputPath, SKEncodedImageFormat format)
        //{
        //    // Create SkiaSharp surface
        //    var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        //    using var surface = SKSurface.Create(imageInfo);
        //    using var renderTarget = new SkiaRenderTarget();

        //    // Begin rendering
        //    renderTarget.BeginFrame(surface);

        //    // Clear with white background
        //    renderTarget.Clear(ArgbColor.White);

        //    // ===================================
        //    // Draw demonstration elements
        //    // ===================================

        //    // 1. Draw a red rectangle
        //    var rect = new Rect2(50, 50, 200, 150);
        //    renderTarget.DrawRectangle(
        //        rect,
        //        stroke: ArgbColor.Red,
        //        strokeWidth: 3f,
        //        fill: ArgbColor.FromArgb(unchecked((int)0x40FF0000)) // Semi-transparent red
        //    );

        //    // 2. Draw a blue circle (ellipse)
        //    var circleCenter = new Vector2D(450, 125);
        //    renderTarget.DrawEllipse(
        //        center: circleCenter,
        //        radiusX: 80,
        //        radiusY: 80,
        //        angleRad: 0,
        //        stroke: ArgbColor.Blue,
        //        strokeWidth: 3f,
        //        fill: ArgbColor.FromArgb(unchecked((int)0x400000FF)) // Semi-transparent blue
        //    );

        //    // 3. Draw a green line
        //    var lineStart = new Vector2D(100, 300);
        //    var lineEnd = new Vector2D(700, 300);
        //    renderTarget.DrawLine(
        //        lineStart,
        //        lineEnd,
        //        color: ArgbColor.Green,
        //        width: 5f,
        //        isSelected: false
        //    );

        //    // 4. Draw a dashed line (selected style)
        //    var lineStart2 = new Vector2D(100, 350);
        //    var lineEnd2 = new Vector2D(700, 350);
        //    renderTarget.DrawLine(
        //        lineStart2,
        //        lineEnd2,
        //        color: ArgbColor.FromArgb(unchecked((int)0xFF8B4513)), // Brown
        //        width: 3f,
        //        isSelected: true // This makes it dashed
        //    );

        //    // 5. Draw text labels
        //    renderTarget.DrawString(
        //        "Rectangle",
        //        new Vector2D(60, 220),
        //        ArgbColor.Black,
        //        "Arial",
        //        16f
        //    );

        //    renderTarget.DrawString(
        //        "Circle",
        //        new Vector2D(420, 220),
        //        ArgbColor.Black,
        //        "Arial",
        //        16f
        //    );

        //    renderTarget.DrawString(
        //        "Solid Line",
        //        new Vector2D(350, 290),
        //        ArgbColor.Black,
        //        "Arial",
        //        14f
        //    );

        //    renderTarget.DrawString(
        //        "Dashed Line (Selected)",
        //        new Vector2D(300, 340),
        //        ArgbColor.Black,
        //        "Arial",
        //        14f
        //    );

        //    // 6. Draw a title
        //    renderTarget.DrawString(
        //        "Cross-Platform Rendering Demo",
        //        new Vector2D(width / 2 - 150, 30),
        //        ArgbColor.FromArgb(unchecked((int)0xFF2C3E50)), // Dark blue-gray
        //        "Arial",
        //        24f
        //    );

        //    // 7. Draw a polygon (triangle)
        //    var trianglePoints = new Vector2D[]
        //    {
        //        new Vector2D(width / 2, 400),
        //        new Vector2D(width / 2 - 60, 500),
        //        new Vector2D(width / 2 + 60, 500)
        //    };
        //    renderTarget.DrawPolygon(
        //        trianglePoints,
        //        stroke: ArgbColor.FromArgb(unchecked((int)0xFFFF6B6B)), // Coral
        //        strokeWidth: 3f,
        //        fill: ArgbColor.FromArgb(unchecked((int)0x40FF6B6B)) // Semi-transparent coral
        //    );

        //    renderTarget.DrawString(
        //        "Triangle (Polygon)",
        //        new Vector2D(width / 2 - 70, 520),
        //        ArgbColor.Black,
        //        "Arial",
        //        14f
        //    );

        //    // End rendering
        //    renderTarget.EndFrame();

        //    // Save to file
        //    using var image = surface.Snapshot();
        //    using var data = image.Encode(format, 100);
        //    using var fileStream = File.OpenWrite(outputPath);
        //    data.SaveTo(fileStream);
        //}
        #endregion 

        //  Button event handlers
        #region "Button event handlers"
        Rect2 GetUsableViewportwithControls()
        {
            int width = (int)DrawCanvas.Bounds.Width;
            int height = (int)DrawCanvas.Bounds.Height;
            //
            if (width>0 & height > 0)
            {
             return new Rect2(0, 0, width, height);
            }
            return new Rect2(0, 0, 600, 800);


        }
        private void OnRotationSliderChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            // Update the text displays
            RotationXText.Text = "X: " + ((float)RotationXSlider.Value ).ToString("0.00") + " °";
            RotationYText.Text = "Y: " + ((float)RotationYSlider.Value ).ToString("0.00") + " °";
            RotationZText.Text = "Z: " + ((float)RotationZSlider.Value ).ToString("0.00") + " °";

            Rect2 usableview = GetUsableViewportwithControls();
            Vector2D centerPixel = new Vector2D(usableview.Width  / 2.0f, usableview.Height / 2.0f);

            // Use the new method that doesn't reverse rotation
           Arnaoot.Core.Vector3D rotatePoint = _viewSettings.PictToViewPlane(centerPixel, 0.0f);

            Arnaoot.Core.Vector3D RotationAngle = new Arnaoot.Core.Vector3D(
                (float)RotationXSlider.Value *3.14f  / 180f ,
                (float)RotationYSlider.Value * 3.14f / 180f,
                (float)RotationZSlider.Value * 3.14f / 180f);
              _viewSettings =new ViewSettings (GetUsableViewportwithControls(), _viewSettings.ZoomFactor , _viewSettings.ShiftWorld, RotationAngle, rotatePoint);
             UpdateViewSettingsFromTransform();
            RedrawCanvas();
         }

        private void OnResetRotationClicked(object? sender, RoutedEventArgs e)
        {
            // Reset all sliders to 0
            RotationXSlider.Value = 0;
            RotationYSlider.Value = 0;
            RotationZSlider.Value = 0;

            // The ValueChanged event will automatically trigger and update displays
        }
        private void DrawCanvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            try
            {
                var position = e.GetPosition(DrawCanvas);
                double mouseX = position.X;
                double mouseY = position.Y;

                // Convert screen coordinates to world coordinates if needed
                // This depends on your view settings and transform
                Arnaoot.Core.Vector3D RealLocation = _viewSettings.PictToReal(new Arnaoot.Core.Vector2D((float)position.X, (float)position.Y));

                // Update the display
                InfoText.Text = $"Pan: ({(int)_viewSettings.ShiftWorld.X}, {(int)_viewSettings.ShiftWorld.Y}) | Zoom: {_viewSettings.ZoomFactorAverage:F2}x | Mouse: ({RealLocation.X:F0}, {RealLocation.Y:F0})";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mouse move error: {ex.Message}");
            }
        }

        private void DrawCanvas_PointerEntered(object? sender, PointerEventArgs e)
        {
            // Optional: Change cursor or provide visual feedback
            DrawCanvas.Cursor = new Cursor(StandardCursorType.Cross);
        }

        private void DrawCanvas_PointerExited(object? sender, PointerEventArgs e)
        {
            // Clear coordinates when mouse leaves canvas
            //MousePositionText.Text = "Mouse outside canvas";
            DrawCanvas.Cursor = new Cursor(StandardCursorType.Arrow);
        }
        private async void LoadFileButton_Click(object? sender, RoutedEventArgs e)
        {
            LoadSvgFile();
        }

        private async void SaveImageButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                string? filePath = await SelectFileToSave();

                if (filePath != null)
                {
                    // Get the displayed image from the canvas
                    var displayedImage = GetDisplayedImageFromCanvas();

                    if (displayedImage != null)
                    {
                        await SaveDisplayedImage(displayedImage, filePath);
                        await ShowMessageBox(this,"Success", $"Canvas image saved to:\n{Path.GetFileName(filePath)}");
                    }
                    else
                    {
                        // Fallback to  engine rendering
                        //RenderToFile((int)DrawCanvas.Bounds.Width, (int)DrawCanvas.Bounds.Height, filePath, GetFormatFromPath(filePath));
                       // await ShowMessageBox(this, "Success", $"Image saved to:\n{Path.GetFileName(filePath)}");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowMessageBox(this, "Error", $"Failed to save image:\n{ex.Message}");
            }
        }

        private Image? GetDisplayedImageFromCanvas()
        {
            // Look for an Image control in the canvas children
            foreach (var child in DrawCanvas.Children)
            {
                if (child is Image image)
                {
                    return image;
                }
            }
            return null;
        }

        private async Task SaveDisplayedImage(Image image, string filePath)
        {
            if (image.Source is Bitmap bitmap)
            {
                // Save the bitmap directly
                bitmap.Save(filePath);
                Console.WriteLine($"✓ Canvas image saved to: {filePath}");
            }
            else
            {
                throw new InvalidOperationException("Image source is not a bitmap");
            }
        }
        private async void RenderToImage_Click(object? sender, RoutedEventArgs e)
        {
            //
             string? filePath = await SelectFileToOpen();
            string? outputPath = await SelectFileToSave();
            //
            if (filePath != null && outputPath != null)
            {
                RenderDemoLoadFile(width: 800, height: 600, outputPath: outputPath, format: SKEncodedImageFormat.Png,
                filePath: filePath);
            }
        }
        async void RenderDemoLoadFile(int width, int height, string outputPath, SKEncodedImageFormat format, string filePath)
        {
            try
            {
                Console.WriteLine($"Loading SVG from: {filePath}");

                // 1. Load SVG file
                var layerManager = new LayerManager();
                var importer = new Arnaoot.VectorGraphics.Formats.Svg.SvgImporter();
                importer.LoadFromSvg(filePath, layerManager);

                Console.WriteLine($"Loaded {layerManager.Layers.Count} layers with {layerManager.GetAllElements().Count()} elements");

                // 2. Setup view
                //var viewport = new Rect2(0, 0, width, height);
                IViewSettings viewSettings = new ViewSettings(
                    GetUsableViewportwithControls(),
                    new Arnaoot.Core.Vector3D(1, 1, 1),  // zoom
                    new Arnaoot.Core.Vector3D(),         // pan
                    new Arnaoot.Core.Vector3D(),         // rotation
                    new Arnaoot.Core.Vector3D()          // center
                );

                // 3. Zoom to extents
                var zooming = new Zooming();
                viewSettings = zooming.ZoomExtents(viewSettings, layerManager, 5f);

                Console.WriteLine($"View bounds: {layerManager.GetVisibleBounds()}");

                // 4. Create Skia surface and render target
                //var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
                //using var surface = SKSurface.Create(imageInfo);
                using var renderTarget = new SkiaRenderTarget();

                // 5. Create render manager
                var renderManager = new RenderManager(renderTarget)
                {
                    ShowGrid = false,
                    ShowAxes = false,
                    ShowScaleBar = false
                };

                // 6. Get visible elements (with culling)
                var allElements = layerManager.GetVisibleElements();
                var visibleElements = allElements
                    .Where(el => renderManager.IsBoundsVisible(el.GetBounds(), viewSettings))
                    .ToList();

                Console.WriteLine($"Rendering {visibleElements.Count} visible elements (culled from {allElements.Count})");

                // 7. Render to Skia surface (NOT System.Drawing.Bitmap!)
                //renderManager.RasterizeIntoBuffer(width, height,
                //                  viewSettings,
                //    visibleElements,
                //    new Layer(),          // Empty temp layer
                //    ArgbColor.White       // Background color
                //);

                // 8. Save to file
                //using var image = surface.Snapshot();
                //using var data = image.Encode(format, 100);
                //using var fileStream = File.OpenWrite(outputPath);
                //data.SaveTo(fileStream);

                Console.WriteLine($"✓ Saved to: {outputPath}");
                await ShowMessageBox(this, "Success", $"Image successfully saved to:\n{System.IO.Path.GetFileName(outputPath)}");
            }

            catch (Exception ex)
            {
                // Call as instance method
                await ShowMessageBox(this, "Error", $"Failed to save image:\n{ex.Message}");
            }
        }

        private void OnPanButtonClick(object? sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            if (clickedButton == null) return;

            //  Get current shift from your ViewSettings
            Arnaoot.Core.Vector3D shift = _viewSettings.ShiftWorld;
            int shiftSize =(int)( 5/  _viewSettings.ZoomFactorAverage) ;

            switch (clickedButton.Name)
            {
                case nameof(PanXPlus):
                    shift.X = shift.X + shiftSize;
                    break;
                case nameof(PanXMinus):
                    shift.X = shift.X - shiftSize;
                    break;
                case nameof(PanYPlus):
                    shift.Y = shift.Y + shiftSize;
                    break;
                case nameof(PanYMinus):
                    shift.Y = shift.Y - shiftSize;
                    break;
                // Note: Your XAML only has X/Y buttons, no Z buttons
                default:
                    return;
            }

            //   Update the label text
            //InfoText.Text = $"X: {(int)shift.X}, Y: {(int)shift.Y}, Z: {(int)shift.Z}";
            UpdateInfoText();
            //   Update ViewSettings
            _viewSettings.ShiftWorld = shift;

            RedrawCanvas();  
        }
        private void OnZoomIn(object? sender, RoutedEventArgs e)
        {
            // Use the DrawCanvas dimensions instead of window dimensions
            double centerX = DrawCanvas.Bounds.Width / 2;
            double centerY = DrawCanvas.Bounds.Height / 2;

            _viewSettings = _zooming.ZoomIn(_viewSettings, (float)centerX, (float)centerY);
            UpdateViewSettingsFromTransform();
            RedrawCanvas();

        }

        private void OnZoomOut(object? sender, RoutedEventArgs e)
        {
            // Use the DrawCanvas dimensions instead of window dimensions
            double centerX = DrawCanvas.Bounds.Width / 2;
            double centerY = DrawCanvas.Bounds.Height / 2;

            _viewSettings = _zooming.ZoomOut(_viewSettings, (float)centerX, (float)centerY);
            UpdateViewSettingsFromTransform();
            RedrawCanvas();
        }

        private void OnZoomFit(object? sender, RoutedEventArgs e)
        {
            //  Your existing zoom fit logic
            _viewSettings = _zooming.ZoomExtents(_viewSettings, _layerManager, 5f);
            UpdateTransformFromViewSettings();
            RedrawCanvas();
        }
        #endregion
        #region Update ViewSettings not applied
        //  Sync between your ViewSettings and local transform
        private void UpdateViewSettingsFromTransform()
        {
            // Modify your _viewSettings based on _offsetX, _offsetY, _zoomLevel
            // This depends on your ViewSettings structure - adapt accordingly
        }

        private void UpdateTransformFromViewSettings()
        {
            // Update _offsetX, _offsetY, _zoomLevel from your _viewSettings
            // This depends on your ViewSettings structure - adapt accordingly
        }
        #endregion
        #region Rasteraization
        private void RedrawCanvas()
        {
            try
            {
                Console.WriteLine("=== RedrawCanvas Started ===");

                // Get canvas dimensions
                int width = (int)DrawCanvas.Bounds.Width;
                int height = (int)DrawCanvas.Bounds.Height;
                 
                _viewSettings = new ViewSettings(
                   GetUsableViewportwithControls(),
                   _viewSettings.ZoomFactor,  // zoom
                   _viewSettings.ShiftWorld,         // pan
                  _viewSettings.RotationAngle,         // rotation
                _viewSettings.RotateAroundPoint          // center
               );

                Console.WriteLine($"Canvas dimensions: {width}x{height}");

                if (width <= 0 || height <= 0)
                {
                    Console.WriteLine(" Canvas has invalid dimensions");
                    return;
                }

                // Check if layer manager has content
                Console.WriteLine($"LayerManager has {_layerManager.Layers.Count} layers");
                var allElements = _layerManager.GetVisibleElements();
                Console.WriteLine($"Total visible elements: {allElements.Count()}");

                // 1. Create Skia surface
                var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
                using SKSurface surface = SKSurface.Create(imageInfo);

                if (surface == null)
                {
                    Console.WriteLine(" Failed to create SKSurface");
                    DrawDebugPlaceholder("SKSurface creation failed");
                    return;
                }
                Console.WriteLine("✓ SKSurface created");

                using var renderTarget = new SkiaRenderTarget();
                var renderManager = new RenderManager(renderTarget)
                {
                    ShowGrid = false,
                    ShowAxes = false,
                    ShowScaleBar = false
                };

                // Get visible elements
                var visibleElements = allElements
                    .Where(el => renderManager.IsBoundsVisible(el.GetBounds(), _viewSettings))
                    .ToList();

                Console.WriteLine($"Rendering {visibleElements.Count} visible elements");

                if (visibleElements.Count == 0)
                {
                    Console.WriteLine(" No visible elements to render");
                    DrawDebugPlaceholder("No visible elements");
                    return;
                }

                // Render to Skia surface
                LastRenderTime = renderManager.RasterizeIntoBuffer(
                    width, height,
                    _viewSettings,
                    visibleElements,
                    new Layer(),
                    ArgbColor.White,
                    InvalidationLevel.Full,
                    out PixelData pixels,
                    out bool ok);

                if (!ok)
                {
                    Console.WriteLine("Rasterization failed");
                    return;
                }

                // ✅ Then convert PixelData → Avalonia Bitmap
                if (DisplayPixelData(pixels))
                {
                    Console.WriteLine("Frame displayed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" RedrawCanvas error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                DrawDebugPlaceholder($"Error: {ex.Message}");

            }
        }
        private void DrawDebugPlaceholder(string message)
        {
            try
            {
                DrawCanvas.Children.Clear();

                var border = new Border
                {
                    Background = new SolidColorBrush(Colors.LightYellow),
                    BorderBrush = new SolidColorBrush(Colors.Red),
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(10),
                    [Canvas.LeftProperty] = 10,
                    [Canvas.TopProperty] = 10
                };

                var text = new TextBlock
                {
                    Text = $"DEBUG: {message}\n" +
                           $"Canvas: {DrawCanvas.Bounds.Width}x{DrawCanvas.Bounds.Height}\n" +
                           $"Layers: {_layerManager.Layers.Count}\n" +
                           $"Zoom: {_viewSettings.ZoomFactorAverage:F2}x",
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontWeight = FontWeight.Bold
                };

                border.Child = text;
                DrawCanvas.Children.Add(border);

                Console.WriteLine($" Debug placeholder shown: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Debug placeholder error: {ex.Message}");
            }
        }
        private bool DisplayPixelData(PixelData pixels)
        {
            try
            {
                // Dispose previous
                _currentBitmap?.Dispose();

                var pixelSize = new PixelSize(pixels.Width, pixels.Height);
                _currentBitmap = new WriteableBitmap(
                    pixelSize,
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Premul);

                using (var locked = _currentBitmap.Lock())
                {
                    // Copy pixels (use Marshal.Copy — no unsafe needed)
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

                // Update UI — NOW bitmap is alive
                DrawCanvas.Children.Clear();
                var img = new Image { Source = _currentBitmap };
                DrawCanvas.Children.Add(img);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Display error: {ex}");
                _currentBitmap?.Dispose();
                _currentBitmap = null;
                return false;
            }
        }
        #endregion


        private void UpdateInfoText()
        {
            Arnaoot.Core.Vector3D shift = _viewSettings.ShiftWorld;
            InfoText.Text = $"DrawTime:{LastRenderTime}, Zoom: {_viewSettings.ZoomFactorAverage:F2}x | Shift: X:{(int)shift.X}, Y:{(int)shift.Y}, Z:{(int)shift.Z} ";
        }
        #region Helper
        private static async Task ShowMessageBox(Window parent, string title, string message)
        {
            var dialog = new Window()
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
            };

            var panel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Spacing = 10
            };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            var okButton = new Button { Content = "OK", Width = 80 };
            okButton.Click += (s, e) => dialog.Close();

            panel.Children.Add(okButton);
            dialog.Content = panel;

            await dialog.ShowDialog(parent);
        }
        private async Task<string?> SelectFileToOpen()
        {
            try
            {
                var dialog = new OpenFileDialog()  // ✅ Use the new API
                {
                    Title = "Select SVG File",
                    AllowMultiple = false
                };

                dialog.Filters.Add(new FileDialogFilter { Name = "SVG Files", Extensions = { "svg" } });
                dialog.Filters.Add(new FileDialogFilter { Name = "All Files", Extensions = { "*" } });

                var result = await dialog.ShowAsync(this);

                if (result != null && result.Length > 0 && File.Exists(result[0]))
                {
                    return result[0];
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File selection error: {ex.Message}");
                return null;
            }
        }
        private async Task<string?> SelectFileToSave()
        {
            try
            {
                var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Image As",
                    DefaultExtension = "png",
                    ShowOverwritePrompt = true,
                    FileTypeChoices = new[]
                    {
                new FilePickerFileType("PNG Files")
                {
                    Patterns = new[] { "*.png" }
                },
                new FilePickerFileType("JPEG Files")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*" }
                }
            }
                });

                if (file != null)
                {
                    return file.Path.LocalPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File save dialog error: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}