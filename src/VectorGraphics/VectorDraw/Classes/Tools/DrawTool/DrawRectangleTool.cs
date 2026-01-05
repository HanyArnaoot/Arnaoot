 using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Commands;  
using Arnaoot.VectorGraphics.Core.Models;  
using Arnaoot.VectorGraphics.Elements;
using Arnaoot.VectorGraphics.Rendering;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Core.Tools
{
    /// <summary>
    /// Tool for drawing new rectangle elements.
    /// Allows the user to click and drag to define the top-left and bottom-right corners of a rectangle.
    /// </summary>
    public class DrawRectangleTool : Tool
    {
        #region Constants
        private const float MIN_Rectangle_LENGTH = 0.01f; // Minimum Rectangle length to create
        #endregion
     
        #region Fields
        private Vector3D? _startPoint; // World coordinates of the rectangle's first corner (e.g., top-left)
        private IDrawElement? _tempRectangleElement; // Temporary rectangle element shown during dragging
        private bool _isDrawing = false; // Flag to track if actively drawing
        #endregion


        #region Tool Metadata
        public override string Name => "Rectangle Tool";
        public override string Description => "Draw Rectangle between two points";
        public override Cursor Cursor => Cursors.Cross; // Standard cursor for drawing tools
        public override bool RequiresActiveLayer => true;
        #endregion

        #region Validation
        public override bool CanActivate(VectorDocument document)
        {
            if (document?.Layers?.ActiveLayer == null)
                return false;

            return document.Layers.ActiveLayer.Locked == false;
        }
        #endregion
        #region Mouse Event Handlers

        public override InvalidationLevel OnMouseDown(MouseEventArgs e, VectorDocument document)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Convert mouse coordinates to world coordinates
                _startPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
  
                // Create a temporary rectangle element with zero size initially (start point = end point)
                // This element is not yet added to the layer or command history.
                // We use the start point for both corners initially.
                RectangleElement initialRectangle = new RectangleElement(
                    _startPoint.Value, // Top Left
                    _startPoint.Value, // Bottom Right (initially same as Top Left)
                    false,              // realtive coordiantes
                    2,                  // width  
                    document.DrawColor,     // border Color
                     ArgbColor.Black,// fill color
                     false //filled
                );

                _tempRectangleElement = initialRectangle;
                _isDrawing = true; // Set the drawing flag

                // Clear any existing selection when starting to draw
                document.Layers.ClearSelection(); // Assuming this method exists on ILayerManager
            }
            return InvalidationLevel.None;
         }

        public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
        {
            // Only update if actively drawing
            if (_isDrawing && e.Button == MouseButtons.Left && _startPoint.HasValue && _tempRectangleElement is RectangleElement tempRect)
            {
                // Convert mouse coordinates to world coordinates
                Vector3D worldPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));

                // Update the temporary rectangle's bottom-right corner
                // The RectangleElement class should handle calculating the bounds from TopLeft and BottomRight internally.
                tempRect.EndPoint = worldPoint;
                // Note: The visual update of the temporary element will happen in the main control's redraw logic,
                // which calls _currentTool.GetTemporaryElement().
                return InvalidationLevel.View;
            }
            return InvalidationLevel.None;

            // If not drawing, OnMouseMove does nothing for this tool
        }

        public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document)
        {
            // Only handle left mouse button release
            if (e.Button != MouseButtons.Left)
                return InvalidationLevel.None;
 
            // Only process if we were drawing
            if (!_isDrawing || !_startPoint.HasValue || _tempRectangleElement == null)
            {
                ResetToolState();
                return InvalidationLevel.None;
             }
            try
            {
                // Only finish drawing if was actively drawing
                if (_isDrawing && e.Button == MouseButtons.Left && _startPoint.HasValue && _tempRectangleElement is RectangleElement tempRect)
            {
                // Convert mouse coordinates to world coordinates for the final bottom-right corner
                Vector3D worldPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                //Vector2D endPoint = new Vector2D(worldPoint.X, worldPoint.Y);

                // Check if the rectangle has actual area (start point != end point)
                // A rectangle with zero width or height might not be desirable.
                if (_startPoint.Value != worldPoint)
                {
                    // Update the temporary rectangle's bottom-right corner one final time
                    tempRect.EndPoint = worldPoint;

                    // Create the command to add the completed rectangle element to the layer
                    ICommand command = new AddRemoveCommand(tempRect, document.Layers.ActiveLayer, true); // Assuming AddRemoveCommand exists

                    // Execute the command through the document's command manager
                    document.UndoRedo.ExecuteCommand(command);

                    // The command execution should add the element to the layer.
                    // Optionally, select the newly created element
                    // tempRect.IsSelected = true; // Depends on your selection logic after creation
                }
                // else: if the start and end points are the same, no rectangle is created (zero area), and the temporary element is discarded implicitly.
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating line: {ex.Message}");
                // Consider raising an error event here
            }
                  // Always reset state after mouse up
                    ResetToolState();
            return InvalidationLevel.None;
        }
        #endregion
        #region Keyboard Event Handlers
        public override InvalidationLevel  OnKeyDown(KeyEventArgs e, VectorDocument document)
        {
            base.OnKeyDown(e, document);

            // Allow ESC to cancel current drawing operation
            if (e.KeyCode == Keys.Escape && _isDrawing)
            {
                ResetToolState();
                e.Handled = true;
            }
            return InvalidationLevel.None;
        }
        #endregion
        #region Lifecycle Methods
        public override void OnDeactivate(VectorDocument document)
        {
            // Ensure any temporary state is cleared when the tool is switched away from
            _isDrawing = false; // Clear flag on deactivation too
            _startPoint = null;
            _tempRectangleElement = null;
            base.OnDeactivate(document);
        }
        #endregion
        #region Tool State
          public override IDrawElement? GetTemporaryElement()
        {
            // Only provide the temporary element if actively drawing
            return _isDrawing ? _tempRectangleElement : null;
        }
        public bool IsDrawing => _isDrawing;

        private void ResetToolState()
        {
            _startPoint = null;
            _tempRectangleElement = null;
            _isDrawing = false;
        }
        #endregion
 
    }
}