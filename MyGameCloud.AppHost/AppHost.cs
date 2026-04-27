var builder = DistributedApplication.CreateBuilder(args);

var gameServer = builder.AddProject<Projects.MyGameCloud_GameServer>("game-server");

var gameClient = builder.AddExternalService("gameclient", builder.Configuration["External:GameEndpoint"]!);

gameServer.WithReference(gameClient);

builder.Build().Run();
