using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GXPEngine;
using Debug = application.utils.Debug;

namespace application {
    public class NiceDungeon : Dungeon {
        public readonly DungeonType DungeonType;
        private int minimumRoomSize;
        private List<RoomDefinition> roomDefinitions;
        private List<DoorDefinition> doorDefinitions;

        public NiceDungeon(Size pSize, DungeonType dungeonType) : base(pSize) {
            DungeonType = dungeonType;
        }

        protected override void generate(int minimumRoomSize) {
            this.minimumRoomSize = minimumRoomSize;
            roomDefinitions = new List<RoomDefinition>();
            doorDefinitions = new List<DoorDefinition>();

            // Rand.PushState(1233);
            // Generate the dungeon rooms  in a data-oriented way using the RoomDefinition struct
            GenerateRoomDefinitionsRecurse(new Rectangle(Point.Empty, size));
            Debug.LogInfo("Printing rooms:");
            roomDefinitions.ForEach(definition => Debug.LogInfo($"\t{definition.ID}: {definition.RoomArea}"));

            GenerateDoorDefinitions();

            // Convert the generated dungeon room and door definitions to actual Room and Door objects
            ConvertDefinitionsToDungeon();

            if (DungeonType >= DungeonType.Good)
                CleanRooms(); // Removing rooms which have the area the same as maximum and minimum area
            if (DungeonType == DungeonType.Excellent)
                ShrinkRooms(); // Make rooms smaller in size by some amount
        }

