using System;
using System.Runtime.InteropServices;

namespace Algorithms.Domain
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct MapLocation : IEquatable<MapLocation>
    {
        public const int SizeInBytes = 8;

        /// <summary>
        /// The position along the horizontal axis that this map location represents.
        /// </summary>
        [FieldOffset(0)]
        public int X;
        /// <summary>
        /// The position along the vertical axis that this map location represents.
        /// </summary>
        [FieldOffset(4)]
        public int Y;

        public MapLocation(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }

        public override bool Equals(object obj)
        {
            return obj is MapLocation other && Equals(other);
        }

        public bool Equals(MapLocation other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        /// <summary>
        /// Returns a new <see cref="MapLocation"/> that has been moved <paramref name="numberOfSteps"/> away relative to this <see cref="MapLocation"/> in the direction indicated by <paramref name="directionOfTravel"/>.
        /// </summary>
        /// <param name="directionOfTravel">The direction to move in, this can indicate multiple directions using flags.</param>
        /// <param name="numberOfSteps">The number of tiles to move.</param>
        /// <returns></returns>
        public MapLocation Move(MapDirection directionOfTravel, int numberOfSteps = 1)
        {
            var delta = new MapLocation();

            if ((directionOfTravel & MapDirection.North) == MapDirection.North) delta.Y++;
            if ((directionOfTravel & MapDirection.South) == MapDirection.South) delta.Y--;
            if ((directionOfTravel & MapDirection.East) == MapDirection.East) delta.X++;
            if ((directionOfTravel & MapDirection.West) == MapDirection.West) delta.X--;

            delta *= numberOfSteps;

            return this + delta;
        }

        /// <summary>
        /// Gets the direction of travel that would move towards destination.
        /// </summary>
        /// <param name="destination">The location you want to travel towards.</param>
        public MapDirection GetDirectionTo(MapLocation destination)
        {
            var delta = destination - this;
            MapDirection result = 0;

            if (delta.Y < 0) result |= MapDirection.South;
            if (delta.Y > 0) result |= MapDirection.North;
            if (delta.X < 0) result |= MapDirection.West;
            if (delta.X > 0) result |= MapDirection.East;

            if (delta.X != 0 && delta.Y != 0)
            {
                delta = ConvertDeltaToPositiveValues(delta);

                if (delta.X < delta.Y)
                    result &= ~(MapDirection.East | MapDirection.West);
                else if (delta.X > delta.Y)
                    result &= ~(MapDirection.South | MapDirection.North);
                else
                    result &= ~(MapDirection.South | MapDirection.North);
            }

            return result;
        }
        
        private static MapLocation ConvertDeltaToPositiveValues(MapLocation delta)
        {
            if (delta.X < 0)
                delta.X = delta.X * -1;
            if (delta.Y < 0)
                delta.Y = delta.Y * -1;
            return delta;
        }

        public static bool operator ==(MapLocation l, MapLocation r)
        {
            return l.X == r.X && l.Y == r.Y;
        }

        public static bool operator !=(MapLocation l, MapLocation r)
        {
            return l.X != r.X || l.Y != r.Y;
        }

        public static MapLocation operator +(MapLocation l, MapLocation r)
        {
            return new MapLocation(l.X + r.X, l.Y + r.Y);
        }

        public static MapLocation operator -(MapLocation l, MapLocation r)
        {
            return new MapLocation(l.X - r.X, l.Y - r.Y);
        }

        public static MapLocation operator *(MapLocation l, int r)
        {
            return new MapLocation(l.X * r, l.Y * r);
        }

        public static MapLocation operator /(MapLocation l, int r)
        {
            return new MapLocation(l.X / r, l.Y / r);
        }

    }
}