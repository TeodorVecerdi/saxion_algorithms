using System.Drawing;

namespace application {
    internal class TiledDungeonView : TiledView {
        private NiceDungeon dungeon;

        public TiledDungeonView(NiceDungeon pDungeon, TileType pDefaultTileType) : base(pDungeon.size.Width, pDungeon.size.Height, (int) pDungeon.scale, pDefaultTileType) {
            dungeon = pDungeon;
        }

        protected override void generate() {
            foreach (Room room in dungeon.rooms) {
                // get the inside of the room
                Rectangle center = room.Area;
                center.Inflate(-1, -1); // shrinks the room by 1 in all directions

                // fill the room with the ground tile
                for (int x = center.Left; x < center.Right; x++)
                for (int y = center.Top; y < center.Bottom; y++)
                    SetTileType(x, y, TileType.GROUND);
            }

            foreach (Door door in dungeon.doors) {
                SetTileType(door.Location.X, door.Location.Y, TileType.GROUND);
            }
            
            foreach (Hallway hallway in dungeon.hallways) {
                if (hallway.Horizontal)
                    for (int i = hallway.Start.X; i <= hallway.End.X; i++)
                        SetTileType(i, hallway.Start.Y, TileType.GROUND);
                else
                    for (int i = hallway.Start.Y; i <= hallway.End.Y; i++)
                        SetTileType(hallway.Start.X, i, TileType.GROUND);
            }
        }
    }
}