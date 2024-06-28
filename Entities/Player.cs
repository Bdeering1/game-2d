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

    private Input input { get; }
    private int tileSize { get; }

    public Player(GameServiceContainer services)
    {
        input = services.GetService<Input>();

        var config = services.GetService<ConfigurationService>();
        Acceleration = (float)config.GetValue("player", "acceleration");
        tileSize = (int)config.GetValue("tile", "size");

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

        var deltaTime = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;

        var digitalDirection = input.GetDigitalDirection();
        var analogDirection = input.GetAnalogDirection();
        var gravityAcc = new Vector2(0.0f, 9.8f) * tileSize;
        var playerAcc = Acceleration * tileSize *
                        (!digitalDirection.Equals(Vector2.Zero)
                         ? digitalDirection
                         : analogDirection);
        Velocity += (gravityAcc + playerAcc) * deltaTime;
        Position.Pos += Velocity * deltaTime;
    }

    public void Collided(Hitbox other, Hitbox intersection)
    {
        Position.Pos.Y = 10.0f;
        Velocity = Vector2.Zero;
    }
}
