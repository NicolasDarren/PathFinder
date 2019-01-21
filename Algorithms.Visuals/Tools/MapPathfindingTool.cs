using System;
using System.Drawing;
using System.Windows.Forms;
using Algorithms.Domain;
using OpenGL;

namespace Algorithms.Visuals
{
    public class MapPathfindingTool : MapVisualTool
    {
        private IPathfinder _pathfinder;
        private ToolStep _toolStep;
        private MapLocation _origin;
        private MapLocation _destination;
        private PathResult _computedPath;
        
        public event Action<string> MessageChanged;

        public MapPathfindingTool(IPathfinder pathfinder)
        {
            _pathfinder = pathfinder;
        }

        protected override void OnToolActivated()
        {
            _toolStep = ToolStep.SetOrigin;
            _computedPath = null;
            MessageChanged?.Invoke("Please click anywhere in the map to select the origin of the path.");
        }


        public override bool NotifyMouseClick(MouseEventArgs args)
        {
            if (_toolStep == ToolStep.SetOrigin)
            {
                _origin = Owner.Value.ConstrainToBounds(Owner.GetMapLocationFromClient(args.Location));
                _toolStep = ToolStep.SetDestination;
                MessageChanged?.Invoke("Please click anywhere in the map to select the destination of the path.");
                return true;
            }

            if (_toolStep == ToolStep.SetDestination)
            {
                var destination = Owner.Value.ConstrainToBounds(Owner.GetMapLocationFromClient(args.Location));

                if (destination == _origin)
                {
                    MessageChanged?.Invoke("Path destination cannot be the same as the origin.");
                    return false;
                }

                _destination = destination;
                _toolStep = ToolStep.Pathfind;
                _computedPath = _pathfinder.Compute(_origin, _destination);


                if (_computedPath.IsSuccess)
                {
                    MessageChanged?.Invoke("Path computed successfully.");
                }
                else MessageChanged?.Invoke("Failed to compute path.");

                return true;
            }

            return false;
        }

        public override void Paint(Rectangle visibleTiles, MapLocation mouseLocationOnMap)
        {
            if (_toolStep > ToolStep.SetOrigin)
            {
                Gl.Begin(PrimitiveType.Triangles);
                
                RenderQuad(Owner.TextureMap[MapTileVisual.Origin], Owner.GetMapLocationOnClient(_origin), new Color4() { A = 1.0f, R = 1.0f, G = 1.0F, B = 1.0F });

                if (_toolStep > ToolStep.SetDestination)
                {
                    RenderQuad(Owner.TextureMap[MapTileVisual.Destination], Owner.GetMapLocationOnClient(_destination), new Color4() {A = 1.0f, R = 1.0f, G = 1.0F, B = 1.0F});
                }

                Gl.End();

                if (_toolStep == ToolStep.Pathfind && _computedPath?.Path?.Length > 0)
                {
                    Gl.Disable(EnableCap.Texture2d);
                    
                    Gl.Begin(PrimitiveType.Lines);
                    Gl.Color3(0.0f, 0.0f, 1.0f);
                    var start = Owner.GetMapLocationOnClient(_computedPath.Path[0]);

                    for (int i = 1; i < _computedPath.Path.Length; i++)
                    {
                        var end = Owner.GetMapLocationOnClient(_computedPath.Path[i]);
                        Gl.Vertex2(start.X + start.Width / 2, start.Y + start.Height / 2);
                        Gl.Vertex2(end.X + end.Width / 2, end.Y + end.Height / 2);
                        start = end;
                    }
                    Gl.End();

                    Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    Gl.Enable(EnableCap.Blend);
                    Gl.Begin(PrimitiveType.Triangles);

                    foreach (var coloredTile in _computedPath.ColoredTiles)
                    {
                        RenderQuad(Owner.TextureMap[MapTileVisual.White], Owner.GetMapLocationOnClient(coloredTile), new Color4() { A = 0.2f, R = 0.0f, G = 0.0F, B = 1.0F });
                    }

                    Gl.End();
                }
            }
        }

        private void RenderQuad(RectangleF texels, Rectangle area, Color4 color)
        {
            Gl.Color4(color.R,color.G,color.B,color.A);
            Gl.TexCoord2(texels.X, texels.Y);
            Gl.Vertex2(area.X, area.Y);
            Gl.TexCoord2(texels.Width, texels.Y);
            Gl.Vertex2(area.Right, area.Y);
            Gl.TexCoord2(texels.Width, texels.Height);
            Gl.Vertex2(area.Right, area.Bottom);

            Gl.TexCoord2(texels.Width, texels.Height);
            Gl.Vertex2(area.Right, area.Bottom);
            Gl.TexCoord2(texels.X, texels.Height);
            Gl.Vertex2(area.X, area.Bottom);
            Gl.TexCoord2(texels.X, texels.Y);
            Gl.Vertex2(area.X, area.Y);
        }

        private enum ToolStep
        {
            SetOrigin = 0,
            SetDestination = 1,
            Pathfind = 2
        }

        private struct Color4
        {
            public float A;
            public float R;
            public float G;
            public float B;
        }
    }
}