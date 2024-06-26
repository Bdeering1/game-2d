using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Stage
{
    public List<Chunk> Chunks { get; } = new();
    private List<IMovable> movables = [];
    private SpriteBatch SpriteBatch { get; }
    private Player Player { get; }

    public Stage(GameServiceContainer services)
    {
        Player = new(services);
        SpriteBatch = services.GetService<SpriteBatch>();

        Chunks.Add(new(services));
        movables.Add(Player);
    }

    public void Update(GameTime gameTime)
    {
        CheckCollisions();
        Player.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var chunk in Chunks) {
            chunk.Draw(gameTime);
        }
        SpriteBatch.Draw(Player.Texture, Player.Position, Color.Green);
        SpriteBatch.Draw(Player.Animations.sheet.img, Player.Position, Player.Animations.clipRect, Color.White);
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
