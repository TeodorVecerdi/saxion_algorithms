using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Debug = application.utils.Debug;

namespace application {
    internal class DijkstraPathFinder : PathFinder {
        public DijkstraPathFinder(NodeGraph pGraph) : base(pGraph) { }

        protected override List<Node> generate(Node start, Node target) {
            Dictionary<Node, Node> previous = Dijkstra(start, target);
            List<Node> path = GetPath(previous, target);
            return path;
        }

        private Dictionary<Node, Node> Dijkstra(Node start, Node target) {
            int nodeCoverage = 0;
            Dictionary<Node, Node> previous = new Dictionary<Node, Node>();
            Dictionary<Node, float> distance = new Dictionary<Node, float>();
            HashSet<Node> nodes = new HashSet<Node>();

            // initialize the data structures;
            foreach (Node node in _nodeGraph.nodes) {
                if(node.isEnabled) 
                    nodes.Add(node);
                distance.Add(node, float.MaxValue);
            }

            distance[start] = 0f;

            // main loop
            while (nodes.Count > 0) {
                nodeCoverage++;
                Node current = FindNodeWithMinimumDistance(nodes, distance);
                nodes.Remove(current);

                // terminate the search early if we reached the target
                if (current == target) {
                    Debug.LogInfo($"Dijkstra Node Coverage: {nodeCoverage}/{_nodeGraph.nodes.Count}");
                    return previous;
                }

                foreach (Node adjacent in current.connections) {
                    // skip the node if we already visited it
                    if (!nodes.Contains(adjacent))
                        continue;

                    // calculate the distance from the start node to the neighbour node
                    float newDistance = distance[current] + DistanceSqr(current, adjacent);
                    if (newDistance < distance[adjacent]) {
                        distance[adjacent] = newDistance;
                        previous[adjacent] = current;
                    }
                }
            }

            Debug.LogInfo($"Dijkstra Node Coverage: {nodeCoverage}/{_nodeGraph.nodes.Count}");
            return previous;
        }

        private Node FindNodeWithMinimumDistance(HashSet<Node> nodes, Dictionary<Node, float> distance) {
            float minDistance = float.MaxValue;
            Node minDistanceNode = null;

            foreach (Node node in nodes) {
                if (distance[node] >= minDistance)
                    continue;

                minDistance = distance[node];
                minDistanceNode = node;
            }

            return minDistanceNode;
        }

        private float DistanceSqr(Node from, Node to) {
            // we can use the square distance to further optimize the algorithm
            return (to.location.X - from.location.X) * (to.location.X - from.location.X) + (to.location.Y - from.location.Y) * (to.location.Y - from.location.Y);
        }

        private List<Node> GetPath(Dictionary<Node, Node> previous, Node target) {
            if (!previous.ContainsKey(target))
                return null;

            // Walk backwards from the target node to the start node using the
            // parent dictionary and build the path.
            List<Node> path = new List<Node> {target};
            Node parent = previous[target];
            while (true) {
                path.Insert(0, parent);
                if (!previous.ContainsKey(parent)) {
                    break;
                }
                parent = previous[parent];
            }

            return path;
        }
    }
}