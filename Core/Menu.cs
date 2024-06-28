using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Menu
{
    private const int MENU_WIDTH = 500;
    private const int MENU_HEIGHT = 400;

    private Input input { get; }
    private GraphicsDevice graphics { get; }
    private SpriteBatch spriteBatch { get; }

    private Texture2D placeholder { get; }

    public Menu(GameServiceContainer services)
    {
        input = services.GetService<Input>();
        spriteBatch = services.GetService<SpriteBatch>();
        graphics = services.GetService<GraphicsDevice>();
        
        placeholder = Utils.CreateRect(graphics, MENU_WIDTH, MENU_HEIGHT, Color.Gold);
    }

    public void Draw(GameTime gameTime)
    {
        spriteBatch.Draw(
            placeholder,
            new Vector2(
                (graphics.Viewport.Bounds.Width / 2) - (MENU_WIDTH / 2),
                (graphics.Viewport.Bounds.Height / 2) - (MENU_HEIGHT / 2)
            ),
            Color.White
        );
    }
}
