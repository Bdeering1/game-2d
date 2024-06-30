using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game2D;

public class Player: IMovable
{
    private const int TEXTURE_WIDTH = 50;
    private const int TEXTURE_HEIGHT = 40;
    private const int HITBOX_WIDTH = 25;
    private const int HITBOX_HEIGHT = 40;

    public Texture2D HitboxTexture { get; set; }
    public Animations Animations { get; }

    public Hitbox Position { get; set; } = new();
    public Vector2 Velocity { get; set; } = new();
    public float Mass { get; set; }
    public float Force { get; }
    
    private float jumpForce { get; }
    private float upGravity { get; }
    private float downGravity { get; }

    private Input input { get; }
    private int tileSize { get; }

    //PLAYER STATE
    private bool onGround;
    private TimeSpan timeSinceOnGround;
    //END PLAYER STATE

    public Player(GameServiceContainer services, Vector2 spawnPos)
    {
        input = services.GetService<Input>();

        var config = services.GetService<ConfigurationService>();
        Force = (float)config.GetValue("player", "force");
        Mass = (float)config.GetValue("player", "mass");
        jumpForce = (float)config.GetValue("player", "jumpforce");
        upGravity = (float)config.GetValue("player", "upgravity");
        downGravity = (float)config.GetValue("player", "downgravity");

        tileSize = (int)config.GetValue("tile", "size");

        HitboxTexture = Utils.CreateRect(services.GetService<GraphicsDevice>(), HITBOX_WIDTH, HITBOX_HEIGHT, Color.Green); 
        
        List<(int fps, int startX, int startY, int numFrames)> anims = [
            (16, 0, 0, 18)
        ];
        Animations = new Animations(
            services,
            "player/red-hood-sheet",
            anims,
            TEXTURE_WIDTH,
            TEXTURE_HEIGHT,
            new Vector2((HITBOX_WIDTH - TEXTURE_WIDTH) / 2, (HITBOX_HEIGHT - TEXTURE_HEIGHT) / 2)
        );

        Position = new Hitbox(spawnPos.X, spawnPos.Y, (float)HITBOX_WIDTH, (float)HITBOX_HEIGHT);
    }

    public void Update(GameTime gameTime)
    {
        Animations.Update(gameTime);

        var deltaTime = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;

        var (fX, fY) = (0.0f, 0.0f);

        var digitalDirection = input.GetDigitalDirection();
        var direction = (!digitalDirection.Equals(Vector2.Zero)
                         ? digitalDirection
                         : input.GetAnalogDirection());
        
        fX += direction.X * Force;
        if (Velocity.Y < -120.0) {
            fY = upGravity * Mass * tileSize;
        } else {
            fY = downGravity * Mass * tileSize;
        }
        if(onGround) {
            fY += direction.Y * jumpForce * tileSize;

            //ground friction
            fX += Velocity.X > 0 ? -(Mass * upGravity * 10.0f) : (Mass * upGravity * 10.0f);
        }

        var playerAcc = new Vector2(fX/Mass, fY/Mass);

        Velocity += playerAcc * deltaTime;
        Position.Pos += Velocity * deltaTime;

        onGround = false;
        timeSinceOnGround += gameTime.ElapsedGameTime;
    }

    public void Collided(Hitbox other, Hitbox intersection)
    {
        //collision on top or bottom (1/8 tile allowance for landing on top of tile)
        if (intersection.Width + tileSize/8 > intersection.Height) {
            if (Position.Pos.Y < other.Pos.Y) { //collision on bottom of player
                //on ground state
                onGround = true;
                timeSinceOnGround = TimeSpan.Zero;
                Position.Pos.Y = other.Pos.Y - Position.Height;
            } else { //collision on top of player
                Position.Pos.Y = other.Pos.Y + other.Height;
            }
            Velocity = new Vector2(Velocity.X, Math.Min(0.0f, Velocity.Y));
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
