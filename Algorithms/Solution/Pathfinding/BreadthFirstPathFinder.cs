using System.Collections.Generic;

namespace application {
    internal class BreadthFirstPathFinder : PathFinder{
        public BreadthFirstPathFinder(NodeGraph pGraph) : base(pGraph) { }
        protected override List<Node> generate(Node start, Node target) {
            // Walk the graph and 'build' the parentDictionary
            Dictionary<Node, Node> parentDictionary = BFS(start);

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

        private Dictionary<Node, Node> BFS(Node start) {
            Dictionary<Node, Node> parentDictionary = new Dictionary<Node, Node>();
            Queue<Node> queue = new Queue<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            
            queue.Enqueue(start);
            visited.Add(start);
            
            while (queue.Count > 0) {
                Node current = queue.Dequeue();
                foreach (Node adjacent in current.connections) {
                    if(visited.Contains(adjacent))
                        continue;
                    parentDictionary.Add(adjacent, current);
                    queue.Enqueue(adjacent);
                    visited.Add(adjacent);
                }
            }
            
            return parentDictionary;
        }
    }
}