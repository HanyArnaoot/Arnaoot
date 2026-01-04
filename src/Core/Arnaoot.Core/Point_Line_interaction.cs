using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arnaoot.Core
{
    public static class Point_Line_interaction
    {
        #region Point/Line interaction
        /// <summary>
        /// Determines whether a given point lies on the line segment defined by two points.
        /// </summary>
        /// <param name="checkedPoint">The point to check.</param>
        /// <param name="lineStart">The starting point of the line segment.</param>
        /// <param name="lineEnd">The ending point of the line segment.</param>
        /// <returns>True if the point lies on the line segment; otherwise, False.</returns>
        private static bool IsPointOnLine(Vector3D checkedPoint, Vector3D lineStart, Vector3D lineEnd)
        {
            float distanceToStart = Vector3D.Distance(checkedPoint, lineStart);
            float distanceToEnd = Vector3D.Distance(checkedPoint, lineEnd);
            float lineLength = Vector3D.Distance(lineStart, lineEnd);

            // Allow for a small floating-point error margin
            const float Epsilon = 0.2F;
            return Math.Abs((distanceToStart + distanceToEnd) - lineLength) < Epsilon;
        }

        /// <summary>
        /// Calculates the perpendicular projection of an outside point onto a line defined by two points.
        /// </summary>
        /// <param name="LineStartPoint">The starting point of the line.</param>
        /// <param name="LineEndPoint">The ending point of the line.</param>
        /// <param name="OutSidePoint">The point from which the perpendicular projection is calculated.</param>
        /// <returns>The perpendicular point on the line; or Nothing if the line has zero length.</returns>
        public static Vector3D CalculatePerpendicularPointToLine(Vector3D LineStartPoint, Vector3D LineEndPoint, Vector3D OutSidePoint)
        {
            // Calculate line direction vector
            Vector3D lineDirection = LineEndPoint - LineStartPoint;

            // Handle degenerate case (same start and end points)
            if (lineDirection.Length < 1e-6f)
            {
                System.Diagnostics.Debugger.Break();
                return new Vector3D();
            }

            // Calculate vector from line start to outside point
            Vector3D toOutsidePoint = OutSidePoint - LineStartPoint;

            // Project the outside point vector onto the line direction
            float projectionLength = Vector3D.Dot(toOutsidePoint, lineDirection) / Vector3D.Dot(lineDirection, lineDirection);

            // Calculate the perpendicular point on the line
            Vector3D perpendicularPoint = LineStartPoint + lineDirection * projectionLength;

            return perpendicularPoint;
        }

        /// <summary>
        /// Calculates the shortest distance from an outside point to a line defined by two points.
        /// </summary>
        /// <param name="LineStartPoint">The starting point of the line.</param>
        /// <param name="LineEndPoint">The ending point of the line.</param>
        /// <param name="OutSidePoint">The point outside the line.</param>
        /// <returns>The minimum distance from the outside point to the line.</returns>
        public static float CalPointToLineDistance(Vector3D LineStartPoint, Vector3D LineEndPoint, Vector3D OutSidePoint)
        {
            //
            Vector3D PerpendicularPoint = CalculatePerpendicularPointToLine(LineStartPoint, LineEndPoint, OutSidePoint);
            float dis1 = Vector3D.Distance(LineStartPoint, OutSidePoint);
            float dis2 = Vector3D.Distance(LineEndPoint, OutSidePoint);
            float dis = Math.Min(dis1, dis2);
            //'
            if (!(PerpendicularPoint == new Vector3D()))
            {
                return Math.Min(dis, Vector3D.Distance(PerpendicularPoint, OutSidePoint)); //SafeDistance * 3
            }
            else
            {
                System.Diagnostics.Debugger.Break();
                return dis;
            }
            //
            if (IsPointOnLine(PerpendicularPoint, LineStartPoint, LineEndPoint))
            {
                System.Diagnostics.Debugger.Break();
                return Vector3D.Distance(PerpendicularPoint, OutSidePoint);
            }
            else
            {
                System.Diagnostics.Debugger.Break();
                return Math.Min(Vector3D.Distance(LineStartPoint, OutSidePoint), Vector3D.Distance(LineEndPoint, OutSidePoint)); //SafeDistance * 3
            }
        }
        #endregion
    }
}
