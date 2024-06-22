using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework;
using Game2D;

public class Startup : IHostedService
{
    private readonly Game1 game;
    private readonly GraphicsDeviceManager graphics;
    private readonly IHostApplicationLifetime lifetime;

    public Startup(Game1 game, IHostApplicationLifetime lifetime)
    {
        this.game = game;
        this.lifetime = lifetime;

        this.graphics = new GraphicsDeviceManager(game);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(OnStarted);

        game.Exiting += OnGameExiting;

        return Task.CompletedTask; 
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lifetime.StopApplication();

        return Task.CompletedTask;
    }

    private void OnGameExiting(object sender, EventArgs e)
    {
        StopAsync(new CancellationToken());
    }

    private void OnStarted() {
        game.Run(); 
    }
}
