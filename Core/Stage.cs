using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Stage
{
    public List<Chunk> Chunks { get; } = new();
    private SpriteBatch SpriteBatch { get; }
    private Player Player { get; }

    public Stage(GameServiceContainer services)
    {
        Player = new(services);
        SpriteBatch = services.GetService<SpriteBatch>();

        Chunks.Add(new(services));
    }

    public void Update(GameTime gameTime)
    {
        Player.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var chunk in Chunks) {
            chunk.Draw(gameTime);
        }
        SpriteBatch.Draw(Player.animations.curTexture, Player.Position, Player.animations.clipRect, Color.White);
    }
}
