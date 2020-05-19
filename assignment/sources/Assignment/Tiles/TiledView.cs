using GXPEngine;
using GXPEngine.Core;
using System.Diagnostics;

/**
 * A TileView class that allows you to set a 2D array of tiles and render them on screen.
 * If you are used to an MVC setup, this class is both data and view all rolled into one.
 * Also see the 'A note on architecture' document on BlackBoard.
 * 
 * Subclass this class and override the generate method (note the lower case).
 */
abstract class TiledView : GameObject
{
	//the dimensions of the tileview
	public int columns { get; private set; }
	public int rows { get; private set; }
	//stores the tiletype for each cell (and with it whether a tile is walkable)
	private TileType[,] _tileData;
	//used to reset all data to the default tiletype when requested
	private TileType _defaultTileType;
	//single sprite, used for rendering all tiles
	private AnimationSprite _tileSet;

	public TiledView(int pColumns, int pRows, int pTileSize, TileType pDefaultTileType) {
		Debug.Assert(pColumns > 0, "Invalid amount of columns passed in: " + pColumns);
		Debug.Assert(pRows > 0, "Invalid amount of rows passed in: " + pRows);
		Debug.Assert(pDefaultTileType != null, "Invalid default tile type passed in:" + pDefaultTileType);

		columns = pColumns;
		rows = pRows;

		_defaultTileType = pDefaultTileType;

		//we use a single sprite to render the whole tileview
		_tileSet = new AnimationSprite("assets/tileset.png", 3, 1);
		_tileSet.width = _tileSet.height = pTileSize;

		initializeTiles();
	}

	private void initializeTiles ()
	{
		//initialize all tiles to walkable
		_tileData = new TileType[columns, rows];
		resetAllTilesToDefault();
	}

	protected void resetAllTilesToDefault()
	{
		//a 'trick' to do everything in one for loop instead of a nested loop
		for (int i = 0; i < columns * rows; i++)
		{
			_tileData[i % columns, i / columns] = _defaultTileType;
		}
	}

	public void SetTileType(int pColumn, int pRow, TileType pTileType)
	{
		//an example of hardcore defensive coding;)
		Debug.Assert(pColumn >= 0 && pColumn < columns, "Invalid column passed in: " + pColumn);
		Debug.Assert(pRow >= 0 && pRow < rows, "Invalid row passed in:" + pRow);
		Debug.Assert(pTileType != null, "Invalid tile type passed in:" + pTileType);

		_tileData[pColumn, pRow] = pTileType;
	}

	public TileType GetTileType(int pColumn, int pRow)
	{
		Debug.Assert(pColumn >= 0 && pColumn < columns, "Invalid column passed in: " + pColumn);
		Debug.Assert(pRow >= 0 && pRow < rows, "Invalid row passed in:" + pRow);

		return _tileData[pColumn, pRow];
	}

	protected override void RenderSelf(GLContext glContext)
	{
		//another way of rendering you might not be used to. Instead of adding all 
		//seperate sprites, we override the RenderSelf method, move a sprite around
		//like a stamp and 'stamp' the sprite onto the screen by calling its render method
		for (int column = 0; column < columns; column++)
		{
			for (int row = 0; row < rows; row++)
			{
				_tileSet.currentFrame = GetTileType(column, row).id;
				_tileSet.x = column * _tileSet.width;
				_tileSet.y = row * _tileSet.height;
				_tileSet.Render(glContext);
			}
		}
	}

	/**
	 * Trigger the tile view generation process, do not override this method, 
	 * but override generate (note the lower case) instead.
	 */
	public void Generate()
	{
		System.Console.WriteLine(this.GetType().Name + ".Generate: Generating tile view...");
		generate();
		System.Console.WriteLine(this.GetType().Name + ".Generate: tile view generated.");
	}

	protected abstract void generate();

}

