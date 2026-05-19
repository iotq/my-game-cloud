using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using System.Numerics;

namespace MyGameCloud.GameServer.Logic.Game;

public class SpatialManager
{
    // 使用四叉樹作為空間索引
    private readonly Quadtree<GameEntity> _index = new();
    private readonly Dictionary<int, GameEntity> _allEntities = new();

    // 更新或添加實體位置
    public void UpdateEntity(int id, double x, double y, IEatable refObject)
    {
        var position = new Point(x, y);

        if (_allEntities.TryGetValue(id, out var entity))
        {
            // 如果實體已存在，先從索引中移除舊位置
            _index.Remove(entity.Position.EnvelopeInternal, entity);
            entity.Position = position;
        }
        else
        {
            entity = new GameEntity(id, position, refObject) { Id = id, Position = position };
            _allEntities[id] = entity;
        }

        // 將實體插入新位置的索引
        _index.Insert(entity.Position.EnvelopeInternal, entity);
    }

    // 獲取玩家周圍的實體
    public List<GameEntity> GetNearbyEntities(int id, double radius)
    {

        if (!_allEntities.TryGetValue(id, out var entity))
        {
            return [];
        }

        var x = entity.Position.X;
        var y = entity.Position.Y;

        // 1. 定義一個搜索範圍的矩形邊界 (Envelope)
        var searchBounds = new Envelope(x - radius, x + radius, y - radius, y + radius);

        // 2. 從四叉樹中檢索出可能在範圍內的實體 (粗略篩選)
        var candidates = _index.Query(searchBounds);

        // 3. 精確過濾 (計算實際距離，排除矩形角落但在圓形半徑外的實體)
        var center = new Point(x, y);
        return candidates
            .Where(e => e.Id != id && e.Position.Distance(center) <= radius)
            .ToList();
    }

    private void RemoveEntity(int id)
    {
        if (_allEntities.Remove(id, out var entity))
        {
            _index.Remove(entity.Position.EnvelopeInternal, entity);
        }
    }

    public void RemoveUnusedEntity(HashSet<int> usedEntity)
    {
        int[] keys = _allEntities.Keys.ToArray();

        foreach (var key in keys)
        {
            if (!usedEntity.Contains(key))
            {
                RemoveEntity(key);
            }
        }
    }

}
