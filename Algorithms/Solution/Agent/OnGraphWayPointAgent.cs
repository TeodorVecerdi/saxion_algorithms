using System.Collections.Generic;
using GXPEngine;

namespace application {
    public class OnGraphWayPointAgent : NodeGraphAgent {
        private Node finalTarget;
        private Node currentTarget;
        private Node previousTarget;
        private bool reachedCurrentTarget;
        private bool isMoving;

        public OnGraphWayPointAgent(NodeGraph nodeGraph) : base(nodeGraph) {
            //position ourselves on a random node
            if (nodeGraph.nodes.Count > 0) {
                Node node = nodeGraph.nodes[Rand.Range(0, nodeGraph.nodes.Count)];
                base.jumpToNode(node);
                currentTarget = node;
                reachedCurrentTarget = true;
            }

            //listen to nodeclicks
            nodeGraph.OnNodeLeftClicked += OnNodeClickHandler;
        }

        private void OnNodeClickHandler(Node clickedNode) {
            if (isMoving)
                return;
            
            finalTarget = clickedNode;
            isMoving = true;
        }

        protected override void Update() {
            if (!isMoving) 
                return;
            
            if (reachedCurrentTarget) {
                reachedCurrentTarget = false;
                // stop if we reached the destination
                if (currentTarget == finalTarget) {
                    isMoving = false;
                    return;
                }
                
                // otherwise go to a new node 
                Node node = TryGetNode();
                previousTarget = currentTarget;
                currentTarget = node;
            }

            if (!reachedCurrentTarget && moveTowardsNode(currentTarget)) {
                reachedCurrentTarget = true;
            }
        }

        private Node TryGetNode() {
            // if we reached a dead end, go back
            if (currentTarget.connections.Count == 1)
                return currentTarget.connections[0];
            
            // if we can get to the final target from the current node, do so
            if (currentTarget.connections.Contains(finalTarget))
                return finalTarget;
            
            // get a random node from the connections except the one we just came from
            List<Node> validTargets = new List<Node>();
            validTargets.AddRange(currentTarget.connections);
            validTargets.Remove(previousTarget);

            return validTargets[Rand.Range(0, validTargets.Count)];
        }
    }
}