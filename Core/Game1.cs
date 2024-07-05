using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class Game1 : Game
{
    const int DEFAULT_WIDTH = 1200;
    const int DEFAULT_HEIGHT = 900;
    const int TARGET_TPS = 120;
    const int FPS_SMOOTHING = 10;

    private GraphicsDeviceManager graphics;
    private ConfigurationService config;
    private SpriteBatch spriteBatch;
    private Input input;
    private MapTextures mapTextures;

    private Menu menu;
    private Stage stage;

    private SpriteFont font;
    private FrameCounter tickCounter;
    private FrameCounter frameCounter;

    private TimeSpan updateTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TARGET_TPS);
    private TimeSpan timer;
    private long tickTime = 0;
    private bool paused;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = DEFAULT_WIDTH,
            PreferredBackBufferHeight = DEFAULT_HEIGHT
        };

        IsFixedTimeStep = false;
        graphics.SynchronizeWithVerticalRetrace = false;

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
        tickCounter = new(FPS_SMOOTHING);
        frameCounter = new(FPS_SMOOTHING);
        mapTextures = new MapTextures(Content, config, GraphicsDevice);

        font = Content.Load<SpriteFont>("arial-12");

        Services.AddService(config);
        Services.AddService(input);
        Services.AddService(mapTextures);
        Services.AddService(spriteBatch);
        Services.AddService(GraphicsDevice);
        Services.AddService(Content);

        menu = new(Services);
        stage = new(Services);

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        input.Update();
        if (input.IsKeyPressed(Keys.Escape)) {
            paused = !paused;
            menu.Reset();
        }

        timer += gameTime.ElapsedGameTime;

        while (timer >= updateTime)
        {
             FixedTimeUpdate(new(gameTime.TotalGameTime, updateTime)); 
             timer -= updateTime;
        }
    }

    private void FixedTimeUpdate(GameTime gameTime)
    {
        var timeThisFrame = Stopwatch.StartNew();
        tickCounter.Update(gameTime);

        if (!paused) stage.Update(gameTime);
        else menu.Update(gameTime);
        
        base.Update(gameTime);
        timeThisFrame.Stop();
        tickTime = timeThisFrame.ElapsedMilliseconds;
    }

    protected override void Draw(GameTime gameTime)
    {
        frameCounter.Update(gameTime);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        stage.Draw(gameTime);

        var fps = String.Format($"{frameCounter} fps");
        spriteBatch.DrawString(
            font,
            fps,
            new Vector2(GraphicsDevice.Viewport.Bounds.Width - font.MeasureString(fps).X - 10, 10),
            Color.White
        );
        if (paused) menu.Draw(gameTime);
        var tps = String.Format($"{tickCounter} tps");
        spriteBatch.DrawString(
            font,
            tps,
            new Vector2(GraphicsDevice.Viewport.Bounds.Width - font.MeasureString(tps).X - 10, 30),
            Color.White
        );
        var tickTimeStr = String.Format($"{tickTime}/8 ms/tick");
        spriteBatch.DrawString(
            font,
            tickTimeStr,
            new Vector2(GraphicsDevice.Viewport.Bounds.Width - font.MeasureString(tickTimeStr).X - 10, 50),
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
