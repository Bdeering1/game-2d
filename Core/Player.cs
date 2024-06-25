using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game2D;

public class Player
{
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public float Speed { get; set; }
    public Animations animations { get; private set; }

    private Input Input { get; }

    public Player(Input input)
    {
        Input = input;

        Speed = 100f;
    }

    public void LoadContent(GraphicsDevice graphics, ContentManager content)
    {
        Texture = Utils.CreateRect(graphics, 100, 200, Color.Green); 
        Console.WriteLine("creating animations");

        List<(string path, int fps, int startX, int startY, int numFrames)> anims = new(){
            ("player/red-hood-sheet", 20, 0, 0, 18)
        };

        animations = new Animations(content, anims, 50, 40);
    }

    public void Update(GameTime gameTime)
    {
        animations.Update(gameTime);
        Position += Input.GetDigitalDirection() * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}
