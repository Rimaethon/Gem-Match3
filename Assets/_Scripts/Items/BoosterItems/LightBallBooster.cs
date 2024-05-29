using System.Collections.Generic;
using _Scripts.Managers;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using Scripts.BoosterActions;
using Unity.VisualScripting;
using UnityEngine;

public class LightBallBooster : BoosterBoardItem  
{
    private int _idToMatch;


    public override void OnClick(Board board, Vector2Int pos)
    {
        if(IsMoving||IsExploding||IsSwapping||_isClicked)
            return;
        _isClicked = true;
        _position = pos;
        OnExplode();
    }
    public override void OnSwap(IBoardItem boardItem, IBoardItem otherBoardItem)
    {
        if(IsMoving||IsExploding)
            return;
        _isExploding= true;
        _idToMatch = otherBoardItem.ItemID;
        OnRemove();

        EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID,_idToMatch);
        
    }
    public override void OnExplode()
    {
        if(IsExploding||!IsActive)
            return;
        _isExploding= true; 
        FindMostCommonItem(Board);
        OnRemove();
        if (_idToMatch == -1)
            return;
        EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID,_idToMatch);

    }
    
    public override void OnRemove()
    {
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
        _idToMatch = -1;
        foreach (var item in itemCounter)
        {
            if (item.Value > max&& !LevelManager.Instance.ItemsGettingMatchedByLightBall.Contains(item.Key))
            {
                max = item.Value;
                _idToMatch = item.Key;
            }
        }
        if (_idToMatch != -1)
        {
            LevelManager.Instance.ItemsGettingMatchedByLightBall.Add(_idToMatch);
        }
    }

}
