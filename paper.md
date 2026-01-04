---
title: 'Arnaoot: A Lightweight, MIT-Licensed Vector Graphics Engine for Embedded Scientific Visualization'
tags:
  - .NET
  - vector-graphics
  - robotics
  - embedded-systems
  - ground-control-station
authors:
  - name: Hany M. Arnaoot
    orcid: 0000-0001-5684-602X
    affiliation: "Alexandria Higher Institute of Engineering & Technology, Egypt"
date: 2026-01-04
bibliography: paper.bib
---

## Summary

*Arnaoot* is an open-source, cross-platform vector graphics engine written in pure C#/.NET 8, designed for
 **standalone, CPU-first, embedded scientific visualization** — especially robotics ground control stations (GCS),
 telemetry dashboards, and path-planning tools where licensing cost, footprint, and GPU dependence are prohibitive.

Unlike low-level rasterizers (Skia, Cairo) or ecosystem-bound tools (RViz2, MATLAB), 
*Arnaoot* delivers a **full managed scene graph**—including infinite undo/redo, hierarchical layers, 
octree spatial indexing, SVG round-trip, and 3D wireframe primitives—within a **≤12 MB self-contained binary**,
 under the **MIT license**. It runs identically on Windows (including Windows 7), Linux (Ubuntu/Raspberry Pi),
 and macOS, with optional backends: lightweight GDI+ (≤2 MB, Windows-only) or Skia (≤16 MB, cross-platform),
 swappable via a one-line configuration.

The engine is used in real-world robotic path planning (Arnaoot, 2025), naval war simulation (Arnaoot, 2024), and as the rendering core of the *Arnaoot Robotic System* (open-source ROS-like middleware for legacy hardware).

## Statement of Need

Field-deployed robotics and embedded scientific systems require:
- **Zero licensing obligations** (no GPL/LGPL viral terms),
- **Standalone deployment** (no Python/ROS/MATLAB runtime),
- **≤16 MB footprint** (USB-stick installable on 16 GB eMMC devices),
- **GPU independence** (fully functional on CPU-only hardware),
- **Built-in interactivity** (layers, undo, spatial queries).

No existing open-source solution satisfies all five (Table 2 in full paper).
 ROS-based RViz2 demands ≥400 MB and ROS coupling;
 MATLAB is proprietary and ≥2 GB; Cairo/Skia lack scene management;
 LVGL/NanoVG omit undo, layers, and 3D support.

*Arnaoot* fills this gap: it is the 
**only MIT-licensed, <16 MB, scene-graph-enabled vector engine with zero managed dependencies**,
 enabling deployment in resource-constrained or air-gapped environments—particularly valuable in global South academic and field contexts.

## State of the Field
|-------------------|------------|----------|---------------|------------------------|
| Tool              | License    | Footprint| GPU Required? | Scene Graph / Undo?    |
|-------------------|------------|----------|---------------|------------------------|
| RViz2             | BSD-3      | ≥400 MB  | Recommended   | Yes / No               |
| MATLAB            | Commercial | ≥2 GB    | Recommended   | Yes / Yes (GUI only)   |
| Cairo             | LGPL/MPL   | 5–15 MB  | No            | No / No                |
| Skia (standalone) | BSD-3      | 10–25 MB | Optional      | No / No 			     |
| LVGL              | MIT        | ~1 MB    | Optional      | No / No 			     |
| **Arnaoot**       | **MIT**    |**≤12 MB**| **No**        | **Yes/Yes (infinite)** |
|-------------------|------------|----------|---------------|------------------------|
*Arnaoot* is not a replacement for Skia or Cairo — instead, it **provides a managed, modular scene-graph layer atop them**, decoupling application logic from rendering backends via the `IRenderTarget` interface.

## Implementation and Design Highlights

*Arnaoot* enforces five architectural principles to meet embedded constraints:

1. **Value-type geometry**: All spatial types (`Vector3D`, `Rect2`, `Matrix4x4`) are `struct`s, 
enabling **152–168 bytes/element** memory efficiency (including octree, layer metadata, and undo state).

2. **Renderer decoupling (Bridge pattern)**: Rendering backends (GDI+, Skia software/GPU) 
implement `IRenderTarget`. Swapping requires **exactly one line**:  
   ```csharp , this is the actual code from the Windows control GUI
            // Set the appropriate render target
            if (BtnSetRenderSkia.Checked)
            {
                _renderTarget = new SkiaRenderTarget();
            }
            else if (BtnSetRenderGDI.Checked)
            {
                _renderTarget = new WinFormsRenderTarget();
            }

            // Reinitialize render manager with new target, no restart is required , user even does not feel any difference
            _renderManager = new RenderManager(_renderTarget);
            ScheduleInvalidate(InvalidationLevel.Full); //the line that sends draw command and plots thing to screen


## Results Summary

Benchmarks on real-world OpenStreetMap data (1,117 to 1,159,210 primitives) confirm *Arnaoot* meets all design requirements:

- **Memory efficiency**: **152–168 bytes/element** (including scene graph, octree, undo metadata), with linear scaling (R² = 0.9995).  
  → 1.16 M elements fit in **163 MB RAM** — well within 256 MB embedded constraints.

- **CPU-first performance**:  
  - **>320 k primitives/sec** at ≥100 k elements (pure software),  
  - **1.01 M primitives/sec** at scale (1.16 M elements, Skia backend),  
  - **GDI+ faster at small scale** (186 k/sec @ 1k elements), validating optional backend strategy.

- **Predictable overhead**:  
  - Scene management (culling, transforms, command generation) adds only **12–53%  compared with  absolute element drawtime 
  ** — scaling sub-linearly (168 → 152 B/element).  
  - Culling remains ≤23% of frame time even at 1.16 M elements.

- **Cross-platform consistency**:  
  Identical behavior and pixel output on Windows 7/11 and Ubuntu 22.04; self-contained binaries: **11.4 MB (Win)**,
  **14.2 MB (Linux)**.

- **Real-world validation**:  
  Supports interactive pan/zoom/3D rotation of ≥700 k-element maps on Intel i5-5300U (2015 laptop), proving viability for legacy field hardware.

All benchmarks, datasets, and reproduction scripts are in the repository.