using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

/**
 * Very basic implementation of a NodeGraph class that:
 * - contains Nodes
 * - can detect node clicks
 * - can draw itself
 * - add connections between nodes though a helper method
 * 
 * See SampleDungeonNodeGraph for more info on the todos.
 */
public abstract class NodeGraph : Canvas {
    //references to all the nodes in our nodegraph
    public readonly List<Node> nodes = new List<Node>();

    //event handlers, register for any of these events if interested
    //see SampleNodeGraphAgent for an example of a LeftClick event handler.
    //see PathFinder for an example of a Shift-Left/Right Click event handler.
    public Action<Node> OnNodeLeftClicked = delegate { };
    public Action<Node> OnNodeRightClicked = delegate { };
    public Action<Node> OnNodeShiftLeftClicked = delegate { };
    public Action<Node> OnNodeShiftRightClicked = delegate { };

    public bool FullyConnectedStateChanged;
    public bool IsFullyConnected { get; private set; } = true;

    //required for node highlighting on mouse over
    private Node _nodeUnderMouse = null;

    //some drawing settings
    public int nodeSize { get; private set; }
    private Pen _connectionPen = new Pen(Color.Black, 2);
    private Pen _outlinePen = new Pen(Color.Black, 2.1f);
    private Brush _defaultNodeColor = Brushes.CornflowerBlue;
    private Brush _highlightedNodeColor = Brushes.Cyan;
    private Brush _defaultDisabledNodeColor = Brushes.Maroon;
    private Brush _highlightedDisabledNodeColor = Brushes.Red;

    /** 
	 * Construct a nodegraph with the given screen dimensions, eg 800x600
	 */
    public NodeGraph(int pWidth, int pHeight, int pNodeSize) : base(pWidth, pHeight) {
        nodeSize = pNodeSize;

        application.utils.Debug.Log("\n-----------------------------------------------------------------------------");
        application.utils.Debug.Log(this.GetType().Name + " created.");
        application.utils.Debug.Log("* (Shift) LeftClick/RightClick on nodes to trigger the corresponding events.");
        application.utils.Debug.Log("-----------------------------------------------------------------------------");

        
        // Disable a node
        OnNodeShiftLeftClicked += node => {
            node.isEnabled = false;
            bool isConnected = CheckGraphFullyConnected();
            if (isConnected != IsFullyConnected)
                FullyConnectedStateChanged = true;

            IsFullyConnected = isConnected;
        };
        
        // Enable a node
        OnNodeShiftRightClicked += node => {
            node.isEnabled = true;
            bool isConnected = CheckGraphFullyConnected();
            if (isConnected != IsFullyConnected)
                FullyConnectedStateChanged = true;

            IsFullyConnected = isConnected;
        };
    }

    /**
	 * Convenience method for adding a connection between two nodes in the nodegraph
	 */
    public void AddConnection(Node pNodeA, Node pNodeB) {
        if (nodes.Contains(pNodeA) && nodes.Contains(pNodeB)) {
            if (!pNodeA.connections.Contains(pNodeB)) pNodeA.connections.Add(pNodeB);
            if (!pNodeB.connections.Contains(pNodeA)) pNodeB.connections.Add(pNodeA);
        }
    }

    /**
	 * Trigger the node graph generation process, do not override this method, 
	 * but override generate (note the lower case) instead, calling AddConnection as required.
	 */
    public void Generate() {
        application.utils.Debug.Log(this.GetType().Name + ".Generate: Generating graph...");

        //always remove all nodes before generating the graph, as it might have been generated previously
        nodes.Clear();
        generate();
        draw();

        application.utils.Debug.Log(this.GetType().Name + ".Generate: Graph generated.");
    }

    protected abstract void generate();

    /////////////////////////////////////////////////////////////////////////////////////////
    /// NodeGraph visualization helper methods
    ///
    protected virtual void draw() {
        graphics.Clear(Color.Transparent);
        drawAllConnections();
        drawNodes();
    }

    protected virtual void drawNodes() {
        foreach (Node node in nodes) drawNode(node, node.isEnabled?_defaultNodeColor:_defaultDisabledNodeColor);
    }

