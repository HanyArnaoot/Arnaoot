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
    /// Tool for drawing new circle elements.
    /// Allows the user to click and drag to define the center and radius of a circle.
    /// </summary>
    public class DrawCircleTool : Tool
    {
        #region Constants
        private const float MIN_Circle_Radius = 0.01f; // Minimum Circle length to create
        #endregion

        #region Fields
        private Vector3D? _centerPoint; // World coordinates of the circle's center
        private IDrawElement? _tempCircleElement; // Temporary circle element shown during dragging
        private bool _isDrawing = false; // Flag to track if actively drawing
        #endregion

        #region Tool Metadata
        public override string Name => "Circle Tool";
        public override string Description => "Draw Circle based on center point and another as radius";
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
                _centerPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));

                 // Create a temporary circle element with zero radius initially
                // This element is not yet added to the layer or command history.
                // Assuming CircleElement constructor takes (Center, RadiusPoint, ...).
                // We use the center point as the initial radius point, resulting in radius 0.
                CircleElement initialCircle = new CircleElement(_centerPoint.Value , // Center
                    MIN_Circle_Radius, // Initial Radius 
                    false,              // not fixed radius in pixel
                    1,                  // width 
                     document.DrawColor,     // Color
                     document.FillColor ,
                    false,
                    new Vector3D (0,0,1),
                    false
                );

                _tempCircleElement = initialCircle;
                _isDrawing = true; // Set the drawing flag

                // Clear any existing selection when starting to draw
                document.Layers.ClearSelection();  
            }
            return InvalidationLevel.None;
        }

        public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
        {
            // Only update if actively drawing
            if (_isDrawing && e.Button == MouseButtons.Left && _centerPoint.HasValue && _tempCircleElement is CircleElement tempCircle)
            {
                // Convert mouse coordinates to world coordinates
                Vector3D worldPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                 // Update the temporary circle's radius point (this defines the radius)
                  tempCircle.SetCircleRadiusBypoint(worldPoint);
                // Note: The visual update of the temporary element will happen in the main control's redraw logic,
                // which calls _currentTool.GetTemporaryElement().
                return InvalidationLevel.View;
            }
            return InvalidationLevel.None;

            // If not drawing, OnMouseMove does nothing for this tool


        }

        public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document)
        {
            // Only finish drawing if was actively drawing
            if (_isDrawing && e.Button == MouseButtons.Left && _centerPoint.HasValue && _tempCircleElement is CircleElement tempCircle)
            {
                // Convert mouse coordinates to world coordinates for the final radius point
                Vector3D worldPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
 
                // Check if the radius is greater than zero (center point != radius point)
                if (_centerPoint.Value != worldPoint)
                {
                     tempCircle.SetCircleRadiusBypoint(worldPoint);
 
                    // Create the command to add the completed circle element to the layer
                    ICommand command = new AddRemoveCommand(tempCircle, document.Layers.ActiveLayer, true); // Assuming AddRemoveCommand exists

                    // Execute the command through the document's command manager
                    document.UndoRedo.ExecuteCommand(command);

                    // The command execution should add the element to the layer.
                    // Optionally, select the newly created element
                    // tempCircle.IsSelected = true; // Depends on your selection logic after creation
                }
                // else: if the center and radius point are the same, no circle is created (zero radius), and the temporary element is discarded implicitly.
            }

            // Reset the drawing state regardless of whether a circle was created
            _isDrawing = false; // Clear the drawing flag
            _centerPoint = null;
            _tempCircleElement = null; // The temporary element is no longer needed
        return InvalidationLevel.None;
        }
        #endregion

        #region Keyboard Event Handlers
        public override InvalidationLevel OnKeyDown(KeyEventArgs e, VectorDocument document)
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
        public override void OnActivate(VectorDocument document)
        {
            base.OnActivate(document);
            ResetToolState();
            System.Diagnostics.Debug.WriteLine($"{Name} activated");
        }
        public override void OnDeactivate(VectorDocument document)
        {
            // Ensure any temporary state is cleared when the tool is switched away from
            ResetToolState();
            base.OnDeactivate(document);
        }
        #endregion
        #region Tool State
        public override IDrawElement? GetTemporaryElement()
        {
            // Only provide the temporary element if actively drawing
            return _isDrawing ? _tempCircleElement : null;
        }

        public bool IsDrawing => _isDrawing;

        private void ResetToolState()
        {
            _isDrawing = false; // Clear flag on deactivation too
            _centerPoint = null;
            _tempCircleElement = null;
        }
        #endregion

     }
}