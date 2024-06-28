using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using MonoGame.Extended;

namespace Game2D;

public class Chunk {
    public const int TILE_SIZE = 32;
    public const int CHUNK_WIDTH = 15;
    public const int CHUNK_HEIGHT = 10;

    public Vector2 Offset { get; set; }
    public List<Tile> Tiles { get; } = [];
    public List<Hitbox> CollisionBoxes { get; } = [];

    private SpriteBatch spriteBatch { get; }

    public Chunk(GameServiceContainer services) 
    {
        spriteBatch = services.GetService<SpriteBatch>();

        var placeholder = Utils.CreateRect(services.GetService<GraphicsDevice>(), TILE_SIZE, TILE_SIZE, Color.Khaki);
        //placeholder tiles at bottom of chunk for testing
        for (int i = 0; i < 10; i++) {
            Tiles.Add(new Tile(
                placeholder,
                i, 9,
                null,
                CollisionType.Impassable,
                new Vector2(i * TILE_SIZE, 9 * TILE_SIZE),
                new Point(TILE_SIZE, TILE_SIZE)
            ));
        }
        //sort tiles for hitbox generation
        Tiles = [.. Tiles.OrderBy(a => a.Y).ThenBy(a => a.X)];
        
        CollisionBoxes = GenHitboxes();
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var tile in Tiles) {
            spriteBatch.Draw(tile.Texture, Offset + tile.Offset, Color.White);
        }
        //draw outline of hitboxes for debugging
        foreach(var hb in CollisionBoxes) {
            spriteBatch.DrawRectangle(new RectangleF(hb.Pos.X, hb.Pos.Y, hb.Width, hb.Height), Color.Blue, 2);
        }
    }

    private List<Hitbox> GenHitboxes() {
        List<Hitbox> hbs = [];
        //find largest vertical hitboxes that fill all of the boxes
        int curX = Tiles[0].X;
        int hbStart = 0;
        for (int i = 1; i < Tiles.Count; i++) {
            Tile tile = Tiles[i];
            if (tile.X == curX && Tiles[hbStart].X == curX) {
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
                curX = tile.X;
                hbStart = i;
            }
        }
        //add last hitbox
        Hitbox h = CreateHitbox(Tiles[hbStart], Tiles[^1]);
        hbs.Add(h);
        AssociateHitbox(hbStart, Tiles.Count - 1, h);

        return hbs;
    }
    
    private static Hitbox CreateHitbox(Tile start, Tile end) {
        return new Hitbox(start.X * TILE_SIZE, start.Y * TILE_SIZE, (end.X - start.X + 1) * TILE_SIZE, (end.Y - start.Y + 1) * TILE_SIZE);
    }

    //Associates a hitbox with a list of tiles, 
    //since multiple tiles can all be contained in one hitbox
    private void AssociateHitbox(int start, int end, Hitbox hb) {
        for (int i = start; i < end; i++) {
            Tile tmp = Tiles[i];
            tmp.Hitbox = hb;
            Tiles[i] = tmp;
        }
    }
}
