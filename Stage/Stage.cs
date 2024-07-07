using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Stage
{
    private const string STAGES_DIR = "stages";
    private const string DEFAULT_STAGE_NAME = "test.stage";

    public List<Chunk> Chunks = new();
    private Vector2 spawnPos = new();

    private List<IMovable> movables = new();
    private SpriteBatch spriteBatch { get; }
    private Player player { get; }
    private GameServiceContainer services { get; }

    public Stage(GameServiceContainer services)
    {
        this.services = services;
        player = new(services, spawnPos);
        spriteBatch = services.GetService<SpriteBatch>();

        Chunks.Add(new(services));
        movables.Add(player);
    }

    public void Update(GameTime gameTime)
    {
        player.Update(gameTime);
        CheckCollisions();
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var chunk in Chunks) {
            chunk.Draw(gameTime);
        }
        player.Draw(gameTime);
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
        foreach (Chunk c in Chunks) {
            c.GenHitboxes();
        }
        player.Position.Pos = spawnPos;
        player.Velocity = new();
    }

    private void CheckCollisions() 
    {
        foreach(var chunk in Chunks) {
            foreach (var m in movables) {
                foreach(var hb in chunk.CollisionBoxes) {
                    Hitbox intersection = m.Position.Intersects(hb);
                    if(intersection != null) m.Collided(hb, intersection);
                }
            }
        }
    }
}
