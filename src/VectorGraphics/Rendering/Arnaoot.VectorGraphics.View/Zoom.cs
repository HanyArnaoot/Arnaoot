using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Arnaoot.VectorGraphics.Scene;
using Arnaoot.VectorGraphics.Abstractions;
namespace Arnaoot.VectorGraphics.View
{
    public interface IZooming
    {
        // History
        IViewSettings Zoom(IViewSettings current, float zoomFactor, Vector2D pivotPixel);
        IViewSettings ZoomIn(IViewSettings current, float pivotXPixel = -1F, float pivotYPixel = -1F);
        IViewSettings ZoomOut(IViewSettings current, float pivotXPixel = -1F, float pivotYPixel = -1F);
        IViewSettings ZoomPrevious(IViewSettings current);

        // Extents + Region
        IViewSettings ZoomExtents(IViewSettings current, ILayerManager layerManager, float paddingPercent);
        BoundingBox3D GetExtents(ILayerManager usedLayerManager);
        IViewSettings GetRegionViewSettings(IViewSettings current, BoundingBox3D bounds, float padding = 5f);
        void PushToHistory(IViewSettings settings);
        IViewSettings ZoomToRectangle(IViewSettings CurrentViewSettings, Vector3D RectStart, Vector3D RectEnd);
    }
    public class Zooming : IZooming
    {
        #region variable Declare
        private const int MAX_HISTORY_SIZE = 20;
        public LimitedStack<IViewSettings> ZoomHistory = new LimitedStack<IViewSettings>(MAX_HISTORY_SIZE);
        //private VectorGraphicsEngine _MyControl;

        #endregion
        #region Zoom
        public Zooming()
        {
            //_MyControl = MyControl;
        }

        #region zoom history

        public class LimitedStack<T>
        {
            private readonly LinkedList<T> _items;
            private readonly int _maxSize;

            public LimitedStack(int maxSize)
            {
                _maxSize = maxSize;
                _items = new LinkedList<T>();
            }

            public void Push(T item)
            {
                _items.AddFirst(item);

                if (_items.Count > _maxSize)
                {
                    _items.RemoveLast(); // Remove oldest
                }
            }

            public T Pop()
            {
                if (_items.Count == 0)
                    throw new InvalidOperationException("Stack is empty");

                var result = _items.First.Value;
                _items.RemoveFirst();
                return result;
            }

            public int Count => _items.Count;
            public bool Any() => _items.Count > 0;
        }

        #endregion


        public void PushToHistory(IViewSettings settings)
        {
            ZoomHistory.Push(settings);
        }
        public IViewSettings Zoom(IViewSettings CurrentViewSettings, float zoomFactor, Vector2D PivotPointPixel)
        {
            // Create temporary new settings with updated zoom
             ViewSettings newSettings = new  ViewSettings(
                CurrentViewSettings.UsableViewport,
                CurrentViewSettings.ZoomFactor * zoomFactor,
                CurrentViewSettings.ShiftWorld,
                CurrentViewSettings.RotationAngle,
                CurrentViewSettings.RotateAroundPoint);

            // Use PictToViewPlane to avoid rotation-related depth ambiguity
             Vector3D PivotRealBefore = CurrentViewSettings.PictToViewPlane(PivotPointPixel, 0.0f);
             Vector3D PivotRealAfter = newSettings.PictToViewPlane(PivotPointPixel, 0.0f);

            // Calculate shift adjustment to keep pivot point stable
            float x = CurrentViewSettings.ShiftWorld.X - (PivotRealBefore.X - PivotRealAfter.X);
            float y = CurrentViewSettings.ShiftWorld.Y - (PivotRealBefore.Y - PivotRealAfter.Y);
            float z = CurrentViewSettings.ShiftWorld.Z - (PivotRealBefore.Z - PivotRealAfter.Z);

            // Return final settings with corrected shift
            return new  ViewSettings(
                CurrentViewSettings.UsableViewport,
                CurrentViewSettings.ZoomFactor * zoomFactor,
                new  Vector3D(x, y, z),
                CurrentViewSettings.RotationAngle,
                CurrentViewSettings.RotateAroundPoint);
        }

