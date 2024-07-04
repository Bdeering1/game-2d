using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Stage
{
    public List<Chunk> Chunks { get; } = new();

    private List<IMovable> movables = new();
    private SpriteBatch spriteBatch { get; }
    private Player player { get; }
    private Vector2 spawnPos { get; } = new Vector2(0f, 0f);

    public Stage(GameServiceContainer services)
    {
        player = new(services, spawnPos);
        spriteBatch = services.GetService<SpriteBatch>();

        Chunks.Add(new(services));
        movables.Add(player);
    }

    public void Update(GameTime gameTime)
    {
        player.Update(gameTime);
        CheckCollisions();
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var chunk in Chunks) {
            chunk.Draw(gameTime);
        }
        // spriteBatch.Draw(player.HitboxTexture, player.Position, Color.Green);
        spriteBatch.Draw(player.Animations.Sheet.img, (Rectangle)player.Drawbox, player.Animations.ClipRect, Color.White, 0f, Vector2.Zero, player.Animations.reflected ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
    }

    private void CheckCollisions() 
    {
        foreach(var chunk in Chunks) {
            foreach (var m in movables) {
                foreach(var hb in chunk.CollisionBoxes) {
                    Hitbox intersection = m.Position.Intersects(hb);
                    if(intersection != null) m.Collided(hb, intersection);
                }
            }
        }
    }
}
