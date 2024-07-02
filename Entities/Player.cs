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
    private const int HITBOX_WIDTH = 30;
    private const int HITBOX_HEIGHT = 65;

    private Point drawSize { get; } = new Point(TEXTURE_WIDTH * TEXTURE_SCALING, TEXTURE_HEIGHT * TEXTURE_SCALING);
    public RectangleF Drawbox => new RectangleF(Position + Animations.Offset, drawSize);

    public Texture2D HitboxTexture { get; set; }
    public Animations Animations { get; }
    public PlayerState State { get; set; }
    private PlayerState previousState;

    public Hitbox Position { get; set; } = new();
    public Vector2 Velocity { get; set; } = new();

    /* From Config */
    public float Mass { get; set; }
    public float Force { get; }
    
    private float jumpForce { get; }
    private float upGravity { get; }
    private float downGravity { get; }
    private float groundFriction { get; }
    private float velocityCap { get; }
    private int jumpDelay { get; }

    private float cornerMargin { get; }
    
    /* Services */
    private Input input { get; }

    /* Plyaer State */
    private bool facingRight;
    private bool wasFacingRight;
    private bool onGround;
    private bool wasOnGround;
    private int ticksSinceLanded;

    public Player(GameServiceContainer services, Vector2 spawnPos)
    {
        input = services.GetService<Input>();

        var config = services.GetService<ConfigurationService>();
        var metreSize = (int)config.GetValue("tile", "metreSize");

        Mass = (float)config.GetValue("player", "mass");
        Force = (float)config.GetValue("player", "force") * metreSize;
        jumpForce = (float)config.GetValue("player", "jumpForce") * metreSize;
        upGravity = (float)config.GetValue("player", "upGravity") * metreSize;
        downGravity = (float)config.GetValue("player", "downGravity") * metreSize;
        velocityCap = (float)config.GetValue("player", "velocityCap") * metreSize;
        groundFriction = (float)config.GetValue("player", "groundFriction");
        jumpDelay = (int)config.GetValue("player", "jumpDelay");
        cornerMargin = (float)config.GetValue("player", "cornerMargin");

        HitboxTexture = Utils.CreateRect(services.GetService<GraphicsDevice>(), HITBOX_WIDTH, HITBOX_HEIGHT, Color.Green); 
        
        List<(int fps, int startX, int startY, int numFrames)> anims = [
            (16, 0, 0, 18), // idle
            (16, 2, 2, 24), // running
            (16, 2, 5, 11), // jumping
            (16, 5, 6, 3), // falling
            (8, 0, 7, 4), //landing
        ];
        Animations = new Animations(
            services,
            "player/red-hood-sheet",
            anims,
            TEXTURE_WIDTH,
            TEXTURE_HEIGHT,
            new Vector2((HITBOX_WIDTH - drawSize.X) / 2, HITBOX_HEIGHT - drawSize.Y + 2)
        );

        Position = new Hitbox(spawnPos, new Vector2(HITBOX_WIDTH, HITBOX_HEIGHT));
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

        previousState = State;
        State = GetState(fX);
        SetAnimation();

        if(onGround) {
            ticksSinceLanded++;

            if(ticksSinceLanded > jumpDelay)
                fY += direction.Y * jumpForce;

            //ground friction
            if ((fX == 0 || ((fX < 0) != (Velocity.X < 0))) && Math.Abs(Velocity.X) > 5f) {
                fX += Mass * upGravity * groundFriction * (Velocity.X > 0 ? -1: 1);
            }
        }

        var playerAcc = new Vector2(fX/Mass, fY/Mass);

        Velocity += playerAcc * deltaTime;
        Velocity = Velocity with { X = Math.Min(Math.Abs(Velocity.X), velocityCap) * (Velocity.X > 0 ? 1 : -1) };
        if (fX == 0 && Math.Abs(Velocity.X) < 5f) Velocity = Velocity with { X = 0f };
        Position.Pos += Velocity * deltaTime;

        wasOnGround = onGround;
        onGround = false;
    }

    public void Collided(Hitbox other, Hitbox intersection)
    {
        //collision on top or bottom
        if (intersection.Width + (Velocity.X != 0 ? cornerMargin : 0) > intersection.Height) {
            if (Position.Pos.Y < other.Pos.Y) { //collision on bottom of player
                //on ground state
                if(!wasOnGround) ticksSinceLanded = 0;
                onGround = true;
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

    private PlayerState GetState(float fX) {
        bool xVelSignificant = Math.Abs(Velocity.X) > 5f;
        bool yVelSignificant = Math.Abs(Velocity.Y) > 5f;
        var yDir = Velocity.Y > 0;
        
        if (fX != 0) facingRight = fX > 0;
        if (xVelSignificant || fX != 0) wasFacingRight = facingRight;

        Animations.reflected = wasFacingRight;
        if (yVelSignificant) {
            if(yDir) return PlayerState.FALLING;
                else return PlayerState.JUMPING;
        } else if (xVelSignificant) {
            return PlayerState.RUNNING;
        } else {
            return PlayerState.IDLE;
        }
    }

    void SetAnimation() {
        switch ((previousState, State)) {
            case (PlayerState.FALLING, PlayerState.IDLE):
                Animations.StartTransition((int)PlayerAnimations.LANDING, (int)PlayerAnimations.IDLING);
                break;
            case (PlayerState.FALLING, PlayerState.RUNNING):
                Animations.StartTransition((int)PlayerAnimations.LANDING, (int)PlayerAnimations.RUNNING);
                break;
            case (_, PlayerState.IDLE):
                Animations.SetAnimation((int)PlayerAnimations.IDLING);
                break;
            case (_, PlayerState.RUNNING):
                Animations.SetAnimation((int)PlayerAnimations.RUNNING);
                break;
            case (_, PlayerState.JUMPING):
                Animations.StartTransition((int)PlayerAnimations.JUMPING, (int)PlayerAnimations.FALLING);
                break;
            case (_, PlayerState.FALLING):
                Animations.SetAnimation((int)PlayerAnimations.FALLING);
                break;
        }
    }
}

public enum PlayerState {
    IDLE,
    RUNNING,
    JUMPING,
    FALLING
}

public enum PlayerAnimations {
    IDLING = 0,
    RUNNING = 1,
    JUMPING = 2,
    FALLING = 3,
    LANDING = 4,
}
