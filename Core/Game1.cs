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
    const int FPS_SMOOTHING = 5;

    private GraphicsDeviceManager graphics;
    private ConfigurationService config;
    private SpriteBatch spriteBatch;
    private Input input;

    private Menu menu;
    private Stage stage;

    private FrameCounter frameCounter;
    private SpriteFont font;

    private bool paused;

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
        config = new();
        input = new();
        spriteBatch = new(GraphicsDevice);
        frameCounter = new(FPS_SMOOTHING);
        font = Content.Load<SpriteFont>("arial-12");

        Services.AddService<ConfigurationService>(config);
        Services.AddService<Input>(input);
        Services.AddService<SpriteBatch>(spriteBatch);
        Services.AddService<GraphicsDevice>(GraphicsDevice);
        Services.AddService<ContentManager>(Content);

        menu = new(Services);
        stage = new(Services);

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        input.Update();
        frameCounter.Update(gameTime);

        if (input.IsKeyPressed(Keys.Escape)) paused = !paused;
        if (!paused) stage.Update(gameTime);

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
        if (paused) menu.Draw(gameTime);

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
