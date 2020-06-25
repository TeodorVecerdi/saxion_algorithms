using System.Collections.Generic;
using GXPEngine;

namespace application {
    public class OffGraphWayPointAgent : NodeGraphAgent {
        private Node currentTarget;
        private Queue<Node> queuedNodes;
        private bool reachedTarget;

        public OffGraphWayPointAgent(NodeGraph nodeGraph) : base(nodeGraph) {
            //position ourselves on a random node
            if (nodeGraph.nodes.Count > 0) {
                Node node = nodeGraph.nodes[Rand.Range(0, nodeGraph.nodes.Count)];
                base.jumpToNode(node);
                currentTarget = node;
                reachedTarget = true;
            }

            queuedNodes = new Queue<Node>();
            //listen to nodeclicks
            nodeGraph.OnNodeLeftClicked += OnNodeClickHandler;
        }

        private void OnNodeClickHandler(Node clickedNode) {
            queuedNodes.Enqueue(clickedNode);
        }

        protected override void Update() {
            if (reachedTarget) {
                Node node = TryGetNode();
                if(node == null) return;
                currentTarget = node;
                reachedTarget = false;
            }

            if (!reachedTarget && currentTarget != null && moveTowardsNode(currentTarget)) {
                reachedTarget = true;
            }
        }

        private Node TryGetNode() {
            while (true) {
                if (queuedNodes.Count == 0) 
                    return null;
                Node node = queuedNodes.Dequeue();
                if (currentTarget.connections.Contains(node))
                    return node;
            }
        }
    }
}