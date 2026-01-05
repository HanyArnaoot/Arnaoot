Memory consumption was measured by forcing full garbage collection before and after scene loading, then calculating the difference divided by element count.
This isolates the engine's per-element overhead from .NET runtime baseline and temporary allocation during parsing. 
The attached  Tables show Seven scene memory load.

this is the code used for RAM measurement
// Measure PROPERLY
GC.Collect(); // Clean up any garbage first
GC.WaitForPendingFinalizers();
GC.Collect();
//
long before = GC.GetTotalMemory(true); // 'true' forces full collection

var importer = new Arnaoot.VectorGraphics.Formats.Svg.SvgImporter();
importer.LoadFromSvg(filePath, _document.LayersManager );
//
_document.FilePath = filePath;
_document.IsModified = false;
//
//
GC.Collect(); // Clean temp allocations
GC.WaitForPendingFinalizers();
GC.Collect();
//
long after = GC.GetTotalMemory(true);
int ElementsCount = _document.LayersManager.GetAllElements().Count();
double bytesPerElement = (after - before) / ElementsCount;
Console.WriteLine($"{bytesPerElement:F0} bytes per element");
