using Microsoft.Xna.Framework;

namespace Game2D;

public interface IMovable
{
    Hitbox Hitbox { get; set; }
    Vector2 Velocity { get; set; }
    float Mass { get; }

    public abstract void Collided(Hitbox other, Hitbox intersection);

}
