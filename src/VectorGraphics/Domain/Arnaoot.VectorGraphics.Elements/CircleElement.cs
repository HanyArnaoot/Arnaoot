using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Elements
{
    #region Circle Element
    public class CircleElement : DrawElement
    {
        public bool FixedRadius;
        public int Width;
        public ArgbColor Color;
        public ArgbColor FillColor;
        public bool Filled;
        //
        private float _radius;
        private Vector3D _center;

        /// <summary>
        /// The starting coordinate of the line. Setting this value fires ElementBoundsChanged.
        /// </summary>
        public Vector3D Center
        {
            get => _center;
            set
            {
                if (_center.Equals(value)) return;
                _center = value;
                // CRITICAL: Notify the layer that the element's bounds have changed
                OnElementBoundsChanged();
            }
        }
        public float Radius
        {
            get
            { return _radius; }
            set
            {
                if (value == 0 || _radius == value) return;
                _radius = value;
                // CRITICAL: Notify the layer that the element's bounds have changed
                OnElementBoundsChanged();
            }
        }

        public void SetCircleRadiusBypoint(Vector3D RadiusPoint)
        {
            float radius = Vector3D.Distance(RadiusPoint, _center);
            _radius = radius;
        }

        // New 3D properties
        public Vector3D Normal; // Normal vector defining the circle's plane
        public bool Use3DProjection; // Whether to use 3D projection or simple 2D circle

        /// <summary>
        /// Initializes a new instance of the CircleElement class with 3D support.
        /// </summary>
        public CircleElement(Vector3D Center, float radius, bool fixedRadius,
                            int drawWidth, ArgbColor drawColor, ArgbColor fillColor, bool filled,
                             Vector3D? normal = null, bool use3D = true) : base()
        {
            this._center = Center;
            this._radius = radius;
            this.FixedRadius = fixedRadius;
            this.Width = drawWidth;
            this.Color = drawColor;
            this.FillColor = fillColor;
            this.Filled = filled;
            this.Normal = normal ?? new Vector3D(0, 0, 1); // Default to XY plane
            this.Use3DProjection = use3D;

            if (radius < 0F)
            {
                throw new ArgumentException("Radius cannot be negative.");
            }
        }

        /// <summary>
        /// Creates two orthogonal vectors in the plane defined by the normal vector
        /// </summary>
        private (Vector3D u, Vector3D v) GetPlaneVectors()
        {
            Vector3D normal = Normal.Normalized;

            // Find a vector that's not parallel to the normal
            Vector3D temp = Math.Abs(normal.X) < 0.9f ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);

            // Create two orthogonal vectors in the plane
            Vector3D u = Vector3D.Cross(normal, temp).Normalized;
            Vector3D v = Vector3D.Cross(normal, u).Normalized;

            return (u, v);
        }

        /// <summary>
        /// Generates points for a 3D circle using parametric equations
        /// </summary>
        private Vector3D[] Generate3DCirclePoints(int numPoints = 64)
        {
            var (u, v) = GetPlaneVectors();
            Vector3D[] points = new Vector3D[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                float angle = (float)(2 * Math.PI * i / numPoints);
                float cosAngle = (float)Math.Cos(angle);
                float sinAngle = (float)Math.Sin(angle);

                // Point on circle in 3D space
                points[i] = _center + u * (_radius * cosAngle) + v * (_radius * sinAngle);
            }

            return points;
        }

        /// <summary>
        /// Creates a smooth ellipse using proper ellipse fitting after 3D projection
        /// </summary>
        private void DrawProjectedEllipse(IRenderTarget target, IViewSettings UsedViewSetting)
        {
            // Generate many points on the 3D circle
            Vector3D[] circlePoints3D = Generate3DCirclePoints(64);
            Vector2D[] projectedPoints = new Vector2D[circlePoints3D.Length];

            // Project all points to screen coordinates
            for (int i = 0; i < circlePoints3D.Length; i++)
            {
                projectedPoints[i] = UsedViewSetting.RealToPict(circlePoints3D[i], out _);
            }

            // Fit an ellipse to the projected points
            var ellipseParams = FitEllipseToPoints(projectedPoints);

            if (ellipseParams.HasValue)
            {
                var (center, width, height, angle) = ellipseParams.Value;

                // Clamp to reasonable bounds
                center = UsedViewSetting.ClampToGdiRangePoint(center);
                width = Math.Max(1, Math.Min(width, 10000));
                height = Math.Max(1, Math.Min(height, 10000));

                try
                {
                    if (Filled)
                    {
                        target.DrawEllipse(center, width / 2, height / 2, (float)(angle * 180.0 / Math.PI), Color, Width, FillColor);
                    }
                    target.DrawEllipse(center, width / 2, height / 2, (float)(angle * 180.0 / Math.PI), Color, Width, null);
                }
                finally
                {
                }
            }
            else
            {
                // Fallback to line drawing if ellipse fitting fails
                Draw3DCircleAsLines(target, UsedViewSetting);
            }
        }

        /// <summary>
        /// Fits an ellipse to a set of 2D points using least squares
        /// </summary>
        private (Vector2D center, float width, float height, double angle)? FitEllipseToPoints(Vector2D[] points)
        {
            if (points.Length < 5) return null;

            try
            {
                // Calculate center as average of points
                float cx = 0, cy = 0;
                foreach (var p in points)
                {
                    cx += p.X;
                    cy += p.Y;
                }
                cx /= points.Length;
                cy /= points.Length;

                // Calculate covariance matrix
                double mxx = 0, mxy = 0, myy = 0;
                foreach (var p in points)
                {
                    double dx = p.X - cx;
                    double dy = p.Y - cy;
                    mxx += dx * dx;
                    mxy += dx * dy;
                    myy += dy * dy;
                }
                mxx /= points.Length;
                mxy /= points.Length;
                myy /= points.Length;

                // Calculate eigenvalues and eigenvectors
                double trace = mxx + myy;
                double det = mxx * myy - mxy * mxy;
                double discriminant = trace * trace - 4 * det;

                if (discriminant < 0) return null;

                double sqrtDiscriminant = Math.Sqrt(discriminant);
                double lambda1 = (trace + sqrtDiscriminant) / 2;
                double lambda2 = (trace - sqrtDiscriminant) / 2;

                // Calculate angle
                double angle = 0;
                if (Math.Abs(mxy) > 1e-10)
                {
                    angle = Math.Atan2(lambda1 - mxx, mxy);
                }
                else if (mxx > myy)
                {
                    angle = 0;
                }
                else
                {
                    angle = Math.PI / 2;
                }

                // Calculate semi-axes lengths (multiply by factor for proper sizing)
                double factor = 2.5; // Adjust this to get proper ellipse size
                float width = (float)(2 * Math.Sqrt(lambda1) * factor);
                float height = (float)(2 * Math.Sqrt(lambda2) * factor);

                return (new Vector2D(cx, cy), width, height, angle);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Draws a 3D circle as connected line segments
        /// </summary>
        private void Draw3DCircleAsLines(IRenderTarget target, IViewSettings UsedViewSetting)
        {
            Vector3D[] circlePoints = Generate3DCirclePoints(32); // 32 segments for smooth appearance
            Vector2D[] projectedPoints = new Vector2D[circlePoints.Length];

            // Project all points to screen coordinates
            for (int i = 0; i < circlePoints.Length; i++)
            {
                projectedPoints[i] = UsedViewSetting.RealToPict(circlePoints[i], out _);
                projectedPoints[i] = UsedViewSetting.ClampToGdiRangePoint(projectedPoints[i]);
            }

            // Draw lines connecting the points
            for (int i = 0; i < projectedPoints.Length; i++)
            {
                int nextIndex = (i + 1) % projectedPoints.Length;

                if (!float.IsNaN(projectedPoints[i].X) && !float.IsNaN(projectedPoints[i].Y) &&
                    !float.IsNaN(projectedPoints[nextIndex].X) && !float.IsNaN(projectedPoints[nextIndex].Y))
                {
                    target.DrawLine(projectedPoints[i], projectedPoints[nextIndex], Color, Width, IsSelected);
                }
            }
        }


        /// <summary>
        /// Gets the bounding rectangle of the circle in real-world coordinates.
        /// </summary>
        public override BoundingBox3D GetBounds()
        {
            if (Use3DProjection)
            {
                // For 3D circles, calculate bounds based on the plane orientation
                var (u, v) = GetPlaneVectors();

                // Calculate the extents in each axis
                float xExtent = Math.Abs(u.X * _radius) + Math.Abs(v.X * _radius);
                float yExtent = Math.Abs(u.Y * _radius) + Math.Abs(v.Y * _radius);
                float zExtent = Math.Abs(u.Z * _radius) + Math.Abs(v.Z * _radius);

                return new BoundingBox3D(
                    new Vector3D(_center.X - xExtent, _center.Y - yExtent, _center.Z - zExtent),
                    new Vector3D(_center.X + xExtent, _center.Y + yExtent, _center.Z + zExtent)
                );
            }
            else
            {
                return new BoundingBox3D(
                    new Vector3D(_center.X - _radius, _center.Y - _radius, _center.Z - _radius),
                    new Vector3D(_center.X + _radius, _center.Y + _radius, _center.Z + _radius)
                );
            }
        }

        #region circle edit
        public override bool HitTest(Vector3D worldPoint, float worldTolerance)
        {
            // Distance from point to circle center
            float distToCenter = Vector3D.Distance(worldPoint, _center);

            // Check if point is near the circle's perimeter (ring-based selection)
            // Point is "on" the circle if distance ≈ radius (within tolerance)
            float distToPerimeter = Math.Abs(distToCenter - _radius);

            return distToPerimeter <= worldTolerance;

            // Alternative: Select if point is anywhere inside the circle
            // return distToCenter <= (Radius + worldTolerance);
        }

        public override Vector3D[] GetControlPoints()
        {
            // Control points: center + 4 radius handles (top, right, bottom, left)
            return new[]
            {
            _center,                                    // Index 0: Center
            _center + new Vector3D(_radius, 0, 0),      // Index 1: Right
            _center + new Vector3D(0, _radius, 0),      // Index 2: Top
            _center + new Vector3D(-_radius, 0, 0),     // Index 3: Left
            _center + new Vector3D(0, -_radius, 0)      // Index 4: Bottom
        };
        }

        public override bool TryGetControlPointAt(Vector3D worldPoint, float worldTolerance, out int pointIndex)
        {
            var controlPoints = GetControlPoints();

            for (int i = 0; i < controlPoints.Length; i++)
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
                    // Moving center point
                    Center = newPosition;
                    break;

                case 1: // Right handle
                case 2: // Top handle
                case 3: // Left handle
                case 4: // Bottom handle
                        // Moving radius handle - recalculate radius based on distance from center
                    float newRadius = Vector3D.Distance(_center, newPosition);
                    if (newRadius > 0.001f) // Prevent zero/negative radius
                    {
                        _radius = newRadius;
                        OnElementBoundsChanged();
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(pointIndex),
                        $"CircleElement has 5 control points (0-4), got {pointIndex}");
            }
        }


        #endregion
        public override void EmitCommands(IRenderTarget target, IViewSettings view)
        {
            var pts = GetControlPoints(); // [0]=center, [1]=radiusPoint
            if (pts.Length < 2) return;

            Vector2D center2D = view.RealToPict(pts[0], out _);
            Vector2D radiusPt2D = view.RealToPict(pts[1], out _);

            if (!center2D.IsValid || !radiusPt2D.IsValid) return;

            float radius = (radiusPt2D - center2D).Length;
            if (radius <= 0) return;

            // Optional: coarse culling via bounding rect
            var bounds = new Rect2(
                center2D.X - radius, center2D.Y - radius,
                radius * 2, radius * 2
            );
            if (!bounds.IntersectsWith(view.UsableViewport)) return;

            float my_rad = FixedRadius ? _radius : view.DIST_Real_to_Pict(_radius);
            if (my_rad < 1F)
            {
                my_rad = 1F;
            }

            if (Use3DProjection)
            {
                // Use 3D projection with Bézier curves for smooth ellipse
                //
                try
                {
                    DrawProjectedEllipse(target, view);
                }
                catch
                {
                    // Fallback to line-based drawing if ellipse projection fails
                    Draw3DCircleAsLines(target, view);
                }
            }
            else
            {
                // Original 2D circle drawing
                Vector2D DrawPoint = view.RealToPict(_center, out float depth2);
                DrawPoint = view.ClampToGdiRangePoint(DrawPoint);

                if (float.IsNaN(DrawPoint.X) || float.IsNaN(DrawPoint.Y) || float.IsNaN(my_rad))
                {
                    return;
                }
                //
                if (Filled)
                {
                    target.DrawEllipse(new Vector2D(DrawPoint.X  , DrawPoint.Y  ), my_rad  , my_rad  , 0, Color, Width, FillColor);
                }
                else
                {
                    target.DrawEllipse(new Vector2D(DrawPoint.X  , DrawPoint.Y  ), my_rad  , my_rad  , 0, Color, Width, null);
                }
            }
        }
        #endregion
    }
}

