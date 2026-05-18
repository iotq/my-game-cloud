namespace MyGameCloud.GameServer.Logic.Game;

public class GameWorld
{
    public List<Player> Players = new List<Player>();
    public List<Food> Foods = new List<Food>();

    private readonly float _foodCellSize = 50f;
    private readonly Dictionary<int, Food> _foods = new();
    private readonly HashSet<int> _foodOccupiedIndices = new();

    public float WorldWidth = 5000;
    public float WorldHeight = 5000;

    public void Tick(float deltaTime)
    {
        // 1. 處理移動
        // 2. 處理碰撞判定（誰吃了誰）
        HandleCollisions();
    }

    private void HandleCollisions()
    {
        // 這裡寫「大吃小」的判斷：
        // 遍歷所有玩家，檢查是否重疊且半徑大於對方一定比例
        // 如果 A 吃 B: A.Mass += B.Mass; B.IsDead = true;
    }

    private void SpawnFood()
    {
        
    }


    public void OnFoodEaten(Food food)
    {
        _foods.Remove(food.Id);
        _foodOccupiedIndices.Remove(food.GridIndex);
    }

}
