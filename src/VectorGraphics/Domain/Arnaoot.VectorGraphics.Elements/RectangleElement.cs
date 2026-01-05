using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Elements
{
    #region Rectangle Element

    public class RectangleElement : DrawElement
        {
            // public Vector3D StartPoint, EndPoint;
            public bool RelativeCoords;
            public int DrawWidth;
            public ArgbColor DrawColor;
            public ArgbColor FillColor;
            public bool Filled;
            //
            private Vector3D _startPoint;
            private Vector3D _endPoint;

            /// <summary>
            /// The starting coordinate of the line. Setting this value fires ElementBoundsChanged.
            /// </summary>
            public Vector3D StartPoint
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
            public Vector3D EndPoint
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

            /// <summary>
            /// Initializes a new instance of the RectangleElement class.
            /// </summary>
            /// <param name="StartPoint">The  coordinate of the first corner.</param>
            /// <param name="EndPoint">The y-coordinate of the first corner.</param>
            /// <param name="relativeCoords">Indicates if coordinates are relative.</param>
            /// <param name="drawWidth">The width of the rectangle's outline.</param>
            /// <param name="drawColor">The color of the rectangle's outline.</param>
            /// <param name="fillColor">The fill color of the rectangle.</param>
            /// <param name="filled">Indicates if the rectangle should be filled.</param>
            /// <param name="layerNo">The layer number to assign the rectangle to.</param>
            public RectangleElement(Vector3D StartPoint, Vector3D EndPoint, bool relativeCoords, int drawWidth, ArgbColor drawColor, ArgbColor fillColor, bool filled) : base()
            {
                //
                this._startPoint = StartPoint;
                this._endPoint = EndPoint;
                this.RelativeCoords = relativeCoords;
                this.DrawWidth = drawWidth;
                this.DrawColor = drawColor;
                this.FillColor = fillColor;
                this.Filled = filled;
            }

            /// <summary>
            /// Draws the rectangle element on the specified graphics surface.
            /// </summary>
            public override void EmitCommands(IRenderTarget target, IViewSettings view)
            {
                {
                    try
                    {
                        Rect2 rect = new Rect2();
                        Vector2D StartPointPixel = view.RealToPict(_startPoint, out float depth1);
                        StartPointPixel = view.ClampToGdiRangePoint(StartPointPixel);
                        //
                        Vector2D EndPointPixel = view.RealToPict(_endPoint, out float depth2);
                        EndPointPixel = view.ClampToGdiRangePoint(EndPointPixel);
                        //
                        if (float.IsNaN(StartPointPixel.X) || float.IsNaN(StartPointPixel.Y) || float.IsNaN(EndPointPixel.X) || float.IsNaN(EndPointPixel.Y))
                        {
                            return;
                        }
                        //
                        if (!RelativeCoords)
                        {
                            float x = Math.Min(StartPointPixel.X, EndPointPixel.X);
                            float y = Math.Min(StartPointPixel.Y, EndPointPixel.Y);
                            float w = Math.Abs(StartPointPixel.X - EndPointPixel.X);
                            float h = Math.Abs(StartPointPixel.Y - EndPointPixel.Y);
                            rect = new Rect2(x, y, w, h);
                        }
                        else
                        {
                            rect = new Rect2(StartPointPixel.X, StartPointPixel.Y, EndPointPixel.X, EndPointPixel.Y);
                        }
                        if (Filled)
                        {
                            target.DrawRectangle(rect, DrawColor, DrawWidth, FillColor);
                        }
                        else
                        {
                            target.DrawRectangle(rect, DrawColor, DrawWidth, null);
                        }
                     }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Draw failed: " + ex.Message);
                    }
                }
            }

            /// <summary>
            /// Gets the bounding rectangle of the rectangle in real-world coordinates.
            /// </summary>
            public override BoundingBox3D GetBounds()
            {
                if (RelativeCoords)
                {
                    return new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(0, 0, 0));
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


            #region hittest
            public override bool HitTest(Vector3D worldPoint, float worldTolerance)
            {
                // Get rectangle bounds
                float minX = Math.Min(_startPoint.X, _endPoint.X);
                float maxX = Math.Max(_startPoint.X, _endPoint.X);
                float minY = Math.Min(_startPoint.Y, _endPoint.Y);
                float maxY = Math.Max(_startPoint.Y, _endPoint.Y);

                if (Filled)
                {
                    // Area-based selection: check if point is inside rectangle (with tolerance)
                    return worldPoint.X >= (minX - worldTolerance) &&
                           worldPoint.X <= (maxX + worldTolerance) &&
                           worldPoint.Y >= (minY - worldTolerance) &&
                           worldPoint.Y <= (maxY + worldTolerance);
                }
                else
                {
                    // Edge-based selection: check if point is near any of the 4 edges
                    bool nearLeftEdge = Math.Abs(worldPoint.X - minX) <= worldTolerance &&
                                       worldPoint.Y >= (minY - worldTolerance) &&
                                       worldPoint.Y <= (maxY + worldTolerance);

                    bool nearRightEdge = Math.Abs(worldPoint.X - maxX) <= worldTolerance &&
                                        worldPoint.Y >= (minY - worldTolerance) &&
                                        worldPoint.Y <= (maxY + worldTolerance);

                    bool nearTopEdge = Math.Abs(worldPoint.Y - minY) <= worldTolerance &&
                                      worldPoint.X >= (minX - worldTolerance) &&
                                      worldPoint.X <= (maxX + worldTolerance);

                    bool nearBottomEdge = Math.Abs(worldPoint.Y - maxY) <= worldTolerance &&
                                         worldPoint.X >= (minX - worldTolerance) &&
                                         worldPoint.X <= (maxX + worldTolerance);

                    return nearLeftEdge || nearRightEdge || nearTopEdge || nearBottomEdge;
                }
            }

            public override Vector3D[] GetControlPoints()
            {
                // Return the 4 corners as control points
                float minX = Math.Min(_startPoint.X, _endPoint.X);
                float maxX = Math.Max(_startPoint.X, _endPoint.X);
                float minY = Math.Min(_startPoint.Y, _endPoint.Y);
                float maxY = Math.Max(_startPoint.Y, _endPoint.Y);
                float z = _startPoint.Z; // Assuming 2D rectangle in XY plane

                return new[]
                {
            new Vector3D(minX, minY, z),  // Index 0: Bottom-Left
            new Vector3D(maxX, minY, z),  // Index 1: Bottom-Right
            new Vector3D(maxX, maxY, z),  // Index 2: Top-Right
            new Vector3D(minX, maxY, z)   // Index 3: Top-Left
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
                // Moving a corner affects both StartPoint and EndPoint
                // We maintain the opposite corner and adjust the selected corner

                float minX = Math.Min(_startPoint.X, _endPoint.X);
                float maxX = Math.Max(_startPoint.X, _endPoint.X);
                float minY = Math.Min(_startPoint.Y, _endPoint.Y);
                float maxY = Math.Max(_startPoint.Y, _endPoint.Y);

                switch (pointIndex)
                {
                    case 0: // Bottom-Left corner
                            // Keep top-right fixed, move bottom-left
                        _startPoint = new Vector3D(newPosition.X, newPosition.Y, _startPoint.Z);
                        _endPoint = new Vector3D(maxX, maxY, _endPoint.Z);
                        break;

                    case 1: // Bottom-Right corner
                            // Keep top-left fixed, move bottom-right
                        _startPoint = new Vector3D(minX, newPosition.Y, _startPoint.Z);
                        _endPoint = new Vector3D(newPosition.X, maxY, _endPoint.Z);
                        break;

                    case 2: // Top-Right corner
                            // Keep bottom-left fixed, move top-right
                        _startPoint = new Vector3D(minX, minY, _startPoint.Z);
                        _endPoint = new Vector3D(newPosition.X, newPosition.Y, _endPoint.Z);
                        break;

                    case 3: // Top-Left corner
                            // Keep bottom-right fixed, move top-left
                        _startPoint = new Vector3D(newPosition.X, minY, _startPoint.Z);
                        _endPoint = new Vector3D(maxX, newPosition.Y, _endPoint.Z);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(pointIndex),
                            $"RectangleElement has 4 control points (0-3), got {pointIndex}");
                }

                OnElementBoundsChanged();
            }

            //public override void EmitCommands(IRenderTarget target, ViewSettings view )
            //{
            //    Vector3D[]  pts = GetControlPoints();
            //    if (pts.Length < 2) return;

            //    Vector2D p0 = view.RealToPict(pts[0], out _);
            //    Vector2D p1 = view.RealToPict(pts[1], out _);

            //    if (!p0.IsValid || !p1.IsValid) return;

            //    float left = Math.Min(p0.X, p1.X);
            //    float top = Math.Min(p0.Y, p1.Y);
            //    float right = Math.Max(p0.X, p1.X);
            //    float bottom = Math.Max(p0.Y, p1.Y);
            //    float width = right - left;
            //    float height = bottom - top;

            //    if (width <= 0 || height <= 0)
            //    {
            //        return;
            //    }

            //    var screenRect = new Rect2(left, top, width, height);
            //    if (!screenRect.IntersectsWith(view.UsableViewport)) return;

            //    target.DrawRectangle(
            //        rect: screenRect,
            //        stroke: DrawColor,
            //        strokeWidth: DrawWidth,
            //        fill: Filled ? FillColor : (ArgbColor?)null
            //    );
            //}

        }
        #endregion





        #endregion

    }
 
