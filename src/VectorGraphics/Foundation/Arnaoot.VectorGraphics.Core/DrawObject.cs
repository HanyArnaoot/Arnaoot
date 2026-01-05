using Arnaoot.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arnaoot.VectorGraphics.Core
{
    public readonly struct Rect2 : IEquatable<Rect2>
    {
        public readonly float X, Y, Width, Height;

        //public Rect2(float x, float y, float width, float height)
        //    => (X, Y, Width, Height) = (x, y, Math.Max(0, width), Math.Max(0, height));
        public Rect2(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = Math.Max(0, width);
            Height = Math.Max(0, height);
        }
        public float Left => X;
        public float Top => Y;
        public float Right => X + Width;
        public float Bottom => Y + Height;

        public Vector2D TopLeft => new Vector2D(X, Y);
        public Vector2D BottomRight => new Vector2D(Right, Bottom);

        //  Fast containment
        public bool Contains(Vector2D pt) =>
            pt.X >= Left && pt.X <= Right && pt.Y >= Top && pt.Y <= Bottom;

        // Liang-Barsky-friendly bounds
        public bool IntersectsWith(Rect2 other) =>
            Left < other.Right && Right > other.Left &&
            Top < other.Bottom && Bottom > other.Top;

        // Safe conversion from GDI+
        //public static implicit operator RectangleF(Rect2 r)
        //{
        //    return new RectangleF(r.X, r.Y, r.Width, r.Height);
        //}

        //public static implicit operator Rect2(RectangleF r)
        //{
        //    return new Rect2(r.X, r.Y, r.Width, r.Height);
        //}
        public RectangleF ToRectangleF()
        {
            return new RectangleF(X, Y, Width, Height);
        }

        //   Equality & hash
        public bool Equals(Rect2 other) =>
            X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        public override bool Equals(object obj) => obj is Rect2 r && Equals(r);
        //public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public static Rect2 Empty => new Rect2(0, 0, 0, 0);
    }
    public struct BoundingBox3D
    {
        public Vector3D Min;
        public Vector3D Max;

        public BoundingBox3D( Vector3D min,  Vector3D max)
        {
            //Min = min;
            //Max = max;
            Min = new  Vector3D(
               Math.Min(min.X, max.X),
               Math.Min(min.Y, max.Y),
               Math.Min(min.Z, max.Z)
                              );

            Max = new  Vector3D(
                Math.Max(min.X, max.X),
                Math.Max(min.Y, max.Y),
                Math.Max(min.Z, max.Z)
                                );

        }
        public Vector3D Center
        {
            get
            {
                return new Vector3D(
                    (Min.X + Max.X) * 0.5f,
                    (Min.Y + Max.Y) * 0.5f,
                    (Min.Z + Max.Z) * 0.5f
                );
            }
        }
        public static BoundingBox3D Union(BoundingBox3D a, BoundingBox3D b)
        {
            Vector3D min = new Vector3D(
                    Math.Min(a.Min.X, b.Min.X),
                    Math.Min(a.Min.Y, b.Min.Y),
                    Math.Min(a.Min.Z, b.Min.Z)
                );

            Vector3D max = new Vector3D(
                    Math.Max(a.Max.X, b.Max.X),
                    Math.Max(a.Max.Y, b.Max.Y),
                    Math.Max(a.Max.Z, b.Max.Z)
                );
            return new BoundingBox3D(min, max);
        }

        public void Union(Vector3D point)
        {
            Min = new Vector3D(
                Math.Min(Min.X, point.X),
                Math.Min(Min.Y, point.Y),
                Math.Min(Min.Z, point.Z)
            );

            Max = new Vector3D(
                Math.Max(Max.X, point.X),
                Math.Max(Max.Y, point.Y),
                Math.Max(Max.Z, point.Z)
            );
        }
        public float Width()
        {
            return Math.Abs(Max.X - Min.X);
        }
        public float Height()
        {
            return Math.Abs(Max.Y - Min.Y);
        }
        public float Depth()
        {
            return Math.Abs(Max.Z - Min.Z);
        }

        public bool IntersectsWith(BoundingBox3D visibleRegion)
        {
            //bool result =
            //this.Min.X <= visibleRegion.Max.X && this.Max.X >= visibleRegion.Min.X &&
            //this.Min.Y <= visibleRegion.Max.Y && this.Max.Y >= visibleRegion.Min.Y &&
            //this.Min.Z <= visibleRegion.Max.Z && this.Max.Z >= visibleRegion.Min.Z;
            //for now i disabled the z dimension check as it is zero
            bool result =
       this.Min.X <= visibleRegion.Max.X && this.Max.X >= visibleRegion.Min.X &&
       this.Min.Y <= visibleRegion.Max.Y && this.Max.Y >= visibleRegion.Min.Y;
            return result;
        }
        public bool IsEmpty()
        {
            return Min.X > Max.X || Min.Y > Max.Y || Min.Z > Max.Z;
        }

        public void Inflate( Vector3D amount)
        {
            Min = new  Vector3D(Min.X - amount.X, Min.Y - amount.Y, Min.Z - amount.Z);
            Max = new  Vector3D(Max.X + amount.X, Max.Y + amount.Y, Max.Z + amount.Z);
        }
        public static BoundingBox3D Empty => new BoundingBox3D
            (
            //new Vector3D(float.MaxValue, float.MaxValue, float.MaxValue),
            //new Vector3D(float.MinValue, float.MinValue, float.MinValue)
            new Vector3D(0, 0, 0), new Vector3D(0, 0, 0)
            );
        public bool IsValid()
        {
            return
                ! Vector3D.IsNaN(Min) &&
                ! Vector3D.IsNaN(Max) &&
                Min.X <= Max.X &&
                Min.Y <= Max.Y &&
                Min.Z <= Max.Z;
        }
    }


}
