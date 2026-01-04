using System;

namespace Arnaoot.VectorGraphics.Rendering
{
    public enum InvalidationLevel : byte
    {
        None = 0,
        Overlay = 1,    // Tool preview/overlays only
        View = 2,       // Zoom/pan/rotate — geometry unchanged
        Scene = 3,      // Elements added/moved — cache invalid
        Full = 4        // Backend swap, resize, etc.
    }
    public class InvalidationEventArgs : EventArgs
    {
        public InvalidationLevel Level { get; }
        public InvalidationEventArgs(InvalidationLevel level) => Level = level;
    }
}
