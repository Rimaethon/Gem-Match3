using System;
using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class VerticalRocketBoosterAction :IItemAction
    {

         private Transform _upRocket;
        private Transform _downRocket;
        private float _rocketOffset = 0.1f;
        private float _speed = 15;
        private float _boardUpEdge;
        private float _boardDownEdge;
        private int _itemID;
        public void Execute(Board board, Vector2Int pos, int itemID)
        {
            _itemID = itemID;
            for(int i = 0; i < board.Height; i++)
            {
               board.GetCell(pos.x,i).SetIsLocked(true);
            }
            board.GetCell(pos).SetItem(null);
            Vector3 upRocketPos = LevelGrid.Instance.GetCellCenterWorld(pos);
            Vector3 downRocketPos = LevelGrid.Instance.GetCellCenterWorld(pos);
            upRocketPos.y -= _rocketOffset;
            downRocketPos.y += _rocketOffset;
            _upRocket = ObjectPool.Instance.GetBoosterParticleEffect(itemID, upRocketPos,Quaternion.Euler(0, 0, -90)).transform;
            _downRocket =ObjectPool.Instance.GetBoosterParticleEffect(itemID, downRocketPos,Quaternion.Euler(0, 0, 90)).transform;
            _boardUpEdge = board.Cells.GetBoardBoundaryTopY();
            _boardDownEdge = board.Cells.GetBoardBoundaryBottomY();
            Debug.Log("Horizontal Rocket Booster Action"+_boardUpEdge+" "+_boardDownEdge);
            MoveAndDestroy(board, pos).Forget();
        }
        private async UniTask MoveAndDestroy(Board board, Vector2Int pos)
        {
            
            while (_upRocket.transform.position.y < 5.5f || _downRocket.transform.position.y > -5.5f)
            {
                _upRocket.transform.position += Vector3.up * (_speed * Time.fixedDeltaTime);
                _downRocket.transform.position += Vector3.down * (_speed * Time.fixedDeltaTime);
                if (_upRocket.transform.position.y < _boardUpEdge || _downRocket.transform.position.y > _boardDownEdge)
                {
                    Vector2Int upRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_upRocket.position);
                    Vector2Int downRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_downRocket.position);
                    if (board.GetItem(upRocketCell) != null&&!board.GetItem(upRocketCell).IsExploding)
                    {
                        MatchData matchData = new MatchData();
                        matchData.Matches = new Match[1];
                        matchData.MatchType = MatchType.SingleExplosion;
                        matchData.Matches[0].IsMatch = true;
                        matchData.Matches[0].Pos = upRocketCell;
                        EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchData);
                    }
                    if (board.GetItem(downRocketCell) != null&&!board.GetItem(downRocketCell).IsExploding)
                    {
                        MatchData matchData = new MatchData();
                        matchData.Matches = new Match[1];
                        matchData.MatchType = MatchType.SingleExplosion;
                        matchData.Matches[0].IsMatch = true;
                        matchData.Matches[0].Pos = downRocketCell;
                        EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchData);
                    }
                }

                await UniTask.Delay(50);
            }
            for(int i = 0; i<board.Height; i++)
            {
                    board.GetCell(pos.x,i).SetIsLocked(false);
                    Debug.Log(board.GetCell(pos.x,i).HasItem);
                    Debug.Log("Vertical Rocket Booster Action Done");
            }  

        }

        public void Execute()
        {
            
        }

        public bool IsFinished { get; }
    }
}