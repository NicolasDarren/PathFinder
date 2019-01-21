using System;

namespace Algorithms.Domain
{
    [Flags]
    public enum MapDirection
    {
        None = 0,
        /// <summary>
        /// Represents the vertical axis, moving towards the top of the screen, or +Y
        /// </summary>
        North = 1,
        /// <summary>
        /// Represents the vertical axis, moving towards the bottom of the screen, or -Y
        /// </summary>
        South = 2,
        /// <summary>
        /// Represents the horizontal axis, moving towards the right side of the screen, or +X
        /// </summary>
        East = 4,
        /// <summary>
        /// Represents the horizontal axis, moving towards the left side of the screen, or -X
        /// </summary>
        West = 8,
    }
}