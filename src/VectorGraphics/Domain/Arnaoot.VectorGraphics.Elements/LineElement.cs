using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Elements
{
    #region enum
    public enum LinePointType
        {
            Start,
             End
        }
        #endregion
        #region Line Elemnet

        public class LineElement : DrawElement
        {
            //public Vector3D StartPoint, EndPoint;
            public bool RelativeCoords;
            public int Width;
            public ArgbColor Color;
            //

            /// <summary>
            /// Initializes a new instance of the LineElement class.
            /// </summary>
            /// <param name="Start">The  coordinate of the start point.</param>
            /// <param name="End">The  coordinate of the end point.</param>
            /// <param name="relativeCoords">Indicates if coordinates are relative to the start point.</param>
            /// <param name="drawWidth">The width of the line in pixels.</param>
            /// <param name="drawColor">The color of the line.</param>
            /// <param name="layerNo">The layer number to assign the line to.</param>
            public LineElement(Vector3D Start, Vector3D End, bool relativeCoords, int drawWidth, ArgbColor drawColor) : base()
            {
                this._startPoint = Start;
                this._endPoint = End;
                this.RelativeCoords = relativeCoords;
                this.Width = drawWidth;
                this.Color = drawColor;
            }

            private Vector3D _startPoint;
            private Vector3D _endPoint;

            /// <summary>
            /// The starting coordinate of the line. Setting this value fires ElementBoundsChanged.
            /// </summary>
            public Vector3D Start
            {
                get => _startPoint;
                set
                {
                    if (_startPoint.Equals(value)) return;
                    _startPoint = value;
                    // CRITICAL: Notify the layer that the element's bounds have changed
                    OnElementBoundsChanged();
                }
            }

            /// <summary>
            /// The ending coordinate of the line. Setting this value fires ElementBoundsChanged.
            /// </summary>
            public Vector3D End
            {
                get => _endPoint;
                set
                {
                    if (_endPoint.Equals(value)) return;
                    _endPoint = value;
                    // CRITICAL: Notify the layer that the element's bounds have changed
                    OnElementBoundsChanged();
                }
            }

            public override void EmitCommands(IRenderTarget target, IViewSettings view)
            {
                // Transform to screen space
                Vector2D p1 = view.RealToPict(_startPoint, out _);
                Vector2D p2 = view.RealToPict(_endPoint, out _);

                // Safety: skip NaN/infinite
                if (!p1.IsValid  || !p2.IsValid ) return;

                // Clip in screen space (Liang-Barsky, same as in Draw)
                if (!Arnaoot.VectorGraphics.Core.Clipping.ScreenSpaceClipper.ClipLineScreenSpace(p1, p2, view.UsableViewport, out p1, out p2))
                    return;

                target.DrawLine(p1, p2, Color, Width, IsSelected);
            }



            /// <summary>
            /// Gets the bounding rectangle of the line in real-world coordinates.
            /// </summary>
            public override BoundingBox3D GetBounds()
            {
                if (RelativeCoords)
                {
                    return new BoundingBox3D(_startPoint, _endPoint);
                }
                else
                {
                    float minX = Math.Min(_startPoint.X, _endPoint.X);
                    float minY = Math.Min(_startPoint.Y, _endPoint.Y);
                    float minZ = Math.Min(_startPoint.Z, _endPoint.Z);
                    //
                    float maxX = Math.Max(_startPoint.X, _endPoint.X);
                    float maxY = Math.Max(_startPoint.Y, _endPoint.Y);
                    float maxZ = Math.Max(_startPoint.Z, _endPoint.Z);
                    return new BoundingBox3D(new Vector3D(minX, minY, minZ), new Vector3D(maxX, maxY, maxZ));
                }
            }

            #region element selction point choice
            public override bool HitTest(Vector3D worldPoint, float worldTolerance)
            {
            // Point-to-line-segment distance calculation
            Vector3D lineVec = _endPoint - _startPoint;
            Vector3D pointVec = worldPoint - _startPoint;

            float lineLengthSq = Vector3D.DistanceSquared(_startPoint, _endPoint);

            // Handle degenerate case (zero-length line)
            if (lineLengthSq < 1e-8f)
            {
                return Vector3D.Distance(worldPoint, _startPoint) <= worldTolerance;
            }

            // Project point onto line (parameterized as t ∈ [0,1])
            float t = Vector3D.Dot(pointVec, lineVec) / lineLengthSq;

            // Clamp to segment
            t = Math.Max(0, Math.Min(1, t));

            // Find closest point on segment
            Vector3D closestPoint = _startPoint + lineVec * t;

            // Check distance
            return Vector3D.Distance(worldPoint, closestPoint) <= worldTolerance;
            //float distance = Point_Line_interaction.CalPointToLineDistance(_startPoint, _endPoint, worldPoint);
            //return distance <= worldTolerance;
        }

        public override Vector3D[] GetControlPoints()
            {
                return new[] { _startPoint, _endPoint };
            }

            public override bool TryGetControlPointAt(Vector3D worldPoint, float worldTolerance, out int pointIndex)
            {
                // Check start point first (index 0)
                if (Vector3D.Distance(worldPoint, _startPoint) <= worldTolerance)
                {
                    pointIndex = (int) LinePointType.Start ;
                    return true;
                }

                // Check end point (index 1)
                if (Vector3D.Distance(worldPoint, _endPoint) <= worldTolerance)
                {
                    pointIndex = (int)LinePointType.End;
                    return true;
                }

                pointIndex = -1;
                return false;
            }

            public override void MoveControlPoint(int pointIndex, Vector3D newPosition)
            {
                switch (pointIndex)
                {
                    case 0:
                        Start = newPosition; // Uses property setter which fires OnElementBoundsChanged
                        break;
                    case 1:
                        End = newPosition;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pointIndex),
                            $"LineElement only has 2 control points (0-1), got {pointIndex}");
                }
            }


            #endregion 
        }
        #endregion
    }
 
