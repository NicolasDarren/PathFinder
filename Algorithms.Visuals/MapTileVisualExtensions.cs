using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithms.Domain;

namespace Algorithms.Visuals
{
    // ReSharper disable once InconsistentNaming
    public static class __MapTileVisualExtensions
    {
        private static readonly Bitmap[] TileImages;

        static __MapTileVisualExtensions()
        {
            var visuals = Enum.GetValues(typeof(MapTileVisual)) as MapTileVisual[];
            TileImages = new Bitmap[visuals.Length];
            TileImages[(int)MapTileVisual.Grass] = new Bitmap(Assets.Grass);
            TileImages[(int)MapTileVisual.Gravel] = new Bitmap(Assets.Gravel);
            TileImages[(int)MapTileVisual.Sand] = new Bitmap(Assets.Sand);
            TileImages[(int)MapTileVisual.Wall] = new Bitmap(Assets.Wall);
            TileImages[(int)MapTileVisual.Water] = new Bitmap(Assets.Water);
            TileImages[(int)MapTileVisual.Door] = new Bitmap(Assets.Door);
            TileImages[(int)MapTileVisual.White] = new Bitmap(Assets.White);
            TileImages[(int)MapTileVisual.Origin] = new Bitmap(Assets.Origin);
            TileImages[(int)MapTileVisual.Destination] = new Bitmap(Assets.Destination);
        }

        public static Bitmap GetImage(this MapTileVisual visual)
        {
            return new Bitmap(TileImages[(int)visual]);
        }
    }
}
