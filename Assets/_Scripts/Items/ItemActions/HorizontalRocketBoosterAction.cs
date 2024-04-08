using System;
using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class HorizontalRocketBoosterAction :IItemAction
    {
        private readonly Transform _leftRocket;
        private readonly Transform _rightRocket;
        private readonly float _rocketOffset = 0.1f;
        private readonly float _speed =15;
        private readonly float _boardRightEdge;
        private readonly float _boardLeftEdge;
        private readonly Board _board;
        private Vector2Int _pos;
        public bool IsFinished { get; }
        public HorizontalRocketBoosterAction(Board board, int itemID, Vector2Int pos)
        {
            _board = board;
            _pos = pos;
            _boardRightEdge = _board.Cells.GetBoardBoundaryRightX();
            _boardLeftEdge = _board.Cells.GetBoardBoundaryLeftX();
            Vector3 leftRocketPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            Vector3 rightRocketPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            leftRocketPos.x -= _rocketOffset;
            rightRocketPos.x += _rocketOffset;
            _leftRocket = ObjectPool.Instance.GetBoosterParticleEffect(itemID, leftRocketPos).transform;
            _rightRocket =ObjectPool.Instance.GetBoosterParticleEffect(itemID, rightRocketPos,Quaternion.Euler(0, 0, 180)).transform;
            SetRowLock(true);
        }

        public void Execute()
        {
            
             if(_leftRocket.transform.position.x > -3.5f || _rightRocket.transform.position.x < 3.5f)
             {
                
                _leftRocket.transform.position += Vector3.left * (_speed * Time.fixedDeltaTime);
                _rightRocket.transform.position += Vector3.right * (_speed * Time.fixedDeltaTime);

                if (_leftRocket.transform.position.x > _boardLeftEdge || _rightRocket.transform.position.x < _boardRightEdge)
                {
                    Vector2Int leftRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_leftRocket.position);
                    Vector2Int rightRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_rightRocket.position);
                    if (_board.GetCell(leftRocketCell).HasItem&&!_board.GetItem(leftRocketCell).IsExploding)
                    {
                        MatchData matchData = new MatchData();
                        matchData.Matches = new Match[1];
                        matchData.MatchType = MatchType.SingleExplosion;
                        matchData.Matches[0].IsMatch = true;
                        matchData.Matches[0].Pos = leftRocketCell;
                        EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchData);
                    }

                    if (_board.GetItem(rightRocketCell) != null&&!_board.GetItem(rightRocketCell).IsExploding)
                    {
                        MatchData matchData = new MatchData();
                        matchData.Matches = new Match[1];
                        matchData.MatchType = MatchType.SingleExplosion;
                        matchData.Matches[0].IsMatch = true;
                        matchData.Matches[0].Pos = rightRocketCell;
                        EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchData);
                    }
                }
  
             }
           
        }

        private void SetRowLock(bool isLocked)
        {
            for (int i = 0; i < _board.Width; i++)
            {
                _board.GetCell(i, _pos.y).SetIsLocked(isLocked);
            }
        }
    }
}