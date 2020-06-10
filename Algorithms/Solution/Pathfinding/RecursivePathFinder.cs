using System.Collections.Generic;
using System.Threading;

namespace application {
    internal class RecursivePathFinder : PathFinder {
        public RecursivePathFinder(NodeGraph pGraph) : base(pGraph) { }

        protected override List<Node> generate(Node start, Node target) {
            // Create required data types and initialize algorithm
            Dictionary<Node, Node> parentDictionary = new Dictionary<Node, Node>();
            Queue<Node> queue = new Queue<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            queue.Enqueue(start);

            // Walk the graph and 'build' the parentDictionary
            RecursiveBFS(target, queue, visited, parentDictionary);

            // If the target node doesn't have a parent it means there is no path.
            if (!parentDictionary.ContainsKey(target))
                return null;

            // Walk backwards from the target node to the start node using the
            // parent dictionary and build the path.
            List<Node> path = new List<Node> {target};
            Node parent = parentDictionary[target];
            while (true) {
                path.Insert(0, parent);
                if (!parentDictionary.ContainsKey(parent))
                    break;
                parent = parentDictionary[parent];
            }

            return path;
        }

        private void RecursiveBFS(Node target, Queue<Node> queue, HashSet<Node> visited, Dictionary<Node, Node> parentDictionary) {
            if (queue.Count == 0)
                return;

            Node current = queue.Dequeue();
            visited.Add(current);

            foreach (Node adjacent in current.connections) {
                if (visited.Contains(adjacent))
                    continue;
                parentDictionary.Add(adjacent, current);
                queue.Enqueue(adjacent);
                visited.Add(adjacent);
            }

            if (current == target) {
                queue.Clear();
                return;
            }

            RecursiveBFS(target, queue, visited, parentDictionary);
        }
    }
}