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

    private float jumpingGravity { get; }
    private float fallingGravity { get; }

    private Input input { get; }
    private int tileSize { get; }

    public Player(GameServiceContainer services)
    {
        input = services.GetService<Input>();

        var config = services.GetService<ConfigurationService>();
        Acceleration = (float)config.GetValue("player", "acceleration");
        tileSize = (int)config.GetValue("tile", "size");
        jumpingGravity = (float)config.GetValue("player", "jumpinggravity");
        fallingGravity = (float)config.GetValue("player", "fallinggravity");

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
        Vector2 gravityAcc;
        //todo: replace with some better method like a time since on ground, feels too floaty
        if (Velocity.Y < 0.0) { //going up
            gravityAcc = new Vector2(0.0f, jumpingGravity) * tileSize;
        } else { //falling
            gravityAcc = new Vector2(0.0f, fallingGravity) * tileSize;
        }
        var playerAcc = Acceleration * tileSize *
                        (!digitalDirection.Equals(Vector2.Zero)
                         ? digitalDirection
                         : analogDirection);
        
        //temporary to allow jumping
        playerAcc = new Vector2(playerAcc.X, playerAcc.Y * 20.0f);

        Velocity += (gravityAcc + playerAcc) * deltaTime;
        Position.Pos += Velocity * deltaTime;
    }

    public void Collided(Hitbox other, Hitbox intersection)
    {
        //collision on top or bottom (1/8 tile allowance for landing on top of tile)
        if (intersection.Width + tileSize/8 > intersection.Height) {
            if (Position.Pos.Y < other.Pos.Y) { //collision on bottom of player
                //on ground state
                Position.Pos.Y = other.Pos.Y - Position.Height;
            } else { //collision on top of player
                Position.Pos.Y = other.Pos.Y + other.Height;
            }
            Velocity = new Vector2(Velocity.X, 0.0f);
        } else { //collision on left or right sides
            if (Position.Pos.X > other.Pos.X) { // collision on left side of player
                Position.Pos.X = other.Pos.X + other.Width;
            } else { //collision on right side of player
                Position.Pos.X = other.Pos.X - Position.Width;
            }
            Velocity = new Vector2(0.0f, Velocity.Y);
        }
    }
}
