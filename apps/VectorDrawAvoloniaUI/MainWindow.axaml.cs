
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using VectorDrawAvoloniaUI.Classes;
namespace VectorDrawAvoloniaUI
{
    /// <summary>
    /// Main window - handles UI events and delegates to controllers
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DrawingController _drawingController;
        private readonly FileController _fileController;
        private readonly ViewportController _viewportController;
        private readonly UIStateManager _uiStateManager;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize controllers with proper dependencies
            // IMPORTANT: DrawCanvas must be initialized by InitializeComponent() first
            _drawingController = new DrawingController(DrawCanvas);
            _fileController = new FileController(this, _drawingController, DrawCanvas);
            _viewportController = new ViewportController(_drawingController);
            _uiStateManager = new UIStateManager(InfoText, RotationXText, RotationYText, RotationZText);

            SetupEventHandlers();
            UpdateInfoDisplay();
        }

        private void SetupEventHandlers()
        {
            // Pan buttons
            PanXPlus.Click += (s, e) => OnPan(PanDirection.XPlus);
            PanXMinus.Click += (s, e) => OnPan(PanDirection.XMinus);
            PanYPlus.Click += (s, e) => OnPan(PanDirection.YPlus);
            PanYMinus.Click += (s, e) => OnPan(PanDirection.YMinus);

            // Zoom buttons
            ZoomIn.Click += (s, e) => OnZoom(ZoomAction.In);
            ZoomOut.Click += (s, e) => OnZoom(ZoomAction.Out);
            ZoomFit.Click += (s, e) => OnZoom(ZoomAction.Fit);

            // File operations
            LoadFileButton.Click += async (s, e) => await _fileController.LoadSvgFileAsync();
            SaveImageButton.Click += async (s, e) => await _fileController.SaveCanvasImageAsync();
            RenderToImage.Click += async (s, e) => await _fileController.RenderToImageFileAsync();

            // Mouse tracking
            DrawCanvas.PointerMoved += OnCanvasPointerMoved;
            DrawCanvas.PointerEntered += (s, e) => DrawCanvas.Cursor = new Cursor(StandardCursorType.Cross);
            DrawCanvas.PointerExited += (s, e) => DrawCanvas.Cursor = new Cursor(StandardCursorType.Arrow);

            // Viewport changes
            DrawCanvas.SizeChanged += (s, e) => _drawingController.RedrawCanvas();

            // Rotation controls
            RotationXSlider.ValueChanged += OnRotationChanged;
            RotationYSlider.ValueChanged += OnRotationChanged;
            RotationZSlider.ValueChanged += OnRotationChanged;
            ResetRotationButton.Click += (s, e) => ResetRotation();
        }

        protected override void OnClosed(EventArgs e)
        {
            _drawingController?.Dispose();
            base.OnClosed(e);
        }

        #region Event Handlers

        private void OnPan(PanDirection direction)
        {
            _viewportController.Pan(direction);
            UpdateInfoDisplay();
        }

        private void OnZoom(ZoomAction action)
        {
            var canvasBounds = DrawCanvas.Bounds;
            _viewportController.Zoom(action, canvasBounds);
            UpdateInfoDisplay();
        }

        private void OnRotationChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            var rotation = new RotationAngles(
                (float)RotationXSlider.Value,
                (float)RotationYSlider.Value,
                (float)RotationZSlider.Value
            );

            _viewportController.UpdateRotation(rotation, DrawCanvas.Bounds);
            _uiStateManager.UpdateRotationDisplay(rotation);
            UpdateInfoDisplay();
        }

        private void ResetRotation()
        {
            RotationXSlider.Value = 0;
            RotationYSlider.Value = 0;
            RotationZSlider.Value = 0;
        }

        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            var position = e.GetPosition(DrawCanvas);
            var worldCoords = _drawingController.ScreenToWorld(position);
            _uiStateManager.UpdateMousePosition(worldCoords, _drawingController.ViewSettings);
        }

        #endregion

        private void UpdateInfoDisplay()
        {
            _uiStateManager.UpdateInfoText(
                _drawingController.ViewSettings,
                _drawingController.LastRenderTime
            );
        }
    }
}