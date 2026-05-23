namespace MyGameCloud.GameServer.Logic.Game;

public class GameWorld
{
    private Room room;
    private SpatialManager space;
    private readonly float _foodCellSize = 50f;
    public readonly Dictionary<int, Food> Foods = new();
    private readonly HashSet<int> _foodOccupiedIndices = new();

    private int entityIdCursor = 0;

    public float WorldWidth = 1600;
    public float WorldHeight = 1600;


    public GameWorld(Room room)
    {
        this.room = room;
        space = new();
    }

    public void Tick()
    {
        HandleCollisions();
        SpawnFood();
        HashSet<int> existedEntity = new HashSet<int>();

        foreach(Food food in Foods.Values)
        {
            if(food.IsDead) continue;
            existedEntity.Add(food.Id);
        }
        foreach(Player p in room.Players.Values)
        {
            if(p.IsDead) continue;
            existedEntity.Add(p.EntityId);
        }

        space.RemoveUnusedEntity(existedEntity);
    }

    private void HandleCollisions()
    {

        foreach(var p in room.Players.Values)
        {
            if(p.EntityId == -1)
            {
                p.EntityId = entityIdCursor++;
            }
            space.UpdateEntity(p.EntityId, p.X, p.Y, p);
        }

        foreach(var p in room.Players.Values)
        {
            if (p.IsDead) continue;

            double playerRadius = Math.Sqrt(p.Mass/ Math.PI);

            var entities = space.GetNearbyEntities(p.EntityId, playerRadius * 32);

            foreach(var entity in entities)
            {
                if(entity.RefObj is Food food)
                {
                    if (food.IsDead) continue;
                    int mass = food.GetMass();
                    p.Mass += mass;
                    food.OnEaten();
                }else if (entity.RefObj is Player otherPlayer)
                {
                    if (otherPlayer.IsDead) continue;
                    if(otherPlayer.Mass > p.Mass)
                    {

                        otherPlayer.Mass += p.Mass;
                        p.OnEaten();
                        
                    }
                    else if(otherPlayer.Mass < p.Mass)
                    {
                        p.Mass += otherPlayer.Mass;
                        otherPlayer.OnEaten();
                    }
                }
                if (p.IsDead)
                {
                    break;
                }
            }

        }
    }

    private void SpawnFood()
    {
        if (Foods.Count > 50) return;

        int totalCellsX = (int)(WorldWidth / _foodCellSize);
        int totalCellsY = (int)(WorldWidth / _foodCellSize);
        int maxIndex = totalCellsX * totalCellsY;

        for (int i = 0; i < 5; i++)
        {
            int randomIndex = Random.Shared.Next(0, maxIndex);
            if (!_foodOccupiedIndices.Contains(randomIndex))
            {
                int x = randomIndex % totalCellsX;
                int y = randomIndex / totalCellsX;

                var food = new Food(this)
                {
                    Id = entityIdCursor++,
                    GridIndex = randomIndex,
                    Position = new(x * _foodCellSize + _foodCellSize / 2 - WorldWidth /2, y * _foodCellSize + _foodCellSize / 2 - WorldHeight /2)
                };

                Foods.Add(food.Id, food);
                _foodOccupiedIndices.Add(randomIndex);
                space.UpdateEntity(food.Id, food.Position.X, food.Position.Y, food);
                room.OnFoodUpdate(food);
                break;
            }
        }
    }


    public void RemoveFood(Food food)
    {

        room.OnFoodUpdate(food);
        Foods.Remove(food.Id);
        _foodOccupiedIndices.Remove(food.GridIndex);
    }

}
