using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Chunk {
    public const int CHUNK_WIDTH = 32;
    public const int CHUNK_HEIGHT = 24;
    public const uint CHUNK_TERM = UInt32.MaxValue;

    public Vector2 Offset { get; private set; }
    public Rectangle ChunkBounds { get; private set; }
    public List<Tile> Tiles { get; private set; } = new();
    public List<Hitbox> CollisionBoxes { get; private set; } = new();

    private SpriteBatch spriteBatch { get; }
    private MapTextures mapTextures { get; }
    private int tileSize { get; }

    public Chunk(GameServiceContainer services): this(services, Vector2.Zero) {}

    public Chunk(GameServiceContainer services, Vector2 offset)
    {
        spriteBatch = services.GetService<SpriteBatch>();
        mapTextures = services.GetService<MapTextures>();

        var config = services.GetService<ConfigurationService>();
        tileSize = (int)config.GetValue("tile", "size");

        Offset = offset * tileSize;
        ChunkBounds = new Rectangle(Offset.ToPoint(), new Point(CHUNK_WIDTH * tileSize, CHUNK_HEIGHT * tileSize));
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var tile in Tiles) {
            var tileOffset = new Vector2(tile.X, tile.Y);
            spriteBatch.Draw(mapTextures.GetTexture(tile.TextureID), Offset + tileOffset, Color.White);
        }

        // foreach(var hb in CollisionBoxes) {
        //     spriteBatch.DrawRectangle(new RectangleF(hb.Pos.X, hb.Pos.Y, hb.Width, hb.Height), Color.Blue, 2);
        // }

        spriteBatch.DrawRectangle(ChunkBounds, Color.Red);
    }

    public void Read(BinaryReader reader)
    {
        Offset = new Vector2(reader.ReadSingle(), reader.ReadSingle());

        uint next;
        while ((next = reader.ReadUInt32()) != CHUNK_TERM) {
            var tile = new Tile
            {
                TextureID = next,
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Size = new Point(reader.ReadInt32(), reader.ReadInt32()),
                Collision = (CollisionType)reader.ReadByte()
            };
            Tiles.Add(tile);
        }

        ChunkBounds = new Rectangle(Offset.ToPoint(), new Point(CHUNK_WIDTH * tileSize, CHUNK_HEIGHT * tileSize));
        GenHitboxes();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Offset.X);
        writer.Write(Offset.Y);

        foreach (var tile in Tiles) {
            writer.Write(tile.TextureID);
            writer.Write(tile.X);
            writer.Write(tile.Y);
            writer.Write(tile.Size.X);
            writer.Write(tile.Size.Y);
            writer.Write((Byte)tile.Collision);
        }

        writer.Write(CHUNK_TERM);
    }

    public void AddTile(uint texture, int x, int y, CollisionType collision = CollisionType.Impassable) =>
        Tiles.Add(new Tile(
            texture,
            x, y,
            new Point(tileSize, tileSize),
            collision,
            null
        ));

    public bool HasTileAt(int x, int y) {
        foreach (Tile t in Tiles) {
            if (t.X == x && t.Y == y) return true;
        }
        return false;
    }

    public void RemoveTileAt(int x, int y) {
        foreach (Tile t in Tiles) {
            if (t.X == x && t.Y == y)
            {
                Tiles.Remove(t);
                return;
            }
        }
    }

    public void GenHitboxes()
    {
        if (Tiles.Count <= 0) return;
        Tiles = [.. Tiles.OrderBy(a => a.X).ThenBy(a => a.Y)];

        List<Hitbox> hbs = [];
        //find largest vertical hitboxes that fill all of the boxes
        int hbStart = 0;
        for (int i = 1; i < Tiles.Count; i++) {
            Tile tile = Tiles[i];
            if (tile.X == Tiles[hbStart].X)
            {
                //the tiles are not vertically adjacent
                if (tile.Y-1 != Tiles[i-1].Y)
                {
                    Hitbox hb = CreateHitbox(Tiles[hbStart], Tiles[i-1]);
                    hbs.Add(hb);
                    AssociateHitbox(hbStart, i-1, hb);
                    hbStart = i;
                }
            }
            else
            {
                Hitbox hb = CreateHitbox(Tiles[hbStart], Tiles[i-1]);
                hbs.Add(hb);
                AssociateHitbox(hbStart, i-1, hb);
                hbStart = i;
            }
        }
        //add last hitbox
        Hitbox h = CreateHitbox(Tiles[hbStart], Tiles[^1]);
        hbs.Add(h);
        AssociateHitbox(hbStart, Tiles.Count - 1, h);

        CollisionBoxes = hbs;
    }
    
    private Hitbox CreateHitbox(Tile start, Tile end)
    {
        return new Hitbox(start.X + Offset.X, start.Y + Offset.Y, end.X - start.X + 1, end.Y - start.Y + 1);
    }

    //Associates a hitbox with a list of tiles, 
    //since multiple tiles can all be contained in one hitbox
    private void AssociateHitbox(int start, int end, Hitbox hb)
    {
        for (int i = start; i < end; i++) {
            Tiles[i] = Tiles[i] with { Hitbox = hb };
        }
    }
}
