using System;
using Microsoft.Xna.Framework;

namespace Game2D;

public class Hitbox(Vector2 pos, Vector2 dimensions)
{
    public Vector2 XY = pos;
    public Vector2 Dimensions = dimensions;
    public float X
    {
        get => XY.X;
        set => XY.X = value;
    }
    public float Y
    {
        get => XY.Y;
        set => XY.Y = value;
    }
    public float Width
    {
        get => Dimensions.X;
        set => Dimensions.X = value;
    }
    public float Height
    {
        get => Dimensions.Y;
        set => Dimensions.Y = value;
    }
    public Vector2 Center
    {
        get => new (X + Width / 2, Y + Height / 2);
        set
        {
            X = value.X - Width / 2;
            Y = value.Y - Height / 2;
        }
    }

    public Hitbox(): this(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f)) {}
    public Hitbox(float x, float y, float width, float height): this(new Vector2(x, y), new Vector2(width, height)) {}

    public Hitbox Intersects(Hitbox other) 
    {
        if(other == null) return null;
        var x1 = Math.Max(X, other.X);
        var y1 = Math.Max(Y, other.Y);
        var x2 = Math.Min(X + Width, other.X + other.Width);
        var y2 = Math.Min(Y + Dimensions.Y, other.Y + other.Height);

        var res = new Hitbox(new Vector2(x1, y1), new Vector2(x2-x1,y2-y1));
        if (res.Width < 0.0 || res.Height < 0.0) 
            return null;
        return res;
    }

    public Hitbox Intersects(Rectangle other) 
    {
        var x1 = Math.Max(X, other.X);
        var y1 = Math.Max(Y, other.Y);
        var x2 = Math.Min(X + Width, other.X + other.Width);
        var y2 = Math.Min(Y + Dimensions.Y, other.Y + other.Height);

        var res = new Hitbox(new Vector2(x1, y1), new Vector2(x2-x1,y2-y1));
        if (res.Width < 0.0 || res.Height < 0.0) 
            return null;
        return res;
    }

    public bool IsInside(Rectangle rect) {
        return rect.Contains(XY) && rect.Contains(XY + Dimensions - Vector2.One);
    }

    public Rectangle ToRectangle() {
        return new Rectangle(XY.ToPoint(), Dimensions.ToPoint());
    }

    //allows direct casting of this hitbox to a position,
    //because that is this structs primary use
    public static implicit operator Vector2(Hitbox h) 
    {
        return h.XY;
    }
}
