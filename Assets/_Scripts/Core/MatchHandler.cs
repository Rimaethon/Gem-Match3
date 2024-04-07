using System.Collections.Generic;
using _Scripts.Core;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Managers.Matching
{
    public class MatchHandler
    {
        private Board _board;
        private Sequence _sequence;
        private bool[] _dirtyColumns;
        private readonly HashSet<MatchData> _matchesToHandle= new HashSet<MatchData>(); 
        private readonly HashSet<MatchData> _matchesToHandleThisFrame= new HashSet<MatchData>(); 
        public MatchHandler(Board board, bool[] dirtyColumns)
        {
            _board = board;
            _dirtyColumns = dirtyColumns;
            EventManager.Instance.AddHandler<MatchData>(GameEvents.AddMatchToHandle, AddMatchToHandle);

            
        }
        private void AddMatchToHandle(MatchData matchData)
        {
            
            _matchesToHandle.Add(matchData);
        }


        public void HandleMatches()
        {
            _matchesToHandleThisFrame.UnionWith(_matchesToHandle);
            
            foreach (MatchData matchData in _matchesToHandleThisFrame)
            {
                CheckForMatchType(matchData);
                _matchesToHandle.Remove(matchData);
            }
            _matchesToHandleThisFrame.Clear();
            
        }
        
        private void CheckForMatchType(MatchData data)
        {
            if(data.MatchType==MatchType.None)
                return;
            if (data.MatchType == MatchType.Normal)
            {
                HandleMatch(data.Matches);
                return;
            }
            if (data.MatchType == MatchType.SingleExplosion)
            {
               HandleExplosion(data.Matches);
                return;
            }
            LerpAllItemsToPosition(data);
        }

        public void LerpAllItemsToPosition(MatchData matchData)
        {
            Vector2Int mergeVector2IntPos = matchData.Matches[0].Pos;
            _board.GetCell(mergeVector2IntPos).SetIsLocked(true);
            matchData.Matches[0].IsMatch = false;

            Vector3 mergePos = LevelGrid.Instance.GetCellCenterLocalVector2(mergeVector2IntPos);
            foreach (Match match in matchData.Matches)
            {
                if (!IsInBounds(match.Pos) || !_board.GetCell(match.Pos).HasItem || !match.IsMatch)
                    continue;
                _board.GetCell(match.Pos).SetIsLocked(true);
                float duration = Vector2Int.Distance(mergeVector2IntPos, match.Pos) * 0.1f + 0.05f;
                LerpPosition(match.Pos, mergePos, duration);
            }
            _board.GetItem(mergeVector2IntPos).OnRemove();
            EventManager.Instance.Broadcast(GameEvents.AddItemToAddToBoard, mergeVector2IntPos, (int)matchData.MatchType);
            _board.GetCell(mergeVector2IntPos).SetIsLocked(false);
        }

        private void LerpPosition(Vector2Int pos, Vector3 targetPosition, float duration)
        {
            IItem item = _board.GetItem(pos);
            item.Transform.DOLocalMove(targetPosition, duration).SetEase(Ease.Linear);
            DOVirtual.DelayedCall(duration, () =>
            {
                _board.GetCell(pos).SetIsLocked(false);
               item.OnRemove();
            });
        }

        private void  HandleMatch(Match[] matchedItems)
        {
            foreach (var match in matchedItems)
            {
                if(!IsInBounds(match.Pos)||!_board.GetCell(match.Pos).HasItem||!match.IsMatch||!_board.GetItem(match.Pos).IsMatching)
                    continue;
                Cell cell=_board.GetCell(match.Pos);
                cell.Item.OnMatch();
            }
        }

        private void HandleExplosion(Match[] explodingItems)
        {
            foreach (var match in explodingItems)
            {
                if(!IsInBounds(match.Pos)||!_board.GetCell(match.Pos).HasItem||!match.IsMatch)
                    continue;
                Cell cell=_board.GetCell(match.Pos);
                cell.Item.OnExplode();
            }
        }
        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _board.Width && pos.y >= 0 && pos.y < _board.Height;
        }
    
        
    }
}