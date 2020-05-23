using System.Drawing;

namespace application {
    public class Hallway {
        public readonly Point Start;
        public readonly Point End;
        public readonly int ID;
        public readonly int RoomAID;
        public readonly int RoomBID;
        public readonly bool Horizontal;

        public Hallway(Point start, Point end, int id, int roomAID, int roomBID, bool horizontal) {
            Start = start;
            End = end;
            ID = id;
            RoomAID = roomAID;
            RoomBID = roomBID;
            Horizontal = horizontal;
        }

        public Hallway(HallwayDefinition hallwayDefinition) : this(hallwayDefinition.Start, hallwayDefinition.End, hallwayDefinition.ID,
            hallwayDefinition.RoomAID, hallwayDefinition.RoomBID, hallwayDefinition.Direction == Direction.Horizontal) { }
    }
}