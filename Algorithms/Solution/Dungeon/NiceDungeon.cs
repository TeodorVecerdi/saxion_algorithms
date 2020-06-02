using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GXPEngine;
using Debug = application.utils.Debug;
using DungeonDataType = System.ValueTuple<System.Collections.Generic.Dictionary<int, application.RoomDefinition>, System.Collections.Generic.Dictionary<int, application.DoorDefinition>, System.Collections.Generic.Dictionary<int, application.HallwayDefinition>>;

namespace application {
    public class NiceDungeon : Dungeon {
        public readonly List<Hallway> hallways = new List<Hallway>();

        private readonly int canvasScale;
        private readonly EasyDraw debug;
        private readonly DungeonType dungeonType;
        private int minimumRoomSize;

        // ID = System.Int32
        private Dictionary<int, RoomDefinition> roomDefinitions;
        private Dictionary<int, DoorDefinition> doorDefinitions;
        private Dictionary<int, HallwayDefinition> hallwayDefinitions;
        private Dictionary<int, int> doorCounts;
        private Dictionary<int, List<(int roomID, Direction direction, RoomAdjacencyType adjacencyType)>> roomAdjacencyLists;
        
        public DungeonDataType DungeonData => (roomDefinitions, doorDefinitions, hallwayDefinitions);
        public DungeonType DungeonType => dungeonType;

        public NiceDungeon(Size pSize, int canvasScale, DungeonType dungeonType, int randomSeed = -1) : base(pSize) {
            this.canvasScale = canvasScale;
            this.dungeonType = dungeonType;
            debug = new EasyDraw(pSize.Width * canvasScale, pSize.Height * canvasScale) {scale = 1f / canvasScale};
            AddChild(debug);

            if (randomSeed == -1) {
                randomSeed = Environment.TickCount;
            }
            Rand.PushState(randomSeed);
        }

        protected override void generate(int minimumRoomSize) {
            this.minimumRoomSize = minimumRoomSize;

            // Initialize the dictionaries/data structures used by the algorithm
            Initialize();

            // Generate the dungeon rooms
            GenerateRooms_Recursive(new Rectangle(Point.Empty, size));

            // GenerateRooms_Iterative(new Rectangle(Point.Empty, size));

            // Remove rooms which have the area the same as maximum and minimum area
            if ((dungeonType & DungeonType.Good) != DungeonType.None)
                CleanRooms();

            GenerateRoomAdjacencyLists();

            // Link the rooms with doors
            GenerateDoors_Full(); // connect all rooms to all adjacent rooms

            // GenerateDoors_Min(); // only the minimum required amount of doors

            if ((dungeonType & DungeonType.Excellent) != DungeonType.None) {
                ShrinkRooms(); // Make rooms smaller in size by some amount
                CleanDoors(); // Remove doors which are no longer valid, move valid doors to a valid spot
                ConvertDoorsToHallways();
            }

            if ((dungeonType & DungeonType.Good) != DungeonType.None)
                CalculateDoorCount(); // Custom room drawing (Paint all rooms with 0 doors red, 1 door orange, 2 doors yellow, 3+ doors green)

            // Convert the generated dungeon room, door and hallway definitions to actual Room, Door and Hallway objects
            CreateDungeon();

            // Show debug information such as room id, door id, door from->to room ids   
            DrawDebug();
        }

        private void Initialize() {
            roomDefinitions = new Dictionary<int, RoomDefinition>();
            doorDefinitions = new Dictionary<int, DoorDefinition>();
            hallwayDefinitions = new Dictionary<int, HallwayDefinition>();
            doorCounts = new Dictionary<int, int>();
            roomAdjacencyLists = new Dictionary<int, List<(int roomID, Direction direction, RoomAdjacencyType adjacencyType)>>();
        }

        #region Generate Room Definitions
        private const bool randomSplitDirection = false;

