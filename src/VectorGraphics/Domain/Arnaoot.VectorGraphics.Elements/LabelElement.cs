using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Elements
{
    #region Label Element
    public class LabelElement :  DrawElement
        {
            //public Vector3D Position;
            public string Text;
            public ArgbColor DrawColor;
            public int FontSize;


            private Vector3D _Position;

            /// <summary>
            /// The starting coordinate of the line. Setting this value fires ElementBoundsChanged.
            /// </summary>
            public Vector3D Position
            {
                get => _Position;
                set
                {
                    if (_Position.Equals(value)) return;
                    _Position = value;
                    // CRITICAL: Notify the layer that the element's bounds have changed
                    OnElementBoundsChanged();
                }
            }

            /// <summary>
            /// Initializes a new instance of the LabelElement class.
            /// </summary>
            /// <param name="Position">The  coordinate of the label's position.</param>
            /// <param name="text">The text to display.</param>
            /// <param name="drawColor">The color of the text.</param>
            /// <param name="fontSize">The size of the font in points.</param>
            /// <param name="layerNo">The layer number to assign the label to.</param>
            public LabelElement(Vector3D Position, string text, ArgbColor drawColor, int fontSize) : base()
            {
                //
                this._Position = Position;
                this.Text = text;
                this.DrawColor = drawColor;
                this.FontSize = fontSize;
            }

           /// <summary>
            /// Gets the approximate bounding rectangle of the label in real-world coordinates.
            /// </summary>
            public override BoundingBox3D GetBounds()
            {
                float approxWidth = Text.Length * FontSize * 0.6F;
                float approxHeight = FontSize;
                return new BoundingBox3D(_Position, new Vector3D(_Position.X + approxWidth, _Position.Y + approxHeight, _Position.Z + 1));
            }

            #region Hit Test

            public override bool HitTest(Vector3D worldPoint, float worldTolerance)
            {
                // For labels, use a simple circular hit area around the position
                // Since labels have visual extent based on text, you might want to
                // use GetBounds() if you have accurate text bounds calculation

                float distance = Vector3D.Distance(worldPoint, _Position);

                // Use a slightly larger tolerance for labels since they have area
                // You can adjust this multiplier based on font size
                float adjustedTolerance = worldTolerance * (1 + FontSize / 12f);

                return distance <= adjustedTolerance;

                // Alternative: If you have accurate text bounds, use this instead:
                /*
                var bounds = GetBounds();
                return worldPoint.X >= (bounds.Min.X - worldTolerance) && 
                       worldPoint.X <= (bounds.Max.X + worldTolerance) &&
                       worldPoint.Y >= (bounds.Min.Y - worldTolerance) && 
                       worldPoint.Y <= (bounds.Max.Y + worldTolerance);
                */
            }

            public override Vector3D[] GetControlPoints()
            {
                // Labels typically have just one control point - their anchor position
                // You could add more if you want resize handles or rotation handles
                return new[] { _Position };
            }

            public override bool TryGetControlPointAt(Vector3D worldPoint, float worldTolerance, out int pointIndex)
            {
                if (Vector3D.Distance(worldPoint, _Position) <= worldTolerance)
                {
                    pointIndex = 0;
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
                        Position = newPosition;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(pointIndex),
                            $"LabelElement has 1 control point (0), got {pointIndex}");
                }
            }


        #endregion
            public override void EmitCommands(IRenderTarget target, IViewSettings view)
            {
                var pts = GetControlPoints();
                if (pts.Length == 0 || string.IsNullOrEmpty(Text)) return;

                Vector2D pos = view.RealToPict(pts[0], out _);
                if (!pos.IsValid) return;

                // Optional: coarse cull (estimate text bounds)
                // For precision, skip — DrawString is cheap for visible labels
                target.DrawString(
                    text: Text,
                    position: pos,
                    color: DrawColor,
                    fontFamily: "Arial",
                    size: FontSize
                );
            }

 
        }



        #endregion

    }
 