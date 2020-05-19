using System.Drawing;

/**
 * A simple extension class to make sure we can easily scale rectangles and sizes.
 */
static class Extensions
{
	public static Rectangle Scale(this Rectangle pRectangle, float pScale)
	{
		pRectangle.X = (int)(pRectangle.X * pScale);
		pRectangle.Y = (int)(pRectangle.Y * pScale);
		pRectangle.Width = (int)(pRectangle.Width * pScale);
		pRectangle.Height = (int)(pRectangle.Height * pScale);
		return pRectangle;
	}

	public static Size Scale(this Size pSize, float pScale)
	{
		pSize.Width = (int)(pSize.Width * pScale);
		pSize.Height = (int)(pSize.Height * pScale);
		return pSize;
	}

}

