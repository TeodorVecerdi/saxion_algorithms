namespace application {
    public readonly struct ConnectionData {
        public readonly int NodeAID;
        public readonly int NodeBID;

        public ConnectionData(int nodeAID, int nodeBID) {
            NodeAID = nodeAID;
            NodeBID = nodeBID;
        }

        public ConnectionData(NodeData nodeA, NodeData nodeB) {
            NodeAID = nodeA.ID;
            NodeBID = nodeB.ID;
        }
    }
}