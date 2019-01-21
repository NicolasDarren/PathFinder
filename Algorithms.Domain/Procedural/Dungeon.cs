using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Domain.Procedural
{
    public class Atts
    {
        public const int MaxRooms = 40;
        public const int MaxLocsX = 150;
        public const int MaxLocsY = 150;

        public enum LocationType
        {
            Empty,
            WallPoint,
            DoorPoint,
            Corridor,
            TempDoor,
            TempCorridor,
            RoomSpace
        };

        // returns a char for each PointType, empty spaces on gridpoints return +
        public static char MapChar(LocationType p, bool isGridLine)
        {
            var c = @" |DCdc."[(int)p];
            //if (isGridLine && p == LocationType.Empty) c = '+';
            return c;
        }

        public enum Direction
        {
            None,
            North,
            East,
            South,
            West
        };

        public static Random R = new Random();

        public static int RandPc(int pc)
        {
            return R.Next(pc);
        }

        public static bool IsOnMap(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < MaxLocsX && y < MaxLocsY);
        }

        public static bool IsGridPoint(int x, int y) // where two GridLines intersect
        {
            return (x % 6 == 0 && y % 6 == 0);
        }

        // global variables
        public static bool IsFirstLevel { get; set; }
        public static List<Direction> ToDir = new List<Direction>() { Direction.North, Direction.East, Direction.South, Direction.West };
        public static List<Direction> FromDir = new List<Direction>() { Direction.South, Direction.West, Direction.North, Direction.East };
        public static List<int> OffSetDirY = new List<int> { -1, 0, 1, 0 }; // n e s w
        public static List<int> OffSetDirX = new List<int> { 0, 1, 0, -1 };
    }

    public class DungeonLocation
    {
        public Atts.LocationType LocationType { get; set; }
        public Corridor Corridor { get; set; } // for corridors
        public DungeonRoom aRoom { get; set; } // if it's a room holds room index, default -1
        public bool GridLine { get; set; }

        public DungeonLocation()
        {
            LocationType = new Atts.LocationType();
            Corridor = null;
            aRoom = null;
        }
    }

    // Holds working area for a corridor, speeds up generation
    public class WorkingArea
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    // corridors link rooms but cannot exist independently of rooms 
    public class Corridor
    {
        public int StartY { get; set; }
        public int StartX { get; set; }
        public int CurrentX { get; set; }
        public int CurrentY { get; set; }
        private DungeonLocation[,] cmap;

        public DungeonRoom StartRoom { get; set; } // never null
        public DungeonRoom EndRoom { get; set; }   // can be null
        public HashSet<Corridor> AttachedTo = new HashSet<Corridor>();

        public bool IsSameCorridor(Corridor other)
        {
            return (StartY == other.StartY && StartX == other.StartX);
        }

        //Corridors need to know about map so pass in
        public Corridor(ref DungeonLocation[,] themap)
        {
            cmap = themap;
        }

        private void LinkTwoRooms(DungeonRoom room1, DungeonRoom room2)
        {
            room1.ConnectedRooms.UnionWith(room2.ConnectedRooms);
            room2.ConnectedRooms.UnionWith(room1.ConnectedRooms);
        }

        // check not same corridor and not hit another corridor from same starter room return true if it's a new one
        public bool IsNewCorridor(Corridor HitCorridor)
        {
            // Have we just met same corridor?
            if (IsSameCorridor(HitCorridor) && AttachedTo.Contains(HitCorridor))
                return false;

            // Check all corridors from starter room - have we met one?
            for (var iDoor = 0; iDoor < 3; iDoor++)
            {
                if (StartRoom.Doors[iDoor] != null)
                {
                    var PossibleCorridor = StartRoom.Doors[iDoor];
                    if (HitCorridor.IsSameCorridor(PossibleCorridor) || HitCorridor.AttachedTo.Contains(PossibleCorridor))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // pick new direction to go in, return -1 if not possible
        private int PickDirection(int xdoor, int ydoor)
        {
            bool[] canMove = new[] { true, true, true, true };
            while (true)
            {
                var iDir = 0;
                var AreMoves = canMove.Any(b => b); // are there any canMove[] true?
                if (!AreMoves)
                    break;
                do
                {
                    iDir = Atts.R.Next(4);
                } while (!canMove[iDir]);
                var x = xdoor + Atts.OffSetDirX[iDir];
                var y = ydoor + Atts.OffSetDirY[iDir];
                if (Atts.IsOnMap(x, y) && cmap[x, y].LocationType == Atts.LocationType.Empty)
                {
                    return iDir;
                }
                canMove[iDir] = false;
            };
            return -1;
        }

        public int NewDirection(int iDir)
        {
            iDir += Atts.R.Next(3) - 2; // left straight or right?
            if (iDir < 0)
                iDir += 4;
            else if (iDir > 3)
                iDir -= 4;
            return iDir;
        }

        // try to grow corridor
        public bool GrowCorridor(ref int xdoor, ref int ydoor)
        {
            var done = false;
            var x = xdoor;
            var y = ydoor;
            var seglen = Atts.R.Next(4) + 3;

            var iDir = PickDirection(x, y);
            if (iDir == -1)
                return false;
            var xOffset = Atts.OffSetDirX[iDir];
            var yOffset = Atts.OffSetDirY[iDir];

            while (!done)
            {
                x += xOffset;
                y += yOffset;
                if (!Atts.IsOnMap(x, y))
                {
                    RemoveFailedCorridor(xdoor, ydoor);
                    return false;
                }
                var locType = cmap[x, y].LocationType;

                if (locType == Atts.LocationType.Empty)
                {
                    cmap[x, y].LocationType = Atts.LocationType.TempCorridor;
                    if (seglen > 0)
                    {
                        seglen--;
                    }
                    else
                        if (cmap[x, y].GridLine) // Only change direction where 
                    {
                        iDir = NewDirection(iDir);
                        xOffset = Atts.OffSetDirX[iDir];
                        yOffset = Atts.OffSetDirY[iDir];
                        seglen = Atts.R.Next(4) + 3;
                    }
                    continue;
                }

                // hit something - door or Corridor
                if (locType == Atts.LocationType.Corridor)
                {
                    var metCorridor = cmap[x, y].Corridor;
                    if (IsNewCorridor(metCorridor))
                    {
                        MeetANewCorridor(metCorridor);
                        done = true;
                    }
                    else
                    {
                        RemoveFailedCorridor(xdoor, ydoor);
                        return false;
                    }
                }
                if (locType == Atts.LocationType.DoorPoint)
                {
                    var room = cmap[x, y].aRoom;
                    if (!room.IsSameRoom(StartRoom) && !room.ConnectedRooms.Contains(StartRoom) &&
                        room.HasSpareExit())
                    {
                        EndRoom = room;
                        room.AddExit(this, Atts.FromDir[iDir]);
                        LinkTwoRooms(room, StartRoom);
                        done = true;
                    }
                    else
                    {
                        RemoveFailedCorridor(xdoor, ydoor);
                        return false;
                    }
                }
                /* if (locType == Atts.LocationType.WallPoint)
                  {                         
                      RemoveFailedCorridor(xdoor, ydoor);
                      return false;
                  }*/

            }

            // Finalize the TempDoors and TempCorridors
            if (cmap[xdoor, ydoor].LocationType == Atts.LocationType.TempDoor)
            {
                cmap[xdoor, ydoor].LocationType = Atts.LocationType.DoorPoint;
            }
            for (var yp = 0; yp < Atts.MaxLocsY; yp++)
            {
                for (var xp = 0; xp < Atts.MaxLocsX; xp++)
                {
                    if (Atts.IsOnMap(xp, yp) && cmap[xp, yp].LocationType == Atts.LocationType.TempCorridor)
                    {
                        cmap[xp, yp].LocationType = Atts.LocationType.Corridor;
                        cmap[xp, yp].Corridor = this;
                    }
                }
            }

            return true;
        }

        // When corridors meet they get very attached
        private void MeetANewCorridor(Corridor metCorridor)
        {
            AttachedTo.Add(metCorridor);
            metCorridor.AttachedTo.Add(this);
            StartRoom.ConnectedRooms.UnionWith(metCorridor.StartRoom.ConnectedRooms);
            metCorridor.StartRoom.ConnectedRooms.UnionWith(StartRoom.ConnectedRooms);
        }

        public void RemoveFailedCorridor(int xdoor, int ydoor)
        {
            if (cmap[xdoor, ydoor].LocationType == Atts.LocationType.TempDoor)
            {
                cmap[xdoor, ydoor].LocationType = Atts.LocationType.DoorPoint;
                cmap[xdoor, ydoor].Corridor = null;
            }

            // Lazy way of clearing- checks all grid. Could be optimized
            for (var yp = 0; yp < Atts.MaxLocsY - 1; yp++)
            {
                for (var xp = 0; xp < Atts.MaxLocsX - 1; xp++)
                {
                    if (cmap[xp, yp].LocationType == Atts.LocationType.TempCorridor)
                    {
                        cmap[xp, yp].LocationType = Atts.LocationType.Empty;
                    }
                }
            }
        }

    }

    public class DungeonRoom
    {
        private DungeonLocation[,] _map;
        public int Height { get; set; }
        public int Width { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public List<Corridor> Doors { get; set; }
        public List<Atts.Direction> DoorDirections { get; set; }

        public HashSet<DungeonRoom> ConnectedRooms = new HashSet<DungeonRoom>();

        private bool fitsMap;

        public DungeonRoom(int index)
        {
            Index = index;
            Doors = new List<Corridor>() { null, null, null, null, null, null, null, null };
            DoorDirections = new List<Atts.Direction>()
                   {  Atts.Direction.None, Atts.Direction.None, Atts.Direction.None, Atts.Direction.None,
                      Atts.Direction.None, Atts.Direction.None, Atts.Direction.None, Atts.Direction.None};
            ConnectedRooms.Add(this);

        }

        // Height comparison is also done because Top, Left for a rectangular room is different to Top,Left for a circular room
        // Circular rooms have height 0.
        public bool IsSameRoom(DungeonRoom other)
        {
            return (Top == other.Top && Left == other.Left && Height == other.Height);
        }

        public int Index { get; private set; }

        // tries to create rect or round room
        public bool CreateRoom(DungeonLocation[,] map)
        {
            _map = map;
            if (Atts.IsFirstLevel)
            {
                if (Atts.RandPc(100) < 70)
                {
                    CreateRectangularRoom(true);
                }
                else
                {
                    CreateCircularRoom();
                }
            }
            else
            {
                var d100 = Atts.RandPc(100);
                if (d100 < 85)
                {
                    CreateRectangularRoom(true);
                }
                else if (d100 < 95)
                {
                    CreateCircularRoom();
                }
                else
                {
                    CreateRectangularRoom(true);
                }
            }
            return fitsMap;
        }

        private void CreateCircularRoom()
        {
            var NumTries = 0;
            var Radius = 0;
            var isOk = true;
            do
            {
                NumTries++;
                if (Atts.IsFirstLevel)
                {
                    Radius = Atts.R.Next(11) + 8;
                }
                else
                {
                    Radius = Atts.R.Next(5) + 8;
                }

                Top = Atts.R.Next(Atts.MaxLocsY - Radius - 3);
                Left = Atts.R.Next(Atts.MaxLocsX - Radius - 3);
                Width = Radius;
                Height = 0;

                isOk = true;
                for (var ring = 0; ring < Radius; ring++)
                {
                    var numPoints = CircleData.NumPoints[ring];
                    var offSet = CircleData.Offset[ring];
                    for (var pointIndex = 0; pointIndex < numPoints; pointIndex++)
                    {
                        var xp = CircleData.RingData[offSet++] + Left;
                        var yp = CircleData.RingData[offSet++] + Top;
                        if (xp >= 0 && xp < Atts.MaxLocsX && yp >= 0 && yp < Atts.MaxLocsY)
                        {
                            if (_map[xp, yp].LocationType > Atts.LocationType.DoorPoint)
                            {
                                isOk = false;
                                break;
                            }
                        }
                        else
                        {
                            isOk = false;
                            break;
                        }
                    }
                }

            } while (!isOk && NumTries < 100);

            if (isOk)
            {
                for (var ring = 0; ring < Radius; ring++)
                {
                    var numPoints = CircleData.NumPoints[ring];
                    var offSet = CircleData.Offset[ring];
                    for (var pointIndex = 0; pointIndex < numPoints; pointIndex++)
                    {
                        var xp = CircleData.RingData[offSet++] + Left;
                        var yp = CircleData.RingData[offSet++] + Top;
                        if (xp >= 0 && xp < Atts.MaxLocsX && yp >= 0 && yp < Atts.MaxLocsY)
                        {
                            _map[xp, yp].aRoom = this;
                            if (ring == Radius - 1)
                            {
                                if (_map[xp, yp].LocationType == Atts.LocationType.Empty)
                                {
                                    _map[xp, yp].LocationType = Atts.LocationType.WallPoint;
                                }
                                if (_map[xp, yp].GridLine && _map[xp, yp].LocationType == Atts.LocationType.WallPoint)
                                {
                                    _map[xp, yp].LocationType = Atts.LocationType.TempDoor;
                                }
                            }
                            else
                                if (ring < Radius - 1)
                            {
                                _map[xp, yp].LocationType = Atts.LocationType.RoomSpace;
                            }
                        }

                    }
                }
                FixRoomDoors(true);// true means circular
            }
        }

        private void CreateRectangularRoom(bool issmall)
        {
            var NumTries = 0;
            var x1 = 0;
            var y1 = 0;
            fitsMap = false;
            do
            {

                if (issmall && !Atts.IsFirstLevel)
                {
                    Height = Atts.R.Next(4) * 4 + 6;
                    Width = Atts.R.Next(4) * 4 + 6;
                }
                else
                {
                    Height = Atts.R.Next(6) * 5 + 10;
                    Width = Atts.R.Next(6) * 5 + 10;
                }
                Left = Atts.R.Next(Atts.MaxLocsX - Width - 4);
                Top = Atts.R.Next(Atts.MaxLocsY - Height - 4);
                NumTries++;

                var isOk = true;
                for (y1 = Top - 3; y1 < Top + Height + 2; y1++)
                {
                    for (x1 = Left - 3; x1 < Left + Width + 2; x1++)
                    {
                        if (isOk && y1 >= 0 && y1 < Atts.MaxLocsY && x1 >= 0 && x1 < Atts.MaxLocsX)
                        {
                            // ie on map! 
                            if (_map[x1, y1].LocationType > Atts.LocationType.DoorPoint)
                                isOk = false;
                        }
                        else
                        {
                            isOk = false;
                        }
                    }
                }

                fitsMap = isOk;

            } while (!fitsMap || NumTries < 100);

            // Adds room to map

            for (y1 = Top; y1 < Top + Height - 1; y1++)
            {
                for (x1 = Left; x1 < Left + Width - 1; x1++)
                {
                    _map[x1, y1].aRoom = this;

                    if (y1 == Top || y1 == Top + Height - 2 || x1 == Left || x1 == Left + Width - 2)
                    {
                        // Walls go up on empty
                        if (_map[x1, y1].LocationType == Atts.LocationType.Empty)
                        {
                            _map[x1, y1].LocationType = Atts.LocationType.WallPoint;
                        }

                        var canHaveDoor = (x1 % 6 == 0 || y1 % 6 == 0) && Atts.R.Next(10) >= 6;
                        // doors occur at walls on gridpoints- %6 ==0 needed to stop a row of doors on a grid row !
                        if (_map[x1, y1].GridLine && _map[x1, y1].LocationType == Atts.LocationType.WallPoint &&
                            canHaveDoor)
                        {
                            _map[x1, y1].LocationType = Atts.LocationType.TempDoor;
                        }
                    }
                    else
                    {
                        _map[x1, y1].LocationType = Atts.LocationType.RoomSpace;
                    }
                }
            }
            FixRoomDoors(false);
        }

        private void FixRoomDoors(bool isCircular)
        {
            // we've created lots of tempdoor positions but only want up to 4 doors
            // first count how many temp doors.
            // For rectangular rooms, top,left,height and width are ok. 
            // or circular, Width is 0, Height = Radius and Top, Left are the circle centre so need to transform

            var top = Top;
            var left = Left;
            var height = Height;
            var width = Width;
            if (isCircular)
            {
                top -= width;
                if (top < 0) top = 0;
                left -= width;
                if (left < 0) left = 0;
                width = width * 2; // 2 x radius
                height = width;
            }

            var x1 = 0;
            var y1 = 0;
            var countTempDoors = 0;
            for (y1 = top; y1 < top + height; y1++)
            {
                for (x1 = left; x1 < left + width; x1++)
                {
                    if (Atts.IsOnMap(x1, y1) && _map[x1, y1].LocationType == Atts.LocationType.TempDoor)
                    {
                        countTempDoors++;
                    }
                }
            }
            if (countTempDoors > 8)
                countTempDoors = 8;
            while (countTempDoors > 0)
            {
                do
                {
                    y1 = Atts.R.Next(top + height);
                    x1 = Atts.R.Next(left + width);

                } while (_map[x1, y1].LocationType != Atts.LocationType.TempDoor);
                _map[x1, y1].LocationType = Atts.LocationType.DoorPoint;
                countTempDoors--;
            }
            // remove the rest
            for (y1 = top - 1; y1 < top + height + 1; y1++)
            {
                for (x1 = left - 1; x1 < left + width + 1; x1++)
                {
                    if (Atts.IsOnMap(x1, y1) && _map[x1, y1].LocationType == Atts.LocationType.TempDoor)
                    {
                        _map[x1, y1].LocationType = Atts.LocationType.WallPoint;
                    }

                }
            }
        }

        // adds an exit to a corridor in the appropriate direction
        public void AddExit(Corridor corridor, Atts.Direction dir)
        {
            for (var i = 0; i < 8; i++)
            {
                if (Doors[i] == null)
                {
                    Doors[i] = corridor;
                    DoorDirections[i] = dir;
                    break;
                }
            }
        }

        // Clear all exits from this room
        public void ClearExits()
        {
            for (var i = 0; i < 8; i++)
            {
                Doors[i] = null;
                DoorDirections[i] = Atts.Direction.None;
            }
        }

        // Has this room any exits yet?
        public bool HasSpareExit()
        {
            for (var i = 0; i < 8; i++)
            {
                if (Doors[i] == null)
                {
                    return true;
                }
            }
            return false;
        }
    }


    public class DungeonLevel
    {
        private List<DungeonRoom> rooms = new List<DungeonRoom>();
        private bool isLastLevel;
        private DungeonLocation[,] map = new DungeonLocation[Atts.MaxLocsX, Atts.MaxLocsY];
        private int numberRooms;

        private HashSet<DungeonRoom> LinkedRooms = new HashSet<DungeonRoom>();
        private bool cancellinking;
        private int cancelcounter;
        private DungeonRoom starterRoom;

        public DungeonLocation[,] GetMap()
        {
            return map;
        }

        public DungeonLevel(int levelnum, bool islastlevel = false)
        {
            Atts.IsFirstLevel = levelnum == 0;
            isLastLevel = islastlevel;
            InitMaps();
            GenerateRooms();
        }

        private void InitMaps()
        {
            var x = 0;
            var y = 0;
            for (y = 0; y < Atts.MaxLocsY; y++)
            {
                for (x = 0; x < Atts.MaxLocsX; x++)
                {
                    map[x, y] = new DungeonLocation
                    {
                        LocationType = Atts.LocationType.Empty,
                        Corridor = null,
                        aRoom = null,
                        GridLine = false
                    };
                }
            }

            x = 0;
            // set to grid line every 6 vertically
            while (x < Atts.MaxLocsX)
            {
                for (y = 0; y < Atts.MaxLocsY; y++)
                {
                    map[x, y].GridLine = true;
                }
                x += 6;
            }

            // and horizontally
            y = 0;
            while (y < Atts.MaxLocsY)
            {
                for (x = 0; x < Atts.MaxLocsX; x++)
                {
                    map[x, y].GridLine = true;
                }
                y += 6;
            }

        }

        // Gives different numbers of rooms
        private static int GenerateNumberOfRooms()
        {
            var decider = Atts.R.Next(4);
            switch (decider)
            {
                case 0:
                    return Atts.R.Next(10) + 10;
                case 1:
                    return Atts.R.Next(15) + 5;
                case 2:
                    return Atts.R.Next(25) + 5;
                case 3:
                    return Atts.R.Next(10) + 20;
                default:
                    return 20;
            }
        }

        // First generate all the rooms and add to the map
        private void GenerateRooms()
        {
            numberRooms = GenerateNumberOfRooms();
            for (var roomNum = 0; roomNum < numberRooms; roomNum++)
            {
                var room = new DungeonRoom(roomNum);
                room.CreateRoom(map);
                rooms.Add(room);
            }
            DumpTextMap(0);
            LinkAllRooms();
        }

        // Checks to see if all rooms in level are now linked
        private bool CheckAllRoomsAreLinked()
        {
            LinkedRooms.Clear();
            foreach (var room1 in rooms)
            {
                foreach (var room2 in rooms)
                {
                    if (!room1.IsSameRoom(room2) && (room1.ConnectedRooms.Contains(room2) || room2.ConnectedRooms.Contains(room1)))
                    {
                        LinkedRooms.Add(room1);
                        LinkedRooms.Add(room2);
                    }
                }
            }

            foreach (var room in rooms)
            {
                if (!LinkedRooms.Contains(room))
                {
                    return false;
                }
            }
            return true;
        }

        private void LinkAllRooms()
        {
            foreach (var room in rooms)
            {
                room.ClearExits();
            }

            var mapNum = 1;
            DumpTextMap(mapNum++);
            cancellinking = false;
            cancelcounter = 0;
            var allRoomsLinked = false;

            while (!allRoomsLinked && !cancellinking)
            {
                allRoomsLinked = CheckAllRoomsAreLinked();
                if (!allRoomsLinked)
                {
                    TryLinkRooms();
                    DumpTextMap(mapNum++);
                }

            }
            if (cancellinking)
            {
                RemoveUnConnectedRooms();
                cancellinking = false;
            }
        }

        // Remove all rooms that we were unable to link to by corridor
        private void RemoveUnConnectedRooms()
        {
            foreach (var room in rooms)
            {
                if (!LinkedRooms.Contains(room))
                {
                    foreach (var corridor in room.Doors)
                    {
                        if (corridor != null)
                            corridor.RemoveFailedCorridor(corridor.StartX, corridor.StartY);
                    }
                    for (var y1 = room.Top - 1; y1 < room.Top + room.Height + 1; y1++)
                    {
                        for (var x1 = room.Left - 1; x1 < room.Left + room.Width + 1; x1++)
                        {
                            if (Atts.IsOnMap(x1, y1) &&
                                (map[x1, y1].LocationType == Atts.LocationType.WallPoint ||
                                map[x1, y1].LocationType == Atts.LocationType.RoomSpace ||
                                map[x1, y1].LocationType == Atts.LocationType.DoorPoint))
                            {
                                map[x1, y1].LocationType = Atts.LocationType.Empty;
                            }
                        }
                    }
                }
            }
        }

        private void TryLinkRooms()
        {
            var xdoor = 0;
            var ydoor = 0;
            var MaxAttempts = numberRooms * 20;
            while (!cancellinking)
            {
                cancelcounter++;
                var corridor = FindUnusedDoorinRandomRoom(ref xdoor, ref ydoor);
                if (corridor != null)
                {
                    starterRoom = corridor.StartRoom;
                    if (corridor.GrowCorridor(ref xdoor, ref ydoor))
                    {
                        return;
                    }
                }
                if (cancelcounter > MaxAttempts)
                {
                    cancellinking = true;
                }
            }
        }

        private Atts.Direction FindEmptySpaceNexttoDoor(ref int cx, ref int cy)
        {
            Atts.Direction dir = Atts.Direction.None;
            if (cx > 0 && map[cx - 1, cy].GridLine)
            {
                dir = Atts.Direction.West;
                cx--;
            }
            else if (cx + 1 < Atts.MaxLocsX && map[cx + 1, cy].GridLine)
            {
                dir = Atts.Direction.West;
                cx++;
            }
            else if (cy > 0 && map[cx, cy - 1].GridLine)
            {
                dir = Atts.Direction.North;
                cy--;
            }
            else if (cy + 1 < Atts.MaxLocsY && map[cx, cy + 1].GridLine)
            {
                dir = Atts.Direction.South;
                cy++;
            }
            return dir;
        }

        // Finds an unused door at a random map location and starts a corridor from there
        private Corridor FindUnusedDoorinRandomRoom(ref int x1, ref int y1)
        {
            var Dir = Atts.Direction.None;
            var whichExit = -1;
            var foundaDoor = false;
            var tries = 0;
            int cx, cy;

            do
            {
                foundaDoor = false;
                while (!foundaDoor)
                {
                    tries++;
                    if (tries == 1000)
                        return null;
                    do
                    {
                        y1 = Atts.R.Next(Atts.MaxLocsY);
                        x1 = Atts.R.Next(Atts.MaxLocsX);
                    } while (map[x1, y1].LocationType != Atts.LocationType.DoorPoint);
                    starterRoom = map[x1, y1].aRoom;

                    for (var iDir = 0; iDir < 4; iDir++)
                    {
                        if (starterRoom.Doors[iDir] == null)
                        {
                            foundaDoor = true;
                            whichExit = iDir;
                            break;
                        }
                    }
                }

                cx = x1;
                cy = y1;
                Dir = FindEmptySpaceNexttoDoor(ref cx, ref cy);

                map[x1, y1].LocationType = Dir == Atts.Direction.None ? Atts.LocationType.WallPoint : Atts.LocationType.TempDoor;

            } while (Dir == Atts.Direction.None);

            // create new corridor starting at this room
            var corridor = new Corridor(ref map)
            {
                StartX = cx,
                StartY = cy,
                CurrentX = cx,
                CurrentY = cy,
            };
            starterRoom.Doors[whichExit] = corridor;
            starterRoom.DoorDirections[whichExit] = Dir;
            corridor.StartRoom = starterRoom;
            return corridor;
        }

        // saves out a char map
        private void DumpTextMap(int mapnum)
        {
            return;
            var mapDumpFile = String.Format(@"map{0}.txt", mapnum);
            using (var sw = new StreamWriter(mapDumpFile))
            {
                for (var x1 = 0; x1 < Atts.MaxLocsX; x1++)
                {
                    if (x1 % 5 == 0)
                    {
                        sw.Write(x1.ToString("000  "));
                    }
                }
                sw.WriteLine();
                for (var y1 = 0; y1 < Atts.MaxLocsY; y1++)
                {
                    sw.Write(y1.ToString("000 "));
                    for (var x1 = 0; x1 < Atts.MaxLocsX; x1++)
                    {
                        var locType = map[x1, y1].LocationType;
                        var c = Atts.MapChar(locType, map[x1, y1].GridLine);
                        sw.Write(c);

                    }
                    sw.WriteLine(y1.ToString("000 "));
                }
                for (var x1 = 0; x1 < Atts.MaxLocsX; x1++)
                {
                    if (x1 % 5 == 0)
                    {
                        sw.Write(x1.ToString("000  "));
                    }
                }
                sw.WriteLine();
                sw.Close();
            }

        }
    }

    public class Dungeon
    {
        private List<DungeonLevel> levels = new List<DungeonLevel>();

        public Dungeon(int NumberLevels)
        {
            for (var level = 0; level < NumberLevels; level++)
            {
                var dungeonLevel = new DungeonLevel(level, level == NumberLevels);
                levels.Add(dungeonLevel);
            }
        }
    }
}
