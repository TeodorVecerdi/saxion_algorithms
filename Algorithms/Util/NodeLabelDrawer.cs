using GXPEngine;
using System;
using System.Drawing;

/**
 * Helper class that draws nodelabels for a nodegraph.
 */
class NodeLabelDrawer : Canvas
{
	private Font _labelFont;
	private bool _showLabels = false;
	private NodeGraph _graph = null;

	public NodeLabelDrawer(NodeGraph pNodeGraph) : base(pNodeGraph.width, pNodeGraph.height)
	{
		application.utils.Debug.Log("\n-----------------------------------------------------------------------------");
		application.utils.Debug.Log("NodeLabelDrawer created.");
		application.utils.Debug.Log("* L key to toggle node label display.");
		application.utils.Debug.Log("-----------------------------------------------------------------------------");

		_labelFont = new Font(SystemFonts.DefaultFont.FontFamily, pNodeGraph.nodeSize, FontStyle.Bold);
		_graph = pNodeGraph;
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	///							Update loop
	///							

	//this has to be virtual otherwise the subclass won't pick it up
	protected virtual void Update()
	{
		//toggle label display when L is pressed
		if (Input.GetKeyDown(Key.L))
		{
			_showLabels = !_showLabels;
			graphics.Clear(Color.Transparent);
			if (_showLabels) drawLabels();
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	/// NodeGraph visualization helper methods

	protected virtual void drawLabels()
	{
		foreach (Node node in _graph.nodes) drawNode(node);
	}

	protected virtual void drawNode(Node pNode)
	{
		SizeF size = graphics.MeasureString(pNode.id.ToString(), _labelFont);
		graphics.DrawString(pNode.id.ToString(), _labelFont, Brushes.Black, pNode.location.X - size.Width / 2, pNode.location.Y - size.Height / 2);
	}

}
