using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using application.utils;

namespace application {
    public class LowLevelNodeGraph : NodeGraph {
        private NiceDungeon dungeon;

        public LowLevelNodeGraph(NiceDungeon dungeon) : base((int) (dungeon.size.Width * dungeon.scale), (int) (dungeon.size.Height * dungeon.scale), (int) dungeon.scale / 3) {
            // Debug.Assert(dungeon != null, "Please pass in a dungeon.");
            this.dungeon = dungeon;
        }

        protected override void generate() {
            Node[,] nodes = GenerateNodes();
            GenerateConnections(nodes);
            List<Node> nodeList = ConvertToList(nodes);
            this.nodes.Clear();
            this.nodes.AddRange(nodeList);
        }

        private Node[,] GenerateNodes() {
            (Dictionary<int, RoomDefinition> rooms, Dictionary<int, DoorDefinition> doors, Dictionary<int, HallwayDefinition> hallways) = dungeon.DungeonData;
            Node[,] nodes = new Node[dungeon.size.Width, dungeon.size.Height];

            // loop through the rooms and create a node for each room
            foreach (RoomDefinition room in rooms.Values) {
                Rectangle center = room.Area;
                center.Inflate(-1, -1); // shrinks the room by 1 in all directions

                // fill the room with the ground tile
                for (int x = center.Left; x < center.Right; x++)
                for (int y = center.Top; y < center.Bottom; y++)
                    nodes[x, y] = new Node(GetPointCenter(new Point(x, y)));
            }

            // loop through the doors and create the necessary connections and nodes
            foreach (DoorDefinition door in doors.Values) {
                nodes[door.Position.X, door.Position.Y] = new Node(GetPointCenter(new Point(door.Position.X, door.Position.Y)));
            }

            // loop through the hallways and create the necessary connections and nodes
            foreach (HallwayDefinition hallway in hallways.Values) {
                if (hallway.Direction == Direction.Horizontal)
                    for (int i = hallway.Start.X; i <= hallway.End.X; i++)
                        nodes[i, hallway.Start.Y] = new Node(GetPointCenter(new Point(i, hallway.Start.Y)));

                else
                    for (int i = hallway.Start.Y; i <= hallway.End.Y; i++)
                        nodes[hallway.Start.X, i] = new Node(GetPointCenter(new Point(hallway.Start.X, i)));
            }

            return nodes;
        }

        private void GenerateConnections(Node[,] nodes) {
            for (int x = 0; x < dungeon.size.Width; x++) {
                for (int y = 0; y < dungeon.size.Height; y++) {
                    if (nodes[x, y] == null) continue;
                    
                    List<Node> neighbours = GetNeighbours(x, y, nodes);
                    foreach (Node neighbour in neighbours) {
                        // Add connection
                        if(!nodes[x,y].connections.Contains(neighbour)) nodes[x,y].connections.Add(neighbour);
                        if(!neighbour.connections.Contains(nodes[x,y])) neighbour.connections.Add(nodes[x,y]);
                    }
                }
            }
        }

        private List<Node> GetNeighbours(int x, int y, Node[,] nodes) {
            List<Node> neighbours = new List<Node>();
            if (x > 0 && nodes[x - 1, y] != null) 
                neighbours.Add(nodes[x - 1, y]);
            if (y > 0 && nodes[x, y - 1] != null) 
                neighbours.Add(nodes[x, y - 1]);
            if (x > 0 && y > 0 && nodes[x - 1, y - 1] != null) 
                neighbours.Add(nodes[x - 1, y - 1]);
            if (x < dungeon.size.Width - 1 && nodes[x + 1, y] != null) 
                neighbours.Add(nodes[x + 1, y]);
            if (y < dungeon.size.Height - 1 && nodes[x, y + 1] != null) 
                neighbours.Add(nodes[x, y + 1]);
            if (x < dungeon.size.Width - 1 && y < dungeon.size.Height - 1 && nodes[x + 1, y + 1] != null) 
                neighbours.Add(nodes[x + 1, y + 1]);
            if (x < dungeon.size.Width - 1 && y > 0 && nodes[x + 1, y - 1] != null) 
                neighbours.Add(nodes[x + 1, y - 1]);
            if (x > 0 && y < dungeon.size.Height - 1 && nodes[x - 1, y + 1] != null) 
                neighbours.Add(nodes[x - 1, y + 1]);
            return neighbours;
        }

        private List<Node> ConvertToList(Node[,] nodes) {
            // add nodes from 2d array to node list
            List<Node> nodeList = new List<Node>();
            for (int x = 0; x < dungeon.size.Width; x++)
            for (int y = 0; y < dungeon.size.Height; y++)
                if (nodes[x, y] != null)
                    nodeList.Add(nodes[x, y]);
            return nodeList;
        }

        /// <summary>
        /// A helper method for your convenience so you don't have to meddle with coordinate transformations.
        /// </summary>
        /// <param name="room">room definition</param>
        /// <returns>the location of the center of the given room you can use for your nodes in this class</returns>
        private Point GetRoomCenter(RoomDefinition room) {
            float centerX = ((room.Area.Left + room.Area.Right + 1) / 2.0f) * dungeon.scale;
            float centerY = ((room.Area.Top + room.Area.Bottom + 1) / 2.0f) * dungeon.scale;
            return new Point((int) centerX, (int) centerY);
        }

        /// <summary>
        /// A helper method for your convenience so you don't have to meddle with coordinate transformations. 
        /// </summary>
        /// <param name="door">door definition</param>
        /// <returns>the location of the center of the given door you can use for your nodes in this class</returns>
        private Point GetDoorCenter(DoorDefinition door) {
            return GetPointCenter(door.Position);
        }

        /// <summary>
        /// A helper method for your convenience so you don't have to meddle with coordinate transformations.
        /// </summary>
        /// <param name="location">point</param>
        /// <returns>the location of the center of the given point you can use for your nodes in this class</returns>
        private Point GetPointCenter(Point location) {
            float centerX = (location.X + 0.5f) * dungeon.scale;
            float centerY = (location.Y + 0.5f) * dungeon.scale;
            return new Point((int) centerX, (int) centerY);
        }
    }
}