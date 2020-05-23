using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GXPEngine;
using Debug = application.utils.Debug;

using ID = System.Int32;

namespace application {
    public class NiceDungeon : Dungeon {
        private readonly DungeonType dungeonType;
        private int minimumRoomSize;
        
        // ID = System.Int32
        private Dictionary<ID, RoomDefinition> roomDefinitions;
        private Dictionary<ID, DoorDefinition> doorDefinitions;
        private Dictionary<ID, int> doorCounts;
        private Dictionary<ID, List<(ID roomID, Direction direction, RoomAdjacencyType adjacencyType)>> roomAdjacencyLists;

        private readonly EasyDraw debug;
        private readonly int canvasScale;

        public NiceDungeon(Size pSize, int canvasScale, DungeonType dungeonType) : base(pSize) {
            this.canvasScale = canvasScale;
            this.dungeonType = dungeonType;
            debug = new EasyDraw(pSize.Width * canvasScale, pSize.Height * canvasScale);
            debug.scale = 1f / canvasScale;
            AddChild(debug);
        }

        protected override void generate(int minimumRoomSize) {
            this.minimumRoomSize = minimumRoomSize;
            
            roomDefinitions = new Dictionary<ID, RoomDefinition>();
            doorDefinitions = new Dictionary<ID, DoorDefinition>();
            doorCounts = new Dictionary<ID, int>();
            roomAdjacencyLists = new Dictionary<ID, List<(ID roomID, Direction direction, RoomAdjacencyType adjacencyType)>>();

            Rand.PushState(1233); // test seed

            // Generate the dungeon rooms
            GenerateRooms_Recurse(new Rectangle(Point.Empty, size));
            // GenerateRooms_Iter(new Rectangle(Point.Empty, size));
            
            if (dungeonType >= DungeonType.Good)
                CleanRooms(); // Remove rooms which have the area the same as maximum and minimum area
            
            GenerateRoomAdjacencyLists();
            
            // Link the rooms with doors
            // GenerateDoors_Full(); // connect all rooms to all adjacent rooms
            GenerateDoors_Min(); // only the minimum required amount of doors (n-1?)
            
            if (dungeonType >= DungeonType.Good)
                CalculateDoorCount();
            
            // Convert the generated dungeon room and door definitions to actual Room and Door objects
            CreateDungeon();

            if (dungeonType == DungeonType.Excellent)
                ShrinkRooms(); // Make rooms smaller in size by some amount

            DrawDebug();
        }

        #region Generate Room Definitions
        private const bool randomSplitDirection = false;

        private void GenerateRooms_Recurse(Rectangle currentRoomSize) {
            int direction; // -1 = horizontal, 1 = vertical
            if (randomSplitDirection) direction = Rand.Sign;
            else {
                // Calculate direction based on width/height of currentRoomSize.
                // Prefer splitting vertically if it's wider than taller or horizontally otherwise
                // Should yield nicer results.
                direction = currentRoomSize.Width > currentRoomSize.Height ? 1 : -1;
            }

            var minRoomSizeA1 = (direction == -1 ? currentRoomSize.Left : currentRoomSize.Top) + minimumRoomSize + 1;
            var minRoomSizeB1 = (direction == -1 ? currentRoomSize.Right : currentRoomSize.Bottom) - minimumRoomSize - 1;
            var splitPoint = 0;
            if (minRoomSizeA1 >= minRoomSizeB1) {
                // means we can't divide in the chosen direction such that rooms would be bigger than minimum area
                // try to divide in the other direction
                direction = -direction;
                var minRoomSizeA2 = (direction == -1 ? currentRoomSize.Left : currentRoomSize.Top) + minimumRoomSize + 1;
                var minRoomSizeB2 = (direction == -1 ? currentRoomSize.Right : currentRoomSize.Bottom) - minimumRoomSize - 1;

                if (minRoomSizeA2 >= minRoomSizeB2) {
                    // can't divide at all. Get out of here.
                    var roomDef = new RoomDefinition(currentRoomSize);
                    roomDefinitions.Add(roomDef.ID, roomDef);
                    return;
                } else {
                    splitPoint = Rand.RangeInclusive(minRoomSizeA2, minRoomSizeB2);
                }
            } else {
                splitPoint = Rand.RangeInclusive(minRoomSizeA1, minRoomSizeB1);
            }

            if (direction == -1) {
                GenerateRooms_Recurse(new Rectangle(currentRoomSize.X, currentRoomSize.Y, splitPoint + 1 - currentRoomSize.X, currentRoomSize.Height));
                GenerateRooms_Recurse(new Rectangle(splitPoint, currentRoomSize.Y, currentRoomSize.Width - splitPoint + currentRoomSize.X, currentRoomSize.Height));
            } else {
                GenerateRooms_Recurse(new Rectangle(currentRoomSize.X, currentRoomSize.Y, currentRoomSize.Width, splitPoint + 1 - currentRoomSize.Y));
                GenerateRooms_Recurse(new Rectangle(currentRoomSize.X, splitPoint, currentRoomSize.Width, currentRoomSize.Height - splitPoint + currentRoomSize.Y));
            }
        }