        private void GenerateRooms_Recursive(Rectangle currentRoomSize) {
            int direction; // -1 = horizontal, 1 = vertical
            if (randomSplitDirection) direction = Rand.Sign;
            else {
                // Calculate direction based on width/height of currentRoomSize.
                // Prefer splitting vertically if it's wider than taller or horizontally otherwise
                // Should yield nicer results.
                direction = currentRoomSize.Width > currentRoomSize.Height ? 1 : -1;
            }

            var splitMin1 = (direction == -1 ? currentRoomSize.Left : currentRoomSize.Top) + minimumRoomSize; // + 1 if inner size <= minimum
            var splitMax1 = (direction == -1 ? currentRoomSize.Right : currentRoomSize.Bottom) - minimumRoomSize; // - 1 if inner size <= minimum
            var splitPoint = 0;
            if (splitMin1 >= splitMax1) {
                // means we can't divide in the chosen direction such that rooms would be bigger than minimum area
                // try to divide in the other direction
                direction = -direction;
                var splitMin2 = (direction == -1 ? currentRoomSize.Left : currentRoomSize.Top) + minimumRoomSize; // + 1 if inner size <= minimum
                var splitMax2 = (direction == -1 ? currentRoomSize.Right : currentRoomSize.Bottom) - minimumRoomSize; // - 1 if inner size <= minimum

                if (splitMin2 >= splitMax2) {
                    // can't divide at all. Get out of here.
                    var roomDef = new RoomDefinition(currentRoomSize);
                    roomDefinitions.Add(roomDef.ID, roomDef);
                    return;
                } else {
                    splitPoint = Rand.RangeInclusive(splitMin2, splitMax2);
                }
            } else {
                splitPoint = Rand.RangeInclusive(splitMin1, splitMax1);
            }
            
            if (direction == -1) {
                GenerateRooms_Recursive(new Rectangle(currentRoomSize.X, currentRoomSize.Y, splitPoint + 1 - currentRoomSize.X, currentRoomSize.Height));
                GenerateRooms_Recursive(new Rectangle(splitPoint, currentRoomSize.Y, currentRoomSize.Width - splitPoint + currentRoomSize.X, currentRoomSize.Height));
            } else {
                GenerateRooms_Recursive(new Rectangle(currentRoomSize.X, currentRoomSize.Y, currentRoomSize.Width, splitPoint + 1 - currentRoomSize.Y));
                GenerateRooms_Recursive(new Rectangle(currentRoomSize.X, splitPoint, currentRoomSize.Width, currentRoomSize.Height - splitPoint + currentRoomSize.Y));
            }
        }

