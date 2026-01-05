using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core.Models;
using Arnaoot.VectorGraphics.Core.Tools;
using Arnaoot.VectorGraphics.Rendering;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;
//
namespace VectorDrawControl.Classes.Tools
{
    /// <summary>
    /// Manages tool registration, activation, and coordination.
    /// Provides centralized tool switching and state management.
    /// </summary>
    public class ToolManager
    {
        #region Fields
        private readonly Dictionary<string, Tool> _tools = new();
        private Tool _currentTool = NullTool.Instance;
        //private Tool? _currentTool;
        private Tool? _previousTool;
        private readonly Stack<Tool> _toolHistory = new();
        private const int MAX_HISTORY = 10;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the currently active tool.
        /// </summary>
        public Tool? CurrentTool => _currentTool;

        /// <summary>
        /// Gets the previously active tool.
        /// </summary>
        public Tool? PreviousTool => _previousTool;

        /// <summary>
        /// Gets all registered tools.
        /// </summary>
        public IReadOnlyDictionary<string, Tool> RegisteredTools => _tools;

        /// <summary>
        /// Gets the default tool ID to use when no tool is active.
        /// </summary>
        public string DefaultToolId { get; set; } = "SelectTool";
        #endregion

        #region Events
        /// <summary>
        /// Raised before a tool is changed. Can be cancelled.
        /// </summary>
        public event EventHandler<ToolChangingEventArgs>? ToolChanging;

        /// <summary>
        /// Raised after a tool has been changed.
        /// </summary>
        public event EventHandler<ToolChangedEventArgs>? ToolChanged;

        /// <summary>
        /// Raised when a tool activation fails.
        /// </summary>
        public event EventHandler<ToolActivationFailedEventArgs>? ToolActivationFailed;
        #endregion

        #region Tool Registration
        /// <summary>
        /// Registers a tool with the manager.
        /// </summary>
        /// <param name="tool">The tool to register.</param>
        /// <param name="toolId">Optional custom ID. If null, uses tool.Id.</param>
        public void RegisterTool(Tool tool, string? toolId = null)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            string id = toolId ?? tool.Id;

            if (_tools.ContainsKey(id))
                throw new InvalidOperationException($"Tool with ID '{id}' is already registered.");

            _tools[id] = tool;
        }

        /// <summary>
        /// Registers multiple tools at once.
        /// </summary>
        public void RegisterTools(params Tool[] tools)
        {
            foreach (var tool in tools)
            {
                RegisterTool(tool);
            }
        }

        /// <summary>
        /// Unregisters a tool.
        /// </summary>
        public bool UnregisterTool(string toolId)
        {
            if (_currentTool?.Id == toolId)
                throw new InvalidOperationException("Cannot unregister the currently active tool.");

            return _tools.Remove(toolId);
        }

        /// <summary>
        /// Gets a tool by its ID.
        /// </summary>
        public Tool? GetTool(string toolId)
        {
            return _tools.TryGetValue(toolId, out var tool) ? tool : null;
        }
        #endregion

        #region Tool Activation
        public void ClearActiveTool(VectorDocument document)
        {
            ActivateTool(NullTool.Instance, document);
        }
        /// <summary>
        /// Activates a tool by its ID.
        /// </summary>
        /// <param name="toolId">The ID of the tool to activate.</param>
        /// <param name="document">The document context.</param>
        /// <returns>True if activation succeeded; otherwise, false.</returns>
        public bool ActivateTool(string toolId, VectorDocument document)
        {
            if (!_tools.TryGetValue(toolId, out var newTool))
            {
                OnToolActivationFailed(new ToolActivationFailedEventArgs(
                    toolId, $"Tool '{toolId}' not found."));
                return false;
            }

            return ActivateTool(newTool, document);
        }

        /// <summary>
        /// Activates a specific tool instance.
        /// </summary>
        public bool ActivateTool(Tool newTool, VectorDocument document)
        {
            if (newTool == null)
                throw new ArgumentNullException(nameof(newTool));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            // Don't reactivate the same tool
            if (_currentTool == newTool)
                return true;

            // Validate the new tool can be activated
            if (!newTool.CanActivate(document))
            {
                OnToolActivationFailed(new ToolActivationFailedEventArgs(
                    newTool.Id,
                    $"Tool '{newTool.Name}' cannot be activated in the current state."));
                return false;
            }

            // Raise changing event (can be cancelled)
            var changingArgs = new ToolChangingEventArgs(_currentTool, newTool);
            OnToolChanging(changingArgs);

            if (changingArgs.Cancel)
                return false;

            // Deactivate current tool
            if (_currentTool != null)
            {
                _currentTool.OnDeactivate(document);
                _previousTool = _currentTool;

                // Add to history
                _toolHistory.Push(_currentTool);
                if (_toolHistory.Count > MAX_HISTORY)
                {
                    // Keep history bounded
                    var temp = _toolHistory.ToArray();
                    _toolHistory.Clear();
                    for (int i = 0; i < MAX_HISTORY; i++)
                        _toolHistory.Push(temp[i]);
                }
            }

            // Activate new tool
            _currentTool = newTool;
            _currentTool.OnActivate(document);

            // Raise changed event
            OnToolChanged(new ToolChangedEventArgs(_previousTool, _currentTool));

            return true;
        }

