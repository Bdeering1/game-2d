using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class Game1 : Game
{
    private Input input;
    private Stage stage;
    private SpriteBatch spriteBatch;

    public Game1()
    {
        new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        input = new();
        spriteBatch = new(GraphicsDevice);

        Services.AddService<Input>(input);
        Services.AddService<SpriteBatch>(spriteBatch);
        Services.AddService<GraphicsDevice>(GraphicsDevice);
        Services.AddService<ContentManager>(Content);

        stage = new(Services);

        base.Initialize();
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

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        stage.Draw(gameTime);
        spriteBatch.End();

        base.Draw(gameTime);
    }
}
