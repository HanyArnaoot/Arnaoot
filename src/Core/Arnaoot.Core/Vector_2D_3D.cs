using System;
 

namespace Arnaoot.Core
{
    /// <summary>
    /// A 2D vector for screen-space operations (clipping, rendering, UI).
    /// Lightweight, stack-only, and dependency-free.
    /// </summary>
    public readonly struct Vector2D : IEquatable<Vector2D>
    {
        public readonly float X, Y;
        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;

        }        //
        public Vector2D Normalized => this * (1.0f / Length);

        public static implicit operator System.Drawing.PointF(Vector2D v) => new System.Drawing.PointF(v.X, v.Y);
        public static implicit operator Vector2D(System.Drawing.PointF p) => new Vector2D(p.X, p.Y);
        //
        public static implicit operator Vector2D(Vector3D v) => new Vector2D(v.X, v.Y);
        //public bool AlmostEquals(Vector2D other, double eps = 1e-9)
        //{
        //    return Math.Abs(X - other.X) < eps &&
        //           Math.Abs(Y - other.Y) < eps;
        //}

        //  Standard equality
        public bool Equals(Vector2D other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Vector2D v && Equals(v);
        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return !left.Equals(right);
        }
        //public override int GetHashCode() => HashCode.Combine(X, Y);

        // Useful operators (clipping, bounds math)
        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
        public static Vector2D operator *(Vector2D a, float s) => new Vector2D(a.X * s, a.Y * s);
        public static Vector2D operator /(Vector2D a, float s) => new Vector2D(a.X / s, a.Y / s);

        public float Length => (float)Math.Sqrt(X * X + Y * Y);
        public float LengthSquared => X * X + Y * Y;

        //  NaN/Infinity safety — critical for your RealToPict
        //public bool IsFinite => float.IsFinite(X) && float.IsFinite(Y);

        public static Vector2D Empty => new Vector2D(0f, 0f);
        public static Vector2D One => new Vector2D(1f, 1f);
        //public bool IsValid()
        //{
        //     return float.IsNaN( X) || float.IsNaN( Y)  ;

        //}
        /// <summary>
        /// Distance between two points
        /// </summary>
        public static float Distance(Vector2D a, Vector2D b)
        {
            return (b - a).Length;
        }
        public bool IsValid => !float.IsNaN(X) && !float.IsInfinity(X) &&
                          !float.IsNaN(Y) && !float.IsInfinity(Y);
    }

    /// <summary>
    /// A 3D vector for screen-space operations (clipping, rendering, UI).
    /// Lightweight, stack-only, and dependency-free.
    /// </summary>
    public struct Vector3D
    {
        #region Variable Declare
        public float X, Y, Z;
        #endregion

        public Vector3D(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        #region single-expression body
        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public Vector3D Normalized => this * (1.0f / Length);
        #endregion

        #region Value Checkers
        public  static bool IsNaN(Vector3D value)
        {
            return float.IsNaN(value.X) || float.IsNaN(value.Y) || float.IsNaN(value.Z);
        }

        // Equality operators
        public static bool operator ==(Vector3D a, Vector3D b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(Vector3D a, Vector3D b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)// Required for equality operators
        {
            return obj is Vector3D other && this == other;
        }
        public bool IsZero => X == 0f && Y == 0f && Z == 0f;
        public bool IsUnit => Math.Abs(Length - 1f) < 1e-6f;
        #endregion

        #region Operators
        // Scalar multiplication
        public static Vector3D operator *(Vector3D a, float scalar)
        {
            return new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }
        public static Vector3D operator /(Vector3D v, float scalar)
        {
            return new Vector3D(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }
        // Vector addition
        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Vector3D operator +(Vector3D v, float scalar)
        {
            return new Vector3D(v.X + scalar, v.Y + scalar, v.Z + scalar);
        }
        public static Vector3D operator +(float scalar, Vector3D v)
        {
            return new Vector3D(v.X + scalar, v.Y + scalar, v.Z + scalar);
        }
        // Vector subtraction
        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static Vector3D operator -(Vector3D v, float scalar)
        {
            return new Vector3D(v.X - scalar, v.Y - scalar, v.Z - scalar);
        }
        public static Vector3D operator -(float scalar, Vector3D v)
        {
            return new Vector3D(scalar - v.X, scalar - v.Y, scalar - v.Z);
        }

        // NEW: Unary negation operator (needed for normal flipping)
        public static Vector3D operator -(Vector3D v)
        {
            return new Vector3D(-v.X, -v.Y, -v.Z);
        }
        #endregion

        #region Dot Cross
        // Dot product
        public static float Dot(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        // Cross product (vector product)
        public static Vector3D Cross(Vector3D a, Vector3D b)
        {
            return new Vector3D(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }
        #endregion

        #region NEW: Static Utility Methods (Required for intersection code)

        /// <summary>
        /// Normalizes a vector to unit length
        /// </summary>
        public static Vector3D Normalize(Vector3D vector)
        {
            float length = vector.Length;
            if (length < 1e-8f) // Avoid division by zero
                return new Vector3D(0, 0, 0);
            return vector / length;
        }

        /// <summary>
        /// Returns the component-wise minimum of two vectors
        /// </summary>
        public static Vector3D Min(Vector3D a, Vector3D b)
        {
            return new Vector3D(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y),
                Math.Min(a.Z, b.Z)
            );
        }

        /// <summary>
        /// Returns the component-wise maximum of two vectors
        /// </summary>
        public static Vector3D Max(Vector3D a, Vector3D b)
        {
            return new Vector3D(
                Math.Max(a.X, b.X),
                Math.Max(a.Y, b.Y),
                Math.Max(a.Z, b.Z)
            );
        }

        /// <summary>
        /// Linear interpolation between two vectors
        /// </summary>
        public static Vector3D Lerp(Vector3D a, Vector3D b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// Distance between two points
        /// </summary>
        public static float Distance(Vector3D a, Vector3D b)
        {
            return (b - a).Length;
        }

        /// <summary>
        /// Squared distance between two points (faster than Distance)
        /// </summary>
        public static float DistanceSquared(Vector3D a, Vector3D b)
        {
            Vector3D diff = b - a;
            return diff.X * diff.X + diff.Y * diff.Y + diff.Z * diff.Z;
        }

        #endregion

        #region Debugging
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
        }
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
        #endregion
    }
    #region Matrices


    public static class Matrices
    {
        private static double[,] MultiplyMatrices(double[,] a, double[,] b)
        {
            double[,] result = new double[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }

            return result;
        }
    }
    public  static class Helper
    {
        public static double Val(string expression)
        {
            if (expression == null)
                return 0;
            //try the entire string, then progressively smaller
            //substrings to simulate the behavior of VB's 'Val',
            //which ignores trailing characters after a recognizable value:
            for (int size = expression.Length; size > 0; size--)
            {
                double testDouble;
                if (double.TryParse(expression.Substring(0, size), out testDouble))
                    return testDouble;
            }
            //no value is recognized, so return 0:
            return 0;
        }
    }
    #endregion 
}
