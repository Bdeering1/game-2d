using Microsoft.Xna.Framework;

namespace Game2D;

public enum CollisionType
{
    Passable,
    Impassable,
    Platform
}

public enum CollisionDirection
{
    Up,
    Down,
    Left,
    Right
}

public interface IMovable
{
    CollisionType Collision { get; set; }
    Hitbox Hitbox { get; set; }
    Vector2 Velocity { get; set; }
    float Mass { get; }

    public void Collided(CollisionDirection collisionDir);
}
