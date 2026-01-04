using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Arnaoot.VectorGraphics.Abstractions
{
    public partial class Abstractions
    {
        /// <summary>
        /// Defines the contract for drawable elements within the VectorGraphicsEngine.
        /// </summary>
        public interface IDrawElement
        {

            /// <summary>
            /// Gets the bounding rectangle of the element in real-world coordinates.
            /// </summary>
            /// <returns>A BoundingBox3D representing the bounds of the element.</returns>
            BoundingBox3D GetBounds(); // Returns real-world bounds
            /// <summary>
            /// Gets or sets if element is selected.
            /// </summary>
            bool IsSelected { get; set; }
            /// <summary>
            /// Event fired when the element's position or size changes, 
            /// indicating the layer's spatial index and bounds need updating.
            /// </summary>
            event Action<IDrawElement> ElementBoundsChanged;
            //



            /// <summary>
            /// Tests if a point in world coordinates is near enough to select this element.
            /// </summary>
            /// <param name="worldPoint">Point to test in world/real coordinates</param>
            /// <param name="worldTolerance">Selection tolerance in world units</param>
            /// <returns>True if the point is within tolerance of the element</returns>
            bool HitTest(Vector3D worldPoint, float worldTolerance);

            /// <summary>
            /// Gets all editable control points of this element in world coordinates.
            /// </summary>
            /// <returns>Array of control points (e.g., start/end for lines, corners for rectangles)</returns>
            Vector3D[] GetControlPoints();

            /// <summary>
            /// Tests if a point is near a control point and returns its index.
            /// </summary>
            /// <param name="worldPoint">Point to test</param>
            /// <param name="worldTolerance">Selection tolerance</param>
            /// <param name="pointIndex">Index of the control point if found</param>
            /// <returns>True if a control point was found within tolerance</returns>
            bool TryGetControlPointAt(Vector3D worldPoint, float worldTolerance, out int pointIndex);

            /// <summary>
            /// Moves a control point to a new position.
            /// </summary>
            /// <param name="pointIndex">Index of the control point to move</param>
            /// <param name="newPosition">New position in world coordinates</param>
            void MoveControlPoint(int pointIndex, Vector3D newPosition);

            /// <summary>
            /// Emits rendering commands for this element.
            /// Clipping and culling should be done BEFORE calling this.
            /// </summary>
            void EmitCommands(IRenderTarget target,  IViewSettings view);
        }
    }
}
