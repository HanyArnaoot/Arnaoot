using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.Elements;
using Arnaoot.VectorGraphics.Scene;
 using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Formats.Svg
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Xml;


    // ============================================
    // EXPORT CLASS - Fully Decoupled
    // ============================================
    public interface ISvgExporter
    {
        void SaveAsSvg(string filePath, bool saveRelative, ILayerManager layerManager, IViewSettings currentViewSettings);
    }
    public class SvgExporter : ISvgExporter
    {
        /// <summary>
        /// Exports drawing elements to an SVG file.
        /// </summary>
        public void SaveAsSvg(string filePath, bool saveRelative, ILayerManager layerManager, IViewSettings currentViewSettings)
        {
            try
            {
                using (System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(filePath, System.Text.Encoding.UTF8))
                {
                    writer.Formatting = System.Xml.Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("svg");
                    writer.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");

                    if (saveRelative == false)
                    {
                        Rect2 bounds = GetDrawingBounds(layerManager);
                        writer.WriteAttributeString("width", bounds.Width.ToString("F2"));
                        writer.WriteAttributeString("height", bounds.Height.ToString("F2"));
                        writer.WriteAttributeString("viewBox", $"{bounds.X:F2} {bounds.Y:F2} {bounds.Width:F2} {bounds.Height:F2}");
                    }
                    else
                    {
                        writer.WriteAttributeString("width", currentViewSettings.UsableViewport.Width.ToString());
                        writer.WriteAttributeString("height", currentViewSettings.UsableViewport.Height.ToString());
                    }

                    // Iterate through layers in render order
                    foreach (ILayer layer in layerManager.LayersInRenderOrder)
                    {
                        WriteLayerToSvg(writer, layer, saveRelative, currentViewSettings);
                    }

                    writer.WriteEndElement(); // </svg>
                    writer.WriteEndDocument();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving SVG: {ex.Message}", ex);
            }
        }

        private Rect2 GetDrawingBounds(ILayerManager layerManager)
        {
            BoundingBox3D bounds = layerManager.GetVisibleBounds();

            if (bounds.IsEmpty())
            {
                return new Rect2(0F, 0F, 100F, 100F);
            }

            return new Rect2(bounds.Min.X, bounds.Min.Y,
                            bounds.Max.X - bounds.Min.X,
                            bounds.Max.Y - bounds.Min.Y);
        }

        private void WriteLayerToSvg(System.Xml.XmlTextWriter writer, ILayer layer, bool saveRelative, IViewSettings viewSettings)
        {
            writer.WriteStartElement("g");
            writer.WriteAttributeString("id", $"layer-{layer.Id}");
            writer.WriteAttributeString("data-layer-id", layer.Id.ToString());
            writer.WriteAttributeString("data-layer-name", layer.Name);
            writer.WriteAttributeString("data-layer-visible", layer.Visible.ToString().ToLower());
            writer.WriteAttributeString("data-layer-locked", layer.Locked.ToString().ToLower());
            writer.WriteAttributeString("data-layer-color", $"#{layer.LayerColor.R:X2}{layer.LayerColor.G:X2}{layer.LayerColor.B:X2}");

            if (!layer.Visible)
            {
                writer.WriteAttributeString("style", "display:none");
            }

            foreach (IDrawElement element in layer.GetLayerElements())
            {
                WriteElementToSvg(writer, element, saveRelative, viewSettings);
            }

            writer.WriteEndElement(); // </g>
        }

        private void WriteElementToSvg(System.Xml.XmlTextWriter writer, IDrawElement element, bool saveRelative, IViewSettings viewSettings)
        {
            Type elementType = element.GetType();

            if (elementType == typeof(LineElement))
            {
                WriteLineElement(writer, (LineElement)element, saveRelative, viewSettings);
            }
            else if (elementType == typeof(CircleElement))
            {
                WriteCircleElement(writer, (CircleElement)element, saveRelative, viewSettings);
            }
            else if (elementType == typeof(RectangleElement))
            {
                WriteRectangleElement(writer, (RectangleElement)element, saveRelative, viewSettings);
            }
            else if (elementType == typeof(LabelElement))
            {
                WriteLabelElement(writer, (LabelElement)element, saveRelative, viewSettings);
            }
        }

        private void WriteLineElement(System.Xml.XmlTextWriter writer, LineElement line, bool saveRelative, IViewSettings viewSettings)
        {
            if (Vector3D.IsNaN(line.Start) || Vector3D.IsNaN(line.End))
                return;

            writer.WriteStartElement("line");

            if (saveRelative)
            {
                Vector2D p1 = viewSettings.RealToPict(line.Start, out float depth1);
                Vector2D p2 = viewSettings.RealToPict(line.End, out float depth2);
                writer.WriteAttributeString("x1", p1.X.ToString("F2"));
                writer.WriteAttributeString("y1", p1.Y.ToString("F2"));
                writer.WriteAttributeString("x2", p2.X.ToString("F2"));
                writer.WriteAttributeString("y2", p2.Y.ToString("F2"));
                writer.WriteAttributeString("stroke-width", viewSettings.DIST_Real_to_Pict(line.Width).ToString("F2"));
            }
            else
            {
                writer.WriteAttributeString("x1", line.Start.X.ToString("F2"));
                writer.WriteAttributeString("y1", line.Start.Y.ToString("F2"));
                writer.WriteAttributeString("x2", line.End.X.ToString("F2"));
                writer.WriteAttributeString("y2", line.End.Y.ToString("F2"));
                writer.WriteAttributeString("stroke-width", line.Width.ToString());
            }

            writer.WriteAttributeString("data-z1", line.Start.Z.ToString("F2"));
            writer.WriteAttributeString("data-z2", line.End.Z.ToString("F2"));
            writer.WriteAttributeString("stroke", $"#{line.Color.R:X2}{line.Color.G:X2}{line.Color.B:X2}");
            writer.WriteEndElement();
        }

        private void WriteCircleElement(System.Xml.XmlTextWriter writer, CircleElement circle, bool saveRelative, IViewSettings viewSettings)
        {
            float radius = circle.FixedRadius ?
                (saveRelative ? circle.Radius : circle.Radius * viewSettings.ZoomFactorAverage) :
                (saveRelative ? viewSettings.DIST_Real_to_Pict(circle.Radius) : circle.Radius);

            if (Vector3D.IsNaN(circle.Center) || float.IsNaN(radius))
                return;

            writer.WriteStartElement("circle");

            if (saveRelative)
            {
                Vector2D center = viewSettings.RealToPict(circle.Center, out float depth);
                writer.WriteAttributeString("cx", center.X.ToString("F2"));
                writer.WriteAttributeString("cy", center.Y.ToString("F2"));
            }
            else
            {
                writer.WriteAttributeString("cx", circle.Center.X.ToString("F2"));
                writer.WriteAttributeString("cy", circle.Center.Y.ToString("F2"));
            }

            writer.WriteAttributeString("data-z", circle.Center.Z.ToString("F2"));
            writer.WriteAttributeString("r", radius.ToString("F2"));
            writer.WriteAttributeString("stroke", $"#{circle.Color.R:X2}{circle.Color.G:X2}{circle.Color.B:X2}");
            writer.WriteAttributeString("stroke-width", circle.Width.ToString());
            writer.WriteAttributeString("fill", circle.Filled ? $"#{circle.FillColor.R:X2}{circle.FillColor.G:X2}{circle.FillColor.B:X2}" : "none");
            writer.WriteEndElement();
        }

        private void WriteRectangleElement(System.Xml.XmlTextWriter writer, RectangleElement rect, bool saveRelative, IViewSettings viewSettings)
        {
            if (Vector3D.IsNaN(rect.StartPoint) || Vector3D.IsNaN(rect.EndPoint))
                return;

            float x, y, width, height;

            if (saveRelative)
            {
                Vector2D p1 = viewSettings.RealToPict(rect.StartPoint, out float depth1);
                Vector2D p2 = viewSettings.RealToPict(rect.EndPoint, out float depth2);
                x = Math.Min(p1.X, p2.X);
                y = Math.Min(p1.Y, p2.Y);
                width = Math.Abs(p2.X - p1.X);
                height = Math.Abs(p2.Y - p1.Y);
            }
            else
            {
                x = Math.Min(rect.StartPoint.X, rect.EndPoint.X);
                y = Math.Min(rect.StartPoint.Y, rect.EndPoint.Y);
                width = Math.Abs(rect.StartPoint.X - rect.EndPoint.X);
                height = Math.Abs(rect.StartPoint.Y - rect.EndPoint.Y);
            }

            writer.WriteStartElement("rect");
            writer.WriteAttributeString("x", x.ToString("F2"));
            writer.WriteAttributeString("y", y.ToString("F2"));
            writer.WriteAttributeString("width", width.ToString("F2"));
            writer.WriteAttributeString("height", height.ToString("F2"));
            writer.WriteAttributeString("data-z1", rect.StartPoint.Z.ToString("F2"));
            writer.WriteAttributeString("data-z2", rect.EndPoint.Z.ToString("F2"));
            writer.WriteAttributeString("stroke", $"#{rect.DrawColor.R:X2}{rect.DrawColor.G:X2}{rect.DrawColor.B:X2}");
            writer.WriteAttributeString("stroke-width", rect.DrawWidth.ToString());
            writer.WriteAttributeString("fill", rect.Filled ? $"#{rect.FillColor.R:X2}{rect.FillColor.G:X2}{rect.FillColor.B:X2}" : "none");
            writer.WriteEndElement();
        }

        private void WriteLabelElement(System.Xml.XmlTextWriter writer, LabelElement label, bool saveRelative, IViewSettings viewSettings)
        {
            if (float.IsNaN(label.Position.X) || float.IsNaN(label.Position.Y))
                return;

            writer.WriteStartElement("text");

            if (saveRelative)
            {
                Vector2D pos = viewSettings.RealToPict(label.Position, out float depth);
                writer.WriteAttributeString("x", pos.X.ToString("F2"));
                writer.WriteAttributeString("y", pos.Y.ToString("F2"));
            }
            else
            {
                writer.WriteAttributeString("x", label.Position.X.ToString("F2"));
                writer.WriteAttributeString("y", label.Position.Y.ToString("F2"));
            }

            writer.WriteAttributeString("data-z", label.Position.Z.ToString("F2"));
            writer.WriteAttributeString("fill", $"#{label.DrawColor.R:X2}{label.DrawColor.G:X2}{label.DrawColor.B:X2}");
            writer.WriteAttributeString("font-family", "Arial");
            writer.WriteAttributeString("font-size", label.FontSize.ToString());
            writer.WriteString(label.Text);
            writer.WriteEndElement();
        }
    }

    // ============================================
    // IMPORT CLASS - Decoupled with Element Creation
    // ============================================
    public interface ISvgImporter
    {
       SvgImportResult LoadFromSvg(string filePath, ILayerManager layerManager);
    }
    public class SvgImporter: ISvgImporter
    {
        /// <summary>
        /// Loads drawing elements from an SVG file.
        /// Returns the imported layers and elements for command wrapping.
        /// </summary>
        public SvgImportResult LoadFromSvg(string filePath, ILayerManager layerManager)
        {
            var result = new SvgImportResult();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("svg", "http://www.w3.org/2000/svg");

                // Store original state for undo
                result.OriginalLayerState = layerManager.SaveLayerState();

                // Clear all existing layers
                //layerManager.RemoveAllLayers(); // this line was disabled so that in the future i can try import which is adding new items and layers to existing items and layers
                //
                //
                // Check if SVG has layer groups
                XmlNodeList layerGroups = doc.SelectNodes("//svg:g[@data-layer-id]", nsMgr);

                if (layerGroups.Count > 0)
                {
                    // SVG has layer information
                    foreach (XmlNode groupNode in layerGroups)
                    {
                        var layerResult = ParseLayerGroup(groupNode, nsMgr, layerManager);
                        result.ImportedLayers.Add(layerResult);
                    }
                }
                else
                {
                    // No layer information - load into default layer
                    ILayer defaultLayer = layerManager.AddLayer("Imported");
                    layerManager.SetActiveLayer(defaultLayer);

                    var elements = ParseElementsInNode(doc.DocumentElement, nsMgr);

                    foreach (var element in elements)
                    {
                        defaultLayer.AddElement(element, false);
                    }

                    result.ImportedLayers.Add(new ImportedLayerData
                    {
                        Layer = defaultLayer,
                        Elements = elements
                    });
                }

                // Set first non-locked layer as active
                var firstUnlocked = layerManager.Layers.FirstOrDefault(l => !l.Locked);
                if (firstUnlocked != null)
                {
                    layerManager.SetActiveLayer(firstUnlocked);
                }

                layerManager.UpdateAllLayersBounds();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private ImportedLayerData ParseLayerGroup(XmlNode groupNode, XmlNamespaceManager nsMgr, ILayerManager layerManager)
        {
            // Parse layer metadata
            string layerIdStr = GetAttributeValue(groupNode, "data-layer-id", Guid.NewGuid().ToString());
            Guid layerId = Guid.Parse(layerIdStr);
            string layerName = GetAttributeValue(groupNode, "data-layer-name", "Layer");
            bool visible = bool.Parse(GetAttributeValue(groupNode, "data-layer-visible", "true"));
            bool locked = bool.Parse(GetAttributeValue(groupNode, "data-layer-locked", "false"));
            string colorStr = GetAttributeValue(groupNode, "data-layer-color", "#000000");
            Color layerColor = ParseColor(colorStr, Color.Black);

            // Create new layer
            var layer = layerManager.AddLayer(layerName);
            layer.Visible = visible;
            layer.Locked = locked;
            layer.LayerColor = layerColor;
            layerManager.SetActiveLayer(layer);

            // Parse elements within this layer
            var elements = ParseElementsInNode(groupNode, nsMgr);

            foreach (var element in elements)
            {
                layer.AddElement(element, false);
            }

            return new ImportedLayerData { Layer = layer, Elements = elements };
        }

        private List<IDrawElement> ParseElementsInNode(XmlNode parentNode, XmlNamespaceManager nsMgr)
        {
            var elements = new List<IDrawElement>();

            // Parse lines
            foreach (XmlNode lineNode in parentNode.SelectNodes(".//svg:line", nsMgr))
            {
                try
                {
                    float x1 = float.Parse(GetAttributeValue(lineNode, "x1", "0"));
                    float y1 = float.Parse(GetAttributeValue(lineNode, "y1", "0"));
                    float x2 = float.Parse(GetAttributeValue(lineNode, "x2", "0"));
                    float y2 = float.Parse(GetAttributeValue(lineNode, "y2", "0"));
                    float z1 = float.Parse(GetAttributeValue(lineNode, "data-z1", "0"));
                    float z2 = float.Parse(GetAttributeValue(lineNode, "data-z2", "0"));

                    string strokeValue = GetAttributeValue(lineNode, "stroke", "black");
                    Color color = ParseColor(strokeValue, Color.Black);
                    int width = (int)float.Parse(GetAttributeValue(lineNode, "stroke-width", "1"));

                    if (strokeValue.ToLower() != "none")
                    {
                        elements.Add(new LineElement(
                            new Vector3D(x1, y1, z1),
                            new Vector3D(x2, y2, z2),
                            false,
                            width,
                            color
                        ));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing line: {ex.Message}");
                }
            }

            // Parse circles
            foreach (XmlNode circleNode in parentNode.SelectNodes(".//svg:circle", nsMgr))
            {
                try
                {
                    float cx = float.Parse(GetAttributeValue(circleNode, "cx", "0"));
                    float cy = float.Parse(GetAttributeValue(circleNode, "cy", "0"));
                    float cz = float.Parse(GetAttributeValue(circleNode, "data-z", "0"));
                    float r = float.Parse(GetAttributeValue(circleNode, "r", "1"));

                    Color stroke = ParseColor(GetAttributeValue(circleNode, "stroke", "black"), Color.Black);
                    int width = (int)float.Parse(GetAttributeValue(circleNode, "stroke-width", "1"));

                    string fillStr = GetAttributeValue(circleNode, "fill", "none");
                    Color fill = ParseColor(fillStr, Color.Transparent);
                    bool hasFill = (fillStr.ToLower() != "none" && fill != Color.Transparent);

                    elements.Add(new CircleElement(
                        new Vector3D(cx, cy, cz),
                        r,
                        false,
                        width,
                        stroke,
                        fill,
                        hasFill,
                        null,
                        false
                    ));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing circle: {ex.Message}");
                }
            }

            // Parse polylines
            foreach (XmlNode polylineNode in parentNode.SelectNodes(".//svg:polyline", nsMgr))
            {
                try
                {
                    string pointsValue = GetAttributeValue(polylineNode, "points", "");
                    if (string.IsNullOrWhiteSpace(pointsValue))
                        continue;

                    string[] points = pointsValue.Trim().Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    string strokeValue = GetAttributeValue(polylineNode, "stroke", "black");
                    Color stroke = ParseColor(strokeValue, Color.Black);
                    int width = (int)float.Parse(GetAttributeValue(polylineNode, "stroke-width", "1"));

                    if (strokeValue.ToLower() != "none")
                    {
                        Vector3D? prevPoint = null;

                        for (int i = 0; i <= points.Length - 2; i += 2)
                        {
                            float x = float.Parse(points[i].Trim());
                            float y = float.Parse(points[i + 1].Trim());
                            float z = 0;

                            if (prevPoint.HasValue)
                            {
                                elements.Add(new LineElement(
                                    prevPoint.Value,
                                    new Vector3D(x, y, z),
                                    false,
                                    width,
                                    stroke
                                ));
                            }
                            prevPoint = new Vector3D(x, y, z);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing polyline: {ex.Message}");
                }
            }

            // Parse rectangles
            foreach (XmlNode rectNode in parentNode.SelectNodes(".//svg:rect", nsMgr))
            {
                try
                {
                    float x = float.Parse(GetAttributeValue(rectNode, "x", "0"));
                    float y = float.Parse(GetAttributeValue(rectNode, "y", "0"));
                    float width = float.Parse(GetAttributeValue(rectNode, "width", "1"));
                    float height = float.Parse(GetAttributeValue(rectNode, "height", "1"));
                    float z1 = float.Parse(GetAttributeValue(rectNode, "data-z1", "0"));
                    float z2 = float.Parse(GetAttributeValue(rectNode, "data-z2", "0"));

                    Color stroke = ParseColor(GetAttributeValue(rectNode, "stroke", "black"), Color.Black);
                    int strokeWidth = (int)float.Parse(GetAttributeValue(rectNode, "stroke-width", "1"));

                    string fillStr = GetAttributeValue(rectNode, "fill", "none");
                    Color fill = ParseColor(fillStr, Color.Transparent);
                    bool hasFill = (fillStr.ToLower() != "none" && fill != Color.Transparent);

                    elements.Add(new RectangleElement(
                        new Vector3D(x, y, z1),
                        new Vector3D(x + width, y + height, z2),
                        false,
                        strokeWidth,
                        stroke,
                        fill,
                        hasFill
                    ));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing rect: {ex.Message}");
                }
            }

            // Parse text
            foreach (XmlNode textNode in parentNode.SelectNodes(".//svg:text", nsMgr))
            {
                try
                {
                    float x = float.Parse(GetAttributeValue(textNode, "x", "0"));
                    float y = float.Parse(GetAttributeValue(textNode, "y", "0"));
                    float z = float.Parse(GetAttributeValue(textNode, "data-z", "0"));
                    string content = textNode.InnerText ?? "";

                    string fontSizeStr = GetAttributeValue(textNode, "font-size", "12");
                    int fontSize = int.Parse(fontSizeStr);

                    Color fill = ParseColor(GetAttributeValue(textNode, "fill", "black"), Color.Black);

                    elements.Add(new LabelElement(
                        new Vector3D(x, y, z),
                        content,
                        fill,
                        fontSize
                    ));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing text: {ex.Message}");
                }
            }

            // Parse <path> elements
            foreach (XmlNode pathNode in parentNode.SelectNodes(".//svg:path", nsMgr))
            {
                try
                {
                    string d = GetAttributeValue(pathNode, "d", "");
                    if (string.IsNullOrWhiteSpace(d))
                        continue;

                    string strokeValue = GetAttributeValue(pathNode, "stroke", "black");
                    Color stroke = ParseColor(strokeValue, Color.Black);
                    int width = (int)float.Parse(GetAttributeValue(pathNode, "stroke-width", "1"));

                    if (strokeValue.ToLower() == "none")
                        continue;

                    // ---- Parse simple "M x y x y x y ... Z" format ----
                    // Remove commas and split on whitespace
                    string cleaned = d.Replace(",", " ");
                    string[] tokens = cleaned.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    List<Vector3D> pts = new List<Vector3D>();

                    int i = 0;
                    while (i < tokens.Length)
                    {
                        string token = tokens[i];

                        if (token == "M" || token == "m")
                        {
                            i++;
                            continue; // next entries will be numbers
                        }
                        else if (token == "Z" || token == "z")
                        {
                            // Close path: add the first point again if needed
                            if (pts.Count > 1)
                                pts.Add(pts[0]);
                            i++;
                            break;
                        }
                        else
                        {
                            // Expect: x y
                            if (i + 1 >= tokens.Length)
                                break;

                            float x = float.Parse(tokens[i], CultureInfo.InvariantCulture);
                            float y = float.Parse(tokens[i + 1], CultureInfo.InvariantCulture);

                            pts.Add(new Vector3D(x, y, 0));

                            i += 2;
                        }
                    }

                    // ---- Draw the lines ----
                    for (int p = 1; p < pts.Count; p++)
                    {
                        elements.Add(new LineElement(
                            pts[p - 1],
                            pts[p],
                            false,
                            width,
                            stroke
                        ));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing path: {ex.Message}");
                }
            }
            return elements;
        }

        private string GetAttributeValue(XmlNode node, string attributeName, string defaultValue)
        {
            XmlAttribute attr = node.Attributes[attributeName];
            return (attr == null || string.IsNullOrEmpty(attr.Value)) ? defaultValue : attr.Value;
        }

        private Color ParseColor(string colorStr, Color defaultColor)
        {
            if (string.IsNullOrEmpty(colorStr) || colorStr.ToLower() == "none")
                return defaultColor;

            try
            {
                return ColorTranslator.FromHtml(colorStr);
            }
            catch
            {
                // Fallback to common color names
                switch (colorStr.ToLower())
                {
                    case "red": return Color.Red;
                    case "blue": return Color.Blue;
                    case "green": return Color.Green;
                    case "cyan": return Color.Cyan;
                    case "yellow": return Color.Yellow;
                    case "magenta": return Color.Magenta;
                    case "black": return Color.Black;
                    case "white": return Color.White;
                    case "orange": return Color.Orange;
                    case "purple": return Color.Purple;
                    case "pink": return Color.Pink;
                    case "gray":
                    case "grey": return Color.Gray;
                    default: return defaultColor;
                }
            }
        }
    }

    // ============================================
    // RESULT CLASSES
    // ============================================
    public class SvgImportResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<ImportedLayerData> ImportedLayers { get; set; } = new List<ImportedLayerData>();
        public List<LayerState> OriginalLayerState { get; set; }
    }

    public class ImportedLayerData
    {
        public ILayer Layer { get; set; }
        public List<IDrawElement> Elements { get; set; }
    }

    // ============================================
    // COMMAND FOR UNDO/REDO
    // ============================================
    //    public sealed class ImportSvgCommand : Arnaoot.VectorGraphics.Commands.ICommand
    //    {
    //        private readonly ILayerManager _layerManager;
    //        private readonly List<LayerState> _beforeState;
    //        private readonly List<LayerState> _afterState;

    //        public string Name { get; }

    //        public ImportSvgCommand(ILayerManager layerManager, List<LayerState> beforeState, string fileName = null)
    //        {
    //            _layerManager = layerManager ?? throw new ArgumentNullException(nameof(layerManager));
    //            _beforeState = beforeState ?? throw new ArgumentNullException(nameof(beforeState));
    //            _afterState = layerManager.SaveLayerState();

    //            Name = string.IsNullOrEmpty(fileName)
    //                ? "Import SVG"
    //                : $"Import {System.IO.Path.GetFileName(fileName)}";
    //        }

    //        public void Execute()
    //        {
    //            _layerManager.RestoreLayerState(_afterState);
    //        }

    //        public void Undo()
    //        {
    //            _layerManager.RestoreLayerState(_beforeState);
    //        }

    //        public bool CanMergeWith(Arnaoot.VectorGraphics.Commands.ICommand next) => false;

    //        public void MergeWith(Arnaoot.VectorGraphics.Commands.ICommand next) { } // No-op - imports don't merge
    //    }
    //}
}