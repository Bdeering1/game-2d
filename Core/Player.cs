using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game2D;

public class Player
{
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public float Speed { get; set; }

    private Input Input { get; }

    public Player(Input input)
    {
        Input = input;

        Speed = 100f;
    }

    public void LoadContent(GraphicsDevice graphics, ContentManager content)
    {
        Texture = Utils.CreateRect(graphics, 100, 200, Color.Orange); 
    }

    public void Update(GameTime gameTime)
    {
        Position += Input.GetDigitalDirection() * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}