        private void GenerateRooms_Iter(Rectangle startingSize) {
            var availableRooms = new List<Rectangle>();
            var finalRooms = new List<Rectangle>();
            availableRooms.Add(startingSize);
            while (true) {
                var roomsToAdd = new List<Rectangle>();
                var roomsToDelete = new List<Rectangle>();
                foreach (var availableRoom in availableRooms) {
                    var direction = Rand.Sign; // -1 = horizontal, 1 = vertical
                    var from = (direction == -1 ? availableRoom.Left : availableRoom.Top) + minimumRoomSize;
                    var to = (direction == -1 ? availableRoom.Right : availableRoom.Bottom) - minimumRoomSize - 1;
                    if (from < to) {
                        roomsToDelete.Add(availableRoom);
                        Debug.Log($"Generating rooms from room: {availableRoom} with min/max splitPoint {from}, {to}");

                        var splitPoint = Rand.RangeInclusive(from, to);

                        // var splitPoint = (from + to) / 2;
                        if (direction == -1) {
                            roomsToAdd.Add(new Rectangle(availableRoom.X, availableRoom.Y, splitPoint + 1 - availableRoom.X, availableRoom.Height));
                            roomsToAdd.Add(new Rectangle(splitPoint, availableRoom.Y, availableRoom.Width - splitPoint + availableRoom.X, availableRoom.Height));
                        } else {
                            roomsToAdd.Add(new Rectangle(availableRoom.X, availableRoom.Y, availableRoom.Width, splitPoint + 1 - availableRoom.Y));
                            roomsToAdd.Add(new Rectangle(availableRoom.X, splitPoint, availableRoom.Width, availableRoom.Height - splitPoint + availableRoom.Y));
                        }
                    } else {
                        direction = -direction; // -1 = horizontal, 1 = vertical
                        var from2 = (direction == -1 ? availableRoom.Left : availableRoom.Top) + minimumRoomSize;
                        var to2 = (direction == -1 ? availableRoom.Right : availableRoom.Bottom) - minimumRoomSize - 1;
                        if (from2 < to2) {
                            roomsToDelete.Add(availableRoom);
                            Debug.Log($"Generating rooms from room: {availableRoom} with min/max splitPoint {from2}, {to2}");

                            var splitPoint = Rand.RangeInclusive(from2, to2);

                            // var splitPoint = (from2 + to2) / 2;
                            if (direction == -1) {
                                roomsToAdd.Add(new Rectangle(availableRoom.X, availableRoom.Y, splitPoint + 1 - availableRoom.X, availableRoom.Height));
                                roomsToAdd.Add(new Rectangle(splitPoint, availableRoom.Y, availableRoom.Width - splitPoint + availableRoom.X, availableRoom.Height));
                            } else {
                                roomsToAdd.Add(new Rectangle(availableRoom.X, availableRoom.Y, availableRoom.Width, splitPoint + 1 - availableRoom.Y));
                                roomsToAdd.Add(new Rectangle(availableRoom.X, splitPoint, availableRoom.Width, availableRoom.Height - splitPoint + availableRoom.Y));
                            }
                        } else {
                            roomsToDelete.Add(availableRoom);
                            finalRooms.Add(availableRoom);
                        }
                    }
                }

                if (roomsToAdd.Count == 0) break;

                roomsToDelete.ForEach(room => availableRooms.Remove(room));
                availableRooms.AddRange(roomsToAdd);
            }
            
            foreach(var room in finalRooms.Select(room => new RoomDefinition(room))){
                roomDefinitions.Add(room.ID, room);
            }
        }
        #endregion

        #region Generate Room Adjancency Lists
        private void GenerateRoomAdjacencyLists() {
            foreach (var roomID in roomDefinitions.Keys) {
                roomAdjacencyLists.Add(roomID, FindAdjacentRooms(roomID));
            }
        }

