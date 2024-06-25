using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game2D;

public class Player
{
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public float Speed { get; set; }
    public Animations animations { get; }

    private Input Input { get; }

    public Player(GameServiceContainer services)
    {
        Input = services.GetService<Input>();

        Texture = Utils.CreateRect(services.GetService<GraphicsDevice>(), 100, 200, Color.Green); 
        Console.WriteLine("creating animations");

        List<(int fps, int startX, int startY, int numFrames)> anims = [
            (16, 0, 0, 18)
        ];
        animations = new Animations(services, "player/red-hood-sheet", anims, 50, 40);

        Speed = 100f;
    }

    public void Update(GameTime gameTime)
    {
        animations.Update(gameTime);
        Position += Input.GetDigitalDirection() * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}
