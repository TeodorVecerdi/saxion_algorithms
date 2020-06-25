using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using application.utils;

namespace application {
    public class HighLevelNodeGraph : NodeGraph {
        private NiceDungeon dungeon;

        public HighLevelNodeGraph(NiceDungeon dungeon) : base((int) (dungeon.size.Width * dungeon.scale), (int) (dungeon.size.Height * dungeon.scale), (int) dungeon.scale / 3) {
            // Debug.Assert(dungeon != null, "Please pass in a dungeon.");
            this.dungeon = dungeon;
        }

        protected override void generate() {
            (List<NodeData> nodes, List<ConnectionData> connections) = ConvertDungeonDataToGraphData();
            CreateGraph(nodes, connections);
        }

        private (List<NodeData> nodes, List<ConnectionData> connections) ConvertDungeonDataToGraphData() {
            (Dictionary<int, RoomDefinition> rooms, Dictionary<int, DoorDefinition> doors, Dictionary<int, HallwayDefinition> hallways) = dungeon.DungeonData;

            Dictionary<int, NodeData> nodeDataDictionary = new Dictionary<int, NodeData>();
            Dictionary<int, int> roomIdToNodeId = new Dictionary<int, int>();
            List<ConnectionData> connections = new List<ConnectionData>();

            // loop through the rooms and create a node for each room
            foreach (RoomDefinition room in rooms.Values) {
                NodeData nodeData = new NodeData(GetRoomCenter(room));
                nodeDataDictionary.Add(nodeData.ID, nodeData);
                roomIdToNodeId.Add(room.ID, nodeData.ID);
            }

            // loop through the doors and create the necessary connections and nodes
            foreach (DoorDefinition door in doors.Values) {
                NodeData doorNodeData = new NodeData(GetDoorCenter(door));
                nodeDataDictionary.Add(doorNodeData.ID, doorNodeData);
                connections.Add(new ConnectionData(doorNodeData.ID, roomIdToNodeId[door.RoomAID]));
                connections.Add(new ConnectionData(doorNodeData.ID, roomIdToNodeId[door.RoomBID]));
            }

            // loop through the hallways and create the necessary connections and nodes
            foreach (HallwayDefinition hallway in hallways.Values) {
                NodeData hallwayStartNodeData = new NodeData(GetPointCenter(hallway.Start));
                NodeData hallwayEndNodeData = new NodeData(GetPointCenter(hallway.End));
                nodeDataDictionary.Add(hallwayStartNodeData.ID, hallwayStartNodeData);
                nodeDataDictionary.Add(hallwayEndNodeData.ID, hallwayEndNodeData);

                connections.Add(new ConnectionData(hallwayStartNodeData.ID, roomIdToNodeId[hallway.RoomAID]));
                connections.Add(new ConnectionData(hallwayStartNodeData.ID, hallwayEndNodeData.ID));
                connections.Add(new ConnectionData(hallwayEndNodeData.ID, roomIdToNodeId[hallway.RoomBID]));
            }

            return (nodeDataDictionary.Values.ToList(), connections.ToList());
        }

        private void CreateGraph(List<NodeData> nodes, List<ConnectionData> connections) {
            Dictionary<int, Node> nodeIdToNode = new Dictionary<int, Node>();
            nodes.ForEach(node => {
                Node n = new Node(node.Position);
                nodeIdToNode.Add(node.ID, n);
                this.nodes.Add(n);
            });
            connections.ForEach(connection => AddConnection(nodeIdToNode[connection.NodeAID], nodeIdToNode[connection.NodeBID]));
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