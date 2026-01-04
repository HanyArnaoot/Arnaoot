using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Elements
{
    public abstract class DrawElement : IDrawElement
        {
            public bool IsSelected { get; set; }
            //
            public abstract void EmitCommands(IRenderTarget target, IViewSettings view);
            /// <summary>
            /// Gets the bounding rectangle of the element in real-world coordinates.
            /// </summary>
            /// <returns>A RectangleF representing the bounds of the element.</returns>
            public abstract  BoundingBox3D GetBounds();
            public DrawElement()
            {
            }
            // Required implementation of the event
            public event Action<IDrawElement> ElementBoundsChanged;
            /// <summary>
            /// Helper method for derived classes to call when their bounds change.
            /// </summary>
            protected void OnElementBoundsChanged()
            {
                // Invoke the event, passing 'this' (the element that changed)
                ElementBoundsChanged?.Invoke(this);
            }

            //element selection methods 
            public abstract bool HitTest(Vector3D worldPoint, float worldTolerance);
            public abstract Vector3D[] GetControlPoints();
            public abstract bool TryGetControlPointAt(Vector3D worldPoint, float worldTolerance, out int pointIndex);
            public abstract void MoveControlPoint(int pointIndex, Vector3D newPosition);
        }
 
        #region EventArgs
        public class LayerNotInitializedEventArgs : EventArgs
        {
            public int LayerNumber { get; set; }
            public LayerNotInitializedEventArgs(int layerNo)
            {
                LayerNumber = layerNo;
            }
        }
        #endregion
    }



