using System.Drawing;

namespace application {
    public struct HallwayDefinition {
        public Point Start;
        public Point End;
        public readonly Direction Direction;
        public readonly int ID;
        public readonly int RoomAID;
        public readonly int RoomBID;

        private static int ids = 0;

        private HallwayDefinition(Point start, Point end, Direction direction, int id, int roomAID, int roomBID) {
            Start = start;
            End = end;
            Direction = direction;
            ID = id;
            RoomAID = roomAID;
            RoomBID = roomBID;
        }

        public HallwayDefinition(Point start, Point end, Direction direction, int roomAID, int roomBID) : this(start, end, direction, ids++, roomAID, roomBID) { }
        public static HallwayDefinition Error = new HallwayDefinition(Point.Empty, Point.Empty, Direction.Horizontal, -1, -1, -1);
    }
}