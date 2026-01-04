A Lightweight, MIT-Licensed Vector Graphics Engine for Embedded Scientific Visualization
“Sometimes, progress starts with one person refusing to accept the status .”
  
________________________________________
🔍 What is Arnaoot?
Arnaoot is a wireframe-focused, scene-graph-based vector engine for robotics, telemetry, and scientific visualization — built for environments where:
•	Deployment simplicity matters (USB-stick install, no internet),
•	Licensing freedom is non-negotiable (MIT, no LGPL/GPL obligations),
•	Hardware is constrained (256 MB RAM, CPU-only),
•	Real-time interaction is required (pan/zoom/updates at ≤50 ms).
It is not a general-purpose renderer like Skia or Cairo.
It is not a shaded 3D engine like Unity or OGRE.
It is a managed, modular scene-graph layer — giving you:
•	🧩 Infinite undo/redo (command merging included)
•	🗺️ Hierarchical layers + octree spatial indexing
•	📐 SVG round-trip + 3D wireframe primitives (orthographic & perspective)
•	🔄 One-line renderer swap: GDI+ (≤1 MB, Windows) or Skia (≤12 MB, cross-platform)
•	💡 Zero managed dependencies — pure .NET Standard 2.0
✅ Why “Arnaoot”?
This engine is named after its creator,  to honor years of solo development in resource-constrained environments where no existing tool fit. The name is a reminder: sometimes, progress starts with one person refusing to accept the status quo.
________________________________________

🛠️ Quick Start (5 lines)
  Arnaoot.VectorGraphics.UI.EngineControl MyDataDisplayer = new Arnaoot.VectorGraphics.UI.EngineControl();
ILayer MapLayer=  MyDataDisplayer.UsedLayerManager.AddLayer ("Map");
  MapLayer.AddElement ( new LineElement (  new Vector3D (10,5,20), new Vector3D(30, 40, 50),false,1,Arnaoot.VectorGraphics.Abstractions.ArgbColor.Black) , false); // wireframe only
 //
  MyDataDisplayer.Dock = DockStyle.Fill;
  this.Controls.Add(MyDataDisplayer);
