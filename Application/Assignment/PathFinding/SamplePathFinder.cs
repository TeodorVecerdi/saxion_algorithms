using GXPEngine;
using System;
using System.Collections.Generic;

/**
 * An example of a PathFinder implementation which completes the process by rolling a die 
 * and just returning the straight-as-the-crow-flies path if you roll a 6 ;). 
 */
class SamplePathFinder : PathFinder	{

	public SamplePathFinder(NodeGraph pGraph) : base(pGraph) {}

	protected override List<Node> generate(Node pFrom, Node pTo)
	{
		//at this point you know the FROM and TO node and you have to write an 
		//algorithm which finds the path between them
		Console.WriteLine("For now I'll just roll a die for a random path!!");

		int dieRoll = Utils.Random(1, 7);
		Console.WriteLine("I rolled a ..." + dieRoll);

		if (dieRoll == 6)
		{
			Console.WriteLine("Yes 'random' path found!!");
			return new List<Node>() { pFrom , pTo };
		}
		else
		{
			Console.WriteLine("Too bad, no path found !!");
			return null;
		}
	}

}