        public IViewSettings ZoomIn(IViewSettings CurrentViewSettings, float pivotXPixel = -1F, float pivotYPixel = -1F)
        {
            return Zoom(CurrentViewSettings, 1.1F, new Vector2D(pivotXPixel, pivotYPixel));
        }
        public IViewSettings ZoomToRectangle(IViewSettings CurrentViewSettings, Vector3D RectStart, Vector3D RectEnd)
        {
             BoundingBox3D extents =new BoundingBox3D(RectStart, RectEnd);
            if (extents.IsEmpty())
            {
                return CurrentViewSettings;
            }
            //
            ZoomHistory.Push(CurrentViewSettings.Clone());//PushToHistory(CurrentViewSettings); //
            CurrentViewSettings = GetRegionViewSettings(CurrentViewSettings, extents, 5);
            CurrentViewSettings.RotationAngle = new Vector3D(0F, 0F, 0F);
            return CurrentViewSettings;
         }
        public IViewSettings ZoomOut(IViewSettings CurrentViewSettings, float pivotXPixel = -1F, float pivotYPixel = -1F)
        {
            return Zoom(CurrentViewSettings, 0.9F, new Vector2D(pivotXPixel, pivotYPixel));
        }
        public IViewSettings ZoomPrevious(IViewSettings CurrentViewSettings)
        {
            if (ZoomHistory.Count > 0)
            {
                IViewSettings PreviousViewSetting = ZoomHistory.Pop();
                return CurrentViewSettings = PreviousViewSetting.Clone();
                //ScheduleInvalidate();
            }
            return CurrentViewSettings;
        }

        public IViewSettings ZoomExtents(IViewSettings CurrentViewSettings,  ILayerManager useLayerManager, float paddingPercent)
        {
            BoundingBox3D extents = GetExtents(useLayerManager);
            if (extents.IsEmpty())
            {
                return CurrentViewSettings;
            }
            //
            ZoomHistory.Push(CurrentViewSettings.Clone());//PushToHistory(CurrentViewSettings); //
            CurrentViewSettings = GetRegionViewSettings(CurrentViewSettings, extents, paddingPercent);
            CurrentViewSettings.RotationAngle = new Vector3D(0F, 0F, 0F);
            return CurrentViewSettings;
        }


        #endregion
        public BoundingBox3D GetExtents( ILayerManager usedlayermanager)
        {
            BoundingBox3D VisbileBoxBounds = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(1, 1, 1));
            if (usedlayermanager == null)
            {
                return VisbileBoxBounds;
            }
            //
            //IEnumerable<Layer> visibleLayers = usedlayermanager.Layers.Values.Where(e => e.Visible);
            //{
            //    return VisbileBoxBounds;
            //}
            List< ILayer> visibleLayers = usedlayermanager.Layers
                                                                 .Where(l => l.Visible)
                                                                 .ToList();
            //
            if (visibleLayers == null || visibleLayers.Count == 0)
            {
                return new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(50, 50, 50));
            }
            BoundingBox3D bounds = visibleLayers.First().Bounds;
            foreach ( Layer layer in visibleLayers.Skip(1))
            {
                BoundingBox3D elementBounds = layer.Bounds;
                bounds = BoundingBox3D.Union(bounds, elementBounds);
            }
            //
            return bounds;
        }

        public IViewSettings GetRegionViewSettings(IViewSettings current, BoundingBox3D bounds, float padding = 5f)
        {
            if (bounds.IsEmpty()) return current;

            float pad = (100.0f + padding) / 100; // e.g., 5% padding → scale to 95% of viewport
            float worldW = bounds.Max.X - bounds.Min.X;
            float worldH = bounds.Max.Y - bounds.Min.Y;

            // Guard against degenerate bounds
            if (worldW <= 0) worldW = 1;
            if (worldH <= 0) worldH = 1;

            float scaleX = current.UsableViewport.Width / (worldW * pad);
            float scaleY = current.UsableViewport.Height / (worldH * pad);
            float zoom = Math.Min(scaleX, scaleY);

            // Center in world space
            Vector3D worldCenter = bounds.Center;

            // Screen center in *drawing coordinates* (relative to UsableViewport origin)
            float screenCenterX = current.UsableViewport.Left + current.UsableViewport.Width / 2;
            float screenCenterY = current.UsableViewport.Top + current.UsableViewport.Height / 2;

            // Solve for shift such that: RealToPict(worldCenter) = (screenCenterX, screenCenterY)
            // From RealToPict logic (simplified 2D ortho, no rotation):
            float shiftX = (screenCenterX / zoom) - worldCenter.X;
            float shiftY = -(screenCenterY / zoom) - worldCenter.Y;

            // Keep Z shift unchanged or center at bounds mid-Z
            float shiftZ = worldCenter.Z; // or current.ShiftWorld.Z

            IViewSettings  newSettings = current.Clone();
            newSettings.ZoomFactor = new Vector3D(zoom, zoom, zoom); // Z zoom can be zoom or 1 — ortho ignores it
            newSettings.ShiftWorld = new Vector3D(shiftX, shiftY, shiftZ);
            newSettings.RotationAngle = new Vector3D();

            return newSettings;
        }
    }
}
