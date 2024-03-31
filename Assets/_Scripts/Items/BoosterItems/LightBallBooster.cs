using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using Scripts.BoosterActions;
using UnityEngine;

public class LightBallBooster : ItemBase  
{
    private LightBallBoosterAction _action;
    private int _idToMatch;

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        _isBooster = true;
        _isSwappable = true;
        _isMatchable = false;
        isFallAble = true;
        IsActive = true;
        _isExploding = false;
        _isHighlightAble = true;
        IsMoving = false;
        IsMatching = false;

    }

    
    public override void OnClick(Board board, Vector2Int pos)
    {
        if(IsMoving||IsExploding)
            return;
        OnExplode();
    }
    public override void OnSwap(IItem item, IItem otherItem)
    {
        if(IsMoving||IsExploding)
            return;
        _isExploding= true;
        _idToMatch = otherItem.ItemID;
        _action= new LightBallBoosterAction(Board,_itemID,_idToMatch,Position,2.0f,3f);
        EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, _action);
        OnRemove();
    }
    
    public override void OnExplode()
    {
        if(IsExploding)
            return;
        _isExploding= true;
        FindMostCommonItem(Board);
        _action= new LightBallBoosterAction(Board,_itemID,_idToMatch,Position,2.0f,3f);
        EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, _action);
        OnRemove();
    }
    
    public override void OnRemove()
    {
        ObjectPool.Instance.ReturnItem(Item, ItemID);
        EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
    }
    private void FindMostCommonItem(Board board)
    {
        Dictionary<int, int> itemCounter = new Dictionary<int, int>();
        for (int i = 0; i < board.Width; i++)
        {
            for (int j = 0; j < board.Height; j++)
            {
                var item = board.GetItem(i, j);
                if (item != null && item.ItemID >= 0 && item.ItemID <= 4)
                {
                    if (itemCounter.ContainsKey(item.ItemID))
                    {
                        itemCounter[item.ItemID]++;
                    }
                    else
                    {
                        itemCounter.Add(item.ItemID, 1);
                    }
                }
            }
        }

        int max = 0;
        foreach (var item in itemCounter)
        {
            if (item.Value > max)
            {
                max = item.Value;
                _idToMatch = item.Key;
            }
        }
    }

}
