using Microsoft.Xna.Framework;

namespace Game2D;

public interface IMovable
{

    Hitbox Position { get; set; }
    Vector2 Velocity { get; set; }

    public abstract void Collided(Hitbox other, Hitbox intersection);

}