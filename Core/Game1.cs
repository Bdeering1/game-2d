using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class Game1 : Game
{
    Texture2D ballTexture;
    Vector2 ballPosition;

    private readonly Input input;
    private readonly Stage stage;
    private SpriteBatch spriteBatch;

    public Game1(Input input, Stage stage)
    {
        this.input = input;
        this.stage = stage;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        stage.LoadContent(GraphicsDevice, Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        input.Update();
        stage.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin();
        stage.Draw(gameTime, spriteBatch);
        spriteBatch.End();

        base.Draw(gameTime);
    }
}
