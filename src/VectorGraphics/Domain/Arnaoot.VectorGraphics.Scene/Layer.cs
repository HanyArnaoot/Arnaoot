
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Arnaoot.VectorGraphics.Abstractions;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;
namespace Arnaoot.VectorGraphics.Scene
{
    public class LayerState
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public bool Locked { get; set; }
        public int Order { get; set; }  // Index in save order
        public ArgbColor LayerColor { get; set; }
    }
    public interface ILayer
    {
        // === Identity ===
        Guid Id { get; }

        // === Properties ===
        string Name { get; set; }
        bool Visible { get; set; }
        bool Locked { get; set; }
        ArgbColor LayerColor { get; set; }

        // === Read-only access ===
        IReadOnlyCollection<IDrawElement> Elements { get; }
        BoundingBox3D Bounds { get; }

        // === Events ===
        event Action<IDrawElement> ElementModified;

        // === Element Management ===
        bool AddElement(IDrawElement element, bool batchUpdate);
        bool RemoveElement(IDrawElement element);
        void Clear();

        // === Selection ===
        void SelectAll();
        void ClearSelected();
        IEnumerable<IDrawElement> GetSelected();
        IReadOnlyCollection<IDrawElement> GetLayerElements();

        // === Spatial Query ===
        IReadOnlyCollection<IDrawElement> QueryElementsInFrustum(
            BoundingBox3D frustum);

        // === Maintenance ===
        void RebuildBounds();

        // === Utility ===
        string ToString();
    }

    // === LAYER ===
    public class Layer : ILayer
    {
        // === Identity ===
        public Guid Id { get; } = Guid.NewGuid();

        // === Properties ===
        public string Name { get; set; }
        public bool Visible { get; set; } = true;
        public bool Locked { get; set; } = false;
        public ArgbColor LayerColor { get; set; } = ArgbColor.Black;

        // === Elements ===
        private readonly HashSet<IDrawElement> _elements = new HashSet<IDrawElement>();
        private Octree<IDrawElement> _spatialIndex;
        private bool _isDirty = true;
        private BoundingBox3D _bounds = BoundingBox3D.Empty;

        // === Read-only access ===
        public IReadOnlyCollection<IDrawElement> Elements =>
            new ReadOnlyCollection<IDrawElement>(_elements.ToList());

        public BoundingBox3D Bounds => _bounds;

        // Event to notify the LayerManager or UI that the content bounds have changed
        public event Action<IDrawElement> ElementModified;

        // === Constructor ===
        public Layer(string name = "Layer")
        {
            Name = name;
        }

        // === Element Management ===
        public bool AddElement(IDrawElement element, bool ScheduleInvalidateNeed)
        {
            if (Locked) return false;
            if (!_elements.Add(element)) return false; // Already exists

            // CRITICAL: Subscribe to the element's change event
            element.ElementBoundsChanged += OnElementBoundsChanged;
            if (ScheduleInvalidateNeed == false)
            {
                return true;
            }
            else
            {
                UpdateBounds(element.GetBounds());
                _isDirty = true;
                ElementModified?.Invoke(element);
                return true;
            }
        }

        public bool RemoveElement(IDrawElement element)
        {
            if (Locked) return false;
            if (!_elements.Remove(element)) return false;

            // CRITICAL: Unsubscribe from the element's change event
            element.ElementBoundsChanged -= OnElementBoundsChanged;

            _isDirty = true;
            RebuildBounds(); // Recalculate bounds since an element was removed
            ElementModified?.Invoke(element);
            return true;
        }

        public void Clear()
        {
            if (Locked) return;
            // Ensure we unsubscribe from all elements before clearing
            foreach (var element in _elements)
            {
                element.ElementBoundsChanged -= OnElementBoundsChanged;
            }

            _elements.Clear();
            _bounds = BoundingBox3D.Empty;
            _spatialIndex?.Clear();
            _spatialIndex = null;
            _isDirty = true;
            ElementModified?.Invoke(null); // Signal a major change
        }

        #region Selection
        public void ClearSelected() => _elements.ToList().ForEach(e => e.IsSelected = false);
        public void SelectAll() => _elements.ToList().ForEach(e => e.IsSelected = true);
        public IEnumerable<IDrawElement> GetSelected() => _elements.Where(e => e.IsSelected);
        public IReadOnlyCollection<IDrawElement> GetLayerElements() => Elements;
        #endregion


        #region Spatial Query
         public IReadOnlyCollection<IDrawElement> QueryElementsInFrustum(BoundingBox3D frustum)
        {
            if (!_bounds.IntersectsWith(frustum))
                return Array.Empty<IDrawElement>();

            if (_isDirty && !_bounds.IsEmpty())
            {
                // The spatial index must be rebuilt or initialized
                _spatialIndex = new  Octree<IDrawElement>(_bounds, 8);
                foreach (var elem in _elements)
                    _spatialIndex.Insert(elem);
                _isDirty = false;
            }

            if (_spatialIndex == null)
                return Array.Empty<IDrawElement>();

            var hits = _spatialIndex.Query(frustum);
            var filtered = hits.Where(e => frustum.IntersectsWith(e.GetBounds())).ToList();
            return new ReadOnlyCollection<IDrawElement>(filtered);
        }
        #endregion

        #region  === Internal Helpers ===

        /// <summary>
        /// Handles the event when an element's position or size changes.
        /// This keeps the layer's overall bounds and spatial index accurate.
        /// </summary>
        private void OnElementBoundsChanged(IDrawElement element)
        {
            // An element moved or resized, so the spatial index is now invalid.
            _isDirty = true;

            // Since the element might have moved outside the current layer bounds,
            // we must force a full bounds recalculation.
            RebuildBounds();

            // Propagate the event so the LayerManager or Renderer knows to re-render
            ElementModified?.Invoke(element);
        }

        private void UpdateBounds(BoundingBox3D newBounds)
        {
            if (_bounds.IsEmpty())
                _bounds = newBounds;
            else
                _bounds = BoundingBox3D.Union(_bounds, newBounds);
        }

        public void RebuildBounds()
        {
            // Completely recalculate the aggregated bounds from all elements.
            _bounds = _elements.Aggregate(
                BoundingBox3D.Empty,
                (b, e) => b.IsEmpty() ? e.GetBounds() : BoundingBox3D.Union(b, e.GetBounds())
            );

            // Invalidate the existing spatial index, forcing a rebuild on the next query.
            _spatialIndex?.Clear();
            _isDirty = true;
        }

        public override string ToString() => $"{Name} ({Id:B})";
    }
#endregion

}
