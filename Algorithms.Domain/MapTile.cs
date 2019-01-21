using System.Runtime.InteropServices;

namespace Algorithms.Domain
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct MapTile
    {
        public const int SizeInBytes = 4;

        /// <summary>
        /// Indicates the index of the tile template that created this map tile.
        /// </summary>
        [FieldOffset(0)]
        public readonly byte Template;
        /// <summary>
        /// Specifies the visual image drawn to represent this map tile.
        /// </summary>
        [FieldOffset(1)]
        public MapTileVisual Visual;
        /// <summary>
        /// Indicates the energy required to navigate through this tile, higher values mean more energy is needed.
        /// </summary>
        [FieldOffset(2)]
        public readonly byte TravelCost;
        /// <summary>
        /// Indicates whether or not this tile is considered an obstacle that cannot be navigated through.
        /// </summary>
        [FieldOffset(3)]
        public readonly bool IsObstacle;

        public MapTile(byte travelCost, bool isObstacle, MapTileVisual visual, byte template)
        {
            TravelCost = travelCost;
            IsObstacle = isObstacle;
            Visual = visual;
            Template = template;
        }
    }
}