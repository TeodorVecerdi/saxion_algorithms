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

            // Generate the dungeon rooms  in a data-oriented way using the RoomDefinition struct
            GenerateRoomDefinitions(new Rectangle(Point.Empty, size));
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
        private void _GenerateRoomDefinitions(Rectangle currentRoomSize) {
            var direction = Rand.Sign; // -1 = horizontal, 1 = vertical
            var minRoomSizeA1 = (direction == -1 ? currentRoomSize.X : currentRoomSize.Y) + minimumRoomSize + 1;
            var minRoomSizeB1 = (direction == -1 ? currentRoomSize.X + currentRoomSize.Width : currentRoomSize.Y + currentRoomSize.Height) - minimumRoomSize - 1;
            var splitPoint = 0;
            if (minRoomSizeA1 >= minRoomSizeB1) {
                // means we can't divide in the chosen direction such that rooms would be bigger than minimum area
                // try to divide in the other direction
                direction = -direction;
                var minRoomSizeA2 = (direction == -1 ? currentRoomSize.X : currentRoomSize.Y) + minimumRoomSize + 1;
                var minRoomSizeB2 = (direction == -1 ? currentRoomSize.X + currentRoomSize.Width : currentRoomSize.Y + currentRoomSize.Height) - minimumRoomSize - 1;

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
                GenerateRoomDefinitions(new Rectangle(currentRoomSize.X, currentRoomSize.Y, splitPoint + 1, currentRoomSize.Height));
                GenerateRoomDefinitions(new Rectangle(currentRoomSize.X + splitPoint, currentRoomSize.Y, currentRoomSize.Width - splitPoint, currentRoomSize.Height));
            } else {
                GenerateRoomDefinitions(new Rectangle(currentRoomSize.X, currentRoomSize.Y, currentRoomSize.Width, splitPoint + 1));
                GenerateRoomDefinitions(new Rectangle(currentRoomSize.X, currentRoomSize.Y + splitPoint, currentRoomSize.Width, currentRoomSize.Height - splitPoint));
            }
        }

        private void GenerateRoomDefinitions(Rectangle currentRoomSize) {
            var direction = Rand.Sign; // -1 = horizontal, 1 = vertical
            var minRoomSizeA1 = (direction == -1 ? currentRoomSize.X : currentRoomSize.Y) + minimumRoomSize + 1;
            var minRoomSizeB1 = (direction == -1 ? currentRoomSize.X + currentRoomSize.Width : currentRoomSize.Y + currentRoomSize.Height) - minimumRoomSize - 1;
            if (minRoomSizeA1 >= minRoomSizeB1) {
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