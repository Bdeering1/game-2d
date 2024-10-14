using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Game2D;

public class Stage
{
    private const string STAGES_DIR = "stages";
    private const string DEFAULT_STAGE_NAME = "test.stage";

    private const float VELOCITY_RESET_THRESHOLD = 4f;

    public List<Chunk> Chunks = new();
    private Vector2 spawnPos = new();

    private GameServiceContainer services { get; }
    private SpriteBatch spriteBatch { get; }
    private List<Player> players { get; }
    private List<InputBinding> playerBindings = new List<InputBinding> { new(Keys.Left, Keys.Right, Keys.Up, Keys.Down), new(Keys.A, Keys.D, Keys.W, Keys.S) };
    private List<Vector2> playerSpawns = new List<Vector2> { new(), new(450, 0) };
    private Camera camera { get; }

    private List<IMovable> movables = new();
    private float cornerMargin;

    public Stage(GameServiceContainer services)
    {
        this.services = services;
        spriteBatch = services.GetService<SpriteBatch>();
        camera = services.GetService<Camera>();
        cornerMargin = (float)(services.GetService<ConfigurationService>()).GetValue("physics", "cornerMargin");

        players = new List<Player> {
            new(services, playerBindings[0], playerSpawns[0]),
            new(services, playerBindings[1], playerSpawns[1])
        };
		camera.TrackedObjects = new List<IMovable>(){players[0], players[1]};
		camera.TrackedObjectsSize = new Vector2(players[0].Hitbox.Width, players[0].Hitbox.Height);
        camera.Center();

        Chunks.Add(new(services));
        foreach (var player in players)
        {
            movables.Add(player);
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var player in players)
        {
            player.Update(gameTime);
        }
        CheckCollisions();
        camera.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var chunk in Chunks)
        {
            chunk.Draw(gameTime);
        }
        foreach (var player in players)
        {
            player.Draw(gameTime);
        }

