using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithms.Domain;

namespace Algorithms.TestBed.Implementation
{
    public class Pathfinder : IPathfinder
    {
        private readonly Map _map;

        public Pathfinder(Map map)
        {
            _map = map;
        }
        
        public PathResult Compute(MapLocation origin, MapLocation destination)
        {
            List<MapLocation> path = new List<MapLocation>();
            List<MapLocation> safeLocations = new List<MapLocation>();
            List<MapLocation> unsafeLocations = new List<MapLocation>();
            path = new List<MapLocation>{ origin };
            MapLocation currentLocation = origin;
            while (currentLocation != destination)
            {
                var directionOfMovement = currentLocation.GetDirectionTo(destination);
                var closestSafeLocation = new MapDirection();
                var nextPosition = currentLocation.Move(directionOfMovement, 1);
                var nextPositionTile = _map.GetTile(nextPosition);
                if (nextPositionTile.IsObstacle)
                {
                    unsafeLocations.Add(nextPosition);

                    if (directionOfMovement == MapDirection.East || directionOfMovement == MapDirection.West)
                    {
                        closestSafeLocation = FindEastWestClosestSafeLocation(nextPosition, closestSafeLocation);

                        nextPosition = currentLocation.Move(closestSafeLocation, 1);
                    }

                    if (directionOfMovement == MapDirection.North || directionOfMovement == MapDirection.South)
                    {
                        closestSafeLocation = FindNorthSouthClosestSafeLocation(nextPosition, closestSafeLocation);

                        nextPosition = currentLocation.Move(closestSafeLocation, 1);
                    }
                }
                else
                {
                    safeLocations.Add(nextPosition);
                }

                currentLocation = nextPosition;
                //var tile = _map.GetTile(currentLocation);
                //var template =  tile.Template;
                //_map.SetTile(currentLocation, new MapTileTemplate(template), true);

                path.Add(nextPosition);

                //var nextPositionTile = _map.GetTile(nextPosition);
                //if (nextPositionTile.IsObstacle)
                //{
                //    var directionMoved = 
                //}
            }
            path.Add(destination);

            var pathFound = false;
            return new PathResult(pathFound, path, safeLocations);
        }

        private MapDirection FindNorthSouthClosestSafeLocation(MapLocation nextPosition, MapDirection closestSafeLocation)
        {
            var isObstacle = true;
            var tileWest = nextPosition;
            var tileEast = nextPosition;
            while (isObstacle)
            {
                tileWest = tileWest.Move(MapDirection.West);
                tileEast = tileEast.Move(MapDirection.East);
                var westTileIsObstacle = _map.GetTile(tileWest).IsObstacle;
                if (westTileIsObstacle)
                {
                    var eastTileIsObstacle = _map.GetTile(tileEast).IsObstacle;
                    if (eastTileIsObstacle)
                        continue;

                    closestSafeLocation = MapDirection.East;
                }
                else
                {
                    closestSafeLocation = MapDirection.West;
                }

                isObstacle = false;
            }

            return closestSafeLocation;
        }

        private MapDirection FindEastWestClosestSafeLocation(MapLocation nextPosition, MapDirection closestSafeLocation)
        {
            var isObstacle = true;
            var tileNorth = nextPosition;
            var tileSouth = nextPosition;

            while (isObstacle)
            {
                tileNorth = tileNorth.Move(MapDirection.North);
                tileSouth = tileSouth.Move(MapDirection.South);

                var northTileIsObstacle = _map.GetTile(tileNorth).IsObstacle;
                if (northTileIsObstacle)
                {
                    var southTileIsObstacle = _map.GetTile(tileSouth).IsObstacle;
                    if (southTileIsObstacle)
                        continue;

                    closestSafeLocation = MapDirection.South;
                }
                else
                {
                    closestSafeLocation = MapDirection.North;
                }

                isObstacle = false;
            }

            return closestSafeLocation;
        }
    }
}
