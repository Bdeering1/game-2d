using Game2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class MapTextures {

    private Texture2D[] Textures { get; }
    private Texture2D Default { get; }

    public MapTextures(ContentManager Content, ConfigurationService config, GraphicsDevice graphics) {
        Texture2D tilesImg = Content.Load<Texture2D>("tiles/tiles");
        int tileSize = (int)config.GetValue("tile", "size");
        Textures = new Texture2D[tilesImg.Width/tileSize];
        for (int i = 0; i < Textures.Length; i++) {
            Textures[i] = new Texture2D(graphics, tileSize, tileSize);
            Rectangle srcRect = new (i * tileSize, 0, tileSize, tileSize);
            Color[] data = new Color[tileSize * tileSize];
            tilesImg.GetData(0, srcRect, data, 0, data.Length);
            Textures[i].SetData(data);
        }

        Default = Utils.CreateRect(graphics, tileSize, tileSize, Color.Violet);

    }

    //same as indexing textures but ensures no indexoutofbounds
    public Texture2D GetTexture(int texture) {
        if (texture >= 0 && texture < Textures.Length) return Textures[texture];
        else return Default;
    }

}