        private void GenerateRooms_Iterative(Rectangle startingSize) {
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

            foreach (var room in finalRooms.Select(room => new RoomDefinition(room))) {
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

        private List<(int roomID, Direction direction, RoomAdjacencyType adjacencyType)> FindAdjacentRooms(int roomID) {
            var adjacentRooms = new List<(int roomID, Direction direction, RoomAdjacencyType adjacencyType)>();
            foreach (var possibleRoomID in roomDefinitions.Keys) {
                if (possibleRoomID == roomID) continue;
                var (direction, adjacencyType) = CheckAdjacent(roomID, possibleRoomID);
                if (adjacencyType == RoomAdjacencyType.NonAdjacent) continue;
                adjacentRooms.Add((possibleRoomID, direction, adjacencyType));
            }

            return adjacentRooms;
        }

        private (Direction, RoomAdjacencyType) CheckAdjacent(int roomAid, int roomBid) {
            var roomAFullRect = roomDefinitions[roomAid].Area;
            var roomBFullRect = roomDefinitions[roomBid].Area;

            // Shrink the rects to the inner room size for calculating adjacency type and direction 
            var roomARect = roomAFullRect.ShrinkedCenter(1, 1);
            var roomBRect = roomBFullRect.ShrinkedCenter(1, 1);

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
            var connectedRooms = new HashSet<(int, int)>();

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
            foreach (var roomID in roomDefinitions.Keys) visited.Add(roomID, false);
            DFS(roomDefinitions.Keys.First(), visited);
        }

        private void DFS(int startNode, Dictionary<int, bool> visited) {
            visited[startNode] = true;
            foreach (var room in roomAdjacencyLists[startNode]) {
                if (visited[room.roomID]) continue;
                var door = CreateDoor(startNode, room);
                if (door.ID != -1)
                    doorDefinitions.Add(door.ID, door);
                DFS(room.roomID, visited);
            }
        }

        private DoorDefinition CreateDoor(int roomAid, (int roomID, Direction direction, RoomAdjacencyType adjacencyType) adjacentRoom) {
            var roomAArea = roomDefinitions[roomAid].Area;
            var roomBArea = roomDefinitions[adjacentRoom.roomID].Area;
            if (adjacentRoom.direction == Direction.Horizontal) {
                var minHeight = Mathf.Max(roomAArea.Top, roomBArea.Top);
                var maxHeight = Mathf.Min(roomAArea.Bottom, roomBArea.Bottom);
                if (minHeight > maxHeight) // can't place a door between the two rooms
                    return DoorDefinition.Error;
                var doorY = Rand.RangeInclusive(minHeight + 1, maxHeight - 1);

                if (adjacentRoom.adjacencyType == RoomAdjacencyType.AtoB) {
                    // the door should be on the right edge of roomA / left edge of roomB 
                    var doorX = roomAArea.Right;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, roomAid, adjacentRoom.roomID);
                } else {
                    // the door should be on the left edge of roomA / right edge of roomB
                    var doorX = roomBArea.Right;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, adjacentRoom.roomID, roomAid);
                }
            } else {
                var minWidth = Mathf.Max(roomAArea.Left, roomBArea.Left);
                var maxWidth = Mathf.Min(roomAArea.Right, roomBArea.Right);
                if (minWidth > maxWidth) // can't place a door between the two rooms
                    return DoorDefinition.Error;
                var doorX = Rand.RangeInclusive(minWidth + 1, maxWidth - 1);

                if (adjacentRoom.adjacencyType == RoomAdjacencyType.AtoB) {
                    // the door should be on the bottom edge of roomA / top edge of roomB 
                    var doorY = roomAArea.Bottom;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, roomAid, adjacentRoom.roomID);
                } else {
                    // the door should be on the top edge of roomA / bottom edge of roomB
                    var doorY = roomBArea.Bottom;
                    return new DoorDefinition(new Point(doorX, doorY), adjacentRoom.direction, adjacentRoom.roomID, roomAid);
                }
            }
        }
        #endregion

        private void CalculateDoorCount() {
            doorCounts.Clear();
            foreach (var roomID in roomDefinitions.Keys) {
                doorCounts.Add(roomID, 0);
            }

            foreach (var door in doorDefinitions.Values) {
                doorCounts[door.RoomAID]++;
                doorCounts[door.RoomBID]++;
            }

            foreach (var hallway in hallwayDefinitions.Values) {
                doorCounts[hallway.RoomAID]++;
                doorCounts[hallway.RoomBID]++;
            }
        }