        /// <summary>
        /// Restores the previously active tool.
        /// </summary>
        public bool RestorePreviousTool(VectorDocument document)
        {
            if (_previousTool == null)
                return false;

            return ActivateTool(_previousTool, document);
        }

        /// <summary>
        /// Activates the default tool.
        /// </summary>
        public bool ActivateDefaultTool(VectorDocument document)
        {
            return ActivateTool(DefaultToolId, document);
        }

        /// <summary>
        /// Temporarily activates a tool, storing the current tool for later restoration.
        /// Useful for operations like zoom rectangles that need to return to the previous tool.
        /// </summary>
        public bool ActivateTemporaryTool(string toolId, VectorDocument document)
        {
            // The activation already stores previous tool, so just activate normally
            return ActivateTool(toolId, document);
        }
        #endregion

        #region Input Event Processing (NEW - Complete)
        /// <summary>
        /// Processes a mouse down event. The tool manager owns all interaction logic
        /// and will update the document's invalidation level accordingly.
        /// </summary>
        public void ProcessMouseDown(
            Vector2D pixel,
            MouseButtons button,
            Keys modifiers,
            VectorDocument document)
        {
            if (document == null) return;

            var prevLevel = document.InvalidationLevel;
            var mouseArgs = new MouseEventArgs(button, 0, (int)pixel.X, (int)pixel.Y, 0);

            _currentTool?.OnMouseDown(mouseArgs, document);

            var newLevel = _currentTool?.GetInvalidationLevelOnInteraction() ?? InvalidationLevel.None;
            if (newLevel > prevLevel)
                document.RequestRedraw(newLevel);
        }

        /// <summary>
        /// Processes a mouse move event.
        /// </summary>
        public void ProcessMouseMove(
            Vector2D pixel,
            Keys modifiers,
            VectorDocument document)
        {
            if (document == null) return;

            var prevLevel = document.InvalidationLevel;
            var mouseArgs = new MouseEventArgs(MouseButtons.None, 0, (int)pixel.X, (int)pixel.Y, 0);

            _currentTool?.OnMouseMove(mouseArgs, document);

            var newLevel = _currentTool?.GetInvalidationLevelOnInteraction() ?? InvalidationLevel.None;
            if (newLevel > prevLevel)
                document.RequestRedraw(newLevel);
        }

        /// <summary>
        /// Processes a mouse up event.
        /// </summary>
        public void ProcessMouseUp(
            Vector2D pixel,
            MouseButtons button,
            Keys modifiers,
            VectorDocument document)
        {
            if (document == null) return;

            var prevLevel = document.InvalidationLevel;
            var mouseArgs = new MouseEventArgs(button, 0, (int)pixel.X, (int)pixel.Y, 0);

            _currentTool?.OnMouseUp(mouseArgs, document);

            var newLevel = _currentTool?.GetInvalidationLevelOnInteraction() ?? InvalidationLevel.None;
            if (newLevel > prevLevel)
                document.RequestRedraw(newLevel);
        }

        /// <summary>
        /// Processes a mouse wheel event.
        /// </summary>
        public void ProcessMouseWheel(
            Vector2D pixel,
            int delta,
            Keys modifiers,
            VectorDocument document)
        {
            if (document == null) return;

            var prevLevel = document.InvalidationLevel;
            var mouseArgs = new MouseEventArgs(MouseButtons.None, 0, (int)pixel.X, (int)pixel.Y, delta);

            _currentTool?.OnMouseWheel(mouseArgs, document);

            var newLevel = _currentTool?.GetInvalidationLevelOnInteraction() ?? InvalidationLevel.None;
            if (newLevel > prevLevel)
                document.RequestRedraw(newLevel);
        }

        /// <summary>
        /// Processes a key down event.
        /// </summary>
        public void ProcessKeyDown(
            Keys keyCode,
            Keys modifiers,
            VectorDocument document)
        {
            if (document == null) return;

            var prevLevel = document.InvalidationLevel;
            var keyArgs = new KeyEventArgs(keyCode | modifiers);

            _currentTool?.OnKeyDown(keyArgs, document);

            var newLevel = _currentTool?.GetInvalidationLevelOnInteraction() ?? InvalidationLevel.None;
            if (newLevel > prevLevel)
                document.RequestRedraw(newLevel);
        }

