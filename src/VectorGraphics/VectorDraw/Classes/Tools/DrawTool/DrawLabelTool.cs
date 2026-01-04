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
    /// Tool for drawing new label elements.
    /// Allows the user to click to place the anchor point of a label.
    /// The user is then prompted for the text content.
    /// </summary>
    public class DrawLabelTool : Tool
    {
        #region Constants
        //private const float MIN_LINE_LENGTH = 0.01f; // Minimum line length to create
        #endregion
        #region Fields
        private Vector3D? _anchorPoint; // World coordinates where the label will be placed
        private IDrawElement? _tempLabelElement; // Temporary label element shown during creation (might just be a marker)
        private bool _isWaitingForText = false; // Flag to track if waiting for user input after click
        #endregion

        #region Tool Metadata
        public override string Name => "Line Tool";
        public override string Description => "Draw straight lines between two points";
        public override Cursor Cursor => Cursors.Cross; // Standard cursor for drawing tools
        public override bool RequiresActiveLayer => true;
        #endregion

        #region Validation

        #endregion
        #region Mouse Event Handlers
            public override InvalidationLevel OnMouseDown(MouseEventArgs e, VectorDocument document)
        {
            if (e.Button == MouseButtons.Left && !_isWaitingForText)
            {
                // Convert mouse coordinates to world coordinates
                _anchorPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));

                 // Optionally, create a temporary visual marker (e.g., a small crosshair or dot) at the anchor point.
                // For simplicity here, we'll assume the temporary element is null until text is entered,
                // or we could create a temporary marker element if desired.
                // For now, let's assume no persistent temporary element during text input phase.
                // _tempLabelElement = CreateTemporaryMarker(worldPoint); // Example helper
                _tempLabelElement = null; // Or create a temporary marker element if desired
                _isWaitingForText = true; // Set the waiting flag

                // Clear any existing selection when starting to draw
                document.Layers.ClearSelection(); // Assuming this method exists on ILayerManager

                // Prompt the user for text (this is the key difference from geometric shapes)
                // This might involve showing a dialog or using an input field overlay.
                // For now, let's assume a simple modal dialog prompt.
                string labelText = PromptUserForText(); // Implement this helper method

                if (!string.IsNullOrEmpty(labelText))
                {
                    // Create the final label element with the provided text and anchor point
                    LabelElement finalLabel = new LabelElement(
                        _anchorPoint.Value , // Anchor Point
                        labelText,           // Text Content
                         document.DrawColor,     // Text Color (or use a default/style manager)
                        12               // Font Size (or use a default/style manager)
                     );

                    // Create the command to add the completed label element to the layer
                    ICommand command = new AddRemoveCommand(finalLabel, document.Layers.ActiveLayer, true); // Assuming AddRemoveCommand exists

                    // Execute the command through the document's command manager
                    document.UndoRedo.ExecuteCommand(command);

                    // The command execution should add the element to the layer.
                    // Optionally, select the newly created element
                    // finalLabel.IsSelected = true; // Depends on your selection logic after creation
                }
                // else: if the user cancelled or entered empty text, no label is created.

                // Reset the state regardless of whether a label was created
                _isWaitingForText = false;
                _anchorPoint = null;
                _tempLabelElement = null; // Clear temporary marker if one existed
            }
            return InvalidationLevel.None;
        }

        public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
        {
            // For DrawLabelTool, OnMouseMove might not have specific logic during the drawing/waiting phase,
            // unless you want to show a preview or update a temporary marker.
            // If showing a temporary marker, update its position here if _isWaitingForText.
            if (_isWaitingForText && _tempLabelElement != null)
            {
                // Example: Update a temporary marker's position if one exists
                // Vector3D worldPoint = document.ViewSettings.PictToReal(new Vector2D(e.X, e.Y));
                // UpdateMarkerPosition(_tempLabelElement, worldPoint); // Example helper
                return InvalidationLevel.View;
            }
            return InvalidationLevel.None;
            // Otherwise, do nothing during mouse move while waiting for text.
        }

        public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document)
        {
            // OnMouseUp is handled implicitly by OnMouseDown finishing the operation
            // after the text prompt dialog closes.
            // No specific logic needed here beyond the general tool deactivation rules,
            // which are handled by the control's mouse event flow.
            // The state reset happens in OnMouseDown after the prompt.
            return InvalidationLevel.None;
        }

        #endregion
        #region Keyboard Event Handlers
        public override InvalidationLevel OnKeyDown(KeyEventArgs e, VectorDocument document)
        {
            base.OnKeyDown(e, document);

            // Allow ESC to cancel current drawing operation
            if (e.KeyCode == Keys.Escape && _isWaitingForText)
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
            // This is crucial if the text prompt dialog is open when the tool is deactivated.
            _isWaitingForText = false; // Cancel any pending text input action
            _anchorPoint = null;
            _tempLabelElement = null;
            base.OnDeactivate(document);
        }
        #endregion
        #region Tool State
        private void ResetToolState()
        {
         _anchorPoint = null; // World coordinates where the label will be placed
        _tempLabelElement = null; // Temporary label element shown during creation (might just be a marker)
        _isWaitingForText = false; // Flag to track if waiting for user input after click

        }
           // Returns the temporary marker element if waiting for text, otherwise null.
        // This allows the rendering system to draw a visual cue (like a crosshair) where the label will be placed.
        public override IDrawElement? GetTemporaryElement()
        {
             if (_isWaitingForText && _anchorPoint.HasValue)
            {
                 return null; // Or return a specific temporary marker element if implemented.
            }
            return null;
        }
     #endregion


        // --- Implement abstract GetTemporaryElement method ---
        // --- END NEW ---


        // --- Helper Method: Prompt User for Text ---
        // This is a placeholder. You'll need to implement the actual UI for text input.
        // This could be a simple MessageBox.Show, a custom form, or an input overlay.
        private string PromptUserForText()
        {
            // Example using a simple input dialog (you might need to create this or use a library)
            // System.Windows.Forms.DialogResult result = InputBox.Show("Enter Label Text", "Label Text:", out string inputText);
            // if (result == System.Windows.Forms.DialogResult.OK)
            //     return inputText;
            // return string.Empty;

            // Or using a custom form:
            using (var form = new Form())
            {
                form.Width = 300;
                form.Height = 100;
                form.Text = "Enter Label Text";

                var textBox = new TextBox() { Left = 10, Top = 10, Width = 260 };
                var buttonOk = new Button() { Text = "OK", Left = 10, Width = 75, Top = 35 };
                var buttonCancel = new Button() { Text = "Cancel", Left = 90, Width = 75, Top = 35 };

                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);

                var dialogResult = DialogResult.None;
                buttonOk.Click += (sender, e) => { dialogResult = DialogResult.OK; form.Close(); };
                buttonCancel.Click += (sender, e) => { dialogResult = DialogResult.Cancel; form.Close(); };

                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                form.ShowDialog();

                return dialogResult == DialogResult.OK ? textBox.Text : string.Empty;
            }
        }
        // --- END Helper Method ---
    }
}
