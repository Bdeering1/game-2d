using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game2D;

public class Player: IMovable
{
    public Texture2D Texture { get; set; }
    public Animations Animations { get; }

    public Hitbox Position { get; set; } = new();
    public Vector2 Velocity { get; set; } = new();
    public float Acceleration { get; }

    private Input Input { get; }

    public Player(GameServiceContainer services)
    {
        Input = services.GetService<Input>();

        var config = services.GetService<ConfigurationService>();
        Acceleration = config.GetFloat("player", "acceleration");

        Texture = Utils.CreateRect(services.GetService<GraphicsDevice>(), 30, 50, Color.Green); 
        Console.WriteLine("creating animations");

        List<(int fps, int startX, int startY, int numFrames)> anims = [
            (16, 0, 0, 18)
        ];
        Animations = new Animations(services, "player/red-hood-sheet", anims, 50, 40);

        Position = new Hitbox(0f, 0f, 30f, 50f);
    }

    public void Update(GameTime gameTime)
    {
        Animations.Update(gameTime);

        var digitalDirection = Input.GetDigitalDirection();
        var analogDirection = Input.GetAnalogDirection();
        Velocity += new Vector2(0.0f, 9.8f) + Acceleration *
                    (!digitalDirection.Equals(Vector2.Zero)
                     ? digitalDirection
                     : analogDirection);
        Position.Pos += Vector2.Multiply(Velocity, (float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Collided(Hitbox other, Hitbox intersection)
    {
        Position.Pos.Y = 10.0f;
        Velocity = Vector2.Zero;
    }
}