        #region Generate Room Definitions
        private void GenerateRoomDefinitionsRecurse(Rectangle currentRoomSize) {
            var direction = Rand.Sign; // -1 = horizontal, 1 = vertical
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

        private void GenerateRoomDefinitions(Rectangle currentRoomSize) {
            var direction = (sbyte) Rand.Sign; // -1 = horizontal, 1 = vertical
            var minRoomSizeA1 = (direction == -1 ? currentRoomSize.Left : currentRoomSize.Top) + minimumRoomSize + 1;
            var minRoomSizeB1 = (direction == -1 ? currentRoomSize.Right : currentRoomSize.Bottom) - minimumRoomSize - 1;
            if (minRoomSizeA1 >= minRoomSizeB1 || minRoomSizeA1 <= 0 || minRoomSizeB1 <= 0) {
                roomDefinitions.Add(new RoomDefinition(currentRoomSize));
            } else {
                var splitPoint = Rand.RangeInclusive(minRoomSizeA1, minRoomSizeB1);
                if (direction == -1) {
                    GenerateRoomDefinitions(new Rectangle(currentRoomSize.X, currentRoomSize.Y, splitPoint + 1, currentRoomSize.Height));
                    GenerateRoomDefinitions(new Rectangle(currentRoomSize.X + splitPoint, currentRoomSize.Y, currentRoomSize.Width - splitPoint, currentRoomSize.Height));
                } else {
                    GenerateRoomDefinitions(new Rectangle(currentRoomSize.X, currentRoomSize.Y, currentRoomSize.Width, splitPoint + 1));
                    GenerateRoomDefinitions(new Rectangle(currentRoomSize.X, currentRoomSize.Y + splitPoint, currentRoomSize.Width, currentRoomSize.Height - splitPoint));
                }
            }
        }
        #endregion

        #region Generate Door Definitions
        private void GenerateDoorDefinitions() {
            // generates doors between all adjacent rooms
            foreach (var room in roomDefinitions) {
                var adjacentRooms = FindAdjacentRooms(room);
                foreach (var adjacentRoom in adjacentRooms) {
                    doorDefinitions.Add(CreateDoor(room, adjacentRoom.room, adjacentRoom.direction, adjacentRoom.adjacencyType));
                }
            }
        }

        private IEnumerable<(RoomDefinition room, Direction direction, RoomAdjacencyType adjacencyType)> FindAdjacentRooms(RoomDefinition room) {
            var adjacentRooms = new List<(RoomDefinition room, Direction direction, RoomAdjacencyType adjacencyType)>();
            foreach (var possibleRoom in roomDefinitions) {
                if (possibleRoom.ID == room.ID) continue;
                var (direction, adjacencyType) = CheckAdjacent(room, possibleRoom);
                if (adjacencyType == RoomAdjacencyType.NonAdjacent) continue;
                adjacentRooms.Add((possibleRoom, direction, adjacencyType));
            }

            return adjacentRooms;
        }

        private (Direction, RoomAdjacencyType) CheckAdjacent(RoomDefinition roomA, RoomDefinition roomB) {
            // Rect/Rect collision check works just fine since the rooms are technically touching by 1px
            if (!(roomA.RoomArea.X + roomA.RoomArea.Width >= roomB.RoomArea.X &&
                  roomA.RoomArea.X <= roomB.RoomArea.X + roomB.RoomArea.Width &&
                  roomA.RoomArea.Y + roomA.RoomArea.Height >= roomB.RoomArea.Y &&
                  roomA.RoomArea.Y <= roomB.RoomArea.Y + roomB.RoomArea.Height)) {
                // Figure out direction and adjacency type
                if (roomA.RoomArea.Left - roomB.RoomArea.Right == 1) return (Direction.Horizontal, RoomAdjacencyType.BtoA);
                if (roomB.RoomArea.Left - roomA.RoomArea.Right == 1) return (Direction.Horizontal, RoomAdjacencyType.AtoB);
                if (roomA.RoomArea.Top - roomB.RoomArea.Bottom == 1) return (Direction.Vertical, RoomAdjacencyType.BtoA);
                if (roomB.RoomArea.Top - roomA.RoomArea.Bottom == 1) return (Direction.Vertical, RoomAdjacencyType.AtoB);
            }

            return (Direction.Horizontal, RoomAdjacencyType.NonAdjacent);
        }

        private DoorDefinition CreateDoor(RoomDefinition roomA, RoomDefinition roomB, Direction direction, RoomAdjacencyType adjacencyType) {
            if (direction == Direction.Horizontal) {
                var minHeight = Mathf.Max(roomA.RoomArea.Top, roomB.RoomArea.Top);
                var maxHeight = Mathf.Min(roomA.RoomArea.Bottom, roomB.RoomArea.Bottom);
                var doorY = Rand.RangeInclusive(minHeight, maxHeight);
                if (adjacencyType == RoomAdjacencyType.AtoB) {
                    // the door should be on the right edge of roomA / left edge of roomB 
                    var doorX = roomA.RoomArea.Right;
                    return new DoorDefinition(new Point(doorX, doorY), direction, roomA.ID, roomB.ID);
                } else {
                    // the door should be on the left edge of roomA / right edge of roomB
                    var doorX = roomA.RoomArea.Left - 1;
                    return new DoorDefinition(new Point(doorX, doorY), direction, roomA.ID, roomB.ID);
                }
            } else {
                var minWidth = Mathf.Max(roomA.RoomArea.Left, roomB.RoomArea.Left);
                var maxWidth = Mathf.Min(roomA.RoomArea.Right, roomB.RoomArea.Right);
                var doorX = Rand.RangeInclusive(minWidth, maxWidth);
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

            // doors.AddRange(doorDefinitions.Select(doorDef => new Door(doorDef.DoorPosition, doorDef.Direction == Direction.Horizontal, doorDef.DoorID)));
            AssociateDoorsWithRooms();
        }

        private void AssociateDoorsWithRooms() {
            Debug.LogWarning("AssociateDoorsWithRooms not implemented!");
        }

        private void CleanRooms() {
            Debug.LogWarning("CleanRooms not implemented!");
        }

        private void ShrinkRooms() {
            Debug.LogWarning("ShrinkRooms not implemented!");
        }

        protected override void draw() {
            if (DungeonType >= DungeonType.Good) {
                Debug.LogWarning("Custom Drawing for DungeonType >= Good not implemented!");

                // TODO: custom drawing from Assignment 1.2 (different colours depending on number of doors)
                base.draw();
                return;
            }

            base.draw();
        }
    }
}