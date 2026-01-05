using Arnaoot.VectorGraphics.Core.Models;
using Arnaoot.VectorGraphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Core.Tools
{
    public sealed class NullTool : Tool
    {
        public static readonly NullTool Instance = new();

        private NullTool() { }

        public override string Name => "None";
        public override string Description => "No active tool";
        public override Cursor Cursor => Cursors.Default;
        public override bool RequiresActiveLayer => false;
        public override bool ModifiesDocument => false;

        public override InvalidationLevel OnMouseDown(MouseEventArgs e, VectorDocument document) 
        {
            return InvalidationLevel.None;
        }
        public override InvalidationLevel OnMouseMove(MouseEventArgs e, VectorDocument document)
            => InvalidationLevel.None;
        public override InvalidationLevel OnMouseUp(MouseEventArgs e, VectorDocument document) 
        {
            return InvalidationLevel.None;
        }

        public override IDrawElement? GetTemporaryElement() => null;
    }

}
