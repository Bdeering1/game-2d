using MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game2D;

public enum PlayerState {
    IDLE,
    RUNNING,
    JUMPING,
    FALLING
}

public enum PlayerAnimations {
    IDLING,
    RUNNING,
    JUMPING,
    FALLING,
    LANDING,
}

public class Player: IMovable
{
    private const int TEXTURE_SCALING = 2;
    private const int TEXTURE_WIDTH = 50;
    private const int TEXTURE_HEIGHT = 40;
    private const int HITBOX_WIDTH = 30;
    private const int HITBOX_HEIGHT = 65;

    private const float DOWN_GRAVITY_THRESHOLD = -120f;

    public CollisionType Collision { get; set; }
    public Hitbox Hitbox { get; set; } = new();
    public Vector2 Velocity { get; set; } = new();

    public RectangleF Drawbox => new RectangleF(Hitbox.XY + animations.Offset, drawSize);
    private Point drawSize { get; } = new Point(TEXTURE_WIDTH * TEXTURE_SCALING, TEXTURE_HEIGHT * TEXTURE_SCALING);

    private Animations animations { get; }
    private Texture2D hitboxTexture { get; }
    private InputBinding bindings;
    private PlayerState state;
    private PlayerState previousState;

    /* From Config */
    public float Mass { get; private set; }
    public float Force { get; private set; }
    
    private float jumpForce;
    private float upGravity;
    private float downGravity;
    private float groundFriction;
    private float airFriction;
    private float velocityCap;
    private int jumpDelay;
    
    /* Services */
    private Input input { get; }
    private SpriteBatch spriteBatch { get; }
    private Camera camera { get; }

    /* Player state */
    private bool facingRight;
    private bool wasFacingRight;
    private bool onGround;
    private bool wasOnGround;
    private int ticksSinceLanded;

    public Player(GameServiceContainer services, InputBinding bindings, Vector2 spawnPos)
    {
        input = services.GetService<Input>();
        spriteBatch = services.GetService<SpriteBatch>();
        camera = services.GetService<Camera>();

        var config = services.GetService<ConfigurationService>();
        var metreSize = (int)config.GetValue("tile", "metreSize");

        Mass = (float)config.GetValue("player", "mass");
        Force = (float)config.GetValue("player", "force") * metreSize;
        jumpForce = (float)config.GetValue("player", "jumpForce") * metreSize;
        upGravity = (float)config.GetValue("player", "upGravity") * metreSize;
        downGravity = (float)config.GetValue("player", "downGravity") * metreSize;
        velocityCap = (float)config.GetValue("player", "velocityCap") * metreSize;
        groundFriction = (float)config.GetValue("player", "groundFriction");
        airFriction = (float)config.GetValue("player", "airFriction");
        jumpDelay = (int)config.GetValue("player", "jumpDelay");

        hitboxTexture = Utils.CreateRect(services.GetService<GraphicsDevice>(), HITBOX_WIDTH, HITBOX_HEIGHT, Color.Green); 
        
        List<(int fps, int startX, int startY, int numFrames)> anims = [
            (16, 0, 0, 18), // idle
            (16, 2, 2, 24), // running
            (16, 2, 5, 11), // jumping
            (16, 5, 6, 3), // falling
            (8, 0, 7, 4), //landing
        ];
        animations = new Animations(
            services,
            "player/red-hood-sheet",
            anims,
            TEXTURE_WIDTH,
            TEXTURE_HEIGHT,
            new Vector2((HITBOX_WIDTH - drawSize.X) / 2, HITBOX_HEIGHT - drawSize.Y + 2)
        );

        this.bindings = bindings;
        Hitbox = new Hitbox(spawnPos, new Vector2(HITBOX_WIDTH, HITBOX_HEIGHT));
    }

    public void Update(GameTime gameTime)
    {
        animations.Update(gameTime);

        var deltaTime = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;

        (float X, float Y) forces = (0.0f, 0.0f);

        var digitalDirection = input.GetDigitalDirection(bindings);
        var direction = (!digitalDirection.Equals(Vector2.Zero)
                         ? digitalDirection
                         : input.GetAnalogDirection());

        if (wasOnGround && Velocity.Y > 0) Velocity = Velocity with { Y = 0 }; // prevent unnecessary ground collision

        previousState = state;
        state = UpdateState(state, direction, ref forces); 
        SetAnimation();

        var playerAcc = new Vector2(forces.X/Mass, forces.Y/Mass);

        Velocity += playerAcc * deltaTime;
        Velocity = Velocity with { X = Math.Min(Math.Abs(Velocity.X), velocityCap) * (Velocity.X > 0 ? 1 : -1) };
        if (forces.X == 0 && Math.Abs(Velocity.X) <= 5f) Velocity = Velocity with { X = 0f };
        Hitbox.XY += Velocity * deltaTime;

        wasOnGround = onGround;
        onGround = false;
    }

    public void Draw(GameTime gameTime)
    {
        // spriteBatch.Draw(hitboxTexture, camera.GetScreenCoords(Hitbox.XY), Color.Green);
        spriteBatch.Draw(
            animations.Sheet.img,
            (Rectangle)camera.GetScreenCoords(Drawbox),
            animations.ClipRect,
            Color.White,
            0f,
            Vector2.Zero,
            animations.reflected
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None,
            0f);
    }

