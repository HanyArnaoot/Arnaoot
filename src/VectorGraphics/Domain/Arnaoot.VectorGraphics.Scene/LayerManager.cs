using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
 using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Scene
{
    /// <summary>
    /// Interface for managing layers in a vector drawing system.
    /// Provides layer operations, element management, and queries.
    /// </summary>
    public interface ILayerManager
    {
        // === Public Access ===

        /// <summary>
        /// Gets a read-only list of all layers.
        /// </summary>
        IReadOnlyList<ILayer> Layers { get; }

        /// <summary>
        /// Gets layers in their render order (bottom to top).
        /// </summary>
        IEnumerable<ILayer> LayersInRenderOrder { get; }

        /// <summary>
        /// Gets or sets the currently active layer.
        /// </summary>
        ILayer ActiveLayer { get; }

        // === Events ===

        /// <summary>
        /// Occurs when a layer is added.
        /// </summary>
        event Action<ILayer, int> LayerAdded;

        /// <summary>
        /// Occurs when a layer is removed.
        /// </summary>
        event Action<ILayer, int> LayerRemoved;

        /// <summary>
        /// Occurs when a layer's order changes.
        /// </summary>
        event Action<ILayer, int, int> LayerReordered;

        /// <summary>
        /// Occurs when the active layer changes.
        /// </summary>
        event Action<ILayer, ILayer> ActiveLayerChanged;

        /// <summary>
        /// Occurs when any layer-related change happens.
        /// </summary>
        event Action LayersChanged;

        // === Core Operations ===

        /// <summary>
        /// Adds a new layer with the specified name.
        /// </summary>
        /// <param name="name">The name of the layer. If null, a default name is generated.</param>
        /// <returns>The newly created layer.</returns>
        ILayer AddLayer(string name = null);

        /// <summary>
        /// Removes the specified layer.
        /// </summary>
        /// <param name="layer">The layer to remove.</param>
        /// <returns>True if the layer was removed; false otherwise.</returns>
        bool RemoveLayer(ILayer layer);

        /// <summary>
        /// Sets the specified layer as the active layer.
        /// </summary>
        /// <param name="layer">The layer to set as active.</param>
        void SetActiveLayer(ILayer layer);

        // === Reordering ===

        /// <summary>
        /// Moves the layer to the front (top of render order).
        /// </summary>
        void BringToFront(ILayer layer);

        /// <summary>
        /// Moves the layer to the back (bottom of render order).
        /// </summary>
        void SendToBack(ILayer layer);

        /// <summary>
        /// Moves the layer up one position in render order.
        /// </summary>
        void MoveUp(ILayer layer);

        /// <summary>
        /// Moves the layer down one position in render order.
        /// </summary>
        void MoveDown(ILayer layer);

        /// <summary>
        /// Moves the layer to a specific index in the render order.
        /// </summary>
        void MoveLayer(ILayer layer, int newIndex);

        // === Element Bulk Operations ===
        void ClearSelection();
        /// <summary>
        /// Adds multiple elements to the active layer.
        /// </summary>
        void AddElementsToActiveLayer(IEnumerable<IDrawElement> elements);

        /// <summary>
        /// Moves multiple elements from their current layers to the target layer.
        /// </summary>
        void MoveElementsToLayer(IEnumerable<IDrawElement> elements, Layer targetLayer);

        // === State Management ===

        /// <summary>
        /// Saves the current state of all layers (for undo/redo).
        /// </summary>
        /// <returns>A list of layer states.</returns>
        List<LayerState> SaveLayerState();

        /// <summary>
        /// Restores layers to a previously saved state.
        /// </summary>
        /// <param name="state">The saved layer state to restore.</param>
        void RestoreLayerState(List<LayerState> state);

        // === Queries ===

        /// <summary>
        /// Gets a layer by its unique identifier.
        /// </summary>
        /// <param name="id">The layer's GUID.</param>
        /// <returns>The layer if found; null otherwise.</returns>
        Layer GetLayerById(Guid id);

        /// <summary>
        /// Gets all elements from all layers.
        /// </summary>
        IEnumerable<IDrawElement> GetAllElements();

        /// <summary>
        /// Gets all elements from visible layers only.
        /// </summary>
        IReadOnlyCollection<IDrawElement> GetVisibleElements();

        /// <summary>
        /// Gets the combined bounding box of all visible layers.
        /// </summary>
        BoundingBox3D GetVisibleBounds();

        /// <summary>
        /// Queries visible elements within the specified 3D frustum/box.
        /// </summary>
        IReadOnlyCollection<IDrawElement> QueryVisibleElementsInBox(BoundingBox3D frustum);

        /// <summary>
        /// Finds the topmost element at the given world point within tolerance.
        /// </summary>
        IDrawElement FindElementAtPoint(Vector3D worldPoint, float worldTolerance);

        // === Bulk Operations ===

        /// <summary>
        /// Removes all layers and clears the layer manager.
        /// </summary>
        void RemoveAllLayers();

        /// <summary>
        /// Recalculates bounding boxes for all layers.
        /// </summary>
        void UpdateAllLayersBounds();
    }

// === LAYER MANAGER ===
public class LayerManager : ILayerManager
    {
        // === Storage ===
        private readonly List<ILayer> _layers = new List<ILayer>();
        private readonly Dictionary<Guid, Layer> _byId = new Dictionary<Guid, Layer>();

        // === State ===
        private ILayer _activeLayer;

        // === Public Access ===
        public IReadOnlyList<ILayer> Layers => _layers.AsReadOnly();
        public IEnumerable<ILayer> LayersInRenderOrder => _layers; // List order = draw order
        public ILayer ActiveLayer
        {
            get => _activeLayer;
            private set
            {
                if (_activeLayer == value) return;
                ILayer  old = _activeLayer;
                _activeLayer = value;
                ActiveLayerChanged?.Invoke(old, value);
            }
        }

        // === Events ===
        public event Action<ILayer, int> LayerAdded;
        public event Action<ILayer, int> LayerRemoved;
        public event Action<ILayer, int, int> LayerReordered;
        public event Action<ILayer, ILayer> ActiveLayerChanged;
        public event Action LayersChanged;

        // === Construction ===
        public LayerManager()
        {
            AddLayer("Background"); // Default first layer
        }

        // === Core Operations ===
        public ILayer AddLayer(string name = null)
        {
            name = name ?? $"Layer {_layers.Count}";
            name = EnsureUniqueName(name);

            var layer = new Layer(name);
            var index = _layers.Count;

            _layers.Add(layer);
            _byId[layer.Id] = layer;

            if (_layers.Count == 1)
                ActiveLayer = layer;

            // Listen to the ElementModified event from the layer to signal global changes
            layer.ElementModified += (element) => LayersChanged?.Invoke();

            LayerAdded?.Invoke(layer, index);
            LayersChanged?.Invoke();
            return layer;
        }

        private string EnsureUniqueName(string baseName)
        {
            string name = baseName;
            int counter = 1;
            while (_layers.Any(l => l.Name == name))
            {
                name = $"{baseName} ({counter++})";
            }
            return name;
        }

        public bool RemoveLayer(ILayer layer)
        {
            if (layer == null || _layers.Count <= 1 || !_layers.Contains(layer))
                return false;

            int index = _layers.IndexOf(layer);
            _layers.RemoveAt(index);
            _byId.Remove(layer.Id);

            if (ActiveLayer == layer)
                ActiveLayer = _layers.FirstOrDefault(l => !l.Locked) ?? _layers.First();

            LayerRemoved?.Invoke(layer, index);
            LayersChanged?.Invoke();
            return true;
        }

        public void SetActiveLayer(ILayer layer)
        {
            if (layer == null || !_layers.Contains(layer) || layer.Locked)
                return;

            ActiveLayer = layer;
        }

        // === Reordering ===
        public void BringToFront(ILayer layer) => MoveLayer(layer, _layers.Count - 1);
        public void SendToBack(ILayer layer) => MoveLayer(layer, 0);
        public void MoveUp(ILayer layer) => MoveLayer(layer, _layers.IndexOf(layer) + 1);
        public void MoveDown(ILayer layer) => MoveLayer(layer, _layers.IndexOf(layer) - 1);

        public void MoveLayer(ILayer layer, int newIndex)
        {
            if (layer == null || newIndex < 0 || newIndex >= _layers.Count)
                return;

            int oldIndex = _layers.IndexOf(layer);
            if (oldIndex == newIndex || oldIndex == -1)
                return;

            _layers.RemoveAt(oldIndex);
            _layers.Insert(newIndex, layer);

            LayerReordered?.Invoke(layer, oldIndex, newIndex);
            LayersChanged?.Invoke();
        }

        // === Element Bulk Operations ===
        public void AddElementsToActiveLayer(IEnumerable<IDrawElement> elements)
        {
            if (ActiveLayer == null || ActiveLayer.Locked) return;

            bool changed = false;
            foreach (var element in elements)
            {
                if (ActiveLayer.AddElement(element, false))
                    changed = true;
            }
            if (changed)
                LayersChanged?.Invoke();
        }
        public void ClearSelection( )
        {
            foreach (var layer in _layers)
            {
                layer.ClearSelected();
            }
        }
        public void MoveElementsToLayer(IEnumerable<IDrawElement> elements, Layer targetLayer)
        {
            if (targetLayer == null || targetLayer.Locked) return;

            bool changed = false;
            foreach (var element in elements)
            {
                // Iterate over a copy of the list to allow modification (removal)
                foreach (var layer in _layers.ToList())
                {
                    if (layer.RemoveElement(element))
                    {
                        targetLayer.AddElement(element, false);
                        changed = true;
                        // Move the element out of one layer and into the target layer
                        break;
                    }
                }
            }
            if (changed)
                LayersChanged?.Invoke();
        }

        // === Save / Restore Layer State ===
        public List<LayerState> SaveLayerState()
        {
            return _layers
                .Select((layer, index) => new LayerState
                {
                    Id = layer.Id,
                    Name = layer.Name,
                    Visible = layer.Visible,
                    Locked = layer.Locked,
                    Order = index,
                    LayerColor = layer.LayerColor
                })
                .ToList();
        }

        public void RestoreLayerState(List<LayerState> state)
        {
            if (state == null) return;

            // Reorder existing layers by saved Order and map properties
            var ordered = new List<Layer>();
            var stateMap = state.ToDictionary(s => s.Id, s => s);

            foreach (var savedState in state.OrderBy(s => s.Order))
            {
                if (_byId.TryGetValue(savedState.Id, out var layer))
                {
                    // Apply properties to existing layer
                    layer.Name = savedState.Name;
                    layer.Visible = savedState.Visible;
                    layer.Locked = savedState.Locked;
                    layer.LayerColor = savedState.LayerColor;
                    ordered.Add(layer);
                }
            }

            // Remove layers not in saved state
            var toRemove = _layers.Except(ordered).ToList();
            foreach (var layer in toRemove)
                RemoveLayer(layer);

            // Update the internal list to reflect the new order and composition
            _layers.Clear();
            _layers.AddRange(ordered);

            // Reset active layer if possible
            var activeState = state.FirstOrDefault(s => _byId.ContainsKey(s.Id) && _byId[s.Id] == ActiveLayer);
            if (activeState != null && !_byId[activeState.Id].Locked)
                ActiveLayer = _byId[activeState.Id];
            else if (ActiveLayer == null && _layers.Count > 0)
                ActiveLayer = _layers.First(); // Ensure there is always an active layer if possible

            LayersChanged?.Invoke();
        }

        // === Queries ===
        public Layer GetLayerById(Guid id) => _byId.TryGetValue(id, out var layer) ? layer : null;

        public IEnumerable<IDrawElement> GetAllElements() =>
            _layers.SelectMany(l => l.Elements);

        public IReadOnlyCollection<IDrawElement> GetVisibleElements() =>
            new ReadOnlyCollection<IDrawElement>(
                _layers.Where(l => l.Visible).SelectMany(l => l.Elements).ToList()
            );

        public BoundingBox3D GetVisibleBounds()
        {
            var visible = _layers.Where(l => l.Visible).Select(l => l.Bounds).Where(b => !b.IsEmpty());
            return visible.Any()
                ? visible.Aggregate( BoundingBox3D.Union)
                :  BoundingBox3D.Empty;
        }

        public IReadOnlyCollection<IDrawElement> QueryVisibleElementsInBox( BoundingBox3D frustum)
        {
            var result = new HashSet<IDrawElement>();
            foreach (var layer in _layers)
            {
                if (!layer.Visible) continue;
                foreach (var elem in layer.QueryElementsInFrustum(frustum))
                    result.Add(elem);
            }
            return new ReadOnlyCollection<IDrawElement>(result.ToList());
        }

        // === Bulk ===
        public void RemoveAllLayers()
        {
            var layersToRemove = _layers.ToList();
            _layers.Clear();
            _byId.Clear();
            ActiveLayer = null;

            // Clear each layer's content (which also unsubscribes from element events)
            foreach (var layer in layersToRemove)
            {
                layer.Clear();
                LayerRemoved?.Invoke(layer, -1);
            }

            LayersChanged?.Invoke();
        }
        public void UpdateAllLayersBounds()
        {
            foreach (Layer layer in Layers)
            {
                layer.RebuildBounds();
            }
        }

        /// <summary>
        /// Finds the topmost element at the given world point.
        /// Searches through visible layers in reverse render order (top to bottom).
        /// </summary>
        public IDrawElement FindElementAtPoint(Vector3D worldPoint, float worldTolerance)
        {
            // Get visible elements from layer manager (in render order)
            IReadOnlyCollection<IDrawElement> visibleElements = GetVisibleElements();

            // Search in reverse order (topmost drawn elements first)
            foreach (var element in visibleElements.Reverse())
            {
                if (element.HitTest(worldPoint, worldTolerance))
                {
                    return element;
                }
            }

            return null;
        }
    }
}
