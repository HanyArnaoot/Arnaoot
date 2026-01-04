

using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core.Models;
using Arnaoot.VectorGraphics.Rendering;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Core.Tools
{
        /// <summary>
        /// Abstract base class for all tools in the vector graphics engine.
        /// Provides lifecycle management, input handling, and state queries.
        /// </summary>
        public abstract class Tool
        {
            #region Tool Metadata
            /// <summary>
            /// Gets the display name of the tool.
            /// </summary>
            public abstract string Name { get; }

            /// <summary>
            /// Gets a brief description of what this tool does.
            /// </summary>
            public abstract string Description { get; }

            /// <summary>
            /// Gets the cursor that should be displayed when this tool is active.
            /// </summary>
            public abstract Cursor Cursor { get; }

            /// <summary>
            /// Indicates whether this tool requires an active layer to function.
            /// Default is true. Override to return false for tools like Pan or Zoom.
            /// </summary>
            public virtual bool RequiresActiveLayer => true;

            /// <summary>
            /// Indicates whether this tool modifies the document.
            /// Used for determining if changes need to be saved.
            /// </summary>
            public virtual bool ModifiesDocument => false;
            #endregion

            #region State Properties
            /// <summary>
            /// Gets whether this tool is currently active.
            /// </summary>
            public bool IsActive { get; private set; }

            /// <summary>
            /// Gets the tool's unique identifier.
            /// By default, uses the type name. Override if you need custom IDs.
            /// </summary>
            public virtual string Id => GetType().Name;
        #endregion
        #region Invalidation Level (New)
        /// <summary>
        /// Gets the invalidation level this tool requests after interaction.
        /// Override to return the appropriate level based on tool state.
        /// </summary>
        public virtual InvalidationLevel GetInvalidationLevelOnInteraction()
        {
            // Default: tools that modify document trigger scene invalidation
            return ModifiesDocument ? InvalidationLevel.Scene : InvalidationLevel.Overlay;
        }
        #endregion
        #region Validation
        /// <summary>
        /// Determines whether this tool can be activated with the given document state.
        /// Override to add custom validation logic.
        /// </summary>
        /// <param name="document">The document to validate against.</param>
        /// <returns>True if the tool can be activated; otherwise, false.</returns>
        public virtual bool CanActivate(VectorDocument document)
            {
                if (document == null)
                    return false;

                // If tool requires active layer, validate it exists and is unlocked
                if (RequiresActiveLayer)
                {
                    return document.Layers?.ActiveLayer != null &&
                           !document.Layers.ActiveLayer.Locked;
                }

                return true;
            }
            #endregion

            #region Mouse Event Handlers
            /// <summary>
            /// Called when a mouse button is pressed down while this tool is active.
            /// </summary>
            /// <param name="e">The mouse event arguments.</param>
            /// <param name="document">The document being interacted with.</param>
            public abstract InvalidationLevel OnMouseDown(MouseEventArgs e, VectorDocument document);

            /// <summary>
            /// Called when the mouse pointer is moved while this tool is active.
            /// </summary>
            /// <param name="e">The mouse event arguments.</param>
            /// <param name="document">The document being interacted with.</param>
            public abstract InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document);

            /// <summary>
            /// Called when a mouse button is released while this tool is active.
            /// </summary>
            /// <param name="e">The mouse event arguments.</param>
            /// <param name="document">The document being interacted with.</param>
            public abstract InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document);

            /// <summary>
            /// Called when the mouse wheel is rotated while this tool is active.
            /// Override this method if the tool needs specific wheel handling.
            /// </summary>
            /// <param name="e">The mouse event arguments.</param>
            /// <param name="document">The document being interacted with.</param>
            public virtual InvalidationLevel OnMouseWheel(MouseEventArgs e, VectorDocument document)
            {
            // Default implementation does nothing. Override if needed.
            return InvalidationLevel.None;
        }

        /// <summary>
        /// Called when the mouse enters the control area.
        /// </summary>
        public virtual InvalidationLevel OnMouseEnter(EventArgs e, VectorDocument document)
            {
            // Default implementation does nothing.
            return InvalidationLevel.None;
        }

        /// <summary>
        /// Called when the mouse leaves the control area.
        /// </summary>
        public virtual InvalidationLevel OnMouseLeave(EventArgs e, VectorDocument document)
            {
            // Default implementation does nothing.
            return InvalidationLevel.None;
        }
        #endregion

        #region Keyboard Event Handlers
        /// <summary>
        /// Called when a key is pressed down while this tool is active.
        /// </summary>
        /// <param name="e">The key event arguments.</param>
        /// <param name="document">The document being interacted with.</param>
        public virtual InvalidationLevel OnKeyDown(KeyEventArgs e, VectorDocument document)
            {
            // Default implementation does nothing. Override if needed.
            return InvalidationLevel.None;
        }

        /// <summary>
        /// Called when a key is released while this tool is active.
        /// </summary>
        /// <param name="e">The key event arguments.</param>
        /// <param name="document">The document being interacted with.</param>
        public virtual InvalidationLevel OnKeyUp(KeyEventArgs e, VectorDocument document)
            {
            // Default implementation does nothing. Override if needed.
            return InvalidationLevel.None;
            }

            /// <summary>
            /// Called when a key is pressed (includes repeat events).
            /// </summary>
            public virtual InvalidationLevel OnKeyPress(KeyPressEventArgs e, VectorDocument document)
            {
            // Default implementation does nothing.
            return InvalidationLevel.None;
         }
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// Called when the tool is activated (becomes the current tool).
        /// Override this method for setup logic when the tool starts being used.
        /// </summary>
        /// <param name="document">The document being interacted with.</param>
        public virtual void OnActivate(VectorDocument document)
            {
                IsActive = true;
                // Default implementation does nothing. Override if needed.
            }

            /// <summary>
            /// Called when the tool is deactivated (another tool becomes active).
            /// Override this method for cleanup logic when the tool stops being used.
            /// </summary>
            /// <param name="document">The document being interacted with.</param>
            public virtual void OnDeactivate(VectorDocument document)
            {
                IsActive = false;
                // Default implementation does nothing. Override if needed.
            }
            #endregion

            #region Rendering Support
            /// <summary>
            /// Gets the temporary element being created or modified by this tool, if any.
            /// Used by the rendering control to display previews.
            /// Must be implemented by each specific tool.
            /// </summary>
            /// <returns>The temporary element, or null if none.</returns>
            public abstract IDrawElement? GetTemporaryElement();

            /// <summary>
            /// Gets additional visual feedback elements (like guides, handles, etc.).
            /// Override to provide custom overlay rendering.
            /// </summary>
            /// <returns>Collection of overlay elements, or empty collection.</returns>
            public virtual IEnumerable<IDrawElement> GetOverlayElements()
            {
                return Enumerable.Empty<IDrawElement>();
            }
            #endregion

            #region Status and Feedback
            /// <summary>
            /// Gets the current status message to display to the user.
            /// Override to provide context-sensitive help or instructions.
            /// </summary>
            public virtual string GetStatusMessage()
            {
                return $"{Name}: {Description}";
            }

            /// <summary>
            /// Gets hints or tips for using this tool.
            /// </summary>
            public virtual string GetHint()
            {
                return string.Empty;
            }
            #endregion

            #region Helper Methods
            /// <summary>
            /// Converts pixel coordinates to world coordinates using the document's view settings.
            /// </summary>
            protected Vector3D PixelToWorld(Vector2D pixel, VectorDocument document)
            {
                return document.ViewSettings.PictToReal(pixel);
            }

            /// <summary>
            /// Converts world coordinates to pixel coordinates using the document's view settings.
            /// </summary>
            protected Vector2D WorldToPixel(Vector3D world, VectorDocument document)
            {
                return document.ViewSettings.RealToPict(world,out float depth);
            }
            #endregion
        }
    }