        /// <summary>
        /// Processes a key up event.
        /// </summary>
        public void ProcessKeyUp(
            Keys keyCode,
            Keys modifiers,
            VectorDocument document)
        {
            if (document == null) return;

            var prevLevel = document.InvalidationLevel;
            var keyArgs = new KeyEventArgs(keyCode | modifiers);

            _currentTool?.OnKeyUp(keyArgs, document);

            var newLevel = _currentTool?.GetInvalidationLevelOnInteraction() ?? InvalidationLevel.None;
            if (newLevel > prevLevel)
                document.RequestRedraw(newLevel);
        }
        #endregion

        #region Query Methods
        /// <summary>
        /// Gets the temporary element from the current tool for rendering.
        /// </summary>
        public IDrawElement? GetTemporaryElement()
        {
            return _currentTool?.GetTemporaryElement();
        }

        /// <summary>
        /// Gets overlay elements from the current tool for rendering.
        /// </summary>
        public IEnumerable<IDrawElement> GetOverlayElements()
        {
            return _currentTool?.GetOverlayElements() ?? Enumerable.Empty<IDrawElement>();
        }

        /// <summary>
        /// Gets the status message from the current tool.
        /// </summary>
        public string GetStatusMessage()
        {
            return _currentTool?.GetStatusMessage() ?? "No tool active";
        }

        /// <summary>
        /// Gets the cursor for the current tool.
        /// </summary>
        public Cursor GetCursor()
        {
            return _currentTool?.Cursor ?? Cursors.Default;
        }
        #endregion

        #region Event Raising
        protected virtual void OnToolChanging(ToolChangingEventArgs e)
        {
            ToolChanging?.Invoke(this, e);
        }

        protected virtual void OnToolChanged(ToolChangedEventArgs e)
        {
            ToolChanged?.Invoke(this, e);
        }

        protected virtual void OnToolActivationFailed(ToolActivationFailedEventArgs e)
        {
            ToolActivationFailed?.Invoke(this, e);
        }
        #endregion



        #region Legacy Input Delegation (for backward compatibility)
        /// <summary>
        /// Delegates mouse down event to the current tool.
        /// </summary>
        public InvalidationLevel HandleMouseDown(MouseEventArgs e, VectorDocument document)
        {
            return _currentTool.OnMouseDown(e, document);
        }

        /// <summary>
        /// Delegates mouse move event to the current tool.
        /// </summary>
        public InvalidationLevel HandleMouseMove(MouseEventArgs e, VectorDocument document)
        {
            if (_currentTool == null)
                return InvalidationLevel.None;

            return _currentTool.OnMouseMove(e, document);
        }

        /// <summary>
        /// Delegates mouse up event to the current tool.
        /// </summary>
        public InvalidationLevel HandleMouseUp(MouseEventArgs e, VectorDocument document)
        {
            return _currentTool.OnMouseUp(e, document);
        }

        /// <summary>
        /// Delegates mouse wheel event to the current tool.
        /// </summary>
        public InvalidationLevel HandleMouseWheel(MouseEventArgs e, VectorDocument document)
        {
            return _currentTool.OnMouseWheel(e, document);
        }

        /// <summary>
        /// Delegates key down event to the current tool.
        /// </summary>
        public InvalidationLevel HandleKeyDown(KeyEventArgs e, VectorDocument document)
        {
            return _currentTool.OnKeyDown(e, document);
        }

        /// <summary>
        /// Delegates key up event to the current tool.
        /// </summary>
        public InvalidationLevel HandleKeyUp(KeyEventArgs e, VectorDocument document)
        {
            return _currentTool.OnKeyUp(e, document);
        }
        #endregion
    }
    #region Event Arguments
    /// <summary>
    /// Event arguments for when a tool is about to change (cancellable).
    /// </summary>
    public class ToolChangingEventArgs : EventArgs
    {
        public Tool? OldTool { get; }
        public Tool? NewTool { get; }
        public bool Cancel { get; set; }

        public ToolChangingEventArgs(Tool? oldTool, Tool? newTool)
        {
            OldTool = oldTool;
            NewTool = newTool;
        }
    }

    /// <summary>
    /// Event arguments for when a tool has changed.
    /// </summary>
    public class ToolChangedEventArgs : EventArgs
    {
        public Tool? OldTool { get; }
        public Tool NewTool { get; }

        public ToolChangedEventArgs(Tool? oldTool, Tool newTool)
        {
            OldTool = oldTool;
            NewTool = newTool;
        }
    }

    /// <summary>
    /// Event arguments for when tool activation fails.
    /// </summary>
    public class ToolActivationFailedEventArgs : EventArgs
    {
        public string ToolId { get; }
        public string Reason { get; }

        public ToolActivationFailedEventArgs(string toolId, string reason)
        {
            ToolId = toolId;
            Reason = reason;
        }
    }
    #endregion
}
