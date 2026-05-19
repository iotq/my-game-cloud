using NetTopologySuite.Geometries;
using System.Numerics;

namespace MyGameCloud.GameServer.Logic.Game;

public class GameEntity
{
    public int Id { get; set; }
    public Point Position { get; set; } = new Point(new(0, 0));

    public IEatable RefObj;

    public GameEntity(int id, Point pos, IEatable obj)
    {
        Id = id;
        Position = pos;
        RefObj = obj;
    }
}
