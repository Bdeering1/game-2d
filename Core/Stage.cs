using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Stage
{
    private List<Chunk> Chunks { get; } = new();
    private Player Player { get; }

    public Stage(Player player)
    {
        Player = player;

        Chunks.Add(new());
    }

    public void LoadContent(GraphicsDevice graphics, ContentManager content)
    {
        foreach (var chunk in Chunks) {
            chunk.LoadContent(graphics, content);
        }
        Player.LoadContent(graphics, content);
    }

    public void Update(GameTime gameTime)
    {
        Player.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (var chunk in Chunks) {
            chunk.Draw(gameTime, spriteBatch);
        }
        spriteBatch.Draw(Player.animations.curTexture, Player.Position, Player.animations.clipRect, Color.White);
    }
}
