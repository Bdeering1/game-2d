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

    public Stage(GameServiceContainer services)
    {
        player = new(services);
        spriteBatch = services.GetService<SpriteBatch>();

        Chunks.Add(new(services));
        movables.Add(player);
    }

    public void Update(GameTime gameTime)
    {
        CheckCollisions();
        player.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var chunk in Chunks) {
            chunk.Draw(gameTime);
        }
        spriteBatch.Draw(player.Texture, player.Position, Color.Green);
        spriteBatch.Draw(player.Animations.sheet.img, player.Position, player.Animations.clipRect, Color.White);
    }

    private void CheckCollisions() 
    {
        foreach(var chunk in Chunks) {
            foreach (var m in movables) {
                foreach(var hb in chunk.CollisionBoxes) {
                    Hitbox intersection = m.Position.Intersects(hb);
                    if(intersection != null) {
                       m.Collided(hb, intersection);
                    }
                }
            }
        }
    }
}