        private List<(ID roomID, Direction direction, RoomAdjacencyType adjacencyType)> FindAdjacentRooms(ID roomID) {
            var adjacentRooms = new List<(ID roomID, Direction direction, RoomAdjacencyType adjacencyType)>();
            foreach (var possibleRoomID in roomDefinitions.Keys) {
                if (possibleRoomID == roomID) continue;
                var (direction, adjacencyType) = CheckAdjacent(roomID, possibleRoomID);
                if (adjacencyType == RoomAdjacencyType.NonAdjacent) continue;
                adjacentRooms.Add((possibleRoomID, direction, adjacencyType));
            }
            return adjacentRooms;
        }

        private (Direction, RoomAdjacencyType) CheckAdjacent(ID roomAid, ID roomBid) {
            var roomAFullRect = roomDefinitions[roomAid].Area;
            var roomBFullRect = roomDefinitions[roomBid].Area;
            // Shrink the rects to the inner room size for calculating adjacency type and direction 
            var roomARect = roomAFullRect.Shrinked(1, 1);
            var roomBRect = roomBFullRect.Shrinked(1, 1);

            // 2 x Rect/Rect collision check for plus-shaped 'extended' rects
            if ((roomARect.Right + 1 >= roomBRect.Left - 1 && roomARect.Left - 1 <= roomBRect.Right + 1 && roomARect.Bottom >= roomBRect.Top && roomARect.Top <= roomBRect.Bottom) ||
                (roomARect.Right >= roomBRect.Left && roomARect.Left <= roomBRect.Right && roomARect.Bottom + 1 >= roomBRect.Top - 1 && roomARect.Top - 1 <= roomBRect.Bottom + 1)) {
                // Figure out direction and adjacency type
                if (roomARect.Right - roomBRect.Left == -2) return (Direction.Horizontal, RoomAdjacencyType.AtoB);
                if (roomBRect.Right - roomARect.Left == -2) return (Direction.Horizontal, RoomAdjacencyType.BtoA);
                if (roomARect.Bottom - roomBRect.Top == -2) return (Direction.Vertical, RoomAdjacencyType.AtoB);
                return (Direction.Vertical, RoomAdjacencyType.BtoA);
            }

            return (Direction.Horizontal, RoomAdjacencyType.NonAdjacent);
        }
        #endregion

        #region Generate Door Definitions
        private void GenerateDoors_Full() {
            // keep track of which rooms are already connected so we don't connect them again
            var connectedRooms = new HashSet<(ID, ID)>();

            // generates doors between all adjacent rooms
            foreach (var roomID in roomDefinitions.Keys) {
                var adjacentRooms = roomAdjacencyLists[roomID];
                foreach (var adjacentRoom in adjacentRooms) {
                    if (connectedRooms.Contains((roomID, adjacentRoom.roomID))) continue;

                    connectedRooms.Add((roomID, adjacentRoom.roomID));
                    connectedRooms.Add((adjacentRoom.roomID, roomID));
                    var door = CreateDoor(roomID, adjacentRoom);
                    if (door.ID != -1)
                        doorDefinitions.Add(door.ID, door);
                }
            }
        }

        private void GenerateDoors_Min() {
            var visited = new Dictionary<int, bool>();
            foreach(var roomID in roomDefinitions.Keys) visited.Add(roomID, false);
            DFS(roomDefinitions.Keys.First(), visited);
        }

        private void DFS(ID startNode, Dictionary<ID, bool> visited) {
            visited[startNode] = true;
            foreach (var room in roomAdjacencyLists[startNode]) {
                if(visited[room.roomID]) continue;
                var door = CreateDoor(startNode, room);
                if (door.ID != -1)
                    doorDefinitions.Add(door.ID, door);
                DFS(room.roomID, visited);
            }
        }

