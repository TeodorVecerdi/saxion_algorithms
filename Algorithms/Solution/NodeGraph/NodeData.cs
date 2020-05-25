using System.Collections.Generic;
using System.Drawing;

namespace application {
    public readonly struct NodeData {
        public readonly int ID;
        public readonly Point Position;

        private static int ids = 0;

        public NodeData(Point position) {
            Position = position;
            ID = ids++;
        }
    }
}