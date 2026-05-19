using MyGameCloud.GameServer.Logic;
using MyGameCloud.GameServer.Network;
using Serilog;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddHostedService<PollingService>();

builder.Services.AddSingleton<Lobby>();


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();
var app = builder.Build();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
};
app.UseWebSockets(webSocketOptions);

app.MapGet("/", async context =>
{
    context.Response.Redirect("/game/");
});
app.MapDefaultEndpoints();
app.MapControllers();
app.UseSerilogRequestLogging();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
