using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core.Models;
using Arnaoot.VectorGraphics.Elements;
using Arnaoot.VectorGraphics.Rendering;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Core.Tools
{
    /// <summary>
    /// Tool for performing a zoom operation by drawing a rectangle.
    /// Activated typically by a right mouse button drag.
    /// Temporarily replaces the current tool, performs the zoom, and then restores the previous tool.
    /// </summary>
    public class ZoomRectangleTool : Tool
    {
        private Vector3D? _startPoint; // World coordinates of the rectangle's start point
        private RectangleElement? _tempRectangleElement; // Temporary rectangle element shown during dragging
        private Tool? _previousTool; // The tool that was active before this one
        private DateTime _clickStartTime; // To detect very short drags (clicks) which cancel the operation
        private bool _isDragging = false; // Flag to track if actively drawing the rectangle
                                          //
        #region Tool Metadata
        public override string Name => "Pan Tool";
        public override string Description => "pan the displayed view";
        public override Cursor Cursor => Cursors.Cross; // Standard cursor for rectangle drawing
        public override bool RequiresActiveLayer => true;
        #endregion

        /// <summary>
        /// Initializes a new instance of the ZoomRectangleTool.
        /// </summary>
        /// <param name="previousTool">The tool that was active before this one.</param>
        /// <param name="clickStartTime">The time when the mouse button was pressed.</param>
        public ZoomRectangleTool(Tool previousTool, DateTime clickStartTime)
        {
            _previousTool = previousTool;
            _clickStartTime = clickStartTime;
        }

        public override InvalidationLevel OnMouseDown(MouseEventArgs e, VectorDocument document)
        {
            // OnMouseDown should ideally not be called again while this tool is active,
            // as it replaces the active tool upon activation by the control.
            // However, if it is, ignore subsequent down events.
            if (e.Button == MouseButtons.Right && !_isDragging)
            {
                // Store the start point for the zoom rectangle using the document's current ViewSettings
                _startPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                _isDragging = true; // Set the dragging flag
                // Create a temporary rectangle element with zero size initially (start point = end point)
                // This element is not yet added to the layer or command history.
                var initialRect = new RectangleElement(
                    _startPoint.Value, // Top Left (or first corner)
                    _startPoint.Value, // Bottom Right (or second corner) - initially same as first
                    false,             // IsSelected initially
                    1,                 // width or ID
                    ArgbColor.Blue,// Color (assuming ArgbColor constructor or property)
                   ArgbColor.Blue,
                  false);
                _tempRectangleElement = initialRect;
            }
            return InvalidationLevel.None;
        }

        public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
        {
            // Only update if actively dragging the rectangle
            if (_isDragging && e.Button == MouseButtons.Right && _startPoint.HasValue)
            {
                // Convert mouse coordinates to world coordinates using the document's current ViewSettings
                Vector3D currentPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                _tempRectangleElement.EndPoint = currentPoint;
                return InvalidationLevel.View;
            }
            // If not dragging, OnMouseMove does nothing for this tool
            return InvalidationLevel.None;
         }

        public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document)
        {
            // Only finish zooming if was actively dragging and released the right button
            if (_isDragging && e.Button == MouseButtons.Right && _startPoint.HasValue && _tempRectangleElement is RectangleElement tempRect)
            {
                // Convert mouse coordinates to world coordinates for the final end point
                Vector3D endPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));

                // Check if it was a click (very short duration) or a drag
                // Note: _clickStartTime was passed in the constructor when the tool was created (on mouse down)
                if (DateTime.Now.Subtract(_clickStartTime).TotalMilliseconds < 200)
                {
                    ResetToolState();
                    ToolToRestore = _previousTool; // This property needs to be defined in this class or handled differently by the control.
                    _previousTool = null; // Clear reference to previous tool in this instance
                    return InvalidationLevel.None; // Exit early, no zoom performed
                }

                _zoomRectangleStart = _startPoint.Value;
                _zoomRectangleEnd = endPoint;

                // Indicate that the control should perform the zoom and then restore the previous tool.
                ToolToRestore = _previousTool; // Signal control to restore this tool after zoom
                _previousTool = null; // Clear reference to previous tool in this instance
            }

            // Reset the dragging state regardless of the outcome (except when returning early for click)
            ResetToolState();
            return InvalidationLevel.None;
        }

        // --- Properties for Control Communication ---
        // The control needs to know which tool to restore after this one finishes.
        public Tool? ToolToRestore { get; private set; }
        // The control needs the rectangle points to perform the zoom.
        private Vector3D? _zoomRectangleStart;
        private Vector3D? _zoomRectangleEnd;
        public (Vector3D start, Vector3D end)? GetZoomRectanglePoints()
        {
            if (_zoomRectangleStart.HasValue && _zoomRectangleEnd.HasValue)
                return (_zoomRectangleStart.Value, _zoomRectangleEnd.Value);
            return null;
        }
        // --- END Properties ---

        // --- Implement abstract GetTemporaryElement method ---
        // Returns the temporary rectangle element while dragging.
        public override IDrawElement? GetTemporaryElement()
        {
            // Only provide the temporary element if actively dragging
            return _isDragging ? _tempRectangleElement : null;
        }
        // --- END NEW ---

        public override void OnDeactivate(VectorDocument document)
        {
            // Ensure any temporary state is cleared when the tool is switched away from
            ResetToolState();
            // Do NOT clear ToolToRestore or _previousTool here if OnMouseUp handles it correctly.
            // _previousTool = null; // Potentially clear if not needed after deactivation, but OnMouseUp should handle it.
            base.OnDeactivate(document);
        }
        private void ResetToolState()
        {
            _isDragging = false;
            _startPoint = null;
            _tempRectangleElement = null;
        }
    }
}