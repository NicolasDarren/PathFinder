using System;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Domain
{
    public interface IPathfinder
    {
        PathResult Compute(MapLocation origin, MapLocation destination);
    }
}
