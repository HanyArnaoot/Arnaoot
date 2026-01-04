# Benchmarks for Arnaoot Vector Graphics Engine

This directory contains all datasets, scripts, and raw results used to validate *Arnaoot*’s performance and memory claims in the JOSS paper.

## 📁 Contents
|---------------------|-----------------------------------------------------------------------------------------------|
| File                | Description                                                                                   |
|---------------------|-----------------------------------------------------------------------------------------------|
| `scenes/`           | SVG files (OpenStreetMap-derived) used for testing — unsimplified, real-world geographic data |
| `scripts/`          | Benchmark runner (`BenchmarkRunner.cs`) and data exporter (`ExportToCsv.cs`)                  |
| `results/`          | Raw timing/memory outputs (CSV format) — one file per backend/test scene                      |
| `hardware_specs.md` | Detailed hardware and OS configuration used in paper                                          |
|---------------------|-----------------------------------------------------------------------------------------------|


## 🧪 How to Reproduce

### Prerequisites
- .NET 8 SDK (https://dotnet.microsoft.com/download/dotnet/8.0)
- SkiaSharp if want to use skia rendering system
  
- (Optional) SkiaSharp.NativeAssets.Linux (for Linux runs)

### Steps
1. Clone the repo and navigate to `/benchmarks/scripts`:
   ```bash
   git clone https://github.com/HanyArnaoot/Arnaoot.git
   cd Arnaoot/benchmarks/scripts
