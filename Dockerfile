FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.slnx ./
COPY MyGameCloud.GameServer/*.csproj ./MyGameCloud.GameServer/
COPY MyGameCloud.AppHost/*.csproj ./MyGameCloud.AppHost/
COPY MyGameCloud.ServiceDefaults/*.csproj ./MyGameCloud.ServiceDefaults/
RUN dotnet restore

COPY MyGameCloud.GameServer/. ./MyGameCloud.GameServer/
COPY MyGameCloud.AppHost/. ./MyGameCloud.AppHost/
COPY MyGameCloud.ServiceDefaults/. ./MyGameCloud.ServiceDefaults/
WORKDIR /src/MyGameCloud.GameServer
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyGameCloud.GameServer.dll"]
