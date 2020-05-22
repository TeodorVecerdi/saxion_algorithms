using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GXPEngine;
using TiledMapParser;
using Debug = application.utils.Debug;

namespace application {
    public class NiceDungeon : Dungeon {
        private readonly DungeonType dungeonType;
        private int minimumRoomSize;
        private List<RoomDefinition> roomDefinitions;
        private List<DoorDefinition> doorDefinitions;

        private readonly EasyDraw debug;
        private readonly int scale;

        public NiceDungeon(Size pSize, int scale, DungeonType dungeonType) : base(pSize) {
            this.scale = scale;
            this.dungeonType = dungeonType;
            debug = new EasyDraw(pSize.Width * scale, pSize.Height * scale);
            debug.scale = 1f / scale;
            AddChild(debug);
        }

        protected override void generate(int minimumRoomSize) {
            this.minimumRoomSize = minimumRoomSize;
            roomDefinitions = new List<RoomDefinition>();
            doorDefinitions = new List<DoorDefinition>();

            Rand.PushState(1233);

            // Generate the dungeon rooms in a data-oriented way using the RoomDefinition struct
            GenerateRoomDefinitionsRecurse(new Rectangle(Point.Empty, size));
            Debug.LogInfo("Printing rooms:");
            roomDefinitions.ForEach(definition => Debug.LogInfo($"\t{definition.ID}: {definition.RoomArea}"));

            GenerateDoorDefinitions();

            // Convert the generated dungeon room and door definitions to actual Room and Door objects
            ConvertDefinitionsToDungeon();

            if (dungeonType >= DungeonType.Good)
                CleanRooms(); // Removing rooms which have the area the same as maximum and minimum area
            if (dungeonType == DungeonType.Excellent)
                ShrinkRooms(); // Make rooms smaller in size by some amount

            DrawDebug();
        }

        #region Generate Room Definitions
        private const bool randomDirection = false;

        private void GenerateRoomDefinitionsRecurse(Rectangle currentRoomSize) {
            int direction; // -1 = horizontal, 1 = vertical
            if (randomDirection) direction = Rand.Sign;
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
                    roomDefinitions.Add(new RoomDefinition(currentRoomSize));
                    return;
                } else {
                    splitPoint = Rand.RangeInclusive(minRoomSizeA2, minRoomSizeB2);
                }
            } else {
                splitPoint = Rand.RangeInclusive(minRoomSizeA1, minRoomSizeB1);
            }