        spriteBatch.DrawPoint(camera.Hitbox.Center - (Vector2)camera.Hitbox, Color.Blue, 5f);
        // spriteBatch.DrawRectangle(
        //     new RectangleF(camera.SoftFollowBox.X - camera.Hitbox.X, camera.SoftFollowBox.Y - camera.Hitbox.Y, camera.SoftFollowBox.Width, camera.SoftFollowBox.Height),
        //     Color.Blue,
        //     2);
        // spriteBatch.DrawRectangle(
        //     new RectangleF(camera.FollowBox.X - camera.Hitbox.X, camera.FollowBox.Y - camera.Hitbox.Y, camera.FollowBox.Width, camera.FollowBox.Height),
        //     Color.Blue,
        //     2);
    }

    public void Read(string fileName = DEFAULT_STAGE_NAME)
    {
        Chunks.Clear();

        #if DEBUG
            var path = Path.Combine(Utils.GetDebugContentDir(), STAGES_DIR, fileName);
        #else
            var path = Path.Combine(STAGES_DIR, fileName);
        #endif

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        if (reader.BaseStream.Length <= 1)
        {
            Chunks.Add(new(services, new()));
            return;
        }

        spawnPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());

        while (reader.BaseStream.Position != reader.BaseStream.Length) {
            var chunk = new Chunk(services);
            chunk.Read(reader);
            Chunks.Add(chunk);
        }
    }

    public void Write()
    {
        #if DEBUG
            var path = Path.Combine(Utils.GetDebugContentDir(), STAGES_DIR, DEFAULT_STAGE_NAME);
        #else
            var path = Path.Combine(STAGES_DIR, STAGE_NAME);
        #endif

        using var fs = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        writer.Write(spawnPos.X);
        writer.Write(spawnPos.Y);
        
        foreach (var chunk in Chunks) {
            chunk.Write(writer); // chunk offsets should be normalized before this happens
        }
    }

    public void Reload() {
        foreach (Chunk c in Chunks)
        {
            c.GenHitboxes();
        }
        var idx = 0;
        foreach (var player in players)
        {
            player.Hitbox.XY = playerSpawns[idx++];
            player.Velocity = new();
        }
    }

    private void CheckCollisions() 
    {
        foreach (var chunk in Chunks)
        {
            var idx = 0;
            foreach (var m in movables)
            {
                idx++;
                foreach (var m2 in movables.Skip(idx))
                {
                    Hitbox intersection = m.Hitbox.Intersects(m2.Hitbox);
                    if(intersection != null)
                    {
                        PhysicsCollision(m, m2, intersection);
                    }
                }

                foreach (var hb in chunk.CollisionBoxes)
                {
                    Hitbox intersection = m.Hitbox.Intersects(hb);
                    if (intersection != null) StageCollision(m, hb, intersection);
                }
            }
        }
    }

    private void StageCollision(IMovable m, Hitbox hb, Hitbox intersection)
    {
        if (intersection.Width + (m.Velocity.X != 0 ? cornerMargin : 0) >= intersection.Height)
        { // collision on top or bottom
            if (m.Hitbox.Y > hb.Y)
            {
                m.Hitbox.Y = hb.Y + hb.Height;
                m.Collided(CollisionDirection.Up);
            }
            else
            {
                m.Hitbox.Y = hb.Y - m.Hitbox.Height;
                m.Collided(CollisionDirection.Down);
            }
            // if (m.Velocity.Y > 0)
            m.Velocity = new Vector2(m.Velocity.X, 0.0f); // preserve upwards velocity
        }
        else
        { // collision on left or right sides
            if (m.Hitbox.X > hb.X)
            {
                m.Hitbox.X = hb.X + hb.Width;
                m.Collided(CollisionDirection.Left);
            }
            else
            {
                m.Hitbox.X = hb.X - m.Hitbox.Width;
                m.Collided(CollisionDirection.Right);
            }
            m.Velocity = new Vector2(0.0f, m.Velocity.Y);
        }
    }

    private void PhysicsCollision(IMovable m1, IMovable m2, Hitbox intersection) {
        if (intersection.Width + (m1.Velocity.X != 0 ? cornerMargin : 0) >= intersection.Height)
        { // collision on top or bottom
            if (m1.Hitbox.Y > m2.Hitbox.Y)
            {
                if (m1.Velocity.Y + m2.Velocity.Y < 0) // m1 (-) is moving faster
                    m1.Hitbox.Y = m2.Hitbox.Y + m2.Hitbox.Height;
                else
                    m2.Hitbox.Y = m1.Hitbox.Y - m2.Hitbox.Height;

                m1.Collided(CollisionDirection.Up);
                m2.Collided(CollisionDirection.Down);
            }
            else
            {
                if (m1.Velocity.Y + m2.Velocity.Y >= 0) // m1 (+) is moving faster
                    m1.Hitbox.Y = m2.Hitbox.Y - m1.Hitbox.Height;
                else
                    m2.Hitbox.Y = m1.Hitbox.Y + m1.Hitbox.Height;

                m1.Collided(CollisionDirection.Down);
                m2.Collided(CollisionDirection.Up);
            }

            if (m1.Velocity.Y > VELOCITY_RESET_THRESHOLD) m1.Velocity = new Vector2(m1.Velocity.X, 0f);
            if (m2.Velocity.Y > VELOCITY_RESET_THRESHOLD) m2.Velocity = new Vector2(m2.Velocity.X, 0f);
        }
        else
        { // collision on left or right sides
            if (m1.Hitbox.X > m2.Hitbox.X)
            {
                if (m1.Velocity.X + m2.Velocity.X < 0) // m1 (-) is moving faster
                    m1.Hitbox.X = m2.Hitbox.X + m2.Hitbox.Width;
                else
                    m2.Hitbox.X = m1.Hitbox.X - m2.Hitbox.Width;

                m1.Collided(CollisionDirection.Left);
                m2.Collided(CollisionDirection.Right);
            }
            else
            {
                if (m1.Velocity.X + m2.Velocity.X >= 0) // m1 (+) is moving faster
                    m1.Hitbox.X = m2.Hitbox.X - m1.Hitbox.Width;
                else
                    m2.Hitbox.X = m1.Hitbox.X + m1.Hitbox.Width;

                m1.Collided(CollisionDirection.Right);
                m2.Collided(CollisionDirection.Left);
            }

            m1.Velocity = new Vector2(0f, m1.Velocity.Y);
            m2.Velocity = new Vector2(0f, m2.Velocity.Y);
        }
    }
}
