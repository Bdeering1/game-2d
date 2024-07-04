using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public enum CollisionType 
{
    Passable,
    Impassable,
    Platform
}

public record struct Tile(Texture2D Texture, int X, int Y, Point Size, CollisionType Collision, Hitbox Hitbox);
