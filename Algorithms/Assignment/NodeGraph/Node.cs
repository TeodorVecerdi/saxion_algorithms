using System.Collections.Generic;
using System.Drawing;

/**
 * This class represents a single node in a nodegraph.
 * Links between nodes are implemented in the Node itself (through a list of connections).
 * This means that if node A and B are connected, A will have B in its connections list and vice versa.
 * This is also called a bi-directional connection.
 * 
 * Some items are specific to this example, such as position since this node represents a node in a 
 * navigation graph. A node in a boardgame for example might represent completely different data, 
 * such as the current state of the board.
 */
class Node
{
	public readonly List<Node> connections = new List<Node>();

	//node data
	public readonly Point location;
	//what other data could/should you add here to implement the required pathfinding algorithms?

	//Every node has a id that we can display on screen for debugging
	public readonly string id;
	private static int lastID = 0;

	/**
	 * Create a node.
	 * @param pLocation the position of this node
	 * @param pLabel a label for the node, if null a unique id is assigned.
	 */
	public Node(Point pLocation)
	{
		location = pLocation;

		//use an autoincrementing id as label
		id = ""+lastID++;
		System.Console.WriteLine(id);
	}

	public override string ToString()
	{
		return id;
	}
}