        private DoorDefinition CreateDoor(ID roomAid, (ID roomID, Direction direction, RoomAdjacencyType adjacencyType) adjacentRoom) {
            var roomAArea = roomDefinitions[roomAid].Area;
            var roomBArea = roomDefinitions[adjacentRoom.roomID].Area;
            if (adjacentRoom.direction == Direction.Horizontal) {
                var minHeight = Mathf.Max(roomAArea.Top, roomBArea.Top);
                var maxHeight = Mathf.Min(roomAArea.Bottom, roomBArea.Bottom);
                if (minHeight > maxHeight) return new DoorDefinition(Point.Empty, Direction.Horizontal, -1, -1, -1);
                var doorY = Rand.RangeInclusive(minHeight + 1, maxHeight - 1);
                
                if (adjacentRoom.adjacencyType == RoomAdjacencyType.AtoB) {
                    // the door should be on the right edge of roomA / left edge of roomB 
                    var doorX = roomAArea.Right;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, roomAid, adjacentRoom.roomID);
                } else {
                    // the door should be on the left edge of roomA / right edge of roomB
                    var doorX = roomAArea.Left;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, roomAid, adjacentRoom.roomID);
                }
            } else {
                var minWidth = Mathf.Max(roomAArea.Left, roomBArea.Left);
                var maxWidth = Mathf.Min(roomAArea.Right, roomBArea.Right);
                if (minWidth > maxWidth) return new DoorDefinition(Point.Empty, Direction.Horizontal, -1, -1, -1);
                var doorX = Rand.RangeInclusive(minWidth + 1, maxWidth - 1);
                
                if (adjacentRoom.adjacencyType == RoomAdjacencyType.AtoB) {
                    // the door should be on the bottom edge of roomA / top edge of roomB 
                    var doorY = roomAArea.Bottom;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, roomAid, adjacentRoom.roomID);
                } else {
                    // the door should be on the top edge of roomA / bottom edge of roomB
                    var doorY = roomAArea.Top;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, roomAid, adjacentRoom.roomID);
                }
            }
        }
        #endregion

        private void CalculateDoorCount() {
            doorCounts.Clear();
            foreach (var roomID in roomDefinitions.Keys) {
                doorCounts.Add(roomID, 0);
            }
            foreach(var door in doorDefinitions.Values) {
                doorCounts[door.RoomAID]++;
                doorCounts[door.RoomBID]++;
            }
        }
        
        private void CreateDungeon() {
            rooms.AddRange(roomDefinitions.Values.Select(roomDef => new Room(roomDef.Area, roomDef.ID)));
            doors.AddRange(doorDefinitions.Values.Select(doorDef => new Door(doorDef.DoorPosition, doorDef.Direction == Direction.Horizontal, doorDef.ID, doorDef.RoomAID, doorDef.RoomBID)));
        }

        private void CleanRooms() {
            var maxArea = -1;
            var minArea = size.Width * size.Height; // can't get bigger than the whole dungeon
            foreach (var room in roomDefinitions.Values) {
                var area = room.Area.Area;
                if (area < minArea) minArea = area;
                if (area > maxArea) maxArea = area;
            }
            // Filter out rooms that have the same area as the minimum and maximum area
            // into a new list and then remove each item from the room dictionary.
            foreach (var roomDef in roomDefinitions.Where(roomDef => {
                var area = roomDef.Value.Area.Area;
                return area == minArea || area == maxArea;
            }).ToList()) {
                roomDefinitions.Remove(roomDef.Key);
            }
        }

        private void ShrinkRooms() {
            Debug.LogWarning("ShrinkRooms not implemented!");
        }

        private void DrawDebug() {
            debug.Clear(Color.Transparent);
            debug.TextSize(8);
            doors.ForEach(door => {
                debug.Fill(Color.Black);
                debug.Text($"{door.RoomAid}->{door.RoomBid}\n{(door.Horizontal ? "Horizontal" : "Vertical")}", door.Location.X * canvasScale + 1, (door.Location.Y + 1) * canvasScale + 1);
                debug.Fill(Color.White);
                debug.Text($"{door.RoomAid}->{door.RoomBid}\n{(door.Horizontal ? "Horizontal" : "Vertical")}", door.Location.X * canvasScale, (door.Location.Y + 1) * canvasScale);
            });

            debug.TextSize(16);
            rooms.ForEach(room => {
                var pos = new Vector2((room.area.Left + room.area.Right) * canvasScale / 2f, (room.area.Top + room.area.Bottom) * canvasScale / 2f);
                debug.Fill(Color.Crimson);
                debug.Text($"{room.id}", pos.x, pos.y);
            });
        }

        protected override void drawRoom(Room pRoom, Pen pWallColor, Brush pFillColor = null) {
            if (dungeonType >= DungeonType.Good) {
                var doorCount = doorCounts[pRoom.id];
                
                Brush fillColor;
                if (doorCount == 0) fillColor = Brushes.Red;
                else if (doorCount == 1) fillColor = Brushes.DarkOrange;
                else if (doorCount == 2) fillColor = Brushes.Yellow;
                else fillColor = Brushes.GreenYellow;
                
                graphics.FillRectangle(fillColor, pRoom.area.Left, pRoom.area.Top, pRoom.area.Width - 0.5f, pRoom.area.Height - 0.5f);
                graphics.DrawRectangle(pWallColor, pRoom.area.Left, pRoom.area.Top, pRoom.area.Width - 0.5f, pRoom.area.Height - 0.5f);
            }

            base.drawRoom(pRoom, pWallColor, pFillColor);
        }
    }
}