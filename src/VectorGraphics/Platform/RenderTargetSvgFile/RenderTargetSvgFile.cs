using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Platform.SVGFile
{
    public class RenderTargetSvgFile : IRenderTarget
    {
        private System.Xml.XmlTextWriter _writer;
        private string _currentFilePath;
        private int _width;
        private int _height;
        private bool _isFrameActive;
//
        public void BeginFrame(int width, int height)
        {
            _width = width;
            _height = height;

            // Generate timestamp-based filename in executable directory
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            _currentFilePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                $"render_{timestamp}.svg"
            );

            _writer = new System.Xml.XmlTextWriter(_currentFilePath, System.Text.Encoding.UTF8);
            _writer.Formatting = System.Xml.Formatting.Indented;
            _writer.WriteStartDocument();
            _writer.WriteStartElement("svg");
            _writer.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");
            _writer.WriteAttributeString("width", width.ToString());
            _writer.WriteAttributeString("height", height.ToString());
            _writer.WriteAttributeString("viewBox", $"0 0 {width} {height}");
            //
            _isFrameActive = true;
        }

        public void BeginFrameFromCache()
        {
            // Create new file for cached frame too
            BeginFrame(_width, _height);
        }

        public void Clear(ArgbColor color)
        {
            //if (!_isFrameActive || _writer == null) return;
            //// Draw a background rectangle
            //_writer.WriteStartElement("rect");
            //_writer.WriteAttributeString("x", "0");
            //_writer.WriteAttributeString("y", "0");
            //_writer.WriteAttributeString("width", _width.ToString());
            //_writer.WriteAttributeString("height", _height.ToString());
            //_writer.WriteAttributeString("fill", $"#{color.R:X2}{color.G:X2}{color.B:X2}");
            //_writer.WriteEndElement();
        }

        public void DrawLine(Vector2D p1, Vector2D p2, ArgbColor color, float width, bool isSelected = false)
        {
            if (!_isFrameActive || _writer == null) return;
            //
            _writer.WriteStartElement("line");
            _writer.WriteAttributeString("x1", p1.X.ToString("F2"));
            _writer.WriteAttributeString("y1", p1.Y.ToString("F2"));
            _writer.WriteAttributeString("x2", p2.X.ToString("F2"));
            _writer.WriteAttributeString("y2", p2.Y.ToString("F2"));
            _writer.WriteAttributeString("stroke", $"#{color.R:X2}{color.G:X2}{color.B:X2}");
            _writer.WriteAttributeString("stroke-width", width.ToString("F2"));

            if (isSelected)
            {
                _writer.WriteAttributeString("data-selected", "true");
            }

            _writer.WriteEndElement();
        }

        public void DrawEllipse(Vector2D center, float radiusX, float radiusY, float angleRad,
                               ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            if (!_isFrameActive || _writer == null) return;
            //
            _writer.WriteStartElement("ellipse");
            _writer.WriteAttributeString("cx", center.X.ToString("F2"));
            _writer.WriteAttributeString("cy", center.Y.ToString("F2"));
            _writer.WriteAttributeString("rx", radiusX.ToString("F2"));
            _writer.WriteAttributeString("ry", radiusY.ToString("F2"));

            if (angleRad != 0)
            {
                float angleDeg = angleRad * 180f / (float)Math.PI;
                _writer.WriteAttributeString("transform", $"rotate({angleDeg:F2} {center.X:F2} {center.Y:F2})");
            }

            _writer.WriteAttributeString("stroke", $"#{stroke.R:X2}{stroke.G:X2}{stroke.B:X2}");
            _writer.WriteAttributeString("stroke-width", strokeWidth.ToString("F2"));
            _writer.WriteAttributeString("fill", fill.HasValue
                ? $"#{fill.Value.R:X2}{fill.Value.G:X2}{fill.Value.B:X2}"
                : "none");
            _writer.WriteEndElement();
        }

        public void DrawRectangle(Rect2 rect, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            if (!_isFrameActive || _writer == null) return;
            //
            _writer.WriteStartElement("rect");
            _writer.WriteAttributeString("x", rect.X.ToString("F2"));
            _writer.WriteAttributeString("y", rect.Y.ToString("F2"));
            _writer.WriteAttributeString("width", rect.Width.ToString("F2"));
            _writer.WriteAttributeString("height", rect.Height.ToString("F2"));
            _writer.WriteAttributeString("stroke", $"#{stroke.R:X2}{stroke.G:X2}{stroke.B:X2}");
            _writer.WriteAttributeString("stroke-width", strokeWidth.ToString("F2"));
            _writer.WriteAttributeString("fill", fill.HasValue
                ? $"#{fill.Value.R:X2}{fill.Value.G:X2}{fill.Value.B:X2}"
                : "none");
            _writer.WriteEndElement();
        }

        public void DrawPolygon(ReadOnlySpan<Vector2D> points, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            if (!_isFrameActive || _writer == null) return;
            //
            if (points.Length == 0) return;

            _writer.WriteStartElement("polygon");

            // Build points string
            System.Text.StringBuilder pointsStr = new System.Text.StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0) pointsStr.Append(" ");
                pointsStr.Append($"{points[i].X:F2},{points[i].Y:F2}");
            }

            _writer.WriteAttributeString("points", pointsStr.ToString());
            _writer.WriteAttributeString("stroke", $"#{stroke.R:X2}{stroke.G:X2}{stroke.B:X2}");
            _writer.WriteAttributeString("stroke-width", strokeWidth.ToString("F2"));
            _writer.WriteAttributeString("fill", fill.HasValue
                ? $"#{fill.Value.R:X2}{fill.Value.G:X2}{fill.Value.B:X2}"
                : "none");
            _writer.WriteEndElement();
        }

        public void DrawString(string text, Vector2D position, ArgbColor color, string fontFamily = "Arial", float size = 12)
        {
            if (!_isFrameActive || _writer == null) return;
            //
            _writer.WriteStartElement("text");
            _writer.WriteAttributeString("x", position.X.ToString("F2"));
            _writer.WriteAttributeString("y", position.Y.ToString("F2"));
            _writer.WriteAttributeString("fill", $"#{color.R:X2}{color.G:X2}{color.B:X2}");
            _writer.WriteAttributeString("font-family", fontFamily);
            _writer.WriteAttributeString("font-size", size.ToString("F2"));
            _writer.WriteString(text);
            _writer.WriteEndElement();
        }

        public void DrawPath(Path2D path, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            if (!_isFrameActive || _writer == null) return;
            //
             _writer.WriteStartElement("path");
            _writer.WriteAttributeString("d", path.ToSvgPathData()); // You'll need to implement this
            _writer.WriteAttributeString("stroke", $"#{stroke.R:X2}{stroke.G:X2}{stroke.B:X2}");
            _writer.WriteAttributeString("stroke-width", strokeWidth.ToString("F2"));
            _writer.WriteAttributeString("fill", fill.HasValue
                ? $"#{fill.Value.R:X2}{fill.Value.G:X2}{fill.Value.B:X2}"
                : "none");
            _writer.WriteEndElement();
        }

        public void DrawImage(object image, float x, float y, float width, float height)
        {
            // For performance testing, we'll just draw a placeholder rectangle
            if (!_isFrameActive || _writer == null) return;
            //
            _writer.WriteStartElement("rect");
            _writer.WriteAttributeString("x", x.ToString("F2"));
            _writer.WriteAttributeString("y", y.ToString("F2"));
            _writer.WriteAttributeString("width", width.ToString("F2"));
            _writer.WriteAttributeString("height", height.ToString("F2"));
            _writer.WriteAttributeString("fill", "#CCCCCC");
            _writer.WriteAttributeString("stroke", "#666666");
            _writer.WriteAttributeString("stroke-width", "1");
            _writer.WriteAttributeString("data-type", "image-placeholder");
            _writer.WriteEndElement();
        }

        public void EndFrame()
        {
            // Frame ends but file stays open until EndScene
        }

        public void EndScene()
        {
            if (_writer != null)
            {
                _writer.WriteEndElement(); // </svg>
                _writer.WriteEndDocument();
                _writer.Close();
                _writer.Dispose();
                _writer = null;
                _isFrameActive = false;
            }
        }

        public bool TryGetPixelData(out PixelData data)
        {
            // SVG file has no pixel data
            data = default;
            return false;
        }
    }
}
