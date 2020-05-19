using GXPEngine;
using System.Diagnostics;

/**
 * NodeGraphAgent provides a starting point for your own agents that would like to navigate the nodegraph.
 * It provides convenience methods such as moveTowardsNode & jumpToNode.
 * 
 * Create a subclass of this class, override Update and call these methods as required for your specific assignment.
 * See SampleNodeGraphAgent for an example.
 */
abstract class NodeGraphAgent : AnimationSprite
{
	protected const int REGULAR_SPEED = 1;
	protected const int FAST_TRAVEL_SPEED = 10;
	protected const int SPEED_UP_KEY = Key.LEFT_CTRL;

	public NodeGraphAgent(NodeGraph pNodeGraph) : base("assets/orc.png", 4, 2, 7)
	{
		Debug.Assert(pNodeGraph != null, "Please pass in a node graph.");

		SetOrigin(width / 2, height / 2);
		System.Console.WriteLine(this.GetType().Name + " created.");
	}

	//override in subclass to implement any functionality
	protected abstract void Update();

	/////////////////////////////////////////////////////////////////////////////////////////
	///	Movement helper methods

	/**
	 * Moves towards the given node with either REGULAR_SPEED or FAST_TRAVEL_SPEED 
	 * based on whether the RIGHT_CTRL key is pressed.
	 */
	protected virtual bool moveTowardsNode(Node pTarget)
	{
		float speed = Input.GetKey(SPEED_UP_KEY) ? FAST_TRAVEL_SPEED : REGULAR_SPEED;
		//increase our current frame based on time passed and current speed
		SetFrame((int)(speed * (Time.time / 100)) % frameCount);

		//standard vector math as you had during the Physics course
		Vec2 targetPosition = new Vec2(pTarget.location.X, pTarget.location.Y);
		Vec2 currentPosition = new Vec2(x, y);
		Vec2 delta = targetPosition.Sub(currentPosition);

		if (delta.Length() < speed)
		{
			jumpToNode(pTarget);
			return true;
		}
		else
		{
			Vec2 velocity = delta.Normalize().Scale(speed);
			x += velocity.x;
			y += velocity.y;

			scaleX = (velocity.x >= 0) ? 1 : -1;

			return false;
		}
	}

	/**
	 * Jumps towards the given node immediately
	 */
	protected virtual void jumpToNode(Node pNode)
	{
		x = pNode.location.X;
		y = pNode.location.Y;
	}

}

