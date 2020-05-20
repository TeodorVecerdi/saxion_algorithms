using System;
using System.Drawing;

namespace application {
    public struct DoorDefinition {
        public Point DoorPosition;
        public Direction Direction;
        public string DoorID;
        public string RoomAID;
        public string RoomBID;

        public DoorDefinition(Point doorPosition, Direction direction, string doorID, string roomAid, string roomBid) {
            DoorPosition = doorPosition;
            Direction = direction;
            DoorID = doorID;
            RoomAID = roomAid;
            RoomBID = roomBid;
        }
        public DoorDefinition(Point doorPosition, Direction direction, string roomAid, string roomBid) : this(doorPosition, direction, Guid.NewGuid().ToString(), roomAid, roomBid) {}
    }
}