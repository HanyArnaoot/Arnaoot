using Arnaoot.Core;
using System.Collections.Generic;

namespace Arnaoot.VectorGraphics.Core
{
    // ========================================
    // Path2D System - Cross-platform path representation
    // ========================================

    public enum PathSegmentType : byte
    {
        LineTo,
        // Future: QuadraticBezier, CubicBezier, Arc, etc.
    }

    public readonly struct PathSegment
    {
        public readonly PathSegmentType Type;
        public readonly Vector2D Point;
        // For future Bezier curves:
        // public readonly Vector2D ControlPoint1;
        // public readonly Vector2D ControlPoint2;

        public PathSegment(PathSegmentType type, Vector2D point)
        {
            Type = type;
            Point = point;
        }
    }

    public sealed class Path2D
    {
        private readonly List<PathSegment> _segments;
        private readonly List<PathFigure> _figures;
        private Vector2D _currentStart;
        private int _currentFigureStartIndex;

        public int SegmentCount => _segments.Count;
        public IReadOnlyList<PathFigure> Figures => _figures;

        public Path2D()
        {
            _segments = new List<PathSegment>();
            _figures = new List<PathFigure>();
            _currentFigureStartIndex = 0;
        }

        public Path2D(int initialCapacity)
        {
            _segments = new List<PathSegment>(initialCapacity);
            _figures = new List<PathFigure>();
            _currentFigureStartIndex = 0;
        }

        /// <summary>
        /// Moves to a new point, starting a new figure (disconnected from previous segments)
        /// </summary>
        public void MoveTo(Vector2D point)
        {
            // If there are segments since last MoveTo, finalize the current figure
            if (_segments.Count > _currentFigureStartIndex)
            {
                _figures.Add(new PathFigure(
                    _currentStart,
                    _currentFigureStartIndex,
                    _segments.Count - _currentFigureStartIndex,
                    false // open by default
                ));
            }

            // Start new figure
            _currentStart = point;
            _currentFigureStartIndex = _segments.Count;
        }

        /// <summary>
        /// Adds a line segment from current position to the specified point
        /// </summary>
        public void LineTo(Vector2D point)
        {
            _segments.Add(new PathSegment(PathSegmentType.LineTo, point));
        }

        /// <summary>
        /// Closes the current figure
        /// </summary>
        public void ClosePath()
        {
            // Finalize current figure as closed
            if (_segments.Count > _currentFigureStartIndex)
            {
                _figures.Add(new PathFigure(
                    _currentStart,
                    _currentFigureStartIndex,
                    _segments.Count - _currentFigureStartIndex,
                    true // closed
                ));
                _currentFigureStartIndex = _segments.Count;
            }
        }

        /// <summary>
        /// Gets all segments (for internal use)
        /// </summary>
        public IReadOnlyList<PathSegment> GetSegments() => _segments;

        /// <summary>
        /// Finalizes any pending figure and returns all figures
        /// </summary>
        public IReadOnlyList<PathFigure> GetFigures()
        {
            // Finalize any remaining segments as an open figure
            if (_segments.Count > _currentFigureStartIndex)
            {
                _figures.Add(new PathFigure(
                    _currentStart,
                    _currentFigureStartIndex,
                    _segments.Count - _currentFigureStartIndex,
                    false
                ));
                _currentFigureStartIndex = _segments.Count;
            }
            return _figures;
        }

        public void Clear()
        {
            _segments.Clear();
            _figures.Clear();
            _currentFigureStartIndex = 0;
        }

    }
    public static class Path2DExtensions
    {
        public static string ToSvgPathData(this Path2D path)
        {
            var sb = new System.Text.StringBuilder();
            var figures = path.GetFigures();
            var segments = path.GetSegments();

            foreach (var figure in figures)
            {
                // Move to start of figure
                sb.Append($"M {figure.StartPoint.X:F2} {figure.StartPoint.Y:F2} ");

                // Add all segments in this figure
                for (int i = 0; i < figure.SegmentCount; i++)
                {
                    var segment = segments[figure.SegmentStartIndex + i];

                    switch (segment.Type)
                    {
                        case PathSegmentType.LineTo:
                            sb.Append($"L {segment.Point.X:F2} {segment.Point.Y:F2} ");
                            break;
                            // Add other segment types here if you have them (QuadraticBezier, CubicBezier, Arc, etc.)
                    }
                }

                // Close path if needed
                if (figure.IsClosed)
                {
                    sb.Append("Z ");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
    /// <summary>
    /// Represents a single figure (disconnected segment) within a path
    /// </summary>
    public readonly struct PathFigure
    {
        public readonly Vector2D StartPoint;
        public readonly int SegmentStartIndex;
        public readonly int SegmentCount;
        public readonly bool IsClosed;

        public PathFigure(Vector2D startPoint, int segmentStartIndex, int segmentCount, bool isClosed)
        {
            StartPoint = startPoint;
            SegmentStartIndex = segmentStartIndex;
            SegmentCount = segmentCount;
            IsClosed = isClosed;
        }
    }
}
