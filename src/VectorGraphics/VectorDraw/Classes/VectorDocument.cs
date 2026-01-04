using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Commands;
using Arnaoot.VectorGraphics.Rendering;
using Arnaoot.VectorGraphics.Scene;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Core.Models
{
    /// <summary>
    /// Represents a vector graphics document with layers, view settings, and command management.
    /// This is the core data model that holds all document state.
    /// </summary>
    public class VectorDocument : IDisposable
    {
        #region Fields
        private ILayerManager _layers;
        private IViewSettings _viewSettings;
        private ICommandManager _undoRedo;
        private DocumentSettings _settings;
        private bool _isModified;
        private bool _disposed;
        private Rendering.InvalidationLevel _invalidationLevel;
        #endregion

        #region Properties
 
        /// <summary>
        /// Gets or sets the current invalidation level.
        /// </summary>
        public InvalidationLevel InvalidationLevel
        {
            get => _invalidationLevel;
            private set => _invalidationLevel = value;
        }

        /// <summary>
        /// Occurs when the document requests a redraw at a specific invalidation level.
        /// </summary>
        public event EventHandler<InvalidationEventArgs>? InvalidationRequested;

        /// <summary>
        /// Requests a redraw at the specified invalidation level.
        /// This will update the InvalidationLevel property and fire the event.
        /// </summary>
        public void RequestRedraw(InvalidationLevel level)
        {
            if (level > _invalidationLevel)
            {
                _invalidationLevel = level;
                InvalidationRequested?.Invoke(this, new InvalidationEventArgs(level));
            }
        }
        #endregion
         /// <summary>
        /// Gets the layer manager for this document.
        /// </summary>
        public ILayerManager Layers
        {
            get => _layers;
            private set
            {
                if (_layers != null)
                {
                    _layers.LayersChanged -= OnLayersChanged;
                }

                _layers = value;

                if (_layers != null)
                {
                    _layers.LayersChanged += OnLayersChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets the view settings for this document.
        /// </summary>
        public IViewSettings ViewSettings
        {
            get => _viewSettings;
            set
            {
                if (_viewSettings != value)
                {
                    _viewSettings = value;
                    OnViewChanged();
                }
            }
        }

        /// <summary>
        /// Gets the undo/redo command manager.
        /// </summary>
        public ICommandManager UndoRedo => _undoRedo;

        /// <summary>
        /// Gets the document settings (colors, defaults, etc.).
        /// </summary>
        public DocumentSettings Settings => _settings;

        /// <summary>
        /// Gets or sets whether the document has been modified since last save.
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    OnModifiedStateChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the document file path (if saved).
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Gets the document name (filename without path).
        /// </summary>
        public string DocumentName => string.IsNullOrEmpty(FilePath)
            ? "Untitled"
            : System.IO.Path.GetFileNameWithoutExtension(FilePath);

        /// <summary>
        /// Gets the background color for rendering.
        /// </summary>
        public ArgbColor BackColor => _settings.BackColor;

        /// <summary>
        /// Gets or sets the current drawing color.
        /// </summary>
        public ArgbColor DrawColor
        {
            get => _settings.DrawColor;
            set
            {
                if (_settings.DrawColor != value)
                {
                    _settings.DrawColor = value;
                    OnDrawColorChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current fill color.
        /// </summary>
        public ArgbColor FillColor
        {
            get => _settings.FillColor;
            set
            {
                if (_settings.FillColor != value)
                {
                    _settings.FillColor = value;
                    OnFillColorChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether the document is currently in a batch update.
        /// </summary>
        public bool IsInBatchUpdate { get; private set; }

        #region Events
        /// <summary>
        /// Raised when the document content changes (elements added/removed/modified).
        /// </summary>
        public event EventHandler? DocumentChanged;

        /// <summary>
        /// Raised when the view settings change (zoom, pan, rotation).
        /// </summary>
        public event EventHandler? ViewChanged;

        /// <summary>
        /// Raised when the selection changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// Raised when the modified state changes.
        /// </summary>
        public event EventHandler? ModifiedStateChanged;

        /// <summary>
        /// Raised when the draw color changes.
        /// </summary>
        public event EventHandler? DrawColorChanged;

        /// <summary>
        /// Raised when the fill color changes.
        /// </summary>
        public event EventHandler? FillColorChanged;

        /// <summary>
        /// Raised when a batch update begins.
        /// </summary>
        public event EventHandler? BatchUpdateStarted;

        /// <summary>
        /// Raised when a batch update ends.
        /// </summary>
        public event EventHandler? BatchUpdateEnded;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the VectorDocument class.
        /// </summary>
        public VectorDocument(
            ILayerManager? layers = null,
            IViewSettings? viewSettings = null,
            ICommandManager? undoRedo = null,
            DocumentSettings? settings = null)
        {
            _layers = layers ?? new LayerManager();
            _viewSettings = viewSettings ?? CreateDefaultViewSettings();
            _undoRedo = undoRedo ?? new CommandManager();
            _settings = settings ?? new DocumentSettings();

            // Wire up events using property setter
            Layers = _layers;
            _undoRedo.HistoryChanged += OnHistoryChanged;

            IsModified = false;
        }

        private IViewSettings CreateDefaultViewSettings()
        {
            Rect2 CurrentusableViewport;

            if (_viewSettings is null)
            {
                CurrentusableViewport = new Rect2(0, 0, 1200, 800);
            }
            else
            {
                CurrentusableViewport = _viewSettings.UsableViewport;
            }
                return new ViewSettings(
                    usableViewport: CurrentusableViewport,
                    zoomFactor: new Vector3D(1.0F, 1.0F, 1.0F),
                    shiftWorld: new Vector3D(0, 0, 0),
                    rotationAngle: new Vector3D(0, 0, 0),
                    rotateAroundPoint: new Vector3D(0, 0, 0)
                );
        }
        #endregion

        #region Batch Updates
        /// <summary>
        /// Begins a batch update. Events are suppressed until EndUpdate is called.
        /// </summary>
        public void BeginUpdate()
        {
            if (!IsInBatchUpdate)
            {
                IsInBatchUpdate = true;
                OnBatchUpdateStarted();
            }
        }

        /// <summary>
        /// Ends a batch update and raises pending events.
        /// </summary>
        public void EndUpdate()
        {
            if (IsInBatchUpdate)
            {
                IsInBatchUpdate = false;
                OnBatchUpdateEnded();
                OnDocumentChanged(); // Raise once after batch
            }
        }
        #endregion

        #region Document Operations
        /// <summary>
        /// Clears all content from the document.
        /// </summary>
        public void Clear(bool resetView = true)
        {
            BeginUpdate();
            try
            {
                _layers.RemoveAllLayers();
                _layers.AddLayer("Default");

                if (resetView)
                {
                    _viewSettings = CreateDefaultViewSettings();
                }

                _undoRedo.Clear();
                IsModified = false;
                FilePath = null;
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>
        /// Creates a new empty document.
        /// </summary>
        public void New()
        {
            Clear(resetView: true);
        }
        #endregion

        #region Selection Management
        /// <summary>
        /// Gets all currently selected elements across all layers.
        /// </summary>
        public IEnumerable<IDrawElement> GetSelectedElements()
        {
            return _layers.GetAllElements().Where(e => e.IsSelected);
        }

        /// <summary>
        /// Clears the selection across all layers.
        /// </summary>
        public void ClearSelection()
        {
            var selectedElements = GetSelectedElements().ToList();
            if (selectedElements.Any())
            {
                _layers.ClearSelection();
                OnSelectionChanged(new SelectionChangedEventArgs(selectedElements, Array.Empty<IDrawElement>()));
            }
        }

        /// <summary>
        /// Selects the specified elements.
        /// </summary>
        public void Select(IEnumerable<IDrawElement> elements, bool clearExisting = true)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            var elementsList = elements.ToList();
            var oldSelection = clearExisting ? GetSelectedElements().ToList() : new List<IDrawElement>();

            if (clearExisting)
            {
                _layers.ClearSelection();
            }

            foreach (var element in elementsList)
            {
                element.IsSelected = true;
            }

            OnSelectionChanged(new SelectionChangedEventArgs(oldSelection, elementsList));
        }

        /// <summary>
        /// Toggles selection for the specified element.
        /// </summary>
        public void ToggleSelection(IDrawElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.IsSelected = !element.IsSelected;

            var selected = element.IsSelected
                ? new[] { element }
                : Array.Empty<IDrawElement>();

            var deselected = element.IsSelected
                ? Array.Empty<IDrawElement>()
                : new[] { element };

            OnSelectionChanged(new SelectionChangedEventArgs(deselected, selected));
        }
        #endregion

        #region Undo/Redo
        /// <summary>
        /// Executes a command through the undo/redo system.
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _undoRedo.ExecuteCommand(command);
            IsModified = true;
        }

        /// <summary>
        /// Undoes the last command.
        /// </summary>
        public bool Undo()
        {
            if (_undoRedo.CanUndo)
            {
                _undoRedo.Undo();
                IsModified = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Redoes the last undone command.
        /// </summary>
        public bool Redo()
        {
            if (_undoRedo.CanRedo)
            {
                _undoRedo.Redo();
                IsModified = true;
                return true;
            }
            return false;
        }
        #endregion

        #region Query Methods
        /// <summary>
        /// Gets the bounding box of all visible elements.
        /// </summary>
        public BoundingBox3D GetDocumentBounds()
        {
            var elements = _layers.GetVisibleElements().ToList();

            if (!elements.Any())
                return new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(0, 0, 0));

            var bounds = elements[0].GetBounds();

            foreach (var element in elements.Skip(1))
            {
                bounds = BoundingBox3D.Union(element.GetBounds(), bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Gets all visible elements in the document.
        /// </summary>
        public IReadOnlyCollection<IDrawElement> GetVisibleElements()
        {
            return _layers.GetVisibleElements();
        }

        /// <summary>
        /// Gets all elements in the document (including hidden layers).
        /// </summary>
        public IEnumerable<IDrawElement> GetAllElements()
        {
            return _layers.GetAllElements();
        }

        /// <summary>
        /// Finds elements at the specified point within tolerance.
        /// </summary>
        public IEnumerable<IDrawElement> HitTest(Vector3D point, float tolerance)
        {
            return _layers.GetVisibleElements()
                .Where(e => e.HitTest(point, tolerance));
        }

        /// <summary>
        /// Finds elements within the specified rectangular region.
        /// </summary>
        public IEnumerable<IDrawElement> FindInRegion(BoundingBox3D region)
        {
            return _layers.GetVisibleElements()
                .Where(e => region.IntersectsWith(e.GetBounds()));
        }
        #endregion

        #region Statistics
        /// <summary>
        /// Gets statistics about the document.
        /// </summary>
        public DocumentStatistics GetStatistics()
        {
            var allElements = GetAllElements().ToList();
            var visibleElements = GetVisibleElements().ToList();
            var selectedElements = GetSelectedElements().ToList();

            return new DocumentStatistics
            {
                TotalElements = allElements.Count,
                VisibleElements = visibleElements.Count,
                SelectedElements = selectedElements.Count,
                TotalLayers = _layers.Layers.Count,
                VisibleLayers = _layers.Layers.Count(l => l.Visible),
                UndoStackSize = _undoRedo.UndoCount,
                RedoStackSize = _undoRedo.RedoCount,
                DocumentBounds = GetDocumentBounds()
            };
        }
        #endregion

        #region Event Handlers
        private void OnLayersChanged()
        {
            if (!IsInBatchUpdate)
            {
                OnDocumentChanged();
            }
            IsModified = true;
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
        {
            if (!IsInBatchUpdate)
            {
                OnDocumentChanged();
            }
        }
        #endregion

        #region Event Raising
        protected virtual void OnDocumentChanged()
        {
            DocumentChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnViewChanged()
        {
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        protected virtual void OnModifiedStateChanged()
        {
            ModifiedStateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDrawColorChanged()
        {
            DrawColorChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFillColorChanged()
        {
            FillColorChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnBatchUpdateStarted()
        {
            BatchUpdateStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnBatchUpdateEnded()
        {
            BatchUpdateEnded?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Unwire events
                    if (_layers != null)
                    {
                        _layers.LayersChanged -= OnLayersChanged;
                    }

                    if (_undoRedo != null)
                    {
                        _undoRedo.HistoryChanged -= OnHistoryChanged;
                    }

                    // Dispose managed resources if they implement IDisposable
                    (_undoRedo as IDisposable)?.Dispose();
                    (_layers as IDisposable)?.Dispose();
                }

                _disposed = true;
            }
        }
        #endregion
    }

    #region Supporting Classes
    /// <summary>
    /// Contains document-level settings like colors and defaults.
    /// </summary>
    public class DocumentSettings
    {
        /// <summary>
        /// Gets or sets the default drawing/stroke color for new elements.
        /// </summary>
        public ArgbColor DrawColor { get; set; } = ArgbColor.Black;

        /// <summary>
        /// Gets or sets the default fill color for new elements.
        /// </summary>
        public ArgbColor FillColor { get; set; } = ArgbColor.Transparent;

        /// <summary>
        /// Gets or sets the background color for the document.
        /// </summary>
        public ArgbColor BackColor { get; set; } = ArgbColor.White;

        /// <summary>
        /// Gets or sets the default stroke width for new elements.
        /// </summary>
        public int DefaultStrokeWidth { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether new elements should be filled by default.
        /// </summary>
        public bool DefaultFilled { get; set; } = false;

        /// <summary>
        /// Gets or sets the selection tolerance in world units.
        /// </summary>
        public float SelectionTolerance { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the grid size.
        /// </summary>
        public float GridSize { get; set; } = 10.0f;

        /// <summary>
        /// Gets or sets whether snap to grid is enabled.
        /// </summary>
        public bool SnapToGrid { get; set; } = false;

        /// <summary>
        /// Gets or sets the snap tolerance.
        /// </summary>
        public float SnapTolerance { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets whether to show grid.
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show axes.
        /// </summary>
        public bool ShowAxes { get; set; } = true;
    }

    /// <summary>
    /// Statistics about a document.
    /// </summary>
    public class DocumentStatistics
    {
        public int TotalElements { get; set; }
        public int VisibleElements { get; set; }
        public int SelectedElements { get; set; }
        public int TotalLayers { get; set; }
        public int VisibleLayers { get; set; }
        public int UndoStackSize { get; set; }
        public int RedoStackSize { get; set; }
        public BoundingBox3D DocumentBounds { get; set; }

        public override string ToString()
        {
            return $"Elements: {VisibleElements}/{TotalElements}, " +
                   $"Layers: {VisibleLayers}/{TotalLayers}, " +
                   $"Selected: {SelectedElements}, " +
                   $"Undo: {UndoStackSize}, Redo: {RedoStackSize}";
        }
    }

    /// <summary>
    /// Event arguments for selection changes.
    /// </summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        public IEnumerable<IDrawElement> DeselectedElements { get; }
        public IEnumerable<IDrawElement> SelectedElements { get; }

        public SelectionChangedEventArgs(
            IEnumerable<IDrawElement> deselected,
            IEnumerable<IDrawElement> selected)
        {
            DeselectedElements = deselected ?? Enumerable.Empty<IDrawElement>();
            SelectedElements = selected ?? Enumerable.Empty<IDrawElement>();
        }
    }
    #endregion
}