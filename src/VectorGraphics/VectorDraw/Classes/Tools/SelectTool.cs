// ... (using statements) ...
using Arnaoot.Core;
using Arnaoot.VectorGraphics.Commands;
using Arnaoot.VectorGraphics.Core.Models;
using Arnaoot.VectorGraphics.Elements;
using Arnaoot.VectorGraphics.Rendering;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Core.Tools
{
    public class SelectTool : Tool
    {
        // State for dragging control points or elements
        private IDrawElement? _selectedElement;
        private int _selectedControlPointIndex = -1;
        private ICommand? _activeDragCommand;
        private bool _isDraggingControlPoint = false;

        // State for potential panning - ONLY when clicking empty space
        private Vector2D? _panStartPixelPoint;
        private bool _isCheckingForPan = false;
        private bool _clickedOnElement = false; // NEW: Track if we clicked an element

        private const float SELECTION_TOLERANCE_PIXELS = 12.0f; // Increased for easier selection
        private const float PAN_START_TOLERANCE_PIXELS = 5.0f;

        // NEW: Hover state for visual feedback
        private IDrawElement? _hoveredElement;
        private int _hoveredControlPointIndex = -1;

        #region Tool Metadata
        public override string Name => "Select Tool";
        public override string Description => "Select nearby elements";
        public override Cursor Cursor
        {
            get
            {
                // Show different cursors based on state
                if (_isDraggingControlPoint) return Cursors.SizeAll;
                if (_hoveredControlPointIndex >= 0) return Cursors.Cross;
                if (_hoveredElement != null) return Cursors.Hand;
                if (_isCheckingForPan) return Cursors.SizeAll;
                return Cursors.Default;
            }
        }
        public override bool RequiresActiveLayer => true;
        #endregion

        public override InvalidationLevel OnMouseDown(MouseEventArgs e, VectorDocument document)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Convert mouse coordinates to world coordinates
                Vector3D clickPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                //clickPoint.Z = 0;//this is becuase the document.ViewSettings.PictToReal conversion for z is not always accurate
                // IMPROVED: Calculate tolerance based on inverse of zoom
                // This ensures consistent pixel-based tolerance regardless of zoom level
                float tolerance = SELECTION_TOLERANCE_PIXELS / document.ViewSettings.ZoomFactorAverage;

                Vector2D clickPixel = new Vector2D(e.X, e.Y);

                // Reset state
                _clickedOnElement = false;
                _isCheckingForPan = false;
                _panStartPixelPoint = null;

                // First: Check if clicking a control point of the *currently selected* element
                if (_selectedElement != null &&
                    _selectedElement.TryGetControlPointAt(clickPoint, tolerance, out int pointIndex))
                {
                    // Start dragging control point — capture OLD state NOW
                    _selectedControlPointIndex = pointIndex;
                    _activeDragCommand = CreateControlPointCommand(_selectedElement, pointIndex, document);
                    _isDraggingControlPoint = true;
                    _clickedOnElement = true; // Prevent pan checking
                    return InvalidationLevel.None;
                }

                // Try to select a *new* element
                IDrawElement clickedElement = document.Layers.FindElementAtPoint(clickPoint, tolerance);

                if (clickedElement != null)
                {
                    _clickedOnElement = true; // We clicked an element, don't check for pan

                    // If same element but no CP hit → just keep it selected
                    if (clickedElement == _selectedElement)
                    {
                        _selectedControlPointIndex = -1;
                    }
                    else
                    {
                        // New element → full selection
                        SelectElement(clickedElement, document);
                        _selectedControlPointIndex = -1;
                                return InvalidationLevel.Scene ;
            }
                }
                else
                {
                    // Empty space → START checking for pan gesture
                    // Don't clear selection yet - only after we confirm it's not a pan
                    _panStartPixelPoint = clickPixel;
                    _isCheckingForPan = true;
                }
            }
            return InvalidationLevel.None;
        }

        public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
        {
            Vector2D currentPixel = new Vector2D(e.X, e.Y);
            InvalidationLevel invalidation = InvalidationLevel.None;

            // Handle checking for pan gesture (ONLY if we clicked empty space)
            if (_isCheckingForPan && _panStartPixelPoint.HasValue && !_clickedOnElement)
            {
                float distanceSquared = (currentPixel - _panStartPixelPoint.Value).LengthSquared;
                float toleranceSquared = PAN_START_TOLERANCE_PIXELS * PAN_START_TOLERANCE_PIXELS;

                // If moved beyond tolerance, activate PanTool
                if (distanceSquared > toleranceSquared)
                {
                    // Deactivate this SelectTool
                    OnDeactivate(document);
                    _requestedToolForSwitch = new PanTool();
                    return InvalidationLevel.None;
                }
            }

            // Handle dragging control point (if applicable)
            if (_isDraggingControlPoint && _selectedElement != null)
            {
                Vector3D newPt = document.ViewSettings.PictToReal(currentPixel);

                // Mutate immediately for visual feedback
                _selectedElement.MoveControlPoint(_selectedControlPointIndex, newPt);

                // Update command's target value (for final commit)
                if (_activeDragCommand is PropertyCommand<Vector3D> cmd)
                {
                    cmd.UpdateNewValue(newPt);
                }

                return InvalidationLevel.View;
            }



            return invalidation;

            // NEW: Hover detection for visual feedback (when not dragging)
            if (!_isDraggingControlPoint && !_isCheckingForPan)
            {
                Vector3D hoverPoint = document.ViewSettings.PictToReal(currentPixel);
                float tolerance = SELECTION_TOLERANCE_PIXELS / document.ViewSettings.ZoomFactorAverage;

                IDrawElement? previousHovered = _hoveredElement;
                int previousHoveredCP = _hoveredControlPointIndex;

                // Check control points first (if element is selected)
                _hoveredControlPointIndex = -1;
                if (_selectedElement != null &&
                    _selectedElement.TryGetControlPointAt(hoverPoint, tolerance, out int cpIndex))
                {
                    _hoveredControlPointIndex = cpIndex;
                    _hoveredElement = _selectedElement;
                }
                else
                {
                    // Check for element hover
                    _hoveredElement = document.Layers.FindElementAtPoint(hoverPoint, tolerance);
                }

                // Request redraw if hover state changed
                if (_hoveredElement != previousHovered || _hoveredControlPointIndex != previousHoveredCP)
                {
                    invalidation = InvalidationLevel.View;
                }
            }

            return invalidation;
        }

        // --- Property/Method for Control to Check Tool Switch Request ---
        private Tool? _requestedToolForSwitch = null;
        public Tool? GetRequestedToolSwitch()
        {
            var requested = _requestedToolForSwitch;
            _requestedToolForSwitch = null;
            return requested;
        }

        public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document)
        {
            // If we were checking for pan and didn't move enough, it was a click on empty space
            if (_isCheckingForPan && !_clickedOnElement)
            {
                // Clear selection now (it was a click, not a drag)
                ClearSelection(document);
            }

            // Clear the pan check state
            _isCheckingForPan = false;
            _panStartPixelPoint = null;
            _clickedOnElement = false;

            // Handle control point drag completion
            if (_isDraggingControlPoint)
            {
                if (_activeDragCommand != null)
                {
                    document.UndoRedo.ExecuteCommand(_activeDragCommand);
                }
                _activeDragCommand = null;
                _isDraggingControlPoint = false;
            }
            return InvalidationLevel.None;
        }

        // Helper methods
        private ICommand? CreateControlPointCommand(IDrawElement element, int pointIndex, VectorDocument document)
        {
            if (element == null) return null;

            // Line: Start (0), End (1)
            if (element is LineElement line)
            {
                if (pointIndex == 0)
                    return new PropertyCommand<Vector3D>(
                        line, "StartPoint",
                        () => line.Start,
                        v => line.Start = v,
                        line.Start);
                if (pointIndex == 1)
                    return new PropertyCommand<Vector3D>(
                        line, "EndPoint",
                        () => line.End,
                        v => line.End = v,
                        line.End);
            }
            else if (element is CircleElement circle)
            {
                var points = circle.GetControlPoints();
                if (pointIndex == 0)
                    return new PropertyCommand<Vector3D>(
                        circle, "Center",
                        () => points[0],
                        v => circle.MoveControlPoint(0, v),
                        points[0]);
                if (pointIndex == 1)
                    return new PropertyCommand<Vector3D>(
                        circle, "RadiusPoint",
                        () => points[1],
                        v => circle.MoveControlPoint(1, v),
                        points[1]);
            }
            else if (element is RectangleElement rect)
            {
                var points = rect.GetControlPoints();
                if (pointIndex >= 0 && pointIndex < points.Length)
                    return new PropertyCommand<Vector3D>(
                        rect, $"Corner{pointIndex}",
                        () => points[pointIndex],
                        v => rect.MoveControlPoint(pointIndex, v),
                        points[pointIndex]);
            }
            else if (element is LabelElement label)
            {
                var points = label.GetControlPoints();
                if (pointIndex == 0)
                    return new PropertyCommand<Vector3D>(
                        label, "Position",
                        () => points[0],
                        v => label.MoveControlPoint(0, v),
                        points[0]);
            }

            // Fallback: generic control-point command
            var initialPos = element.GetControlPoints()[pointIndex];
            return new PropertyCommand<Vector3D>(
                element, $"ControlPoint{pointIndex}",
                () => element.GetControlPoints()[pointIndex],
                v => element.MoveControlPoint(pointIndex, v),
                initialPos);
        }

        private void SelectElement(IDrawElement element, VectorDocument document)
        {
            ClearSelection(document);
            _selectedElement = element;
            _selectedElement.IsSelected = true;
        }

        private void ClearSelection(VectorDocument document)
        {
            if (_selectedElement != null)
            {
                _selectedElement.IsSelected = false;
                _selectedElement = null;
                _selectedControlPointIndex = -1;
            }
        }

        public override IDrawElement? GetTemporaryElement() => null;

        // NEW: Method to get hover info for rendering (optional - for drawing hover highlights)
        public IDrawElement? GetHoveredElement() => _hoveredElement;
        public int GetHoveredControlPointIndex() => _hoveredControlPointIndex;
    }
}