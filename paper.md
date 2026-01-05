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

| Tool              | License    | Footprint| GPU Required? | Scene Graph / Undo?    |
|-------------------|------------|----------|---------------|------------------------|
| RViz2             | BSD-3      | ≥400 MB  | Recommended   | Yes / No               |
| MATLAB            | Commercial | ≥2 GB    | Recommended   | Yes / Yes (GUI only)   |
| Cairo             | LGPL/MPL   | 5–15 MB  | No            | No / No                |
| Skia (standalone) | BSD-3      | 10–25 MB | Optional      | No / No     			     |
| LVGL              | MIT        | ~1 MB    | Optional      | No / No 		    	     |
| **Arnaoot**       | **MIT**    |**≤12 MB**| **No**        | **Yes/Yes (infinite)** |

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

3-Per-layer lazy octree: Octrees rebuild only when layers change. Static layers (e.g., maps) incur zero CPU cost during robot motion. Culling stays ≤23% of frame time even at 1.16 M elements.
4-Zero-allocation pooling: A CachingHelper reuses immutable pens, brushes, and fonts via string keys — achieving zero GC allocations after warm-up (<80 ms), critical for jitter-free rendering.
5- Screen-space clipping before command emission: Liang–Barsky clipping in the scene graph reduces backend draw calls by 60–95%, preventing overflow during extreme zoom (e.g., 10 km → 1 m).
The engine supports 3D wireframe visualization (orthographic & perspective), though intentionally omits per-pixel depth sorting to preserve determinism, layer semantics, and CPU-first performance. It is optimized for polylines (e.g., trajectories, OSM roads), which constitute >95% of robotics GCS workloads.

Performance (Empirical Validation):-
Benchmarks on real-world OpenStreetMap data (1.1k–1.16M elements) on an Intel i7–1185G7 show:

Memory: Linear scaling (R² = 0.9995), 152 B/element at scale (176 MB for 1.16 M elements).
Throughput: >320 k primitives/sec in pure software (≥1.0 M/sec with Skia at scale); GDI+ faster for <100 k elements (186 k/sec), validating optional backend strategy.
Cross-platform: Identical behavior on Windows 11, Windows 7, and Ubuntu 22.04; self-contained binaries: 11.4 MB (Win), 14.2 MB (Linux).
Full benchmark datasets, scripts, and stress-test SVGs are in the repository.

Availability
Repository: https://github.com/HanyArnaoot/Arnaoot
License: MIT
Documentation: API docs, usage examples, and demo projects (WinForms, Avalonia, headless export) included in /docs and /examples.
Dependencies: .NET 8 SDK only (no NuGet packages required for core features).
Platforms: Windows (7+), Linux (x64/ARM64), macOS (source-portable).
Build: dotnet publish -c Release -r <runtime> --self-contained true
A NuGet package (Arnaoot.Engine) is planned for Q2 2026.

Use Cases
Robotics GCS: Real-time path planning, dynamic obstacle overlay, sensor FOV visualization (Arnaoot, 2025).
Military simulation: Naval war training simulator with 2.5D battlefield rendering (Arnaoot, 2024).
Education: Low-bandwidth deployment in Egyptian engineering labs where ROS/MATLAB are inaccessible.
Acknowledgements
The author confirms no institutional funding was received. Development was self-funded; this submission is made under JOSS’s zero-APC policy.
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


