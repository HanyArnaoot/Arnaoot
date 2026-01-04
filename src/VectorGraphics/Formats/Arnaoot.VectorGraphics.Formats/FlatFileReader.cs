using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Elements;
using Arnaoot.VectorGraphics.Scene;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;


  namespace Arnaoot.VectorGraphics.Formats.FlatFile
    {
        public class Import
        {
            #region import flat lat long files
            public Vector3D Transform(Vector3D origin, double longitude, double latitude, double elevation = 0)
            {

                const double MetersPerDegree = 111320.0;

                // Convert reference latitude to radians and calculate cosine
                double refLatRadians = origin.Y * Math.PI / 180.0;
                double cosRefLat = Math.Cos(refLatRadians);

                // Calculate Cartesian coordinates in meters
                double x = (longitude - origin.X) * cosRefLat * MetersPerDegree;
                double y = (latitude - origin.Y) * MetersPerDegree;
                //
                return new Vector3D((float)x, (float)y, (float)elevation);
            }
            public HashSet<IDrawElement> AddfromFlatFile(string fileName, Layer DrawLayer)
            {
                HashSet<IDrawElement> DrawElements = new HashSet<IDrawElement>();
                try
                {
                    // --- Read longitude/latitude pairs ---
                    var points = File.ReadAllLines(fileName)
                        .Select(line => line.Trim())
                        .Where(line => line.Length > 0)
                        .Where(line => !line.StartsWith("segment", StringComparison.OrdinalIgnoreCase)) // Skip metadata lines
                        .Select(line =>
                        {
                            var parts = line.Split(' ', '\t', ',');
                            double lon = double.Parse(parts[0], CultureInfo.InvariantCulture);
                            double lat = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            return (lon, lat);
                        })
                        .ToList();

                    if (points.Count < 2)
                    {
                        Console.WriteLine("Not enough points to draw lines.");
                        return DrawElements;
                    }
                    //
                    // --- Compute dataset center as origin ---
                    double avgLon = points.Average(p => p.lon);
                    double avgLat = points.Average(p => p.lat);
                    Vector3D origin = new Vector3D((float)avgLon, (float)avgLat, 0);

                    Console.WriteLine($"Origin (lon,lat): ({avgLon:F3}, {avgLat:F3})");

                    // --- Transform and draw all points ---
                    Vector3D lastPoint = Transform(origin, points[0].lon, points[0].lat);
                    //DrawElements.Clear();
                    for (int i = 1; i < points.Count; i++)
                    {
                        Vector3D newPoint = Transform(origin, points[i].lon, points[i].lat);
                    LineElement element = new LineElement(lastPoint, newPoint,false,    1, ArgbColor.Blue );
                     DrawLayer.AddElement(element, false);
                        lastPoint = newPoint;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading flat file: " + ex.Message);
                }
                return DrawElements;
            }
            #endregion

        }
    }
 
