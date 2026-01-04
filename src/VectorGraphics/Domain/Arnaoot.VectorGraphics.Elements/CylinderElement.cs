using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Elements
{ 
         public class CylinderElement :  DrawElement
        {
            public float Radius;
            public bool FixedRadius;
            public int DrawWidth;
            public  ArgbColor  DrawColor;
            public  ArgbColor FillColor;
            public bool Filled;
            public bool _drawEndCaps; // Whether to draw the circular end caps
            public int DetailLevel; // Number of sides for the cylinder (8, 16, 32, etc.)
                                    //
                                    //
            private  Vector3D _startCenter;
            private  Vector3D _endCenter;

            /// <summary>
            /// The starting coordinate of the line. Setting this value fires ElementBoundsChanged.
            /// </summary>
            public  Vector3D StartCenter
            {
                get => _startCenter;
                set
                {
                    if (_startCenter.Equals(value)) return;
                    _startCenter = value;
                    // CRITICAL: Notify the layer that the element's bounds have changed
                    OnElementBoundsChanged();
                }
            }

            /// <summary>
            /// The ending coordinate of the line. Setting this value fires ElementBoundsChanged.
            /// </summary>
            public  Vector3D EndCenter
            {
                get => _endCenter;
                set
                {
                    if (_endCenter.Equals(value)) return;
                    _endCenter = value;
                    // CRITICAL: Notify the layer that the element's bounds have changed
                    OnElementBoundsChanged();
                }
            }

            /// <summary>
            /// Initializes a new instance of the CylinderElement class.
            /// </summary>
            public CylinderElement(Vector3D startCenter, Vector3D endCenter,
                                  float radius, bool fixedRadius, int drawWidth,  ArgbColor drawColor,
                                  ArgbColor fillColor, bool filled, bool drawEndCaps, int layerNo,
                                  int detailLevel = 16) : base()
            {
                this._startCenter = startCenter;
                this._endCenter = endCenter;
                this.Radius = radius;
                this.FixedRadius = fixedRadius;
                this.DrawWidth = drawWidth;
                this.DrawColor = drawColor;
                this.FillColor = fillColor;
                this.Filled = filled;
                this._drawEndCaps = drawEndCaps;
                this.DetailLevel = Math.Max(4, detailLevel); // Minimum 4 sides
                                                             //

                if (radius < 0F)
                {
                    throw new ArgumentException("Radius cannot be negative.");
                }

                if (startCenter == endCenter)
                {
                    throw new ArgumentException("Start and end centers cannot be the same point.");
                }
            }



            /// <summary>
            /// Gets the bounding box of the cylinder in real-world coordinates.
            /// </summary>
            public override BoundingBox3D GetBounds()
            {
                var (axis, u, v) = GetCylinderVectors();

                // Calculate extents in each axis direction
                float xExtent = Math.Abs(u.X * Radius) + Math.Abs(v.X * Radius);
                float yExtent = Math.Abs(u.Y * Radius) + Math.Abs(v.Y * Radius);
                float zExtent = Math.Abs(u.Z * Radius) + Math.Abs(v.Z * Radius);

                // Get the overall bounds including both end points
                float minX = Math.Min(_startCenter.X - xExtent, _endCenter.X - xExtent);
                float maxX = Math.Max(_startCenter.X + xExtent, _endCenter.X + xExtent);
                float minY = Math.Min(_startCenter.Y - yExtent, _endCenter.Y + yExtent);
                float maxY = Math.Max(_startCenter.Y + yExtent, _endCenter.Y + yExtent);
                float minZ = Math.Min(_startCenter.Z - zExtent, _endCenter.Z - zExtent);
                float maxZ = Math.Max(_startCenter.Z + zExtent, _endCenter.Z + zExtent);

                return new BoundingBox3D(
                    new Vector3D(minX, minY, minZ),
                    new Vector3D(maxX, maxY, maxZ)
                );
            }

            #region HitTest




            public override bool HitTest(Vector3D worldPoint, float worldTolerance)
            {
                // Calculate distance from point to the cylinder's axis (line segment from start to end)
                Vector3D axis = _endCenter - _startCenter;
                Vector3D pointVec = worldPoint - _startCenter;

                float axisLengthSq = Vector3D.DistanceSquared(_startCenter, _endCenter);

                // Handle degenerate case (zero-length cylinder)
                if (axisLengthSq < 1e-8f)
                {
                    // Treat as a sphere at start position
                    float distToCenter = Vector3D.Distance(worldPoint, _startCenter);
                    return Math.Abs(distToCenter - Radius) <= worldTolerance;
                }

                // Project point onto cylinder axis (parameterized as t ∈ [0,1])
                float t = Vector3D.Dot(pointVec, axis) / axisLengthSq;

                // Clamp to cylinder length
                t = Math.Max(0, Math.Min(1, t));

                // Find closest point on axis
                Vector3D closestPointOnAxis = _startCenter + axis * t;

                // Calculate perpendicular distance to axis
                float perpDistance = Vector3D.Distance(worldPoint, closestPointOnAxis);

                if (Filled)
                {
                    // Area-based selection: inside the cylinder volume
                    return perpDistance <= (Radius + worldTolerance);
                }
                else
                {
                    // Surface-based selection: near the cylinder's surface
                    float distToSurface = Math.Abs(perpDistance - Radius);
                    return distToSurface <= worldTolerance;
                }
            }

            public override Vector3D[] GetControlPoints()
            {
                // Control points:
                // - Start center (index 0)
                // - End center (index 1)
                // - Radius handle at start (index 2)
                // - Radius handle at end (index 3)

                // Calculate perpendicular vector for radius handles
                Vector3D axis = _endCenter - _startCenter;
                Vector3D perpendicular = GetPerpendicularVector(axis);
                Vector3D radiusOffset = Vector3D.Normalize(perpendicular) * Radius;

                return new[]
                {
            _startCenter,                      // Index 0: Start center
            _endCenter,                        // Index 1: End center
            _startCenter + radiusOffset,       // Index 2: Radius handle at start
            _endCenter + radiusOffset          // Index 3: Radius handle at end
        };
            }

            public override bool TryGetControlPointAt(Vector3D worldPoint, float worldTolerance, out int pointIndex)
            {
                var controlPoints = GetControlPoints();

                // Check control points in priority order: centers first, then radius handles
                int[] checkOrder = { 0, 1, 2, 3 };

                foreach (int i in checkOrder)
                {
                    if (Vector3D.Distance(worldPoint, controlPoints[i]) <= worldTolerance)
                    {
                        pointIndex = i;
                        return true;
                    }
                }

                pointIndex = -1;
                return false;
            }

            public override void MoveControlPoint(int pointIndex, Vector3D newPosition)
            {
                switch (pointIndex)
                {
                    case 0:
                        // Move start center
                        StartCenter = newPosition;
                        break;

                    case 1:
                        // Move end center
                        EndCenter = newPosition;
                        break;

                    case 2:
                        // Move radius handle at start - recalculate radius
                        if (!FixedRadius)
                        {
                            Vector3D axis = _endCenter - _startCenter;
                            Vector3D toHandle = newPosition - _startCenter;

                            // Calculate perpendicular distance (new radius)
                            float axisLengthSq = axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z;
                            if (axisLengthSq > 1e-8f)
                            {
                                float projection = Vector3D.Dot(toHandle, axis) / axisLengthSq;
                                Vector3D projectedPoint = _startCenter + axis * projection;
                                float newRadius = Vector3D.Distance(newPosition, projectedPoint);

                                if (newRadius > 0.001f) // Prevent zero/negative radius
                                {
                                    Radius = newRadius;
                                    OnElementBoundsChanged();
                                }
                            }
                        }
                        break;

                    case 3:
                        // Move radius handle at end - recalculate radius
                        if (!FixedRadius)
                        {
                            Vector3D axis = _endCenter - _startCenter;
                            Vector3D toHandle = newPosition - _endCenter;

                            // Calculate perpendicular distance (new radius)
                            float axisLengthSq = axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z;
                            if (axisLengthSq > 1e-8f)
                            {
                                float projection = Vector3D.Dot(toHandle, axis) / axisLengthSq;
                                Vector3D projectedPoint = _endCenter + axis * projection;
                                float newRadius = Vector3D.Distance(newPosition, projectedPoint);

                                if (newRadius > 0.001f)
                                {
                                    Radius = newRadius;
                                    OnElementBoundsChanged();
                                }
                            }
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(pointIndex),
                            $"CylinderElement has 4 control points (0-3), got {pointIndex}");
                }
            }
            #endregion

 


            #region Cylinder draw



            /// <summary>
            /// Gets the cylinder axis and two orthogonal vectors for the circular cross-section
            /// </summary>
            private (Vector3D axis, Vector3D u, Vector3D v) GetCylinderVectors()
            {
                Vector3D axis = (_endCenter - _startCenter).Normalized;

                // Find a vector that's not parallel to the axis
                Vector3D temp = Math.Abs(axis.X) < 0.9f ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);

                // Create two orthogonal vectors perpendicular to the axis
                Vector3D u = Vector3D.Cross(axis, temp).Normalized;
                Vector3D v = Vector3D.Cross(axis, u).Normalized;

                return (axis, u, v);
            }

            /// <summary>
            /// Generates the vertices for the cylinder mesh
            /// </summary>
            private (Vector3D[] startCircle, Vector3D[] endCircle) GenerateCylinderVertices()
            {
                var (axis, u, v) = GetCylinderVectors();

                Vector3D[] _startCircle = new Vector3D[DetailLevel];
                Vector3D[] _endCircle = new Vector3D[DetailLevel];

                for (int i = 0; i < DetailLevel; i++)
                {
                    float angle = (float)(2 * Math.PI * i / DetailLevel);
                    float cosAngle = (float)Math.Cos(angle);
                    float sinAngle = (float)Math.Sin(angle);

                    Vector3D offset = u * (Radius * cosAngle) + v * (Radius * sinAngle);

                    _startCircle[i] = _startCenter + offset;
                    _endCircle[i] = _endCenter + offset;
                }

                return (_startCircle, _endCircle);
            }

            /// <summary>
            /// Draws the cylindrical surface as a mesh of triangles or quads
            /// </summary>
            private void DrawCylinderSurface(IRenderTarget target, IViewSettings UsedViewSetting)
            {
                var (startCircle, endCircle) = GenerateCylinderVertices();

                // Project all vertices to screen coordinates
                Vector2D[] startProjected = new Vector2D[DetailLevel];
                Vector2D[] endProjected = new Vector2D[DetailLevel];
                float[] startDepths = new float[DetailLevel];
                float[] endDepths = new float[DetailLevel];

                for (int i = 0; i < DetailLevel; i++)
                {
                    startProjected[i] = UsedViewSetting.RealToPict(startCircle[i], out startDepths[i]);
                    endProjected[i] = UsedViewSetting.RealToPict(endCircle[i], out endDepths[i]);
                    startProjected[i] = UsedViewSetting.ClampToGdiRangePoint(startProjected[i]);
                    endProjected[i] = UsedViewSetting.ClampToGdiRangePoint(endProjected[i]);
                }

                // Draw cylinder surface as quads (or triangles)
                for (int i = 0; i < DetailLevel; i++)
                {
                    int nextI = (i + 1) % DetailLevel;

                    // Create quad: startCircle[i] -> startCircle[nextI] -> endCircle[nextI] -> endCircle[i]
                    Vector2D[] quad = new Vector2D[]
                    {
                                startProjected[i],
                                startProjected[nextI],
                                endProjected[nextI],
                                endProjected[i]
                    };

                    // Check if all points are valid
                    bool validQuad = true;
                    foreach (Vector2D point in quad)
                    {
                        if (float.IsNaN(point.X) || float.IsNaN(point.Y))
                        {
                            validQuad = false;
                            break;
                        }
                    }
                    //
                    if (validQuad)
                    {
                        target.DrawPolygon(quad, ArgbColor.Transparent, 0, FillColor);
                        // Draw quad outline
                        target.DrawPolygon(quad, DrawColor, DrawWidth, null);
                                          }
                }
            }

            /// <summary>
            /// Draws the end caps of the cylinder with proper hidden surface handling
            /// </summary>
            private void DrawEndCaps(IRenderTarget target, IViewSettings UsedViewSetting)
            {
                if (!_drawEndCaps) return;

                var (axis, u, v) = GetCylinderVectors();

                // Get view direction to determine which caps are visible
                Vector3D viewDirection = UsedViewSetting.CurrentViewDirection; // You may need to add this method

                // Calculate which end caps should be visible based on viewing angle
                Vector3D cylinderAxis = (_endCenter - _startCenter).Normalized;

                // Draw start cap (check if it's facing the viewer)
                float startCapDot = Vector3D.Dot(0 - cylinderAxis, viewDirection);
                if (startCapDot > 0) // Cap is facing viewer
                {
                    DrawVisibleEndCap(target, UsedViewSetting, _startCenter, 0 - cylinderAxis, u, v, true);
                }

                // Draw end cap (check if it's facing the viewer)
                float endCapDot = Vector3D.Dot(cylinderAxis, viewDirection);
                if (endCapDot > 0) // Cap is facing viewer
                {
                    DrawVisibleEndCap(target, UsedViewSetting, _endCenter, cylinderAxis, u, v, false);
                }
            }

            /// <summary>
            /// Draws a single circular end cap with proper visibility culling
            /// </summary>
            private void DrawVisibleEndCap(IRenderTarget target, IViewSettings UsedViewSetting,
                                           Vector3D center, Vector3D normal, Vector3D u, Vector3D v, bool isStartCap)
            {
                // Generate circle points
                Vector3D[] circlePoints = new Vector3D[DetailLevel];
                for (int i = 0; i < DetailLevel; i++)
                {
                    float angle = (float)(2 * Math.PI * i / DetailLevel);
                    float cosAngle = (float)Math.Cos(angle);
                    float sinAngle = (float)Math.Sin(angle);

                    circlePoints[i] = center + u * (Radius * cosAngle) + v * (Radius * sinAngle);
                }
                // Project to screen and get depths
                Vector2D[] projectedPoints = new Vector2D[DetailLevel];
                float[] depths = new float[DetailLevel];
                //
                for (int i = 0; i < DetailLevel; i++)
                {
                    projectedPoints[i] = UsedViewSetting.RealToPict(circlePoints[i], out depths[i]);
                    projectedPoints[i] = UsedViewSetting.ClampToGdiRangePoint(projectedPoints[i]);
                }
                // Now we need to clip the end cap based on which parts are visible
                // For simplicity, we'll use a different approach: only draw segments that are on the visible side
                Vector3D viewDirection = UsedViewSetting.CurrentViewDirection;
                List<Vector2D> visiblePoints = new List<Vector2D>();
                //
                for (int i = 0; i < DetailLevel; i++)
                {
                    // Calculate the normal at this point on the cylinder surface
                    Vector3D pointOnCircle = circlePoints[i];
                    Vector3D radialDirection = (pointOnCircle - center).Normalized;

                    // Check if this part of the rim is visible (not hidden by the cylinder surface)
                    float visibility = Vector3D.Dot(radialDirection, viewDirection);

                    if (visibility > 0) // This part of the rim is visible
                    {
                        if (!float.IsNaN(projectedPoints[i].X) && !float.IsNaN(projectedPoints[i].Y))
                        {
                            visiblePoints.Add(projectedPoints[i]);
                        }
                    }
                }
                //
                // If we have enough visible points, draw the cap
                if (visiblePoints.Count >= 3)
                {
                    // For filled caps, we need to be more sophisticated
                    if (Filled)
                    {
                        // Create a more complex polygon that represents only the visible part
                        DrawPartialEndCap(target, UsedViewSetting, center, normal, u, v, viewDirection);
                    }
                    else
                    {
                        // For wireframe, just draw the visible rim segments
                        DrawVisibleRimSegments(target, UsedViewSetting, center, u, v, viewDirection);
                    }
                }
            }
            /// <summary>
            /// Draws only the visible rim segments of the end cap
            /// </summary>
            private void DrawVisibleRimSegments(IRenderTarget target, IViewSettings UsedViewSetting,
                                                Vector3D center, Vector3D u, Vector3D v, Vector3D viewDirection)
            {
                Vector3D[] circlePoints = new Vector3D[DetailLevel];
                Vector2D[] projectedPoints = new Vector2D[DetailLevel];
                bool[] isVisible = new bool[DetailLevel];

                // Generate points and check visibility
                for (int i = 0; i < DetailLevel; i++)
                {
                    float angle = (float)(2 * Math.PI * i / DetailLevel);
                    float cosAngle = (float)Math.Cos(angle);
                    float sinAngle = (float)Math.Sin(angle);

                    circlePoints[i] = center + u * (Radius * cosAngle) + v * (Radius * sinAngle);
                    projectedPoints[i] = UsedViewSetting.RealToPict(circlePoints[i], out _);
                    projectedPoints[i] = UsedViewSetting.ClampToGdiRangePoint(projectedPoints[i]);

                    // Check if this point is on the visible side
                    Vector3D radialDirection = (circlePoints[i] - center).Normalized;
                    isVisible[i] = Vector3D.Dot(radialDirection, viewDirection) > 0;
                }

                // Draw only the visible segments
                for (int i = 0; i < DetailLevel; i++)
                {
                    int nextI = (i + 1) % DetailLevel;

                    // Draw segment only if both endpoints are visible
                    if (isVisible[i] && isVisible[nextI])
                    {
                        if (!float.IsNaN(projectedPoints[i].X) && !float.IsNaN(projectedPoints[i].Y) &&
                            !float.IsNaN(projectedPoints[nextI].X) && !float.IsNaN(projectedPoints[nextI].Y))
                        {
                            target.DrawLine(projectedPoints[i], projectedPoints[nextI], DrawColor, DrawWidth, false);
                        }
                    }
                }
            }

            /// <summary>
            /// Draws the visible part of a filled end cap
            /// </summary>
            private void DrawPartialEndCap(IRenderTarget target, IViewSettings UsedViewSetting,
                                           Vector3D center, Vector3D normal, Vector3D u, Vector3D v, Vector3D viewDirection)
            {
                // This is more complex - we need to create a polygon that represents only the visible part
                // For now, let's use a simpler approach: draw triangular segments from center to visible rim

                Vector3D[] circlePoints = new Vector3D[DetailLevel];
                Vector2D[] projectedPoints = new Vector2D[DetailLevel];
                bool[] isVisible = new bool[DetailLevel];

                Vector2D centerProjected = UsedViewSetting.RealToPict(center, out _);
                centerProjected = UsedViewSetting.ClampToGdiRangePoint(centerProjected);

                // Generate points and check visibility
                for (int i = 0; i < DetailLevel; i++)
                {
                    float angle = (float)(2 * Math.PI * i / DetailLevel);
                    float cosAngle = (float)Math.Cos(angle);
                    float sinAngle = (float)Math.Sin(angle);

                    circlePoints[i] = center + u * (Radius * cosAngle) + v * (Radius * sinAngle);
                    projectedPoints[i] = UsedViewSetting.RealToPict(circlePoints[i], out _);
                    projectedPoints[i] = UsedViewSetting.ClampToGdiRangePoint(projectedPoints[i]);

                    Vector3D radialDirection = (circlePoints[i] - center).Normalized;
                    isVisible[i] = Vector3D.Dot(radialDirection, viewDirection) > 0;
                }

                // Draw triangular segments for visible parts
                for (int i = 0; i < DetailLevel; i++)
                {
                    int nextI = (i + 1) % DetailLevel;

                    if (isVisible[i] && isVisible[nextI])
                    {
                        Vector2D[] triangle = new Vector2D[]
                        {
                    centerProjected,
                    projectedPoints[i],
                    projectedPoints[nextI]
                        };

                        bool validTriangle = true;
                        foreach (var point in triangle)
                        {
                            if (float.IsNaN(point.X) || float.IsNaN(point.Y))
                            {
                                validTriangle = false;
                                break;
                            }
                        }

                        if (validTriangle)
                        {
                            if (Filled)
                            {
                                target.DrawPolygon(triangle, ArgbColor.Transparent, 0, FillColor);
                            }
                            target.DrawPolygon(triangle, DrawColor, DrawWidth, null);
                        }
                    }
                }
            }

       
            private bool TryGetScreenGeometry(
   IViewSettings view,
   out Vector2D bottomCenter,
   out Vector2D topCenter,
   out float radiusPx)
            {
                bottomCenter = view.RealToPict(_endCenter, out _);
                topCenter = view.RealToPict(_startCenter, out _);

                if (!bottomCenter.IsValid || !topCenter.IsValid)
                {
                    radiusPx = 0;
                    return false;
                }

                // Project radius point to get screen-space radius
                Vector2D radiusPt = view.RealToPict(_endCenter + new Vector3D(Radius, 0, 0), out _);
                radiusPx = Math.Max(1f, (radiusPt - bottomCenter).Length);

                // Coarse culling: skip if both centers + radius outside viewport
                var bounds = new Rect2(
                    Math.Min(bottomCenter.X, topCenter.X) - radiusPx,
                    Math.Min(bottomCenter.Y, topCenter.Y) - radiusPx,
                    Math.Abs(bottomCenter.X - topCenter.X) + 2 * radiusPx,
                    Math.Abs(bottomCenter.Y - topCenter.Y) + 2 * radiusPx
                );
                if (!bounds.IntersectsWith(view.UsableViewport))
                {
                    radiusPx = 0;
                    return false;
                }

                return true;
            }
            /// <summary>
            /// Fallback method to draw cylinder as wireframe
            /// </summary>
            private void DrawWireframeCylinder(IRenderTarget target, IViewSettings UsedViewSetting)
            {
                var (startCircle, endCircle) = GenerateCylinderVertices();

                // Draw start and end circles as wireframe
                for (int i = 0; i < DetailLevel; i++)
                {
                    int nextI = (i + 1) % DetailLevel;

                    // Draw circle edges
                    var startA = UsedViewSetting.RealToPict(startCircle[i], out _);
                    var startB = UsedViewSetting.RealToPict(startCircle[nextI], out _);
                    var endA = UsedViewSetting.RealToPict(endCircle[i], out _);
                    var endB = UsedViewSetting.RealToPict(endCircle[nextI], out _);

                    startA = UsedViewSetting.ClampToGdiRangePoint(startA);
                    startB = UsedViewSetting.ClampToGdiRangePoint(startB);
                    endA = UsedViewSetting.ClampToGdiRangePoint(endA);
                    endB = UsedViewSetting.ClampToGdiRangePoint(endB);

                    // Draw circle segments
                    if (!float.IsNaN(startA.X) && !float.IsNaN(startA.Y) &&
                        !float.IsNaN(startB.X) && !float.IsNaN(startB.Y))
                    {
                        target.DrawLine(startA, startB, DrawColor, 1f, false);
                    }

                    if (!float.IsNaN(endA.X) && !float.IsNaN(endA.Y) &&
                        !float.IsNaN(endB.X) && !float.IsNaN(endB.Y))
                    {
                        target.DrawLine(endA, endB, DrawColor, 1f, false);

                    }

                    // Draw connecting lines
                    if (!float.IsNaN(startA.X) && !float.IsNaN(startA.Y) &&
                        !float.IsNaN(endA.X) && !float.IsNaN(endA.Y))
                    {
                        target.DrawLine(startA, endA, DrawColor, 1f, false);
                    }
                }
            }
            /// <summary>
            /// Helper method to get a perpendicular vector to the given axis.
            /// Used for positioning radius handles.
            /// </summary>
            private Vector3D GetPerpendicularVector(Vector3D axis)
            {
                // Find a vector not parallel to axis
                Vector3D arbitrary = Math.Abs(axis.X) < 0.9f
                    ? new Vector3D(1, 0, 0)
                    : new Vector3D(0, 1, 0);

                // Cross product gives perpendicular vector
                return Vector3D.Cross(axis, arbitrary);
            }
            public override void EmitCommands(IRenderTarget target,  IViewSettings view)
            {

                if (!TryGetScreenGeometry(view, out var bottomCenterx, out var topCenterx, out float radiusPx))
                    return;
 //
                 float my_rad = FixedRadius ? Radius : view.DIST_Real_to_Pict(Radius);
                if (my_rad < 1F)
                {
                    my_rad = 1F;
                }

                // Temporarily update radius for drawing
                float originalRadius = Radius;
                Radius = my_rad;


                try
                {
                    // Draw the cylindrical surface
                    DrawCylinderSurface(target, view);

                    // Draw end caps if requested
                    DrawEndCaps(target, view);
                }
                catch (Exception ex)
                {
                    // Fallback: draw as simple wireframe
                    DrawWireframeCylinder(target, view);
                }
                finally
                {
                    // Restore original radius
                    Radius = originalRadius;
                }

            }
        #endregion 
    }
}
 
 

 