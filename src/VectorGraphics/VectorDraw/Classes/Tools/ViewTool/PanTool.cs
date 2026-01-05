using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core.Models;
using Arnaoot.VectorGraphics.Rendering;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions; // Assuming VectorDocument is here

namespace Arnaoot.VectorGraphics.Core.Tools
{
    /// <summary>
    /// Tool for panning the view.
    /// Allows the user to click and drag to move the viewport.
    /// </summary>
    public class PanTool : Tool
    {
        #region Fields
        private Vector2D? _startPixelPoint; // Screen coordinates where panning started
        private Vector3D? _startWorldShift; // The ShiftWorld value when panning started
        private bool _isPanning = false;
        #endregion

        #region Tool Metadata
        public override string Name => "Pan Tool";
        public override string Description => "Pan the displayed view";
        public override Cursor Cursor => Cursors.Hand;
        public override bool RequiresActiveLayer => false;
        #endregion

        #region Mouse Event Handlers
        public override InvalidationLevel OnMouseDown(MouseEventArgs e, VectorDocument document)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Store BOTH the starting pixel point AND the current ShiftWorld
                _startPixelPoint = new Vector2D(e.X, e.Y);
                _startWorldShift = document.ViewSettings.ShiftWorld; // ← THIS is what you need!
                _isPanning = true;

                System.Diagnostics.Debug.WriteLine($"Pan started at pixel: {_startPixelPoint.Value}, shift: {_startWorldShift.Value}");
            }
            return InvalidationLevel.None;
        }

        public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
        {
            // Only update view if actively panning
            //if (_isPanning && e.Button == MouseButtons.Left && _startPixelPoint.HasValue && _startWorldShift.HasValue)
            if (_isPanning &&
       (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left &&
       _startPixelPoint.HasValue &&
       _startWorldShift.HasValue)
            {
                // Calculate the pixel delta from the ORIGINAL start point
                Vector2D currentPixelPoint = new Vector2D(e.X, e.Y);
                Vector2D pixelDelta = currentPixelPoint - _startPixelPoint.Value;

                // Convert pixel delta to world delta based on current zoom
                float worldDeltaX = pixelDelta.X / document.ViewSettings.ZoomFactor.X;
                float worldDeltaY = -pixelDelta.Y / document.ViewSettings.ZoomFactor.Y; // Invert Y

                // Calculate new shift by adding delta to the ORIGINAL shift
                Vector3D newShift = new Vector3D(
                    _startWorldShift.Value.X + worldDeltaX,
                    _startWorldShift.Value.Y + worldDeltaY,
                    _startWorldShift.Value.Z
                );

                // Update the document's ViewSettings
                document.ViewSettings = new ViewSettings(
                    document.ViewSettings.UsableViewport,
                    document.ViewSettings.ZoomFactor,
                    newShift,                              // ← New calculated shift
                    document.ViewSettings.RotationAngle,
                    document.ViewSettings.RotateAroundPoint
                );
                // View changed, scene unchanged
                return InvalidationLevel.View;
            }
            return InvalidationLevel.None;
        }

        public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document)
        {
            if (_isPanning && e.Button == MouseButtons.Left)
            {
                System.Diagnostics.Debug.WriteLine($"Pan ended at shift: {document.ViewSettings.ShiftWorld}");
                // Panning finished - ViewSettings already updated
            }
             ResetToolState();
            return InvalidationLevel.None;
        }
        #endregion

        #region Keyboard Event Handlers
        public override InvalidationLevel OnKeyDown(KeyEventArgs e, VectorDocument document)
        {
            base.OnKeyDown(e, document);

            if (e.KeyCode == Keys.Escape && _isPanning)
            {
                // Restore original shift on ESC
                if (_startWorldShift.HasValue)
                {
                    document.ViewSettings = new ViewSettings(
                        document.ViewSettings.UsableViewport,
                        document.ViewSettings.ZoomFactor,
                        _startWorldShift.Value, // ← Restore original
                        document.ViewSettings.RotationAngle,
                        document.ViewSettings.RotateAroundPoint
                    );
                }

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
            ResetToolState();
            base.OnDeactivate(document);
            System.Diagnostics.Debug.WriteLine($"{Name} deactivated");
        }
        #endregion

        #region Tool State
        public override IDrawElement? GetTemporaryElement() => null;

        private void ResetToolState()
        {
            _isPanning = false;
            _startPixelPoint = null;
            _startWorldShift = null; // ← Also clear this
        }
        #endregion
    }
}