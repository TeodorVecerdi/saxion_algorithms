using System;

/**
 * It's back. The basic basic Vec2 class from Physics Programming.
 */
namespace GXPEngine
{
	public struct Vec2 
	{
		public static Vec2 zero { get { return new Vec2(0,0); }}
		public static Vec2 temp = new Vec2 ();

		public float x;
		public float y;

		public Vec2 (float pX = 0, float pY = 0)
		{
			x = pX;
			y = pY;
		}

		public override string ToString ()
		{
			return String.Format ("({0}, {1})", x, y);
		}

		public Vec2 Add (Vec2 other) {
			return new Vec2(x + other.x, y + other.y);
		}

		public Vec2 Sub (Vec2 other) {
			return new Vec2(x - other.x, y - other.y);
		}

		public float Length() {
			return (float)Math.Sqrt (x * x + y * y);
		}

		public Vec2 Normalize () {
			if (x == 0 && y == 0) {
				return new Vec2(0,0);
			} else {
				return Scale (1/Length ());
			}
		}
	
		public Vec2 Scale (float scalar) {
			return new Vec2(x*scalar, y*scalar);
		}

		public float DistanceTo (Vec2 other) {
			return (float)Math.Sqrt ((other.x - x) * (other.x - x) + (other.y - y) * (other.y - y));
		}

		public float GetAngleDegrees() {
			return (float)(Math.Atan2 (y, x) * 180.0f / Math.PI);
		}

	}
}

