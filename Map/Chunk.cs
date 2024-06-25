using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Chunk {
    public const int TILE_SIZE = 32;
    public const int CHUNK_WIDTH = 15;
    public const int CHUNK_HEIGHT = 10;

    public Vector2 Offset { get; set; }
    public Tile[,] Tiles { get; } = new Tile[CHUNK_WIDTH, CHUNK_HEIGHT];
    public List<Rectangle> CollisionBoxes { get; }

    private SpriteBatch SpriteBatch { get; }

    public Chunk(GameServiceContainer services) {
        SpriteBatch = services.GetService<SpriteBatch>();

        var placeholder = Utils.CreateRect(services.GetService<GraphicsDevice>(), TILE_SIZE, TILE_SIZE, Color.Khaki);
        for (var x = 0; x < CHUNK_WIDTH; x++) {
            for (var y = 0; y < CHUNK_HEIGHT; y++) {
                Tiles[x,y] = new Tile(
                    placeholder,
                    CollisionType.Passable,
                    new(x * TILE_SIZE, y * TILE_SIZE),
                    new(TILE_SIZE)
                );
            }
        }
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var tile in Tiles) {
            SpriteBatch.Draw(tile.Texture, Offset + tile.Offset, Color.White);
        }
    }
}
