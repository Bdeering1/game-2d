using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Menu
{
    private const int MENU_WIDTH = 500;
    private const int MENU_HEIGHT = 400;

    private Stage stage { get; }

    private Input input { get; }
    private GraphicsDevice graphics { get; }
    private SpriteBatch spriteBatch { get; }

    private Texture2D placeholder { get; }

    private MenuButton button;
    private LevelEditor editor;
    private bool inLevelEditor = false;

    public Menu(GameServiceContainer services, Stage stage)
    {
        this.stage = stage;

        input = services.GetService<Input>();
        spriteBatch = services.GetService<SpriteBatch>();
        graphics = services.GetService<GraphicsDevice>();
        
        placeholder = Utils.CreateRect(graphics, MENU_WIDTH, MENU_HEIGHT, Color.Gold);

        editor = new LevelEditor(services, stage);
        button = new MenuButton(services, new Rectangle((graphics.Viewport.Bounds.Width / 2) - 100/2, (graphics.Viewport.Bounds.Height / 2) - 50/2, 100, 50), onButtonClick, "Level Editor");
    }

    public void Update(GameTime gameTime) {
        if (inLevelEditor)
        {
            editor.Update(gameTime);
            if (editor.exiting)
            {
                inLevelEditor = false;
                editor.exiting = false;
            }
        }
        else button.Update();
    }

    public void Draw(GameTime gameTime)
    {
        if (inLevelEditor) editor.Draw();
        else 
        {
            spriteBatch.Draw(
                placeholder,
                new Vector2(
                    (graphics.Viewport.Bounds.Width / 2) - (MENU_WIDTH / 2),
                    (graphics.Viewport.Bounds.Height / 2) - (MENU_HEIGHT / 2)
                ),
                Color.White
            );
            button.Draw();
        }
    }

    public void Reset(bool reloadStage) {
        if (reloadStage || inLevelEditor)
        {
            stage.Write();
            stage.Reload();
        }
        inLevelEditor = false;
    }

    void onButtonClick() {
        inLevelEditor = true;
        editor.timeSinceInteract = 0;
    }
}
