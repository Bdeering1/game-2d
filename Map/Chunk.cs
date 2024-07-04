using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game2D;

public class Chunk {
    public const int CHUNK_WIDTH = 32;
    public const int CHUNK_HEIGHT = 24;

    public Vector2 Offset { get; set; }
    public Rectangle ChunkBounds { get; set; }
    public List<Tile> Tiles { get; } = new();
    public List<Hitbox> CollisionBoxes { get; } = new();

    private SpriteBatch spriteBatch { get; }
    private MapTextures mapTextures { get; }
    private int tileSize { get; }

    public Chunk(GameServiceContainer services)
    {
        spriteBatch = services.GetService<SpriteBatch>();
        mapTextures = services.GetService<MapTextures>();

        var config = services.GetService<ConfigurationService>();
        tileSize = (int)config.GetValue("tile", "size");

        //placeholder tiles at bottom of chunk for testing
        var placeholder = Utils.CreateRect(services.GetService<GraphicsDevice>(), tileSize, tileSize, Color.Khaki);
        for (int i = 0; i < CHUNK_WIDTH; i++) {
            AddImpassable(placeholder, i, CHUNK_HEIGHT - 1);
        }
        AddImpassable(placeholder, CHUNK_WIDTH / 2, CHUNK_HEIGHT - 2);
        AddImpassable(placeholder, CHUNK_WIDTH / 2, CHUNK_HEIGHT - 3);
        AddImpassable(placeholder, CHUNK_WIDTH - 1, CHUNK_HEIGHT - 2);
        AddImpassable(placeholder, CHUNK_WIDTH - 1, CHUNK_HEIGHT - 3);
        ChunkBounds = new Rectangle(Offset.ToPoint(), new Point(CHUNK_WIDTH * tileSize, CHUNK_HEIGHT * tileSize));

        //sort tiles for hitbox generation
        Tiles = [.. Tiles.OrderBy(a => a.X).ThenBy(a => a.Y)];
        CollisionBoxes = GenHitboxes();
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var tile in Tiles) {
            var tileOffset = new Vector2(tile.X * tileSize, tile.Y * tileSize);
            spriteBatch.Draw(tile.Texture, Offset + tileOffset, Color.White);
        }
        //draw outline of hitboxes for debugging
        foreach(var hb in CollisionBoxes) {
            spriteBatch.DrawRectangle(new RectangleF(hb.Pos.X, hb.Pos.Y, hb.Width, hb.Height), Color.Blue, 2);
        }

        spriteBatch.DrawRectangle(new RectangleF(Offset.X, Offset.Y, CHUNK_WIDTH * tileSize, CHUNK_HEIGHT * tileSize), Color.Red, 2);
    }

    private List<Hitbox> GenHitboxes()
    {
        List<Hitbox> hbs = [];
        //find largest vertical hitboxes that fill all of the boxes
        int hbStart = 0;
        for (int i = 1; i < Tiles.Count; i++) {
            Tile tile = Tiles[i];
            if (tile.X == Tiles[hbStart].X) {
                //the tiles are not vertically adjacent
                if (tile.Y-1 != Tiles[i-1].Y) {
                    Hitbox hb = CreateHitbox(Tiles[hbStart], Tiles[i-1]);
                    hbs.Add(hb);
                    AssociateHitbox(hbStart, i-1, hb);
                    hbStart = i;
                }
            } else {
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

        return hbs;
    }
    
    private Hitbox CreateHitbox(Tile start, Tile end)
    {
        return new Hitbox(start.X * tileSize, start.Y * tileSize, (end.X - start.X + 1) * tileSize, (end.Y - start.Y + 1) * tileSize);
    }

    //Associates a hitbox with a list of tiles, 
    //since multiple tiles can all be contained in one hitbox
    private void AssociateHitbox(int start, int end, Hitbox hb)
    {
        for (int i = start; i < end; i++) {
            Tiles[i] = Tiles[i] with { Hitbox = hb };
        }
    }

    public void AddTile(int texture, int x, int y, CollisionType collisionType) {
        Tiles.Add(new Tile(
            mapTextures.GetTexture(texture),
            x, y,
            new Point(tileSize, tileSize),
            collisionType,
            null
        ));
    }

    private void AddImpassable(Texture2D texture, int x, int y) =>
        Tiles.Add(new Tile(
            texture,
            x, y,
            new Point(tileSize, tileSize),
            CollisionType.Impassable,
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
            if (t.X == x && t.Y == y) {
                Tiles.Remove(t);
                return;
            }
        }
    }
}
