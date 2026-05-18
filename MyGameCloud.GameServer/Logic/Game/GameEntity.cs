using NetTopologySuite.Geometries;
using System.Numerics;

namespace MyGameCloud.GameServer.Logic.Game;

public class GameEntity
{
    public int Id { get; set; }
    public Point Position { get; set; } = new Point(new(0, 0));
    public float Radius { get; set; }
    public int Skin { get; set; }

    public int Mass { get; set; }
}