            if (direction == -1) {
                GenerateRoomDefinitionsRecurse(new Rectangle(currentRoomSize.X, currentRoomSize.Y, splitPoint + 1 - currentRoomSize.X, currentRoomSize.Height));
                GenerateRoomDefinitionsRecurse(new Rectangle(splitPoint, currentRoomSize.Y, currentRoomSize.Width - splitPoint + currentRoomSize.X, currentRoomSize.Height));
            } else {
                GenerateRoomDefinitionsRecurse(new Rectangle(currentRoomSize.X, currentRoomSize.Y, currentRoomSize.Width, splitPoint + 1 - currentRoomSize.Y));
                GenerateRoomDefinitionsRecurse(new Rectangle(currentRoomSize.X, splitPoint, currentRoomSize.Width, currentRoomSize.Height - splitPoint + currentRoomSize.Y));
            }
        }

        private void GenerateRoomDefinitionsIter(Rectangle startingSize) {
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

            roomDefinitions.AddRange(finalRooms.Select(room => new RoomDefinition(room)));
        }
        #endregion

        #region Generate Door Definitions
        private void GenerateDoorDefinitions() {
            // keep track of which rooms are already connected so we don't connect them again
            var connectedRooms = new HashSet<(int, int)>();

            // generates doors between all adjacent rooms
            foreach (var room in roomDefinitions) {
                var adjacentRooms = FindAdjacentRooms(room);
                // Debug.LogInfo($"Printing rooms adjacent to room[ID={room.ID}]:", "ADJ");
                // foreach (var adj in adjacentRooms) Debug.LogInfo($"    {adj.room.ID}, {adj.direction}", "ADJ");
                foreach (var adjacentRoom in adjacentRooms) {
                    if (connectedRooms.Contains((room.ID, adjacentRoom.room.ID))) continue;

                    connectedRooms.Add((room.ID, adjacentRoom.room.ID));
                    connectedRooms.Add((adjacentRoom.room.ID, room.ID));
                    var door = CreateDoor(room, adjacentRoom.room, adjacentRoom.direction, adjacentRoom.adjacencyType);
                    if (door.ID != -1)
                        doorDefinitions.Add(door);
                }
            }

            /*Debug.LogInfo($"Printing doors[Count={doorDefinitions.Count}]", "DOOR");
            foreach (var door in doorDefinitions) {
                Debug.LogInfo($"    Door[@{door.DoorPosition}, {door.Direction}]", "DOOR");
            }*/
        }

        private IEnumerable<(RoomDefinition room, Direction direction, RoomAdjacencyType adjacencyType)> FindAdjacentRooms(RoomDefinition room) {
            var adjacentRooms = new List<(RoomDefinition room, Direction direction, RoomAdjacencyType adjacencyType)>();
            foreach (var possibleRoom in roomDefinitions) {
                if (possibleRoom.ID == room.ID) continue;
                var (direction, adjacencyType) = CheckAdjacent(room.RoomArea, possibleRoom.RoomArea);
                if (adjacencyType == RoomAdjacencyType.NonAdjacent) continue;
                adjacentRooms.Add((possibleRoom, direction, adjacencyType));
            }

            return adjacentRooms;
        }

        private (Direction, RoomAdjacencyType) CheckAdjacent(Rect roomAFullRect, Rect roomBFullRect) {
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

        private DoorDefinition CreateDoor(RoomDefinition roomA, RoomDefinition roomB, Direction direction, RoomAdjacencyType adjacencyType) {
            if (direction == Direction.Horizontal) {
                var minHeight = Mathf.Max(roomA.RoomArea.Top, roomB.RoomArea.Top);
                var maxHeight = Mathf.Min(roomA.RoomArea.Bottom, roomB.RoomArea.Bottom);
                if (minHeight > maxHeight) return new DoorDefinition(Point.Empty, Direction.Horizontal, -1, -1, -1);
                var doorY = Rand.RangeInclusive(minHeight + 1, maxHeight - 1);
                // var doorY = (minHeight + maxHeight) / 2;
                
                if (adjacencyType == RoomAdjacencyType.AtoB) {
                    // the door should be on the right edge of roomA / left edge of roomB 
                    var doorX = roomA.RoomArea.Right;
                    return new DoorDefinition(new Point(doorX, doorY), direction, roomA.ID, roomB.ID);
                } else {
                    // the door should be on the left edge of roomA / right edge of roomB
                    var doorX = roomA.RoomArea.Left - 1;
                    return new DoorDefinition(new Point(doorX + 1, doorY), direction, roomA.ID, roomB.ID);
                }
            } else {
                var minWidth = Mathf.Max(roomA.RoomArea.Left, roomB.RoomArea.Left);
                var maxWidth = Mathf.Min(roomA.RoomArea.Right, roomB.RoomArea.Right);
                if (minWidth > maxWidth) return new DoorDefinition(Point.Empty, Direction.Horizontal, -1, -1, -1);
                var doorX = Rand.RangeInclusive(minWidth + 1, maxWidth - 1);
                // var doorX = (minWidth + maxWidth) / 2;
                
                if (adjacencyType == RoomAdjacencyType.AtoB) {
                    // the door should be on the bottom edge of roomA / top edge of roomB 
                    var doorY = roomA.RoomArea.Bottom;
                    return new DoorDefinition(new Point(doorX, doorY), direction, roomA.ID, roomB.ID);
                } else {
                    // the door should be on the top edge of roomA / bottom edge of roomB
                    var doorY = roomA.RoomArea.Top;
                    return new DoorDefinition(new Point(doorX, doorY), direction, roomA.ID, roomB.ID);
                }
            }
        }
        #endregion

        private void ConvertDefinitionsToDungeon() {
            rooms.AddRange(roomDefinitions.Select(roomDef => new Room(roomDef.RoomArea, roomDef.ID)));
            doors.AddRange(doorDefinitions.Select(doorDef => new Door(doorDef.DoorPosition, doorDef.Direction == Direction.Horizontal, doorDef.ID, doorDef.RoomAID, doorDef.RoomBID)));
        }

        private void CleanRooms() {
            Debug.LogWarning("CleanRooms not implemented!");
        }

        private void ShrinkRooms() {
            Debug.LogWarning("ShrinkRooms not implemented!");
        }

        private void DrawDebug() {
            debug.Clear(Color.Transparent);
            debug.TextSize(8);
            doors.ForEach(door => {
                debug.Fill(Color.Black);
                debug.Text($"{door.RoomAid}->{door.RoomBid}\n{(door.Horizontal ? "Horizontal" : "Vertical")}", door.Location.X * scale + 1, (door.Location.Y + 1) * scale + 1);
                debug.Fill(Color.White);
                debug.Text($"{door.RoomAid}->{door.RoomBid}\n{(door.Horizontal ? "Horizontal" : "Vertical")}", door.Location.X * scale, (door.Location.Y + 1) * scale);
            });

            debug.TextSize(16);
            rooms.ForEach(room => {
                var pos = new Vector2((room.area.Left + room.area.Right) * scale / 2f, (room.area.Top + room.area.Bottom) * scale / 2f);
                debug.Fill(Color.Crimson);
                debug.Text($"{room.id}", pos.x, pos.y);
            });
        }

        protected override void draw() {
            if (dungeonType >= DungeonType.Good) {
                Debug.LogWarning("Custom Drawing for DungeonType >= Good not implemented!");

                // TODO: custom drawing from Assignment 1.2 (different colours depending on number of doors)
                base.draw();
                return;
            }

            base.draw();
        }
    }
}