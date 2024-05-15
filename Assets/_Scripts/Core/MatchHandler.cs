using System;
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
        private readonly HashSet<MatchData> _matchesToHandle= new HashSet<MatchData>(); 
        private List<MatchData> _matchesToHandleThisFrame= new List<MatchData>();
        private List<Vector2Int> _directions = new List<Vector2Int>();
        private bool isDisabled;
        public MatchHandler(Board board)
        {
            _board = board;
            EventManager.Instance.AddHandler<MatchData>(GameEvents.AddMatchToHandle, AddMatchToHandle);
        }
        
        public void OnDisable()
        {
            _board = null;
            isDisabled = true;
            _matchesToHandle.Clear();
            _matchesToHandleThisFrame.Clear();
            _directions.Clear();
            GC.Collect();
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<MatchData>(GameEvents.AddMatchToHandle, AddMatchToHandle);
        }
        private void AddMatchToHandle(MatchData matchData)
        {
            if (isDisabled)
            {
                Debug.LogWarning("MatchHandler is disabled");
                return;
            }
            _matchesToHandle.Add(matchData);
        }


        public bool HandleMatches()
        {
            if (isDisabled)
            {
                Debug.LogWarning("MatchHandler is disabled");
                return false;
            }
            _matchesToHandleThisFrame.AddRange(_matchesToHandle);
            foreach (MatchData matchData in _matchesToHandleThisFrame)
            {
                CheckForMatchType(matchData);
            }
            _matchesToHandleThisFrame.Clear();
            return _matchesToHandle.Count > 0;
        }
        
        private void CheckForMatchType(MatchData data)
        {
            if (data.MatchType == MatchType.None)
            {
                _matchesToHandle.Remove(data);
                return;
            }
            if (data.MatchType == MatchType.Normal)
            {
                HandleMatch(data.Matches);
                _matchesToHandle.Remove(data);
                return;
            }
            LerpAllItemsToPosition(data);
        }
        private void  HandleMatch(List<Vector2Int> matchedItems)
        {
            foreach (var pos in matchedItems)
            {
                if(!_board.IsInBoundaries(pos)||!_board.Cells[pos.x,pos.y].HasItem||!_board.GetItem(pos).IsMatching)
                    continue;
                ExplodeAllDirections(pos);
                Cell cell=_board.Cells[pos.x,pos.y];
                cell.BoardItem.OnMatch();
            }
            AudioManager.Instance.PlaySFX(SFXClips.MatchItemExplodeSound);
            IBoardItem item = _board.GetItem(matchedItems[0]);
            EventManager.Instance.Broadcast<Vector3,int,int>(GameEvents.OnMainEventGoalMatch,item.Transform.position,item.ItemID,matchedItems.Count);
        }
        private void LerpAllItemsToPosition(MatchData matchData)
        {
            if (!matchData.IsInitialized)
            {
                InitializeLerp(matchData);
            }
            bool isAllMerged = true;
            Vector2Int mergeVector2IntPos = matchData.Matches[0];
            Vector3 mergePos = LevelGrid.Instance.GetCellCenterLocalVector2(mergeVector2IntPos);
            foreach (Vector2Int pos in matchData.Matches)
            {
                if (!_board.IsInBoundaries(pos) || !_board.Cells[pos.x,pos.y].HasItem)
                    continue;
                Cell cell = _board.Cells[pos.x,pos.y];
                cell.BoardItem.Transform.localPosition = Vector3.MoveTowards(cell.BoardItem.Transform.localPosition, mergePos, 0.09f);
                if (Vector3.Distance(cell.BoardItem.Transform.localPosition, mergePos) > 0.05f)
                {
                    isAllMerged = false;
                }
            }
            if(!isAllMerged)
                return;
            foreach (Vector2Int match in matchData.Matches)
            {
                if (!_board.IsInBoundaries(match) || !_board.Cells[match.x,match.y].HasItem)
                    continue;
                ExplodeAllDirections(match);
                Cell cell = _board.Cells[match.x,match.y];
                cell.BoardItem.OnRemove();
            }

            AudioManager.Instance.PlaySFX(SFXClips.MatchSound);
            IBoardItem item = _board.GetItem(mergeVector2IntPos);
            EventManager.Instance.Broadcast<Vector3,int,int>(GameEvents.OnMainEventGoalMatch,item.Transform.position,item.ItemID,matchData.Matches.Count);
            EventManager.Instance.Broadcast(GameEvents.AddItemToAddToBoard, mergeVector2IntPos, (int)matchData.MatchType);
            _matchesToHandle.Remove(matchData);
        }

        private void InitializeLerp(MatchData matchData)
        {
            foreach (Vector2Int match in matchData.Matches)
            {
                if (!_board.IsInBoundaries(match) || !_board.Cells[match.x,match.y].HasItem)
                    continue;
                Cell cell = _board.Cells[match.x,match.y];
                cell.BoardItem.IsMatching = true;
                cell.BoardItem.IsActive= false;
            }
            matchData.IsInitialized = true;
        }
            
        private void ExplodeAllDirections(Vector2Int pos)
        {
           _directions = pos.GetFourDirections();
            for (int i = 0; i < 4; i++)
            {
                if (_board.IsInBoundaries(_directions[i]) && _board.Cells[_directions[i].x,_directions[i].y].HasItem &&
                    !_board.GetItem(_directions[i]).IsExploding && !_board.GetItem(_directions[i]).IsMatching &&
                    _board.GetItem(_directions[i]).IsExplodeAbleByNearMatches)
                    _board.GetItem(_directions[i]).OnExplode();
            }
        }
        
    }
}