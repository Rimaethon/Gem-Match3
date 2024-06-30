using Rimaethon.Scripts.Utility;
using UnityEngine;

public class LevelGrid : Singleton<LevelGrid>
{
    public static Grid Grid => _grid;
    private static Grid _grid;
    protected override void Awake()
    {
        base.Awake();
        _grid=gameObject.GetComponent<Grid>();
    }

    public Vector3 GetCellCenterWorld(Vector2Int position)
    {
        return _grid.GetCellCenterWorld(new Vector3Int(position.x,position.y,0));
    }
    public  Vector2 GetCellCenterLocalVector2(Vector2Int position)
    {
        return new Vector2(_grid.GetCellCenterLocal(new Vector3Int(position.x,position.y,0)).x,_grid.GetCellCenterLocal(new Vector3Int(position.x,position.y,0)).y);
    }

    public  Vector2Int WorldToCellVector2Int( Vector2 pos)
    {
        return new Vector2Int(_grid.WorldToCell(pos).x,_grid.WorldToCell(pos).y);
    }
    public  Vector2Int WorldToCellVector2Int( Vector3 pos)
    {
        return new Vector2Int(_grid.WorldToCell(pos).x,_grid.WorldToCell(pos).y);
    }

}
