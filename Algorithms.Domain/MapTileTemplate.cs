using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Domain
{
    public sealed class MapTileTemplate
    {
        /// <summary>
        /// Gets the unique index of this map tile template.
        /// </summary>
        public byte Index { get; }

        /// <summary>
        /// Gets the name that should be shown for tiles created by this template.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Indicates whether or not this template creates tiles that are considered an obstacle that cannot be navigated through.
        /// </summary>
        public bool IsObstacle { get; set; }
        /// <summary>
        /// Indicates the energy required to navigate through tiles created by this template, higher values mean more energy is needed.
        /// </summary>
        public byte TravelCost { get; set; }
        /// <summary>
        /// Specifies the visual image drawn to represent this map tile template.
        /// </summary>
        public MapTileVisual Visual { get; set; }

        public MapTileTemplate(byte index)
        {
            Index = index;
        }

        public override string ToString()
        {
            return Name;
        }

        public MapTile CreateTile()
        {
            return new MapTile(TravelCost, IsObstacle, Visual, Index);
        }
    }
}
