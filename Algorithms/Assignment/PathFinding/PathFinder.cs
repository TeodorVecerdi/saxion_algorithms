using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

/**
 * This class is the base class for your pathfinder, you 'only' have to override generate so that it returns
 * the requested path and then it will handle the visualization part for you. This class can be used in two ways:
 * 1. By setting the start and end node by left/right shift-clicking and then pressing G (for Generate)
 * 2. By calling Generate directly with the given start and end node
 * 
 * TODO:
 * - create a subclass for this class and override the generate method (See SamplePathFinder for an example)
 */
abstract class PathFinder : Canvas
{
	protected Node _startNode;							
	protected Node _endNode;
	protected List<Node> _lastCalculatedPath = null;

	protected NodeGraph _nodeGraph;

	//some values for drawing the path
	private Pen _outlinePen = new Pen(Color.Black, 4);
	private Pen _connectionPen1 = new Pen(Color.Black, 10);
	private Pen _connectionPen2 = new Pen(Color.Yellow, 3);

	private Brush _startNodeColor = Brushes.Green;
	private Brush _endNodeColor = Brushes.Red;
	private Brush _pathNodeColor = Brushes.Yellow;

	public PathFinder (NodeGraph pGraph) : base (pGraph.width, pGraph.height)
	{
		_nodeGraph = pGraph;
		_nodeGraph.OnNodeShiftLeftClicked += (node) => { _startNode = node; draw(); };
		_nodeGraph.OnNodeShiftRightClicked += (node) => { _endNode = node; draw(); };

		Console.WriteLine("\n-----------------------------------------------------------------------------");
		Console.WriteLine(this.GetType().Name + " created.");
		Console.WriteLine("* Shift-LeftClick to set the starting node.");
		Console.WriteLine("* Shift-RightClick to set the target node.");
		Console.WriteLine("* G to generate the Path.");
		Console.WriteLine("* C to clear the Path.");
		Console.WriteLine("-----------------------------------------------------------------------------");
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	/// Core PathFinding methods

	public List<Node> Generate(Node pFrom, Node pTo)
	{
		System.Console.WriteLine(this.GetType().Name + ".Generate: Generating path...");

		_lastCalculatedPath = null;
		_startNode = pFrom;
		_endNode = pTo;

		if (_startNode == null || _endNode == null)
		{
			Console.WriteLine("Please specify start and end node before trying to generate a path.");
		}
		else
		{
			_lastCalculatedPath = generate(pFrom, pTo);
		}

		draw();

		System.Console.WriteLine(this.GetType().Name + ".Generate: Path generated.");
		return _lastCalculatedPath;
	}

	/**
	 * @return the last found path. 
	 *	-> 'null'		means	'Not completed.'
	 *	-> Count == 0	means	'Completed but empty (no path found).'
	 *	-> Count > 0	means	'Yolo let's go!'
	 */
	protected abstract List<Node> generate(Node pFrom, Node pTo);

	/////////////////////////////////////////////////////////////////////////////////////////
	/// PathFinder visualization helpers method
	///	As you can see this looks a lot like the code in NodeGraph, but that is just coincidence
	///	By not reusing any of that code you are free to tweak the visualization anyway you want

	protected virtual void draw()
	{
		//to keep things simple we redraw all debug info every frame
		graphics.Clear(Color.Transparent);

		//draw path if we have one
		if (_lastCalculatedPath != null) drawPath();

		//draw start and end if we have one
		if (_startNode != null) drawNode(_startNode, _startNodeColor);
		if (_endNode != null) drawNode(_endNode, _endNodeColor);

		//TODO: you could override this method and draw your own additional stuff for debugging
	}

	protected virtual void drawPath()
	{
		//draw all lines
		for (int i = 0; i < _lastCalculatedPath.Count - 1; i++)
		{
			drawConnection(_lastCalculatedPath[i], _lastCalculatedPath[i + 1]);
		}

		//draw all nodes between start and end
		for (int i = 1; i < _lastCalculatedPath.Count - 1; i++)
		{
			drawNode(_lastCalculatedPath[i], _pathNodeColor);
		}
	}

	protected virtual void drawNodes (IEnumerable<Node> pNodes, Brush pColor)
	{
		foreach (Node node in pNodes) drawNode(node, pColor);
	}

	protected virtual void drawNode(Node pNode, Brush pColor)
	{
		int nodeSize = _nodeGraph.nodeSize+2;

		//colored fill
		graphics.FillEllipse(
			pColor,
			pNode.location.X - nodeSize,
			pNode.location.Y - nodeSize,
			2 * nodeSize,
			2 * nodeSize
		);

		//black outline
		graphics.DrawEllipse(
			_outlinePen,
			pNode.location.X - nodeSize - 1,
			pNode.location.Y - nodeSize - 1,
			2 * nodeSize + 1,
			2 * nodeSize + 1
		);
	}

	protected virtual void drawConnection(Node pStartNode, Node pEndNode)
	{
		//draw a thick black line with yellow core
		graphics.DrawLine(_connectionPen1,	pStartNode.location,pEndNode.location);
		graphics.DrawLine(_connectionPen2,	pStartNode.location,pEndNode.location);
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	///							Keypress handling
	///							

	public void Update()
	{
		handleInput();
	}

	protected virtual void handleInput()
	{
		if (Input.GetKeyDown(Key.C))
		{
			//clear everything
			graphics.Clear(Color.Transparent);
			_startNode = _endNode = null;
			_lastCalculatedPath = null;
		}

		if (Input.GetKeyDown(Key.G))
		{
			if (_startNode != null && _endNode != null)
			{
				Generate(_startNode, _endNode);
			}
		}
	}


}
