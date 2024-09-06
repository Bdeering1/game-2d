using Microsoft.Xna.Framework;

namespace Game2D;

public record struct Tile(uint TextureID, int X, int Y, Point Size, CollisionType Collision, Hitbox Hitbox);
