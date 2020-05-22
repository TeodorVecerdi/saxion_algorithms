using System;
using System.Drawing;

namespace application {
    /// <summary>
    ///     Representation of a Rectangle. Just like <see cref="System.Drawing.Rectangle" /> but Bottom and Right properties
    ///     return the correct point for Bottom and Right instead of one unit too much
    /// </summary>
    public struct Rect : IEquatable<Rect>, IEquatable<Rectangle> {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rect(Rectangle rectangle) : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height) { }
        public Rect(Point position, Size size) : this(position.X, position.Y, size.Width, size.Height) { }

        public Rect(int x, int y, int width, int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Left => X;
        public int Top => Y;
        public int Right => X + Width - 1;
        public int Bottom => Y + Height - 1;
        public int Area => Width * Height;

        public void Shrink(Size size) {
            Shrink(size.Width, size.Height);
        }

        public void Shrink(int width, int height) {
            X += width;
            Y += height;
            Width -= 2 * width;
            Height -= 2 * height;
        }

        public Rect Shrinked(Size size) {
            return Shrinked(size.Width, size.Height);
        }

        public Rect Shrinked(int width, int height) {
            return new Rect(X + width, Y + height, Width - 2 * width, Height - 2 * height);
        }

        public bool Equals(Rect other) {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public bool Equals(Rectangle other) {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj) {
            return obj is Rect other && Equals(other) || obj is Rectangle otherR && Equals(otherR);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                return hashCode;
            }
        }

        public override string ToString() {
            return $"Rect{{{X}, {Y}, {Width}, {Height}}}";
        }

        public static bool operator ==(Rect left, Rect right) {
            return left.Equals(right);
        }

        public static bool operator !=(Rect left, Rect right) {
            return !left.Equals(right);
        }

        public static implicit operator Rect(Rectangle r) {
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }

        public static implicit operator Rectangle(Rect r) {
            return new Rectangle(r.X, r.Y, r.Width, r.Height);
        }
    }
}