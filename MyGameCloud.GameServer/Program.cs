using MyGameCloud.GameServer.Logic;
using MyGameCloud.GameServer.Network;
using Serilog;

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

app.MapDefaultEndpoints();
app.MapControllers();
app.UseSerilogRequestLogging();
app.Run();
