using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Scene
{
    public class Octree<T> where T :  IDrawElement
    {
        private readonly int _capacity;           // Max elements per node
        private readonly BoundingBox3D _bounds;   // 3D region this node covers
        private List<T> _elements = new List<T>(); // Elements in this node
        private Octree<T>[] _children;            // 8 child nodes
        private bool _divided = false;

        public Octree(BoundingBox3D bounds, int capacity = 8)
        {
            _bounds = bounds;
            _capacity = capacity;
        }

        // ---------------------------------------------------------
        // Insert
        // ---------------------------------------------------------
        public void Insert(T element)
        {
            BoundingBox3D elemBounds = element.GetBounds();

            // If element outside this node entirely, skip
            if (!_bounds.IntersectsWith(elemBounds))
                return;

            // If we have room and not yet subdivided
            if (_elements.Count < _capacity && !_divided)
            {
                _elements.Add(element);
                return;
            }

            // Otherwise subdivide if not already
            if (!_divided) Subdivide();

            int index = GetOctant(elemBounds);
            if (index != -1 && _children[index]._bounds.IntersectsWith(elemBounds))
                _children[index].Insert(element);
            else
                _elements.Add(element); // crosses multiple children, keep here
        }

        // ---------------------------------------------------------
        // Query (find all elements that intersect a region)
        // ---------------------------------------------------------
        public IEnumerable<T> Query(BoundingBox3D queryRegion)
        {
            List<T> results = new List<T>();

            if (!_bounds.IntersectsWith(queryRegion))
                return results;

            // Add all intersecting elements in this node
            for (int i = 0; i < _elements.Count; i++)
            {
                T e = _elements[i];
                if (queryRegion.IntersectsWith(e.GetBounds()))
                    results.Add(e);
            }

            // Recurse into children
            if (_divided)
            {
                for (int i = 0; i < 8; i++)
                {
                    List<T> childResults = new List<T>(_children[i].Query(queryRegion));
                    results.AddRange(childResults);
                }
            }

            return results;
        }

        // ---------------------------------------------------------
        // Subdivide into 8 child nodes
        // ---------------------------------------------------------
        private void Subdivide()
        {
            float midX = (_bounds.Min.X + _bounds.Max.X) / 2f;
            float midY = (_bounds.Min.Y + _bounds.Max.Y) / 2f;
            float midZ = (_bounds.Min.Z + _bounds.Max.Z) / 2f;

            _children = new Octree<T>[8];

            // 8 octants (XYZ combinations: lower/upper)
            _children[0] = new Octree<T>(new BoundingBox3D(
                new Vector3D(_bounds.Min.X, _bounds.Min.Y, _bounds.Min.Z),
                new Vector3D(midX, midY, midZ)));

            _children[1] = new Octree<T>(new BoundingBox3D(
                new Vector3D(midX, _bounds.Min.Y, _bounds.Min.Z),
                new Vector3D(_bounds.Max.X, midY, midZ)));

            _children[2] = new Octree<T>(new BoundingBox3D(
                new Vector3D(_bounds.Min.X, midY, _bounds.Min.Z),
                new Vector3D(midX, _bounds.Max.Y, midZ)));

            _children[3] = new Octree<T>(new BoundingBox3D(
                new Vector3D(midX, midY, _bounds.Min.Z),
                new Vector3D(_bounds.Max.X, _bounds.Max.Y, midZ)));

            _children[4] = new Octree<T>(new BoundingBox3D(
                new Vector3D(_bounds.Min.X, _bounds.Min.Y, midZ),
                new Vector3D(midX, midY, _bounds.Max.Z)));

            _children[5] = new Octree<T>(new BoundingBox3D(
                new Vector3D(midX, _bounds.Min.Y, midZ),
                new Vector3D(_bounds.Max.X, midY, _bounds.Max.Z)));

            _children[6] = new Octree<T>(new BoundingBox3D(
                new Vector3D(_bounds.Min.X, midY, midZ),
                new Vector3D(midX, _bounds.Max.Y, _bounds.Max.Z)));

            _children[7] = new Octree<T>(new BoundingBox3D(
                new Vector3D(midX, midY, midZ),
                new Vector3D(_bounds.Max.X, _bounds.Max.Y, _bounds.Max.Z)));

            // Move existing elements into new children
            List<T> temp = new List<T>(_elements);
            _elements.Clear();

            for (int i = 0; i < temp.Count; i++)
            {
                Insert(temp[i]);
            }

            _divided = true;
        }

        // ---------------------------------------------------------
        // Determine which octant the element belongs to
        // ---------------------------------------------------------
        private int GetOctant(BoundingBox3D bounds)
        {
            float midX = (_bounds.Min.X + _bounds.Max.X) / 2f;
            float midY = (_bounds.Min.Y + _bounds.Max.Y) / 2f;
            float midZ = (_bounds.Min.Z + _bounds.Max.Z) / 2f;

            bool left = bounds.Max.X <= midX;
            bool right = bounds.Min.X >= midX;
            bool bottom = bounds.Max.Y <= midY;
            bool top = bounds.Min.Y >= midY;
            bool front = bounds.Max.Z <= midZ;
            bool back = bounds.Min.Z >= midZ;

            // If it spans more than one half-space, keep it in parent
            if ((!left && !right) || (!bottom && !top) || (!front && !back))
                return -1;

            // Determine index (000..111 bitwise: x, y, z)
            int index = 0;
            if (right) index |= 1;
            if (top) index |= 2;
            if (back) index |= 4;

            return index;
        }

        // ---------------------------------------------------------
        // Clear (for rebuilds)
        // ---------------------------------------------------------
        public void Clear()
        {
            _elements.Clear();
            if (_divided)
            {
                for (int i = 0; i < 8; i++)
                    _children[i].Clear();

                _children = null;
                _divided = false;
            }
        }
    }

}
