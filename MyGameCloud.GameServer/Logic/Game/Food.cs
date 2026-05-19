using NetTopologySuite.Geometries;

namespace MyGameCloud.GameServer.Logic.Game;

public class Food: IEatable
{
    public int Id = 0;
    public int GridIndex = 0;
    public int LifeTime = 0;
    public bool IsDead = false;
    public Point Position = Point.Empty;

    private GameWorld world;

    public Food(GameWorld world)
    {
        this.world = world;
    }

    public void OnEaten()
    {
        IsDead = true;
        world.RemoveFood(this);
    }
    public int GetMass()
    {
        return 1;
    }

    public Protos.FoodContent ToProto()
    {
        return new Protos.FoodContent
        {
            Id = Id,
            X = (float)Position.X,
            Y = (float)Position.Y,
            IsDead = IsDead,
        };
    }

}
