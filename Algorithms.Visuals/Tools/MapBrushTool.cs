using System;
using System.Drawing;
using System.Windows.Forms;
using Algorithms.Domain;
using OpenGL;

namespace Algorithms.Visuals
{
    public class MapBrushTool : MapVisualTool
    {
        private readonly MapTileTemplate[] _templates;

        public MapBrushTool(MapTileTemplate[] templates)
        {
            _templates = templates;
            ActiveTemplate = _templates[0];
        }

        public MapTileTemplate[] Templates => _templates;
        public MapTileTemplate ActiveTemplate { get; set; }

        public int Size { get; set; } = 1;

        public override bool NotifyMouseDown(MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Left)
                return base.NotifyMouseDown(args);

            ApplyBrush(args);
            return true;
        }

        public override void NotifyMouseMove(MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Left)
                return;
            ApplyBrush(args);
        }

        private void ApplyBrush(MouseEventArgs args)
        {
            int size = Math.Max(0, Size);
            var origin = Owner.GetMapLocationFromClient(args.Location).Move(MapDirection.South | MapDirection.West, size);
            size = size * 2 + 1;

            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                var location = origin.Move(MapDirection.East, x).Move(MapDirection.North, y);
                Owner.Value.SetTile(location, ActiveTemplate, false);
            }

        }

        public override bool NotifyMouseWheel(MouseEventArgs args)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                if (args.Delta > 0)
                {
                    Size -= 1;
                }
                else if (args.Delta < 0)
                {
                    Size += 1;
                }

                if (Size < 0) Size = 0;
                if (Size > 8) Size = 8;

                return true;

            }

            return false;
        }

        public override void Paint(Rectangle visibleTiles, MapLocation mouseLocationOnMap)
        {
            int size = Math.Max(0, Size);
            var minimum = mouseLocationOnMap.Move(MapDirection.South | MapDirection.West, size);
            var maximum = mouseLocationOnMap.Move(MapDirection.North | MapDirection.East, size);
            var area = Rectangle.Union(Owner.GetMapLocationOnClient(minimum), Owner.GetMapLocationOnClient(maximum));
            
            Gl.Begin(PrimitiveType.Lines);

            Gl.Color3(0.0f, 0.0f, 0.0f);
            Gl.Vertex2(area.X, area.Y);
            Gl.Vertex2(area.Right, area.Y);
            Gl.Vertex2(area.Right, area.Y);
            Gl.Vertex2(area.Right, area.Bottom);
            Gl.Vertex2(area.Right, area.Bottom);
            Gl.Vertex2(area.X, area.Bottom);
            Gl.Vertex2(area.X, area.Bottom);
            Gl.Vertex2(area.X, area.Y);

            Gl.End();
        }
    }
}