using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class Game1 : Game
{
    const int DEFAULT_WIDTH = 1200;
    const int DEFAULT_HEIGHT = 900;

    private GraphicsDeviceManager graphics;
    private Input input;
    private Stage stage;
    private SpriteBatch spriteBatch;

    private FrameCounter frameCounter;
    private SpriteFont font;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = DEFAULT_WIDTH;
        graphics.PreferredBackBufferHeight = DEFAULT_HEIGHT;

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        input = new();
        spriteBatch = new(GraphicsDevice);
        frameCounter = new(3);
        font = Content.Load<SpriteFont>("arial-12");

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
        frameCounter.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        stage.Draw(gameTime);

        var fps = frameCounter.ToString();
        spriteBatch.DrawString(
            font,
            fps,
            new Vector2(GraphicsDevice.Viewport.Bounds.Width - font.MeasureString(fps).X - 10, 10),
            Color.White
        );
        spriteBatch.End();

        base.Draw(gameTime);
    }

    public void OnResize(Object sender, EventArgs e)
    {
        if ((graphics.PreferredBackBufferWidth != GraphicsDevice.Viewport.Width) ||
            (graphics.PreferredBackBufferHeight != GraphicsDevice.Viewport.Height))
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.Viewport.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.Viewport.Height;
            graphics.ApplyChanges();
        }
    }
}
