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
abstract class NodeGraph : Canvas
{
	//references to all the nodes in our nodegraph
	public readonly List<Node> nodes = new List<Node>();

	//event handlers, register for any of these events if interested
	//see SampleNodeGraphAgent for an example of a LeftClick event handler.
	//see PathFinder for an example of a Shift-Left/Right Click event handler.
	public Action<Node> OnNodeLeftClicked = delegate { };
	public Action<Node> OnNodeRightClicked = delegate { };
	public Action<Node> OnNodeShiftLeftClicked = delegate { };
	public Action<Node> OnNodeShiftRightClicked = delegate { };

	//required for node highlighting on mouse over
	private Node _nodeUnderMouse = null;

	//some drawing settings
	public int nodeSize { get; private set; }
	private Pen _connectionPen = new Pen(Color.Black, 2);
	private Pen _outlinePen = new Pen(Color.Black, 2.1f);
	private Brush _defaultNodeColor = Brushes.CornflowerBlue;
	private Brush _highlightedNodeColor = Brushes.Cyan;
	
	/** 
	 * Construct a nodegraph with the given screen dimensions, eg 800x600
	 */
	public NodeGraph(int pWidth, int pHeight, int pNodeSize) : base(pWidth, pHeight)
	{
		nodeSize = pNodeSize;

		Console.WriteLine("\n-----------------------------------------------------------------------------");
		Console.WriteLine(this.GetType().Name + " created.");
		Console.WriteLine("* (Shift) LeftClick/RightClick on nodes to trigger the corresponding events.");
		Console.WriteLine("-----------------------------------------------------------------------------");
	}

	/**
	 * Convenience method for adding a connection between two nodes in the nodegraph
	 */
	public void AddConnection(Node pNodeA, Node pNodeB)
	{
		if (nodes.Contains(pNodeA) && nodes.Contains(pNodeB))
		{
			if (!pNodeA.connections.Contains(pNodeB)) pNodeA.connections.Add(pNodeB);
			if (!pNodeB.connections.Contains(pNodeA)) pNodeB.connections.Add(pNodeA);
		}
	}

	/**
	 * Trigger the node graph generation process, do not override this method, 
	 * but override generate (note the lower case) instead, calling AddConnection as required.
	 */
	public void Generate()
	{
		System.Console.WriteLine(this.GetType().Name + ".Generate: Generating graph...");

		//always remove all nodes before generating the graph, as it might have been generated previously
		nodes.Clear();
		generate();
		draw();

		System.Console.WriteLine(this.GetType().Name + ".Generate: Graph generated.");
	}

	protected abstract void generate();

	/////////////////////////////////////////////////////////////////////////////////////////
	/// NodeGraph visualization helper methods
	///

	protected virtual void draw()
	{
		graphics.Clear(Color.Transparent);
		drawAllConnections();
		drawNodes();
	}

	protected virtual void drawNodes()
	{
		foreach (Node node in nodes) drawNode(node, _defaultNodeColor);
	}

	protected virtual void drawNode(Node pNode, Brush pColor)
	{
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

	protected virtual void drawAllConnections()
	{
		//note that this means all connections are drawn twice, once from A->B and once from B->A
		//but since is only a debug view we don't care
		foreach (Node node in nodes) drawNodeConnections(node);
	}

	protected virtual void drawNodeConnections(Node pNode)
	{
		foreach (Node connection in pNode.connections)
		{
			drawConnection(pNode, connection);
		}
	}

	protected virtual void drawConnection(Node pStartNode, Node pEndNode)
	{
		graphics.DrawLine(_connectionPen, pStartNode.location, pEndNode.location);
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	///							Update loop
	///							

	//this has to be virtual or public otherwise the subclass won't pick it up
	protected virtual void Update()
	{
		handleMouseInteraction();
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	///							Node click handling
	///							

	protected virtual void handleMouseInteraction()
	{
		//then check if one of the nodes is under the mouse and if so assign it to _nodeUnderMouse
		Node newNodeUnderMouse = null;
		foreach (Node node in nodes)
		{
			if (IsMouseOverNode(node))
			{
				newNodeUnderMouse = node;

				break;
			}
		}

		//do mouse node hightlighting
		if (newNodeUnderMouse != _nodeUnderMouse)
		{
			if (_nodeUnderMouse != null) drawNode(_nodeUnderMouse, _defaultNodeColor);
			_nodeUnderMouse = newNodeUnderMouse;
			if (_nodeUnderMouse != null) drawNode(_nodeUnderMouse, _highlightedNodeColor);
		}

		//if we are still not hovering over a node, we are done
		if (_nodeUnderMouse == null) return;

		//If _nodeUnderMouse is not null, check if we released the mouse on it.
		//This is architecturally not the best way, but for this assignment 
		//it saves a lot of hassles and the trouble of building a complete event system

		if (Input.GetKey(Key.LEFT_SHIFT) || Input.GetKey(Key.RIGHT_SHIFT))
		{
			if (Input.GetMouseButtonUp(0)) OnNodeShiftLeftClicked(_nodeUnderMouse);
			if (Input.GetMouseButtonUp(1)) OnNodeShiftRightClicked(_nodeUnderMouse);
		}
		else
		{
			if (Input.GetMouseButtonUp(0)) OnNodeLeftClicked(_nodeUnderMouse);
			if (Input.GetMouseButtonUp(1)) OnNodeRightClicked(_nodeUnderMouse);
		}
	}

	/**
	 * Checks whether the mouse is over a Node.
	 * This assumes local and global space are the same.
	 */
	public bool IsMouseOverNode(Node pNode)
	{
		//ah life would be so much easier if we'd all just use Vec2's ;)
		float dx = pNode.location.X - Input.mouseX;
		float dy = pNode.location.Y - Input.mouseY;
		float mouseToNodeDistance = Mathf.Sqrt(dx * dx + dy * dy);

		return mouseToNodeDistance < nodeSize;
	}

}