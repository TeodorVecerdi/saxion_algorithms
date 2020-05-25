using System;
using System.Drawing;

namespace application {
    public struct DoorDefinition {
        public Point Position;
        public readonly Direction Direction;
        public readonly int ID;
        public readonly int RoomAID;
        public readonly int RoomBID;

        private static int ids = 0;

        private DoorDefinition(Point position, Direction direction, int id, int roomAid, int roomBid) {
            Position = position;
            Direction = direction;
            ID = id;
            RoomAID = roomAid;
            RoomBID = roomBid;
        }
        public DoorDefinition(Point position, Direction direction, int roomAid, int roomBid) : this(position, direction, ids++, roomAid, roomBid) {}
        public static DoorDefinition Error = new DoorDefinition(Point.Empty, Direction.Horizontal, -1, -1, -1);
    }
}