    protected virtual void drawNode(Node pNode, Brush pColor) {
        //colored node fill
        graphics.FillEllipse(
            pColor,
            pNode.location.X - nodeSize,
            pNode.location.Y - nodeSize,
            2 * nodeSize,
            2 * nodeSize
        );

        //black node outline
        graphics.DrawEllipse(
            _outlinePen,
            pNode.location.X - nodeSize - 1,
            pNode.location.Y - nodeSize - 1,
            2 * nodeSize + 1,
            2 * nodeSize + 1
        );
    }

    protected virtual void drawAllConnections() {
        //note that this means all connections are drawn twice, once from A->B and once from B->A
        //but since is only a debug view we don't care
        foreach (Node node in nodes) drawNodeConnections(node);
    }

    protected virtual void drawNodeConnections(Node pNode) {
        foreach (Node connection in pNode.connections) {
            drawConnection(pNode, connection);
        }
    }

    protected virtual void drawConnection(Node pStartNode, Node pEndNode) {
        graphics.DrawLine(_connectionPen, pStartNode.location, pEndNode.location);
    }

    /////////////////////////////////////////////////////////////////////////////////////////
    ///							Update loop
    ///							

    //this has to be virtual or public otherwise the subclass won't pick it up
    protected virtual void Update() {
        handleMouseInteraction();
    }

    /////////////////////////////////////////////////////////////////////////////////////////
    ///							Node click handling
    ///							
    protected virtual void handleMouseInteraction() {
        //then check if one of the nodes is under the mouse and if so assign it to _nodeUnderMouse
        Node newNodeUnderMouse = null;
        foreach (Node node in nodes) {
            if (IsMouseOverNode(node)) {
                newNodeUnderMouse = node;

                break;
            }
        }

        //do mouse node hightlighting
        if (newNodeUnderMouse != _nodeUnderMouse) {
            if (_nodeUnderMouse != null) drawNode(_nodeUnderMouse, _nodeUnderMouse.isEnabled?_defaultNodeColor:_defaultDisabledNodeColor);
            _nodeUnderMouse = newNodeUnderMouse;
            if (_nodeUnderMouse != null) drawNode(_nodeUnderMouse, _nodeUnderMouse.isEnabled?_highlightedNodeColor:_highlightedDisabledNodeColor);
        }

        //if we are still not hovering over a node, we are done
        if (_nodeUnderMouse == null) return;

        //If _nodeUnderMouse is not null, check if we released the mouse on it.
        //This is architecturally not the best way, but for this assignment 
        //it saves a lot of hassles and the trouble of building a complete event system

        if (Input.GetKey(Key.LEFT_SHIFT) || Input.GetKey(Key.RIGHT_SHIFT)) {
            if (Input.GetMouseButtonUp(0)) OnNodeShiftLeftClicked?.Invoke(_nodeUnderMouse);
            if (Input.GetMouseButtonUp(1)) OnNodeShiftRightClicked?.Invoke(_nodeUnderMouse);
        } else {
            if (Input.GetMouseButtonUp(0)) OnNodeLeftClicked?.Invoke(_nodeUnderMouse);
            if (Input.GetMouseButtonUp(1)) OnNodeRightClicked?.Invoke(_nodeUnderMouse);
        }
    }

    /**
	 * Checks whether the mouse is over a Node.
	 * This assumes local and global space are the same.
	 */
    public bool IsMouseOverNode(Node pNode) {
        //ah life would be so much easier if we'd all just use Vec2's ;)
        float dx = pNode.location.X - Input.mouseX;
        float dy = pNode.location.Y - Input.mouseY;
        var mouseToNodeDistanceSqr = dx * dx + dy * dy;
        return mouseToNodeDistanceSqr < nodeSize * nodeSize;
    }

    /// <summary>
    /// Checks if the graph is fully connected
    /// </summary>
    /// <returns><code>true</code> if the graph is fully connected, <code>false</code> otherwise</returns>
    public bool CheckGraphFullyConnected() {
        HashSet<Node> allActiveNodes = new HashSet<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        Node firstActiveNode = null;
        foreach (var node in nodes) {
            if (!node.isEnabled)
                continue;
            
            allActiveNodes.Add(node);
            if (firstActiveNode == null) 
                firstActiveNode = node;
        }
        DFS(firstActiveNode, visited);
        return visited.Count == allActiveNodes.Count;
    }

    private void DFS(Node node, HashSet<Node> visited) {
        visited.Add(node);
        foreach (Node adjacent in node.connections) {
            if(visited.Contains(adjacent) || !adjacent.isEnabled)
                continue;
            DFS(adjacent, visited);
        }
    }
}