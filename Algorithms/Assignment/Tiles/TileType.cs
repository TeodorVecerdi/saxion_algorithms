/**
 * Describes a certain tiletype and whether it's walkable or not.
 * Note how we maintain controls over the possible TileType's in this class (constructor is private)
 */
class TileType
{
	//all possible tile types based on this class' properties
	public static readonly TileType WALL = new TileType(false);		//wall, not walkable
	public static readonly TileType GROUND = new TileType(true);	//ground, walkable
	public static readonly TileType VOID = new TileType(false);		//IT'S DA VOID RUN! -> just kidding, it's not walkable

	//each tiletype gets assigned a unique auto incrementing id, used for texture look ups
	private static int _lastID = 0;
        
	//tile instance specific properties
	public readonly bool walkable;
	public readonly int id; 
        
	private TileType(bool pWalkable)
	{
		walkable = pWalkable;
		id = _lastID++;
	}   
}

 
