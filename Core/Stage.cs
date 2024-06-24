using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Stage
{
    private Player Player { get; }

    public Stage(Player player)
    {
        Player = player;
    }

    public void LoadContent(GraphicsDevice graphics, ContentManager content)
    {
        Player.LoadContent(graphics, content);
    }

    public void Update(GameTime gameTime)
    {
        Player.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Player.animations.curTexture, Player.Position, Player.animations.clipRect, Color.White);
        //spriteBatch.Draw(Player.Texture, Player.Position, Color.White);        
    }
}
