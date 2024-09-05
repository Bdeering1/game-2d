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

    public List<Chunk> Chunks = new();
    private Vector2 spawnPos = new();

    private GameServiceContainer services { get; }
    private SpriteBatch spriteBatch { get; }
    private List<Player> players { get; }
    private List<InputBinding> playerBindings = new List<InputBinding> { new(Keys.Left, Keys.Right, Keys.Up, Keys.Down), new(Keys.A, Keys.D, Keys.W, Keys.S) };
    private List<Vector2> playerSpawns = new List<Vector2> { new(), new(500, 0) };
    private Camera camera { get; }

    private List<IMovable> movables = new();

    public Stage(GameServiceContainer services)
    {
        this.services = services;
        spriteBatch = services.GetService<SpriteBatch>();
        camera = services.GetService<Camera>();

        players = new List<Player> {
            new(services, playerBindings[0], playerSpawns[0]),
            new(services, playerBindings[1], playerSpawns[1])
        };
        camera.TrackedObject = players[0];
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

        spriteBatch.DrawPoint(camera.Hitbox.Center - camera.Hitbox, Color.Blue, 5f);
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
            var idx = 1;
            foreach (var m in movables)
            {
                foreach (var hb in chunk.CollisionBoxes)
                {
                    Hitbox intersection = m.Hitbox.Intersects(hb);
                    if(intersection != null) m.Collided(hb, intersection);
                }
                foreach (var m2 in movables.Skip(idx))
                {
                    if (ReferenceEquals(m, m2)) continue;
                    Hitbox intersection = m.Hitbox.Intersects(m2.Hitbox);
                    if(intersection != null)
                    {
                        m.Collided(m2.Hitbox, intersection);
                        m2.Collided(m.Hitbox, intersection);
                    }
                }
                idx++;
            }
        }
    }
}
