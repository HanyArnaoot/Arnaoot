using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arnaoot.VectorGraphics.Abstractions
{
    public interface IViewSettings
    {
        // Properties
        Rect2 UsableViewport { get; }
        float ZoomFactorAverage { get; }
        Vector3D ZoomFactor { get; set; }
        Vector3D ShiftWorld { get; set; }
        Vector3D RotationAngle { get; set; }
        Vector3D RotateAroundPoint { get; set; }
        Vector3D CurrentViewDirection { get; }
        double[,] TransformMatrix { get; }

        // Methods
        IViewSettings Clone();
        void UpdateUsableViewport(float x, float y, float width, float height);
        void UpdateUsableViewport(Rect2 newRect);
        void UpdateTransformMatrixToNewSetting();
        Vector2D RealToPict(Vector3D worldPt, out float depth);
        Vector3D PictToReal(Vector2D screenPt, float worldZ = 0.0f);
        Vector3D PictToViewPlane(Vector2D screenPt, float worldZ = 0.0f);
        float DIST_Real_to_Pict(double distance);
        Vector3D GetViewDirection();
        Vector2D ClampToScreenRange(Vector2D p);

        // Static-like methods can be left out or implemented as extension methods
        Vector3D ClampToGdiRangePoint(Vector3D value);
        Vector2D ClampToGdiRangePoint(Vector2D value);
    }
    public class ViewSettings: IViewSettings
    {
        private double[,] _transformMatrix;
        private int UpdateTransformMatrix_callCount = 0;
         private Vector3D _currentViewDirection;
        public Vector3D CurrentViewDirection
        {
            get
            {
                return   _currentViewDirection ;
            }
        }
        //
        private Vector3D _zoomFactor;
        private float _zoomFactorAverage;
        //
        private Vector3D _shiftWorld;           //  truly in world coordinates
                                                                     //
        private Vector3D _rotationAngle;
        private Vector3D _rotateAroundPoint;
        // private int _height;
        //private int _width;
        public Rect2 UsableViewport { get; private set; } = Rect2.Empty;// Usable area (excludes toolbars, status bars) — set by host
        public ViewSettings(Rect2 usableViewport, Vector3D zoomFactor, Vector3D shiftWorld, Vector3D rotationAngle, Vector3D rotateAroundPoint)
        {
            this.UsableViewport = usableViewport;
            //
            _transformMatrix = new double[4, 4];
            this.ZoomFactor = zoomFactor;
            this._shiftWorld = shiftWorld;          // Store world coordinates
            this.RotationAngle = rotationAngle;
            this.RotateAroundPoint = rotateAroundPoint;
            UpdateTransformMatrixToNewSetting();
        }
        public IViewSettings Clone()
        {
            return new  ViewSettings(
                   UsableViewport,
                   ZoomFactor,
                   _shiftWorld,
                   RotationAngle,
                   RotateAroundPoint
               );
        }
        public void UpdateUsableViewport(float x, float y, float width, float height)
        {
            UpdateUsableViewport(new Rect2(x, y, width, height));
        }

        public void UpdateUsableViewport(Rect2 newRect)
        {
            // If no change → exit without triggering updates
            if (UsableViewport.X == newRect.X &&
                UsableViewport.Y == newRect.Y &&
                UsableViewport.Width == newRect.Width &&
                UsableViewport.Height == newRect.Height)
            {
                return; // nothing changed
            }

            // Update stored viewport
            UsableViewport = newRect;

            // Apply dependent changes only when something changed
            UpdateTransformMatrixToNewSetting();
        }
        public float ZoomFactorAverage
        {
            get { return _zoomFactorAverage; }
        }

        public Vector3D ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                if (Vector3D.IsNaN(value))
                {
                    Debug.WriteLine($"Invalid ZoomFactor: {value}");
                    return;
                }
                _zoomFactor = new Vector3D(
                    Math.Max(value.X, 0.0001F),
                    Math.Max(value.Y, 0.0001F),
                    Math.Max(value.Z, 0.0001F)
                );
                // FIXED: Calculate average correctly
                _zoomFactorAverage = (float)(_zoomFactor.X + _zoomFactor.Y + _zoomFactor.Z) / 3;
                UpdateTransformMatrixToNewSetting();
            }
        }

        public Vector3D ShiftWorld
        {
            get { return _shiftWorld; }
            set
            {
                if (Vector3D.IsNaN(value))
                {
                    Debug.WriteLine($"Invalid ShiftWorld: {value}");
                    return;
                }
                _shiftWorld = value;
                UpdateTransformMatrixToNewSetting();
            }
        }
        public Vector3D RotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                if (Vector3D.IsNaN(value))
                {
                    Debug.WriteLine($"Invalid RotationAngle: {value}");
                    return;
                }

                // Normalize angles to prevent extreme values
                _rotationAngle = new Vector3D(
                    NormalizeAngle(value.X),
                    NormalizeAngle(value.Y),
                    NormalizeAngle(value.Z)
                );

               _currentViewDirection = GetViewDirection();
                UpdateTransformMatrixToNewSetting();
            }
        }

        private float NormalizeAngle(float angle)
        {
            // Keep angles within -2π to 2π to prevent numerical issues
            while (angle > Math.PI * 2) angle -= (float)(Math.PI * 2);
            while (angle < -Math.PI * 2) angle += (float)(Math.PI * 2);
            return angle;
        }


        public Vector3D RotateAroundPoint
        {
            get { return _rotateAroundPoint; }
            set
            {
                if (Vector3D.IsNaN(value))
                {
                    Debug.WriteLine($"Invalid RotateAroundPoint: {value}");
                    return;
                }
                _rotateAroundPoint = value;
                UpdateTransformMatrixToNewSetting();
            }
        }

        public double[,] TransformMatrix
        {
            get { return _transformMatrix; }
        }

        public void UpdateTransformMatrixToNewSetting()
        {
            UpdateTransformMatrix();
        }

        // SIMPLIFIED: Direct world-to-screen transformation
        public void UpdateTransformMatrix()
        {
            UpdateTransformMatrix_callCount++;
            Debug.WriteLine($"UpdateTransformMatrix call #{UpdateTransformMatrix_callCount} at {DateTime.Now:HH:mm:ss.fff}");
            try
            {
                // Initialize identity matrix
                _transformMatrix = new double[,]
                                    {
                                            {1, 0, 0, 0},
                                            {0, 1, 0, 0},
                                            {0, 0, 1, 0},
                                            {0, 0, 0, 1}
                                     };

                // 1. Translate to rotate around point
                double[,] translateToOrigin = {
                                              {1, 0, 0, -_rotateAroundPoint.X},
                                              {0, 1, 0, -_rotateAroundPoint.Y},
                                              {0, 0, 1, -_rotateAroundPoint.Z},
                                              {0, 0, 0, 1}
                                                     };

                // 2. Rotation matrices (same as before)
                double[,] rotX = {
                                             {1, 0, 0, 0},
                                             {0, Math.Cos(_rotationAngle.X), -Math.Sin(_rotationAngle.X), 0},
                                             {0, Math.Sin(_rotationAngle.X), Math.Cos(_rotationAngle.X), 0},
                                             {0, 0, 0, 1}
                                      };

                double[,] rotY = {
                                             {Math.Cos(_rotationAngle.Y), 0, Math.Sin(_rotationAngle.Y), 0},
                                             {0, 1, 0, 0},
                                             {-Math.Sin(_rotationAngle.Y), 0, Math.Cos(_rotationAngle.Y), 0},
                                             {0, 0, 0, 1}
                                            };

                double[,] rotZ = {
                                            {Math.Cos(_rotationAngle.Z), -Math.Sin(_rotationAngle.Z), 0, 0},
                                            {Math.Sin(_rotationAngle.Z), Math.Cos(_rotationAngle.Z), 0, 0},
                                            {0, 0, 1, 0},
                                            {0, 0, 0, 1}
                                    };

                // 3. Translate back
                double[,] translateBack = {
                                            {1, 0, 0, _rotateAroundPoint.X},
                                            {0, 1, 0, _rotateAroundPoint.Y},
                                            {0, 0, 1, _rotateAroundPoint.Z},
                                             {0, 0, 0, 1}
                                            };

                // 4. Apply world shift (before scaling)
                double[,] worldShift = {
                                            {1, 0, 0, _shiftWorld.X},
                                            {0, 1, 0, _shiftWorld.Y},
                                            {0, 0, 1, _shiftWorld.Z},
                                            {0, 0, 0, 1}
                                         };

                // 5. Scale (zoom) - Note: negative Y to flip coordinate system
                double[,] scaling = {
                                         {_zoomFactor.X, 0, 0, 0},
                                         {0, -_zoomFactor.Y, 0, 0},  // Negative Y for screen coordinate flip
                                         {0, 0, _zoomFactor.Z, 0},
                                         {0, 0, 0, 1}
                                       };
                // Build transformation matrix (order matters!)
                // Apply in order: translate to origin -> rotate -> translate back -> world shift -> scale -> center on screen
                _transformMatrix = scaling;// MultiplyMatrices(screenCenter, scaling);
                _transformMatrix = MultiplyMatrices(_transformMatrix, worldShift);
                _transformMatrix = MultiplyMatrices(_transformMatrix, translateBack);
                _transformMatrix = MultiplyMatrices(_transformMatrix, rotZ);
                _transformMatrix = MultiplyMatrices(_transformMatrix, rotY);
                _transformMatrix = MultiplyMatrices(_transformMatrix, rotX);
                _transformMatrix = MultiplyMatrices(_transformMatrix, translateToOrigin);
                // Validate final matrix before assignment
                if (!IsMatrixValid(_transformMatrix))
                {
                    Debug.WriteLine("Invalid transformation matrix computed!");
                    throw new InvalidOperationException("Matrix computation resulted in invalid values");
                }

                //_inverseMatrix = ComputeInverseMatrix(_transformMatrix);
                //i replaced _inverseMatrix that was used to get PictToReal becasue of singularity that caused many errors and disabled the control
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break(); // For debugging purposes
            }
        }
        private bool IsMatrixValid(double[,] matrix)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (double.IsNaN(matrix[i, j]) || double.IsInfinity(matrix[i, j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public  Vector2D RealToPict(Vector3D worldPt, out float depth)
        {
            // Input validation
            if (double.IsNaN(worldPt.X) || double.IsNaN(worldPt.Y) || double.IsNaN(worldPt.Z))
            {
                Debug.WriteLine($"Invalid input in RealToPict: {worldPt}");
                depth = 0f;
                return Vector2D.Empty;
            }

            // Transform world point directly to screen coordinates
            double[] worldVector = { worldPt.X, worldPt.Y, worldPt.Z, 1.0 };
            double[] screenVector = new double[4];

            for (int i = 0; i < 4; i++)
            {
                screenVector[i] = 0;
                for (int j = 0; j < 4; j++)
                    screenVector[i] += _transformMatrix[i, j] * worldVector[j];
            }

            // Handle homogeneous coordinates
            if (Math.Abs(screenVector[3]) < 1e-10)
            {
                Debug.WriteLine("Point at infinity in RealToPict");
                depth = float.MaxValue;
                return Vector2D.Empty;
            }

            float screenX = (float)(screenVector[0] / screenVector[3]);
            float screenY = (float)(screenVector[1] / screenVector[3]);
            depth = (float)(screenVector[2] / screenVector[3]);

            // Validate output
            if (float.IsNaN(screenX) || float.IsNaN(screenY))
            {
                Debug.WriteLine($"Invalid result in RealToPict: screenX={screenX}, screenY={screenY}");
                depth = 0f;
                return Vector2D.Empty;
            }

            return new Vector2D(screenX, screenY);
        }
        public Vector3D PictToReal(Vector2D screenPt, float worldZ = 0.0f)
        {
            // Reverse the transformation chain manually
            // This is more stable than matrix inversion

            // Start with screen coordinates
            Vector3D point = new Vector3D(screenPt.X, screenPt.Y, worldZ);

            // 1. Reverse scaling
            point = new Vector3D(point.X / _zoomFactor.X,
                                -point.Y / _zoomFactor.Y,  // Remember the Y flip
                                point.Z / _zoomFactor.Z);

            // 2. Reverse world shift
            point = new Vector3D(point.X - _shiftWorld.X,
                                point.Y - _shiftWorld.Y,
                                point.Z - _shiftWorld.Z);

            // 3. Reverse translate back
            point = new Vector3D(point.X - _rotateAroundPoint.X,
                                point.Y - _rotateAroundPoint.Y,
                                point.Z - _rotateAroundPoint.Z);

            // 4. Reverse rotations (in reverse order: -Z, -Y, -X)
            point = ApplyRotation(point, 0, 0, -_rotationAngle.Z);
            point = ApplyRotation(point, 0, -_rotationAngle.Y, 0);
            point = ApplyRotation(point, -_rotationAngle.X, 0, 0);

            // 5. Reverse translate to origin
            point = new Vector3D(point.X + _rotateAroundPoint.X,
                                point.Y + _rotateAroundPoint.Y,
                                point.Z + _rotateAroundPoint.Z);

            return point;
        }
        public Vector3D PictToViewPlane(Vector2D screenPt, float worldZ = 0.0f)
        {
            // Reverse only scale and shift (not rotation)
            // Returns coordinates in view-aligned space at the specified Z plane

            float x = (screenPt.X / _zoomFactor.X) - _shiftWorld.X;
            float y = (-screenPt.Y / _zoomFactor.Y) - _shiftWorld.Y;  // Y flip
            float z = (worldZ / _zoomFactor.Z) - _shiftWorld.Z;

            return new Vector3D(x, y, z);
        }
        private Vector3D ApplyRotation(Vector3D point, double rotX, double rotY, double rotZ)
        {
            Vector3D result = point;

            // Apply X rotation
            if (Math.Abs(rotX) > 1e-10)
            {
                double cosX = Math.Cos(rotX);
                double sinX = Math.Sin(rotX);
                double newY = result.Y * cosX - result.Z * sinX;
                double newZ = result.Y * sinX + result.Z * cosX;
                result = new Vector3D(result.X, (float)newY, (float)newZ);
            }

            // Apply Y rotation
            if (Math.Abs(rotY) > 1e-10)
            {
                double cosY = Math.Cos(rotY);
                double sinY = Math.Sin(rotY);
                double newX = result.X * cosY + result.Z * sinY;
                double newZ = -result.X * sinY + result.Z * cosY;
                result = new Vector3D((float)newX, result.Y, (float)newZ);
            }

            // Apply Z rotation
            if (Math.Abs(rotZ) > 1e-10)
            {
                double cosZ = Math.Cos(rotZ);
                double sinZ = Math.Sin(rotZ);
                double newX = result.X * cosZ - result.Y * sinZ;
                double newY = result.X * sinZ + result.Y * cosZ;
                result = new Vector3D((float)newX, (float)newY, result.Z);
            }

            return result;
        }


        public  float DIST_Real_to_Pict(double distance)
        {
            if (ZoomFactorAverage <= 0.0 || double.IsNaN(distance) || double.IsNaN(ZoomFactorAverage))
                return 0f;

            double screenDist = distance * ZoomFactorAverage;
            if (double.IsNaN(screenDist) || double.IsInfinity(screenDist))
                return 0f;

            return (float)screenDist;
        }

        //  matrix multiplication 
        private double[,] MultiplyMatrices(double[,] a, double[,] b)
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

        public Vector3D GetViewDirection()
        {
            // Start with the default view direction (looking down negative Z-axis)
            Vector3D defaultDirection = new Vector3D(0, 0, -1);

            // Apply rotations in the same order as your transform matrix
            // (X, Y, Z rotations as defined in UpdateTransformMatrix)

            // Create rotation matrices for each axis
            double cosX = Math.Cos(_rotationAngle.X);
            double sinX = Math.Sin(_rotationAngle.X);
            double cosY = Math.Cos(_rotationAngle.Y);
            double sinY = Math.Sin(_rotationAngle.Y);
            double cosZ = Math.Cos(_rotationAngle.Z);
            double sinZ = Math.Sin(_rotationAngle.Z);

            // Apply X rotation
            double y1 = defaultDirection.Y * cosX - defaultDirection.Z * sinX;
            double z1 = defaultDirection.Y * sinX + defaultDirection.Z * cosX;
            Vector3D afterX = new Vector3D(defaultDirection.X, (float)y1, (float)z1);

            // Apply Y rotation
            double x2 = afterX.X * cosY + afterX.Z * sinY;
            double z2 = -afterX.X * sinY + afterX.Z * cosY;
            Vector3D afterY = new Vector3D((float)x2, afterX.Y, (float)z2);

            // Apply Z rotation
            double x3 = afterY.X * cosZ - afterY.Y * sinZ;
            double y3 = afterY.X * sinZ + afterY.Y * cosZ;
            Vector3D finalDirection = new Vector3D((float)x3, (float)y3, afterY.Z);

            // Normalize the direction vector
            double length = Math.Sqrt(finalDirection.X * finalDirection.X +
                                     finalDirection.Y * finalDirection.Y +
                                     finalDirection.Z * finalDirection.Z);

            if (length > 1e-10)
            {
                finalDirection = new Vector3D(
               finalDirection.X / (float)length,
                   (float)finalDirection.Y / (float)length,
                    finalDirection.Z / (float)length
                );
            }

            return finalDirection;
        }
        public Vector2D ClampToScreenRange(Vector2D p)
        {
            float x = p.X;
            float y = p.Y;

            if (x < 0) x = 0;
            else if (x > UsableViewport.Width) x = UsableViewport.Width;

            if (y < 0) y = 0;
            else if (y > UsableViewport.Height) y = UsableViewport.Height;

            return new Vector2D(x, y);
        }

        public  Vector3D ClampToGdiRangePoint(Vector3D value)
        {
            const float MaxCoord = float.MaxValue;
            const float MinCoord = float.MinValue;
            return new Vector3D(Math.Max(MinCoord, Math.Min(MaxCoord, value.X)), Math.Max(MinCoord, Math.Min(MaxCoord, value.Y)), Math.Max(MinCoord, Math.Min(MaxCoord, value.Z)));
        }
        public  Vector2D ClampToGdiRangePoint(Vector2D value)
        {
            const float MaxCoord = float.MaxValue;
            const float MinCoord = float.MinValue;
            return new Vector2D(
                Math.Max(MinCoord, Math.Min(MaxCoord, value.X)),
                Math.Max(MinCoord, Math.Min(MaxCoord, value.Y))
            );
        }




    }

}
