using System.Drawing;

/**
 * This class represents (the data for) a Door, at this moment only a position in the dungeon.
 * Changes to this class might be required based on your specific implementation of the algorithm.
 */
public class Door {
    public readonly Point Location;
    public readonly int ID;
    public readonly int RoomAid;
    public readonly int RoomBid;

    //You can also keep track of additional information such as whether the door connects horizontally/vertically
    //Again, whether you need flags like this depends on how you implement the algorithm, maybe you need other flags
    public readonly bool Horizontal;

    public Door(Point pLocation, bool pDirection, int pID, int roomAid, int roomBid) {
        Location = pLocation;
        Horizontal = pDirection;
        ID = pID;
        RoomAid = roomAid;
        RoomBid = roomBid;
    }

    public override string ToString() {
        return $"Door(Location:{Location})";
    }
}