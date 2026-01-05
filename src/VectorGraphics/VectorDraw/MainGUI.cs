using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Core.Models;
using Arnaoot.VectorGraphics.Core.Tools;
using Arnaoot.VectorGraphics.Elements;
using Arnaoot.VectorGraphics.Platform.Skia;
using Arnaoot.VectorGraphics.Platform.SVGFile;
using Arnaoot.VectorGraphics.Platform.WinForms;
using Arnaoot.VectorGraphics.Rendering;
using Arnaoot.VectorGraphics.Scene;
using Arnaoot.VectorGraphics.View;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VectorDrawControl.Classes.Tools;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.UI
{
    public partial class EngineControl : UserControl
    {
        #region Fields

        // === Core Document & Tool System ===
        private VectorDocument _document;
        private ToolManager _toolManager;

        // === Rendering System ===
        private IRenderTarget _renderTarget;
        private IRenderManager _renderManager;
        private IZooming _zooming = new Zooming();
        private Bitmap _rasterizedImageBuffer;

        // === Throttling ===
        private bool _pendingInvalidate = false;
        private DateTime _lastInvalidate = DateTime.MinValue;
        private const int THROTTLE_MS = 16; // ~60 FPS

        #endregion

        #region Events

        /// <summary>
        /// Occurs when adding draw element to a layer that has not been initialized.
        /// </summary>
        public event EventHandler<LayerNotInitializedEventArgs> LayerNotInitializedWarning;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the EngineControl class.
        /// </summary>
        public EngineControl()
        {
            // Initialize rendering backend (Skia by default, fallback to GDI+)
            _renderTarget = new SkiaRenderTarget();
            _renderManager = new RenderManager(_renderTarget);

            // Create the document with a default layer manager
            var layerManager = new LayerManager();
            layerManager.AddLayer("Default");

            _document = new VectorDocument(
                layers: layerManager,
                viewSettings: new ViewSettings(
                     GetUsableViewport(),
                    new Vector3D(1.0F, 1.0F, 1.0F),
                    new Vector3D(0, 0, 0),
                    new Vector3D(0, 0, 0),
                    new Vector3D(0, 0, 0))
            );

            // Initialize tool system
            InitializeToolSystem();

            // Wire up document events
            WireDocumentEvents();

            InitializeComponent();
        }

        private void InitializeToolSystem()
        {
            _toolManager = new ToolManager();

            // Register all available tools
            _toolManager.RegisterTool(new SelectTool(), "SelectTool");
            _toolManager.RegisterTool(new DrawLineTool(), "LineTool");
            _toolManager.RegisterTool(new DrawCircleTool(), "CircleTool");
            _toolManager.RegisterTool(new DrawRectangleTool(), "RectangleTool");
            _toolManager.RegisterTool(new DrawLabelTool(), "LabelTool");
            _toolManager.RegisterTool(new PanTool(), "PanTool");

            // Set default tool
            _toolManager.DefaultToolId = "SelectTool";
            _toolManager.ActivateDefaultTool(_document);

            // Wire up tool manager events
            _toolManager.ToolChanged += OnToolChanged;
            _toolManager.ToolActivationFailed += OnToolActivationFailed;
        }

        private void WireDocumentEvents()
        {
            _document.DocumentChanged += (s, e) => ScheduleInvalidate(InvalidationLevel.Full);
            _document.ViewChanged += (s, e) => ScheduleInvalidate(InvalidationLevel.Full);
            _document.SelectionChanged += OnDocumentSelectionChanged;
            _document.ModifiedStateChanged += OnDocumentModifiedStateChanged;
            _document.UndoRedo.HistoryChanged += (s, e) => UpdateUndoRedoButtons();
            _document.Layers.LayersChanged += OnLayersChanged;
        }

        #endregion

        #region Properties (Public Access)

        /// <summary>
        /// Gets the current document.
        /// </summary>
        public VectorDocument Document => _document;

        /// <summary>
        /// Gets the tool manager.
        /// </summary>
        public ToolManager ToolManager => _toolManager;

        /// <summary>
        /// Gets or sets the render manager.
        /// </summary>
        public IRenderManager RenderManager
        {
            get => _renderManager;
            set
            {
                _renderManager = value ?? throw new ArgumentNullException(nameof(value));
                ScheduleInvalidate(InvalidationLevel.Full);
            }
        }

        #endregion

        #region Rendering
        private void ScheduleInvalidate(InvalidationLevel invalidationLevel)
        {
            if (invalidationLevel == InvalidationLevel.None) return;//early exiting if nothing to do here
            if (_pendingInvalidate || !this.IsHandleCreated) return;

            var elapsed = (DateTime.UtcNow - _lastInvalidate).TotalMilliseconds;

            if (elapsed < THROTTLE_MS)
            {
                _pendingInvalidate = true;
                var delay = THROTTLE_MS - (int)elapsed;
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer(_ =>
                {
                    timer?.Dispose();
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            _pendingInvalidate = false;
                            _lastInvalidate = DateTime.UtcNow;
                            RenderContentBuffer(invalidationLevel);
                            UpdateStatusBar();
                        }));
                    }
                }, null, delay, Timeout.Infinite);
            }
            else
            {
                _pendingInvalidate = true;
                this.BeginInvoke(new Action(() =>
                {
                    _pendingInvalidate = false;
                    _lastInvalidate = DateTime.UtcNow;
                    RenderContentBuffer(invalidationLevel);
                    UpdateStatusBar();
                }));
            }
        }

        public void RenderContentBuffer(InvalidationLevel invalidationLevel)
        {
            Stopwatch totalSw = Stopwatch.StartNew();
            Stopwatch cullSw = new Stopwatch();
            List<IDrawElement> visibleElements = new List<IDrawElement>();
            //
            //
            //if (invalidationLevel == InvalidationLevel.None) return;
            if (this.Width <= 0 || this.Height <= 0) return;
            int ViewWidth = (int)_document.ViewSettings.UsableViewport.Width;
            int ViewHeight = (int)_document.ViewSettings.UsableViewport.Height;
            if (ViewWidth <= 0 || ViewHeight <= 0) return;

            // Reuse bitmap if size unchanged
            if (_rasterizedImageBuffer?.Width != ViewWidth || _rasterizedImageBuffer?.Height != ViewHeight)
            {
                _rasterizedImageBuffer?.Dispose();
                _rasterizedImageBuffer = new Bitmap(ViewWidth, ViewHeight);
            }
            //only if full redraw is needed i will apply culling
            if (invalidationLevel > InvalidationLevel.Overlay)
            {
                // Culling
                cullSw = Stopwatch.StartNew();
                visibleElements = _document.GetVisibleElements()
                   .Where(el => _renderManager.IsBoundsVisible(el.GetBounds(), _document.ViewSettings))
                   .ToList();
                cullSw.Stop();
            }
            // Temp layer for tool preview
            Layer tempLayer = new Layer();
            IDrawElement tempElement = _toolManager.GetTemporaryElement();
            if (tempElement != null)
            {
                tempLayer.AddElement(tempElement, false);
            }

            // Add overlay elements from current tool
            foreach (var overlay in _toolManager.GetOverlayElements())
            {
                tempLayer.AddElement(overlay, false);
            }

            // Rasterize
            Stopwatch rasterSw = Stopwatch.StartNew();
            long rasterTime = _renderManager.RasterizeIntoBuffer(
                ViewWidth, ViewHeight,
                _document.ViewSettings,
                visibleElements,
                tempLayer,
                _document.BackColor,
                invalidationLevel,
                out PixelData pixels,
                out bool pixelsOk
            );
            rasterSw.Stop();

            // Update bitmap from pixels
            if (pixelsOk)
            {
                UpdateBitmapFromPixels(_rasterizedImageBuffer, pixels);
            }

            totalSw.Stop();
            //      if (totalSw.ElapsedMilliseconds<10)
            //{
            //    Debugger.Break();
            //}    
            StatusLabelRenderTime.Text = $"T:{totalSw.ElapsedMilliseconds}ms C:{cullSw.ElapsedMilliseconds}ms R:{rasterTime}ms";
            Invalidate();

        }

        private static void UpdateBitmapFromPixels(Bitmap bmp, PixelData pixels)
        {
            if (bmp.Width != pixels.Width || bmp.Height != pixels.Height)
                throw new ArgumentException("Bitmap size mismatch");

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
        }

        private void UpdateStatusBar()
        {
            StatusZoom.Text = $"{_document.ViewSettings.ZoomFactorAverage * 100:0.00}%";

            var stats = _document.GetStatistics();
            toolStripStatusLabel1.Text = $"{stats.VisibleElements} elements";
        }

        #endregion

        #region Control Events

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_rasterizedImageBuffer != null)
            {
                e.Graphics.DrawImageUnscaled(_rasterizedImageBuffer, 0, 0);
            }
            else
            {
                e.Graphics.Clear(_document.BackColor);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _document.ViewSettings.UpdateUsableViewport(GetUsableViewport());
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Handle global shortcuts first
            if (e.Control && e.KeyCode == Keys.Z)
            {
                _document.Undo();
                ScheduleInvalidate(InvalidationLevel.Full);
                return;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                _document.Redo();
                ScheduleInvalidate(InvalidationLevel.Full);
                return;
            }
            else if (e.Control && e.KeyCode == Keys.Oemplus)
            {
                ZoomIn();
                return;
            }
            else if (e.Control && e.KeyCode == Keys.OemMinus)
            {
                ZoomOut();
                return;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _toolManager.ClearActiveTool(_document);

                return;
            }

            // Delegate to current tool
            _toolManager.HandleKeyDown(e, _document);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _toolManager.HandleKeyUp(e, _document);
        }

        #endregion

        #region Mouse Events

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Vector2D pivotPixel = new Vector2D(e.X, e.Y);
            float zoomFactor = e.Delta > 0 ? 1.1F : 0.9F;

            _document.ViewSettings = _zooming.Zoom(_document.ViewSettings, zoomFactor, pivotPixel);
            ScheduleInvalidate(InvalidationLevel.Full);

            // Also delegate to tool
            _toolManager.HandleMouseWheel(e, _document);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Handle global zoom rectangle with right mouse button
            if (e.Button == MouseButtons.Right)
            {
                var zoomTool = new ZoomRectangleTool(_toolManager.CurrentTool, DateTime.Now);
                _toolManager.ActivateTool(zoomTool, _document);
                _toolManager.HandleMouseDown(e, _document);
                return;
            }

            // Delegate to current tool
            //_toolManager.HandleMouseDown(e, _document);
            ScheduleInvalidate(_toolManager.HandleMouseDown(e, _document)); // Light invalidate for interactive feedback
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);


            //    // Throttle coordinate updates (they're expensive to format)
            //    if ((DateTime.UtcNow - _lastInvalidate).TotalMilliseconds > 50)
            //    {

            // Update coordinate display
            Vector3D realPoint = _document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
            StatusCoordinates.Text = $"Location: {realPoint.X:F1}, {realPoint.Y:F1}, {realPoint.Z:F1}";
            //        _lastInvalidate = DateTime.UtcNow;
            //    }

            // Delegate to current tool
            ScheduleInvalidate(_toolManager.HandleMouseMove(e, _document));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // Special handling for ZoomRectangleTool
            if (_toolManager.CurrentTool is ZoomRectangleTool zoomTool && e.Button == MouseButtons.Right)
            {
                _toolManager.HandleMouseUp(e, _document);

                var zoomPoints = zoomTool.GetZoomRectanglePoints();
                var toolToRestore = zoomTool.ToolToRestore;

                // Perform zoom if valid rectangle
                if (zoomPoints.HasValue)
                {
                    var (start, end) = zoomPoints.Value;
                    _document.ViewSettings = _zooming.ZoomToRectangle(_document.ViewSettings, start, end);
                    ScheduleInvalidate(InvalidationLevel.Full);
                }

                // Restore previous tool
                if (toolToRestore != null)
                {
                    _toolManager.ActivateTool(toolToRestore, _document);
                }
                else
                {
                    _toolManager.ActivateDefaultTool(_document);
                }

                return;
            }

            // Normal mouse up handling
            _toolManager.HandleMouseUp(e, _document);
            ScheduleInvalidate(InvalidationLevel.Overlay);
        }

        #endregion

        #region Helper Methods
        private Rect2 GetUsableViewport()
        {
            if (ToolStripMain == null || StatusStripMain == null)
                return new Rect2(0, 0, 0, 0);

            int left = 0;
            int top = ToolStripMain.Height;

            int right = (PanelRightTools != null && PanelRightTools.Visible)
                        ? PanelRightTools.Width
                        : 0;

            int width = Math.Max(0, ClientSize.Width - right);
            int height = Math.Max(0, ClientSize.Height - top);

            return new Rect2(left, top, width, height);
        }

        #endregion

        #region Document Event Handlers

        private void OnDocumentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update UI to reflect selection changes
            ScheduleInvalidate(InvalidationLevel.Overlay);
        }

        private void OnDocumentModifiedStateChanged(object sender, EventArgs e)
        {
            // Update title bar or status to show modified state
            // e.g., Form.Text = _document.IsModified ? "*" + _document.DocumentName : _document.DocumentName;
        }

        private void OnLayersChanged()
        {
            UpdateLayerMenu();
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        #endregion

        #region Tool Event Handlers

        private void OnToolChanged(object sender, ToolChangedEventArgs e)
        {
            // Update cursor
            this.Cursor = _toolManager.GetCursor();

            // Update status message
            // StatusMessage.Text = _toolManager.GetStatusMessage();

            // Update tool button states if needed
            UpdateToolButtonStates();
        }

        private void OnToolActivationFailed(object sender, ToolActivationFailedEventArgs e)
        {
            MessageBox.Show(
                e.Reason,
                "Cannot Activate Tool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void UpdateToolButtonStates()
        {
            // Update checked state of tool buttons based on current tool
            var currentToolId = _toolManager.CurrentTool?.Id;
            //
            BtnSelectTool.Checked = (currentToolId == nameof(SelectTool));
            BtnLineTool.Checked = (currentToolId == nameof(DrawLineTool));
            BtnCircleTool.Checked = (currentToolId == nameof(DrawCircleTool));
            BtnRectangleTool.Checked = (currentToolId == nameof(DrawRectangleTool));
            BtnTextTool.Checked = (currentToolId == nameof(DrawLabelTool));
            BtnPan.Checked = (currentToolId == nameof(PanTool));
        }

        #endregion

        #region ToolStrip Command Events

        private void BtnToggleGrid_Click(object sender, EventArgs e)
        {
            _renderManager.ShowGrid = !_renderManager.ShowGrid;
            BtnToggleGrid.Checked = _renderManager.ShowGrid;
            ScheduleInvalidate(InvalidationLevel.Overlay);
        }

        private void BtnToggleAxes_Click(object sender, EventArgs e)
        {
            _renderManager.ShowAxes = !_renderManager.ShowAxes;
            BtnToggleAxes.Checked = _renderManager.ShowAxes;
            ScheduleInvalidate(InvalidationLevel.Overlay);
        }

        private void BtnOrbitPanel_Click(object sender, EventArgs e)
        {
            BtnOrbitPanel.Checked = !BtnOrbitPanel.Checked;
            PanelRightTools.Visible = BtnOrbitPanel.Checked;
            _document.ViewSettings.UpdateUsableViewport(GetUsableViewport());
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        #endregion

        #region File Operations

        private void ToolStripButton_New_file_Click(object sender, EventArgs e)
        {
            _document.New();
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "SVG files (*.SVG)|*.SVG|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var exporter = new Arnaoot.VectorGraphics.Formats.Svg.SvgExporter();
                exporter.SaveAsSvg(saveFileDialog1.FileName, false, _document.Layers, _document.ViewSettings);
                _document.FilePath = saveFileDialog1.FileName;
                _document.IsModified = false;
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "SVG Files (*.svg)|*.svg|All files (*.*)|*.*",
                FilterIndex = 1,
                Title = "Select an SVG File"
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                if (System.IO.Path.GetExtension(filePath).ToLower() == ".svg")
                {
                    try
                    {
                        _document.Clear(resetView: true);
                        // Measure PROPERLY
                        GC.Collect(); // Clean up any garbage first
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        //
                        long before = GC.GetTotalMemory(true); // 'true' forces full collection

                        var importer = new Arnaoot.VectorGraphics.Formats.Svg.SvgImporter();
                        importer.LoadFromSvg(filePath, _document.Layers);
                        //
                        _document.FilePath = filePath;
                        _document.IsModified = false;
                        //
                        //
                        GC.Collect(); // Clean temp allocations
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        //
                        long after = GC.GetTotalMemory(true);
                        int ElementsCount = _document.Layers.GetAllElements().Count();
                        double bytesPerElement = (after - before) / ElementsCount;
                        Console.WriteLine($"{bytesPerElement:F0} bytes per element");
                        StatusMemoryUsed.Text = $"{bytesPerElement:F0} bytes per element";
                        toolStripStatusLabel1.Text = ElementsCount.ToString() + "Element Loaded";
                        StatusLabelTotalMemory.Text = ((after - before) / (1024)).ToString() + "kb";
                        //
                        toolStripLabelFileName.Text = "Opened File: " + filePath;
                        ZoomExtents(5f);
                        //
                        var stats = _document.GetStatistics();
                        toolStripStatusLabel1.Text = $"{stats.TotalElements} elements loaded";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error processing the SVG file: " + ex.Message);
                    }
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                Vector3D topLeft = _document.ViewSettings.PictToReal(new Vector2D(0F, ToolStripMain.Height));
                Vector3D bottomRight = _document.ViewSettings.PictToReal(new Vector2D(Width, Height - StatusStripMain.Height));
                BoundingBox3D imageExtents = new BoundingBox3D(topLeft, bottomRight);

                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "Png file (*.Png)|*.Png|All files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    WinFormsImageExporter.SaveRegionAsImage(
                        saveFileDialog1.FileName,
                        _renderManager,
                        _document.GetVisibleElements(),
                        _document.ViewSettings,
                        imageExtents,
                        Width,
                        Height,
                        System.Drawing.Imaging.ImageFormat.Png,
                        true);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Image export failed: " + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Tool Selection

        private void BtnSelectTool_Click(object sender, EventArgs e)
        {
            _toolManager.ActivateTool("SelectTool", _document);
        }

        private void BtnLineTool_Click(object sender, EventArgs e)
        {
            _toolManager.ActivateTool("LineTool", _document);
        }

        private void BtnCircleTool_Click(object sender, EventArgs e)
        {
            _toolManager.ActivateTool("CircleTool", _document);
        }

        private void BtnRectangleTool_Click(object sender, EventArgs e)
        {
            _toolManager.ActivateTool("RectangleTool", _document);
        }

        private void BtnTextTool_Click(object sender, EventArgs e)
        {
            _toolManager.ActivateTool("LabelTool", _document);
        }

        private void BtnPan_Click(object sender, EventArgs e)
        {
            _toolManager.ActivateTool("PanTool", _document);
        }

        private void chooseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _document.DrawColor = colorDialog1.Color;
                this.chooseColorToolStripMenuItem.BackColor = _document.DrawColor;
                this.chooseColorToolStripMenuItem.ForeColor = GetContrastColor(_document.DrawColor);
            }
        }

        private Color GetContrastColor(Color bgColor)
        {
            double luminance = (0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B) / 255;
            return luminance > 0.5 ? Color.Black : Color.White;
        }

        #endregion

        #region Undo/Redo

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            _document.Undo();
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void BtnRedo_Click(object sender, EventArgs e)
        {
            _document.Redo();
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void UpdateUndoRedoButtons()
        {
            BtnUndo.Enabled = _document.UndoRedo.CanUndo;
            BtnRedo.Enabled = _document.UndoRedo.CanRedo;
        }

        #endregion

        #region Layer Management

        private void UpdateLayerMenu()
        {
            UpdateVisibilityMenu();
            UpdateActiveLayerMenu();
            UpdateDeleteLayerMenu();
        }

        private void UpdateVisibilityMenu()
        {
            setLayerVisibilityToolStripMenuItem.DropDownItems.Clear();

            foreach (ILayer layer in _document.Layers.Layers)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(layer.Name)
                {
                    Checked = layer.Visible,
                    Tag = layer.Id
                };
                item.Click += (s, e) => ToggleLayerVisibility(layer);
                setLayerVisibilityToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void UpdateActiveLayerMenu()
        {
            setActiveLayerToolStripMenuItem.DropDownItems.Clear();

            foreach (ILayer layer in _document.Layers.Layers)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(layer.Name)
                {
                    Checked = (_document.Layers.ActiveLayer == layer),
                    Tag = layer.Id
                };
                item.Click += (s, e) => SetActiveLayer(layer);
                setActiveLayerToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void UpdateDeleteLayerMenu()
        {
            deleteLayerToolStripMenuItem.DropDownItems.Clear();

            if (_document.Layers.Layers.Count > 1)
            {
                foreach (ILayer layer in _document.Layers.Layers)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(layer.Name)
                    {
                        Tag = layer.Id
                    };
                    item.Click += (s, e) => DeleteLayer(layer);
                    deleteLayerToolStripMenuItem.DropDownItems.Add(item);
                }
            }

            deleteLayerToolStripMenuItem.Enabled = deleteLayerToolStripMenuItem.DropDownItems.Count > 0;
        }

        private void ToggleLayerVisibility(ILayer layer)
        {
            layer.Visible = !layer.Visible;
            UpdateLayerMenu();
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void SetActiveLayer(ILayer layer)
        {
            _document.Layers.SetActiveLayer(layer);
            UpdateLayerMenu();
            ScheduleInvalidate(InvalidationLevel.Overlay);
        }

        private void DeleteLayer(ILayer layer)
        {
            if (_document.Layers.Layers.Count <= 1) return;

            DialogResult result = MessageBox.Show(
                $"Delete layer '{layer.Name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _document.Layers.RemoveLayer(layer);
                UpdateLayerMenu();
                ScheduleInvalidate(InvalidationLevel.Full);
            }
        }

        private void AddNewLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string layerName = $"Layer {_document.Layers.Layers.Count + 1}";
            _document.Layers.AddLayer(layerName);
            UpdateLayerMenu();
            ScheduleInvalidate(InvalidationLevel.Overlay);
        }

        #endregion

        #region Zoom Operations
        #region Button Handlers

        private void MenuItemZoomPrevious_Click(object sender, EventArgs e)
        {
            _document.ViewSettings = _zooming.ZoomPrevious(_document.ViewSettings);
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            ZoomIn();
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            ZoomOut();
        }

        private void BtnZoomFit_Click(object sender, EventArgs e)
        {
            ZoomExtents(5f);
        }
        #endregion
        #region Methods

        public void ZoomIn()
        {
            _document.ViewSettings = _zooming.ZoomIn(_document.ViewSettings, this.Width / 2, this.Height / 2);
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        public void ZoomOut()
        {
            _document.ViewSettings = _zooming.ZoomOut(_document.ViewSettings, this.Width / 2, this.Height / 2);
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        public void ZoomExtents(float paddingPercent)
        {
            _document.ViewSettings = _zooming.ZoomExtents(_document.ViewSettings, _document.Layers, paddingPercent);
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        public void CenterViewOnPosition(float span, float realX, float realY)
        {
            BoundingBox3D bounds = new BoundingBox3D(
                new Vector3D(realX - span / 2, realY - span / 2, 0),
                new Vector3D(realX + span / 2, realY + span / 2, 0));
            _document.ViewSettings = _zooming.GetRegionViewSettings(_document.ViewSettings, bounds, 5F);
            ScheduleInvalidate(InvalidationLevel.Full);
        }
        #endregion

        #endregion

        #region Side Panel Controls

        private void BtnResetWorldShift_Click(object sender, EventArgs e)
        {
            ResetViewSetting(false, true, false, false);
        }

        private void btnWorldShift_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null) return;

            Vector3D shift = _document.ViewSettings.ShiftWorld;
            int shiftSize = (int)(5 / _document.ViewSettings.ZoomFactorAverage);

            switch (clickedButton.Name)
            {
                case nameof(btnWorldShiftXDec): shift.X -= shiftSize; break;
                case nameof(btnWorldShiftXInc): shift.X += shiftSize; break;
                case nameof(btnWorldShiftYDec): shift.Y -= shiftSize; break;
                case nameof(btnWorldShiftYInc): shift.Y += shiftSize; break;
                case nameof(btnWorldShiftZDec): shift.Z -= shiftSize; break;
                case nameof(btnWorldShiftZInc): shift.Z += shiftSize; break;
            }

            lblworldShift.Text = $"X: {(int)shift.X}, Y: {(int)shift.Y}, Z: {(int)shift.Z}";

            _document.ViewSettings = new ViewSettings(
                _document.ViewSettings.UsableViewport,
                _document.ViewSettings.ZoomFactor,
                shift,
                _document.ViewSettings.RotationAngle,
                _document.ViewSettings.RotateAroundPoint
            );
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void TrackBarRotation_Scroll(object sender, EventArgs e)
        {
            LblXRotation.Text = $"X: {(float)TrackBarXRotation.Value * 1.8 / 3.14:0.00} °";
            LblYRotation.Text = $"Y: {(float)TrackBarYRotation.Value * 1.8 / 3.14:0.00} °";
            LblZRotation.Text = $"Z: {(float)TrackBarZRotation.Value * 1.8 / 3.14:0.00} °";

            Vector2D centerPixel = new Vector2D(this.Width / 2.0f, this.Height / 2.0f);
            Vector3D rotatePoint = _document.ViewSettings.PictToViewPlane(centerPixel, 0.0f);

            Vector3D rotationAngle = new Vector3D(
                (float)TrackBarXRotation.Value / 100f,
                (float)TrackBarYRotation.Value / 100f,
                (float)TrackBarZRotation.Value / 100f);

            _document.ViewSettings = new ViewSettings(
                GetUsableViewport(),
                _document.ViewSettings.ZoomFactor,
                _document.ViewSettings.ShiftWorld,
                rotationAngle,
                rotatePoint);

            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void BtnResetRotation_Click(object sender, EventArgs e)
        {
            ResetViewSetting(false, false, true, false);
        }

        private void ResetViewSetting(bool resetZoomFactor, bool resetShiftWorld, bool resetRotationAngle, bool resetRotateAroundPoint)
        {
            Vector3D zoomFactor = _document.ViewSettings.ZoomFactor;
            Vector3D shiftWorld = _document.ViewSettings.ShiftWorld;
            Vector3D rotationAngle = _document.ViewSettings.RotationAngle;
            Vector3D rotateAroundPoint = _document.ViewSettings.RotateAroundPoint;

            if (resetZoomFactor)
            {
                zoomFactor = new Vector3D(1, 1, 1);
            }

            if (resetShiftWorld)
            {
                shiftWorld = new Vector3D(0, 0, 0);
            }

            if (resetRotationAngle)
            {
                rotationAngle = new Vector3D(0, 0, 0);
                LblXRotation.Text = "X: 0°";
                LblYRotation.Text = "Y: 0°";
                LblZRotation.Text = "Z: 0°";
                TrackBarXRotation.Value = 0;
                TrackBarYRotation.Value = 0;
                TrackBarZRotation.Value = 0;
            }

            if (resetRotateAroundPoint)
            {
                rotateAroundPoint = new Vector3D(0, 0, 0);
            }

            if (resetRotationAngle || resetShiftWorld || resetRotateAroundPoint || resetZoomFactor)
            {
                _document.ViewSettings = new ViewSettings(
                    GetUsableViewport(),
                    zoomFactor,
                    shiftWorld,
                    rotationAngle,
                    rotateAroundPoint);
                ScheduleInvalidate(InvalidationLevel.Full);
            }
        }

        private void TxtLabelInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                Vector3D realPoint = _document.ViewSettings.PictToReal(new Vector2D(TxtLabelInput.Left, TxtLabelInput.Top));
                LabelElement label = new LabelElement(realPoint, TxtLabelInput.Text, _document.DrawColor, 8);
                _document.Layers.AddElementsToActiveLayer(new[] { label });
                TxtLabelInput.Text = "";
                TxtLabelInput.Visible = false;
                ScheduleInvalidate(InvalidationLevel.Full);
            }
        }

        #endregion

        #region Renderer Switching

        private void BtnSetRender_Click(object sender, EventArgs e)
        {
            // Determine which button was clicked
            ToolStripMenuItem clickedButton = sender as ToolStripMenuItem;

            if (clickedButton == null)
                return;
            //
            // Uncheck all buttons
            BtnSetRenderSkia.Checked = false;
            BtnSetRenderGDI.Checked = false;
            //BtnSetRenderSVGFile.Checked = false;

            // Check the clicked button
            clickedButton.Checked = true;

            // Set the appropriate render target
            if (BtnSetRenderSkia.Checked)
            {
                _renderTarget = new SkiaRenderTarget();
            }
            else if (BtnSetRenderGDI.Checked)
            {
                _renderTarget = new WinFormsRenderTarget();
            }
             // Reinitialize render manager with new target
            _renderManager = new RenderManager(_renderTarget);
            ScheduleInvalidate(InvalidationLevel.Full);
        }

        private void BtnSetRenderSVGFile_Click(object sender, EventArgs e)
        {
            Stopwatch totalSw = Stopwatch.StartNew();
            Stopwatch cullSw = new Stopwatch();
            List<IDrawElement> visibleElements = new List<IDrawElement>();
            //
            IRenderTarget _tempRenderTarget = new RenderTargetSvgFile();
            IRenderManager _TempRenderManager = new RenderManager(_tempRenderTarget);
            //
            int ViewWidth = (int)_document.ViewSettings.UsableViewport.Width;
            int ViewHeight = (int)_document.ViewSettings.UsableViewport.Height;
            if (ViewWidth <= 0 || ViewHeight <= 0) return;
            //
            // Culling
            cullSw = Stopwatch.StartNew();
            visibleElements = _document.GetVisibleElements()
               .Where(el => _renderManager.IsBoundsVisible(el.GetBounds(), _document.ViewSettings))
               .ToList();
            cullSw.Stop();

            Stopwatch rasterSw = Stopwatch.StartNew();
            long rasterTime = _TempRenderManager.RasterizeIntoBuffer(
                ViewWidth, ViewHeight,
                _document.ViewSettings,
                visibleElements,
                new Layer(),
                _document.BackColor,
                InvalidationLevel.Full,
                out PixelData pixels,
                out bool pixelsOk
            );
            rasterSw.Stop();
        }
         #endregion


    }
}
