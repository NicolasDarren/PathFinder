using System.Collections.Generic;
using System.Linq;

namespace Algorithms.Domain
{
    public class PathResult
    {
        /// <summary>
        /// Indicates whether or not the path is complete, ie: reached the destination.
        /// </summary>
        public bool IsSuccess { get; }
        /// <summary>
        /// The sequence of locations that one would navigate to follow the computed path including the original origin and destination as provided to the Compute method of the pathfinder.
        /// </summary>
        public MapLocation[] Path { get; }

        public List<MapLocation> ColoredTiles { get; }

        public PathResult(bool isSuccess, IEnumerable<MapLocation> path, IEnumerable<MapLocation> coloredTiles)
        {
            IsSuccess = isSuccess;
            Path = path?.ToArray() ?? new MapLocation[0];
            ColoredTiles = new List<MapLocation>(coloredTiles ?? new MapLocation[0]);
        }
    }
}