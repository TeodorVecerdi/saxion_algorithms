using System;
using System.Collections.Generic;
using application.utils;

namespace application {
    internal class PathFindingAgent : NodeGraphAgent {
        private PathFinder pathfinder;
        private Queue<Node> path;
        private Node currentNode;
        private Node targetNode;
        private bool reachedCurrentTarget;
        private bool isMoving;

        public PathFindingAgent(NodeGraph nodeGraph, PathFinder pathfinder) : base(nodeGraph) {
            this.pathfinder = pathfinder;
            
            if (nodeGraph.nodes.Count > 0) {
                var node = nodeGraph.nodes[Rand.Range(0, nodeGraph.nodes.Count)];
                base.jumpToNode(node);
                currentNode = node;
                reachedCurrentTarget = true;
            }
            nodeGraph.OnNodeLeftClicked += OnNodeClickHandler;
        }
        
        private void OnNodeClickHandler(Node clickedNode) {
            List<Node> pathList = pathfinder.Generate(currentNode, clickedNode);
            // Exit if there is no path or we're trying to pathfind to the current node
            if (pathList == null || pathList.Count <= 1) 
                return;
            Queue<Node> pathQueue = new Queue<Node>();
            
            // i starts from 2 because we ignore the starting node (because we're already there)
            // and because we set the first node in the path as the current target
            for(var i = 2; i < pathList.Count; i++)
                pathQueue.Enqueue(pathList[i]);
            path = pathQueue;
            reachedCurrentTarget = false;
            targetNode = pathList[1]; // first node in path (Excluding the current node)
            isMoving = true;
        }
        
        protected override void Update() {
            if (!isMoving) 
                return;
            
            if (reachedCurrentTarget) {
                reachedCurrentTarget = false;
                currentNode = targetNode;
                // stop if we reached the destination
                if (path.Count == 0) {
                    isMoving = false;
                    return;
                }
                
                // otherwise go to a new node 
                targetNode = path.Dequeue();
            }

            if (!reachedCurrentTarget && moveTowardsNode(targetNode)) {
                reachedCurrentTarget = true;
            }
        }
    }
}