        private void CreateDungeon() {
            rooms.AddRange(roomDefinitions.Values.Select(roomDef => new Room(roomDef.Area, roomDef.ID)));
            doors.AddRange(doorDefinitions.Values.Select(doorDef => new Door(doorDef.Position, doorDef.Direction == Direction.Horizontal, doorDef.ID, doorDef.RoomAID, doorDef.RoomBID)));
            hallways.AddRange(hallwayDefinitions.Values.Select(hallwayDef => new Hallway(hallwayDef)));
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

        #region Shrink Rooms
        private void ShrinkRooms() {
            foreach (var roomID in roomDefinitions.Keys.ToList()) {
                var room = roomDefinitions[roomID];
                var rect = room.Area;

                var shrinkAmountHorizontal = Rand.Range(0, minimumRoomSize / 2);
                var shrinkAmountVertical = Rand.Range(0, minimumRoomSize / 2);
                var moveAmountHorizontal = shrinkAmountHorizontal / 2;
                var moveAmountVertical = shrinkAmountVertical / 2;
                rect.X += moveAmountHorizontal;
                rect.Width -= (shrinkAmountHorizontal - moveAmountHorizontal + shrinkAmountHorizontal % 2);
                rect.Y += moveAmountVertical;
                rect.Height -= (shrinkAmountVertical - moveAmountVertical + shrinkAmountVertical % 2);
                room.Area = rect;
                roomDefinitions[roomID] = room;
            }
        }

        /// <summary>
        /// Removes invalid doors by calculating the minimum and maximum position along the wall the door can have and checking if
        /// it's valid (min &lt;= max), and moves doors to the correct place if they are not correct anymore (the two rooms can still
        /// be connected but the door is placed in the wrong position as a result of shrinking the rooms)
        /// </summary>
        private void CleanDoors() {
            foreach (var doorID in doorDefinitions.Keys.ToList()) {
                var door = doorDefinitions[doorID];
                if (door.Direction == Direction.Horizontal) {
                    var minY = Mathf.Max(roomDefinitions[door.RoomAID].Area.Top + 1, roomDefinitions[door.RoomBID].Area.Top + 1);
                    var maxY = Mathf.Min(roomDefinitions[door.RoomAID].Area.Bottom - 1, roomDefinitions[door.RoomBID].Area.Bottom - 1);
                    if (minY > maxY) doorDefinitions.Remove(doorID); // can't connect rooms
                    else if (door.Position.Y < minY) {
                        // can still connect but door is placed in the wrong place
                        door.Position.Y = minY;
                        doorDefinitions[doorID] = door;
                    } else if (door.Position.Y > maxY) {
                        // can still connect but door is placed in the wrong place
                        door.Position.Y = maxY;
                        doorDefinitions[doorID] = door;
                    }
                } else {
                    var minX = Mathf.Max(roomDefinitions[door.RoomAID].Area.Left + 1, roomDefinitions[door.RoomBID].Area.Left + 1);
                    var maxX = Mathf.Min(roomDefinitions[door.RoomAID].Area.Right - 1, roomDefinitions[door.RoomBID].Area.Right - 1);
                    if (minX > maxX) doorDefinitions.Remove(doorID); // can't connect rooms
                    else if (door.Position.X < minX) {
                        // can still connect but door is placed in the wrong place
                        door.Position.X = minX;
                        doorDefinitions[doorID] = door;
                    } else if (door.Position.X > maxX) {
                        // can still connect but door is placed in the wrong place
                        door.Position.X = maxX;
                        doorDefinitions[doorID] = door;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the doors need to be converted into hallways, and if so converts them.
        /// </summary>
        private void ConvertDoorsToHallways() {
            foreach (var doorID in doorDefinitions.Keys.ToList()) {
                var door = doorDefinitions[doorID];
                if (door.Direction == Direction.Horizontal) {
                    var hallwayStartX = roomDefinitions[door.RoomAID].Area.Right;
                    var hallwayEndX = roomDefinitions[door.RoomBID].Area.Left;
                    if (hallwayStartX == hallwayEndX)
                        continue;

                    doorDefinitions.Remove(doorID);
                    var hallway = new HallwayDefinition(new Point(hallwayStartX, door.Position.Y), new Point(hallwayEndX, door.Position.Y), door.Direction, door.RoomAID, door.RoomBID);
                    hallwayDefinitions.Add(hallway.ID, hallway);
                } else {
                    var hallwayStartY = roomDefinitions[door.RoomAID].Area.Bottom;
                    var hallwayEndY = roomDefinitions[door.RoomBID].Area.Top;
                    if (hallwayStartY == hallwayEndY)
                        continue;

                    doorDefinitions.Remove(doorID);
                    var hallway = new HallwayDefinition(new Point(door.Position.X, hallwayStartY), new Point(door.Position.X, hallwayEndY), door.Direction, door.RoomAID, door.RoomBID);
                    hallwayDefinitions.Add(hallway.ID, hallway);
                }
            }
        }
        #endregion

        private void DrawDebug() {
            debug.Clear(Color.Transparent);
            debug.TextSize(8);
            doors.ForEach(door => {
                debug.Fill(Color.Black);
                debug.Text($"{door.RoomAID}->{door.RoomBID}\n{(door.Horizontal ? "Horizontal" : "Vertical")}", door.Location.X * canvasScale + 1, (door.Location.Y + 1) * canvasScale + 1);
                debug.Fill(Color.White);
                debug.Text($"{door.RoomAID}->{door.RoomBID}\n{(door.Horizontal ? "Horizontal" : "Vertical")}", door.Location.X * canvasScale, (door.Location.Y + 1) * canvasScale);
            });

            hallways.ForEach(hallway => {
                debug.TextSize(7);
                debug.Fill(Color.Black);
                debug.Text($"S", hallway.Start.X * canvasScale + 1, (hallway.Start.Y + 1) * canvasScale + 1);
                debug.Fill(Color.White);
                debug.Text($"S", hallway.Start.X * canvasScale, (hallway.Start.Y + 1) * canvasScale);

                debug.TextSize(8);
                debug.Fill(Color.Black);
                debug.Text($"{hallway.RoomAID}->{hallway.RoomBID}\n{(hallway.Horizontal ? "Horizontal" : "Vertical")}", (hallway.Start.X + hallway.End.X) / 2 * canvasScale + 1, ((hallway.Start.Y + hallway.End.Y) / 2 + 1) * canvasScale + 1);
                debug.Fill(Color.White);
                debug.Text($"{hallway.RoomAID}->{hallway.RoomBID}\n{(hallway.Horizontal ? "Horizontal" : "Vertical")}", (hallway.Start.X + hallway.End.X) / 2 * canvasScale, ((hallway.Start.Y + hallway.End.Y) / 2 + 1) * canvasScale);

                debug.TextSize(7);
                debug.Fill(Color.Black);
                debug.Text($"E", hallway.End.X * canvasScale + 1, (hallway.End.Y + 1) * canvasScale + 1);
                debug.Fill(Color.White);
                debug.Text($"E", hallway.End.X * canvasScale, (hallway.End.Y + 1) * canvasScale);
            });

            debug.TextSize(16);
            rooms.ForEach(room => {
                var pos = new Vector2((room.Area.Left + room.Area.Right) * canvasScale / 2f, (room.Area.Top + room.Area.Bottom) * canvasScale / 2f);
                debug.Fill(Color.Crimson);
                debug.Text($"{room.ID}", pos.x, pos.y);
            });
        }

        protected override void draw() {
            graphics.Clear(Color.Black);
            drawRooms(rooms, Pens.Black, Brushes.White);
            drawDoors(doors, Pens.Red);
            DrawHallways(hallways, Pens.Crimson, Pens.Orange);
        }

        protected override void drawRoom(Room pRoom, Pen pWallColor, Brush pFillColor = null) {
            if ((dungeonType & DungeonType.Good) != DungeonType.None) {
                var doorCount = doorCounts[pRoom.ID];

                Brush fillColor;
                if (doorCount == 0) fillColor = Brushes.Red;
                else if (doorCount == 1) fillColor = Brushes.DarkOrange;
                else if (doorCount == 2) fillColor = Brushes.Yellow;
                else fillColor = Brushes.GreenYellow;

                graphics.FillRectangle(fillColor, pRoom.Area.Left, pRoom.Area.Top, pRoom.Area.Width - 0.5f, pRoom.Area.Height - 0.5f);
                graphics.DrawRectangle(pWallColor, pRoom.Area.Left, pRoom.Area.Top, pRoom.Area.Width - 0.5f, pRoom.Area.Height - 0.5f);
            } else {
                if (pFillColor != null) graphics.FillRectangle(pFillColor, pRoom.Area.Left, pRoom.Area.Top, pRoom.Area.Width - 0.5f, pRoom.Area.Height - 0.5f);
                graphics.DrawRectangle(pWallColor, pRoom.Area.Left, pRoom.Area.Top, pRoom.Area.Width - 0.5f, pRoom.Area.Height - 0.5f);
            }
        }

        private void DrawHallways(IEnumerable<Hallway> hallways, Pen hallwayEntryColor, Pen hallwayColor) {
            foreach (var hallway in hallways) {
                DrawHallway(hallway, hallwayEntryColor, hallwayColor);
            }
        }

        private void DrawHallway(Hallway hallway, Pen hallwayEntryColor, Pen hallwayColor) {
            graphics.DrawRectangle(hallwayColor, hallway.Start.X, hallway.Start.Y, hallway.End.X - hallway.Start.X + 0.5f, hallway.End.Y - hallway.Start.Y + 0.5f);
            graphics.DrawRectangle(hallwayEntryColor, hallway.Start.X, hallway.Start.Y, 0.5f, 0.5f);
            graphics.DrawRectangle(hallwayEntryColor, hallway.End.X, hallway.End.Y, 0.5f, 0.5f);
        }
    }
}