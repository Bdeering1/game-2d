using MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game2D;

public class Player: IMovable
{
    private const int TEXTURE_SCALING = 2;
    private const int TEXTURE_WIDTH = 50;
    private const int TEXTURE_HEIGHT = 40;
    private const int HITBOX_WIDTH = 40;
    private const int HITBOX_HEIGHT = 70;

    private Point drawSize { get; } = new Point(TEXTURE_WIDTH * TEXTURE_SCALING, TEXTURE_HEIGHT * TEXTURE_SCALING);
    public RectangleF Drawbox => new RectangleF(Position + Animations.Offset, drawSize);

    public Texture2D HitboxTexture { get; set; }
    public Animations Animations { get; }

    public Hitbox Position { get; set; } = new();
    public Vector2 Velocity { get; set; } = new();
    public float Mass { get; set; }
    public float Force { get; }
    
    private float jumpForce { get; }
    private float upGravity { get; }
    private float downGravity { get; }
    private float groundFriction { get; }
    private float velocityCap { get; }

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
        tileSize = (int)config.GetValue("tile", "size");

        Mass = (float)config.GetValue("player", "mass");
        Force = (float)config.GetValue("player", "force") * tileSize;
        jumpForce = (float)config.GetValue("player", "jumpForce") * tileSize;
        upGravity = (float)config.GetValue("player", "upGravity") * tileSize;
        downGravity = (float)config.GetValue("player", "downGravity") * tileSize;
        velocityCap = (float)config.GetValue("player", "velocityCap") * tileSize;
        groundFriction = (float)config.GetValue("player", "groundFriction");

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
            new Vector2((HITBOX_WIDTH - drawSize.X) / 2, HITBOX_HEIGHT - drawSize.Y + 1)
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
        fY = Mass * (Velocity.Y < -120.0 ? upGravity : downGravity);

        if(onGround) {
            fY += direction.Y * jumpForce * tileSize;

            //ground friction
            if (fX == 0 || ((fX < 0) != (Velocity.X < 0))) {
                fX += Mass * upGravity * groundFriction * (Velocity.X > 0 ? -1: 1);
            }
        }

        var playerAcc = new Vector2(fX/Mass, fY/Mass);

        Velocity += playerAcc * deltaTime;
        Velocity = Velocity with { X = Math.Min(Math.Abs(Velocity.X), velocityCap) * (Velocity.X > 0 ? 1 : -1) };
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
