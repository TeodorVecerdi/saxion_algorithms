using GXPEngine;
using GXPEngine.OpenGL;
using System.Collections.Generic;
using System.Drawing;

/**
 * The base Dungeon class. 
 * 
 * This class is very limited, it contains:
 *	- a(n empty) list of rooms
 *	- a(n empty) list of doors
 *	- code to visualize all rooms and doors.
 * 
 * TODO:
 * - Read carefully through all the code below, so that you know which helper methods are available to you.
 * - Create a subclass of this class and override the generate method (see the SampleDungeon for an example).
 */ 
abstract class Dungeon : Canvas
{
	//the (unscaled) dimensions of the dungeon (basically how 'tiles' wide and high)
	public readonly Size size;

	//base implementation assumes dungeon consists of rooms and doors, adapt in subclass if needed
	public readonly List<Room> rooms = new List<Room>();
	public readonly List<Door> doors = new List<Door>();

	//Set this to false if you want to do all drawing yourself from the generate method.
	//This might be handy while debugging your own algorithm.
	protected bool autoDrawAfterGenerate = true;

	//The colors for the walls and doors
	//TODO:try changing 255 to 128 to see where the room boundaries are...
	private Pen wallPen = new Pen(Color.FromArgb(255, Color.Black));
	private Pen doorPen = Pens.White;

	/**
	 * Create empty dungeon instance of the specified size.
	 * It's empty because it doesn't contain any rooms yet.
	 */
	public Dungeon(Size pSize) : base(pSize.Width, pSize.Height)
	{
		size = pSize;

		/**/
		//ignore lines below, this is for rendering scaled canvasses without blurring 
		//Comment it out if you feel lucky or just want to see what this is doing.
		_texture.Bind();
		GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
		GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
		_texture.Unbind();
		/**/

		System.Console.WriteLine(this.GetType().Name + " created.");
	}

	/**
	 * Clears all rooms and doors, calls generate (note the lower case),
	 * and visualizes the result by drawing on the canvas.
	 * 
	 * @param pMinimumRoomSize the minimum size that a room should have
	 */
	public void Generate(int pMinimumRoomSize)
	{
		System.Console.WriteLine(this.GetType().Name + ".Generate:Generating dungeon...");

		rooms.Clear();
		doors.Clear();

		generate(pMinimumRoomSize);

		System.Console.WriteLine(this.GetType().Name + ".Generate:Dungeon generated.");

		if (autoDrawAfterGenerate) draw();
	}

	//TODO: Override this method in your subclass to generate a dungeon as described in assignment 1
	protected abstract void generate(int pMinimumRoomSize);

	/////////////////////////////////////////////////////////////////////////////////////////
	///	This section contains helper methods to draw all or specific doors/rooms
	///	You can call them from your own methods to actually draw the dungeon during/after generation
	///	These methods do not have to be changed.

	protected virtual void draw()
	{
		graphics.Clear(Color.Transparent);
		drawRooms(rooms, wallPen);    
		drawDoors(doors, doorPen);
	}

	/**
	 * Draw all rooms in the given list with the given color, eg drawRooms (_rooms, Pen.Black)
	 * @param pRooms		the list of rooms to draw
	 * @param pWallColor	the color of the walls
	 * @param pFillColor	if not null, the color of the inside of the room, if null insides will be transparent
	 */
	protected virtual void drawRooms(IEnumerable<Room> pRooms, Pen pWallColor, Brush pFillColor = null)
	{
		foreach (Room room in pRooms)
		{
			drawRoom(room, pWallColor, pFillColor);
		}
	}

	/**
	 * Draws a single room in the given color.
	 * @param pRoom			the room to draw
	 * @param pWallColor	the color of the walls
	 * @param pFillColor	if not null, the color of the inside of the room, if null insides will be transparent
	 */
	protected virtual void drawRoom (Room pRoom, Pen pWallColor, Brush pFillColor = null)
	{
		//the -0.5 has two reasons:
		//- Doing it this way actually makes sure that an area of 0,0,4,4 (x,y,width,height) is draw as an area of 0,0,4,4
		//- Doing it this way makes sure that an area of 0,0,1,1 is ALSO drawn (which it wouldn't if you used -1 instead 0.5f)
		if (pFillColor != null) graphics.FillRectangle(pFillColor, pRoom.area.Left, pRoom.area.Top, pRoom.area.Width - 0.5f, pRoom.area.Height - 0.5f);
		graphics.DrawRectangle(pWallColor, pRoom.area.Left, pRoom.area.Top, pRoom.area.Width - 0.5f, pRoom.area.Height - 0.5f);
	}

	protected virtual void drawDoors(IEnumerable<Door> pDoors, Pen pColor)
	{
		foreach (Door door in pDoors)
		{
			drawDoor(door, pColor);
		}
	}

	protected virtual void drawDoor (Door pDoor, Pen pColor)
	{
		//note the 0.5, 0.5, this forces the drawing api to draw at least 1 pixel ;)
		graphics.DrawRectangle(pColor, pDoor.location.X, pDoor.location.Y, 0.5f, 0.5f);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	///	This section contains helper methods to print information about the dungeon to the console

	//TODO: implement a toString/print method for debugging
	public override string ToString()
	{
		return "Dungeon: implement/override this method to print info about all rooms and doors";
	}
}