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
        /// Tool for drawing line elements with real-time preview.
        /// </summary>
        public class DrawLineTool : Tool
        {
            #region Constants
            private const float MIN_LINE_LENGTH = 0.01f; // Minimum line length to create
            #endregion

            #region Fields
            private Vector3D? _startPoint; // World coordinates of the line's start point
            private LineElement? _tempLineElement; // Temporary line element shown during dragging
            private bool _isDrawing; // Explicit state tracking
            #endregion

            #region Tool Metadata
            public override string Name => "Line Tool";
            public override string Description => "Draw straight lines between two points";
            public override Cursor Cursor => Cursors.Cross;
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
                // Only handle left mouse button
                if (e.Button != MouseButtons.Left)
                return InvalidationLevel.None;
            // Validate state
            if (!CanActivate(document))
                {
                    System.Diagnostics.Debug.WriteLine("Cannot activate line tool: no active unlocked layer");
                return InvalidationLevel.None;
            }

            // Convert mouse coordinates to world coordinates
            _startPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                 _isDrawing = true;

                // Create temporary line element (initially a point)
                _tempLineElement = new LineElement(
                    Start: _startPoint.Value,
                    End: _startPoint.Value,
                    drawWidth: document.Settings?.DefaultStrokeWidth ?? 1,
                    drawColor : document.Settings?.DrawColor ?? Color.Black,
                    relativeCoords :false
                );

                // Clear selection when starting new drawing
                document.Layers.ClearSelection();
            return InvalidationLevel.None;
        }

            public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
            {
                // Only process if we're actively drawing
                if (!_isDrawing || !_startPoint.HasValue || _tempLineElement == null)
                return InvalidationLevel.None;

                // Only process left button drag
                if (e.Button != MouseButtons.Left)
                return InvalidationLevel.None;

                // Convert current mouse position to world coordinates
                Vector3D worldPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));

                // Update temporary line's end point
                _tempLineElement.End = worldPoint;
            //
            return InvalidationLevel.View;
              }

            public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document)
            {
                // Only handle left mouse button release
                if (e.Button != MouseButtons.Left)
                return InvalidationLevel.None;
            // Only process if we were drawing
            if (!_isDrawing || !_startPoint.HasValue || _tempLineElement == null)
                {
                    ResetToolState();
                return InvalidationLevel.None;
            }

                try
                {
                    // Convert final mouse position to world coordinates
                    Vector3D worldPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                    //Vector2D endPoint = new Vector2D(worldPoint.X, worldPoint.Y);

                    // Calculate line length
                    float lineLength = Vector3D.Distance ( _startPoint.Value, worldPoint);

                    // Only create line if it has meaningful length
                    if (lineLength >= MIN_LINE_LENGTH)
                    {
                        // Update final end point
                        _tempLineElement.End = worldPoint;

                        // Create and execute command to add the line
                        ICommand command = new AddRemoveCommand(
                            element: _tempLineElement,
                            layer: document.Layers.ActiveLayer,
                            isAdd: true
                        );

                        document.UndoRedo.ExecuteCommand(command);

                        // Optionally select the newly created element
                        // _tempLineElement.IsSelected = true;

                        OnLineCreated(new LineCreatedEventArgs(_tempLineElement, _startPoint.Value, worldPoint));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Line too short ({lineLength:F3}), not creating");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating line: {ex.Message}");
                    // Consider raising an error event here
                }
                finally
                {
                    // Always reset state after mouse up
                    ResetToolState();
                }
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
                // Clean up any in-progress drawing when tool is deactivated
                ResetToolState();
                base.OnDeactivate(document);
                System.Diagnostics.Debug.WriteLine($"{Name} deactivated");
            }
            #endregion

            #region Tool State
            public override IDrawElement? GetTemporaryElement() => _tempLineElement;

            public bool IsDrawing => _isDrawing;

            private void ResetToolState()
            {
                _startPoint = null;
                _tempLineElement = null;
                _isDrawing = false;
            }
            #endregion

 

            #region Events
            public event EventHandler<LineCreatedEventArgs>? LineCreated;

            protected virtual void OnLineCreated(LineCreatedEventArgs e)
            {
                LineCreated?.Invoke(this, e);
            }
            #endregion
        }

        #region Event Args
        /// <summary>
        /// Event arguments for when a line is successfully created.
        /// </summary>
        public class LineCreatedEventArgs : EventArgs
        {
            public IDrawElement Element { get; }
            public Vector2D StartPoint { get; }
            public Vector2D EndPoint { get; }

            public LineCreatedEventArgs(IDrawElement element, Vector2D start, Vector2D end)
            {
                Element = element;
                StartPoint = start;
                EndPoint = end;
            }
        }
        #endregion
    }

