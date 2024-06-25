using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public enum CollisionType {
    Passable,
    Impassable,
    Platform
}

public record struct Tile(Texture2D Texture, CollisionType Collision, Vector2 Offset, Point Size);