    public void Collided(CollisionDirection collisionDir)
    {
        if (collisionDir == CollisionDirection.Down)
        {
            if (!wasOnGround) ticksSinceLanded = 0;
            onGround = true;
        }
    }

    public void SetProperty(string prop, object value)
    {
        switch (prop) {
            case "mass":
                Mass = (float)value;
                break;
            case "force":
                Force = (float)value;
                break;
            case "jumpForce":
                jumpForce = (float)value;
                break;
            case "upGravity":
                upGravity = (float)value;
                break;
            case "downGravity":
                downGravity = (float)value;
                break;
            case "velocityCap":
                velocityCap = (float)value;
                break;
            case "groundFriction":
                groundFriction = (float)value;
                break;
            case "jumpDelay":
                jumpDelay = (int)value;
                break;
            default:
                break;
        }
    }

    private PlayerState UpdateState(PlayerState st, Vector2 direction, ref (float X, float Y) forces)
    {
        bool xVelSignificant = Math.Abs(Velocity.X) > 5f;
        forces.X += direction.X * Force;
        switch (st) 
        {
            case PlayerState.IDLE:
                if (onGround) 
                {
                    doGravity(ref forces);
                    doGroundPhysics(direction, ref forces);
                    
                    updateAnimationDirection(direction, xVelSignificant, forces);

                    if (forces.Y < 0) return PlayerState.JUMPING;
                    if (Math.Abs(direction.X) > 0) return PlayerState.RUNNING;

                    return PlayerState.IDLE;
                }
                else 
                {
                    return PlayerState.FALLING;
                }
            case PlayerState.RUNNING:
                if (onGround)
                {
                    doGravity(ref forces);
                    doGroundPhysics(direction, ref forces);
 
                    updateAnimationDirection(direction, xVelSignificant, forces);

                    if (forces.Y < 0) return PlayerState.JUMPING;
                    if (Math.Abs(direction.X) == 0) return PlayerState.IDLE;
                    
                    return PlayerState.RUNNING;
                }
                else return PlayerState.FALLING;
            case PlayerState.JUMPING:
                doGravity(ref forces);
                doAirPhysics(ref forces);
                
                updateAnimationDirection(direction, xVelSignificant, forces);

                //once player starts moving downwards, state switches to falling
                if (Velocity.Y > 0) {
                    return PlayerState.FALLING;
                }
                else return PlayerState.JUMPING;
            case PlayerState.FALLING:
                doGravity(ref forces);
                doAirPhysics(ref forces);
                
                updateAnimationDirection(direction, xVelSignificant, forces);

                if (onGround)
                {
                    if (Math.Abs(direction.X) > 0) return PlayerState.RUNNING;
                    else return PlayerState.IDLE;
                } 
                else return PlayerState.FALLING;
            default:
                return PlayerState.IDLE;
        }
    }

    private void updateAnimationDirection(Vector2 direction, bool xVelSignificant, (float X, float Y) forces) {
        if (direction.X != 0) facingRight = direction.X > 0;
        if (xVelSignificant || forces.X != 0) wasFacingRight = facingRight;
        animations.reflected = wasFacingRight;
    }

    private void doGroundPhysics(Vector2 direction, ref (float X, float Y) forces) {
        ticksSinceLanded++;

        if(direction.Y < 0 && ticksSinceLanded > jumpDelay) forces.Y += -jumpForce;

        //ground friction
        if ((forces.X == 0 || ((forces.X < 0) != (Velocity.X < 0))) && Math.Abs(Velocity.X) > 5f)
        {
            forces.X += Mass * upGravity * groundFriction * (Velocity.X > 0 ? -1: 1);
        }
    }

    private void doAirPhysics(ref (float X, float Y) forces) {
        //air friction
        if ((forces.X == 0 || ((forces.X < 0) != (Velocity.X < 0))) && Math.Abs(Velocity.X) > 5f)
        {
            forces.X += Mass * upGravity * airFriction * (Velocity.X > 0 ? -1: 1);
        }
    }

    private void doGravity(ref (float X, float Y) forces) {
        forces.Y = Mass * (Velocity.Y < DOWN_GRAVITY_THRESHOLD ? upGravity : downGravity);
    }

    private void SetAnimation()
    {
        switch ((previousState, state)) {
            case (PlayerState.FALLING, PlayerState.IDLE):
                animations.StartTransition((int)PlayerAnimations.LANDING, (int)PlayerAnimations.IDLING);
                break;
            case (PlayerState.FALLING, PlayerState.RUNNING):
                animations.StartTransition((int)PlayerAnimations.LANDING, (int)PlayerAnimations.RUNNING);
                break;
            case (_, PlayerState.IDLE):
                animations.SetAnimation((int)PlayerAnimations.IDLING);
                break;
            case (_, PlayerState.RUNNING):
                animations.SetAnimation((int)PlayerAnimations.RUNNING);
                break;
            case (_, PlayerState.JUMPING):
                animations.StartTransition((int)PlayerAnimations.JUMPING, (int)PlayerAnimations.FALLING);
                break;
            case (_, PlayerState.FALLING):
                animations.SetAnimation((int)PlayerAnimations.FALLING);
                break;
        }
    }
}
