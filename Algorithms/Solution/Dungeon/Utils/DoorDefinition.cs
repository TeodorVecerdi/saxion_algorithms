using System;
using System.Drawing;

namespace application {
    public struct DoorDefinition {
        public Point DoorPosition;
        public Direction Direction;
        public int ID;
        public int RoomAID;
        public int RoomBID;

        private static int ids = 0;

        public DoorDefinition(Point doorPosition, Direction direction, int id, int roomAid, int roomBid) {
            DoorPosition = doorPosition;
            Direction = direction;
            ID = id;
            RoomAID = roomAid;
            RoomBID = roomBid;
        }
        public DoorDefinition(Point doorPosition, Direction direction, int roomAid, int roomBid) : this(doorPosition, direction, ids++, roomAid, roomBid) {}
    }
}