using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Game2D;

var builder = Host.CreateApplicationBuilder();

Console.WriteLine("Adding services...");
builder.Services.AddHostedService<Startup>();
builder.Services.AddScoped<Game1>();
builder.Services.AddScoped<Input>();
builder.Services.AddScoped<Stage>();
builder.Services.AddScoped<Player>();

Console.WriteLine("Building host...");
IHost host = builder.Build();
Console.WriteLine("Running...");
host.Run();
