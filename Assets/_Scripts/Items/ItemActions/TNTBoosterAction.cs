using System;
using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class TNTBoosterAction :IItemAction
    {
        private GameObject _tntParticleEffect;
        private float _speed = 12;
        private float _boardUpEdge;
        private float _boardDownEdge;
        private int _itemID;
        private int _radius = 2;
        public void Execute(Board board, Vector2Int pos, int itemID)
        {
            _itemID = itemID;
            
            SetIsLocked(board, pos,true);
            board.GetCell(pos).SetItem(null);
            Vector3 tntPos = LevelGrid.Instance.GetCellCenterWorld(pos);
            _tntParticleEffect = ObjectPool.Instance.GetBoosterParticleEffect(itemID, tntPos);
            EventManager.Instance.Broadcast(GameEvents.OnBoardShake);
            ExpandAndDestroy(board, pos).Forget();
        }
        private async UniTask ExpandAndDestroy(Board board, Vector2Int pos)
        {
            int explosionArea = 1;
            while (explosionArea <= _radius)
            {
                MatchData matchData = new MatchData();
                matchData.Matches = new Match[30];
                matchData.MatchType = MatchType.SingleExplosion;
                int index = 0;
                
     
                for (int x = pos.x-explosionArea; x <= pos.x +explosionArea; x++)
                {
                    for (int y = pos.y - explosionArea; y <= pos.y + explosionArea; y++)
                    {
                        if (x >= 0 && x < board.Width && y >= 0 && y < board.Height && pos != new Vector2Int(x, y))
                        {
                            matchData.Matches[index].Pos = new Vector2Int(x, y);
                            matchData.Matches[index].IsMatch = true;
                            index++;
                        }

                    }
                }
                EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchData);
                explosionArea++;
                await UniTask.Delay(100);
            }

            await UniTask.Delay(200);
            SetIsLocked(board,pos,false);
            
        }
        
        private void SetIsLocked(Board board, Vector2Int pos,bool isLocked)
        {
            for (int x = pos.x - 2; x <= pos.x + 2; x++)
            {
                for (int y = pos.y - 2; y <= pos.y + 2; y++)
                {
                    if (x >= 0 && x < board.Width&& y >= 0 && y < board.Height)
                    {
                        board.GetCell(x,y).SetIsLocked(isLocked);
                    }
                 
                }
            }
        }

        public void Execute()
        {
            
        }

        public bool IsFinished { get; }
    }
}