using System.Collections.Generic;
using _Scripts.Core;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Managers.Matching
{
    //First version I made  was  checking shapes but there are some shapes that can happen at runtime and this class is not enough to handle all of them.
    //Since items are dropping randomly there can be some instances such as + shape or 5+ same items in a row or column
    //Such as : https://youtu.be/KjkjjvClTGU?t=230   https://youtu.be/KjkjjvClTGU?t=222

    // Also sometimes match logic doesn't work as I assumed it would be. such as : https://youtu.be/KjkjjvClTGU?t=280  Which should be a TNT but it merges to a Rocket instead.

    // Max Possible Matches is 14(3.2*10^-9 probability, at least it is more probable than someone looking at this code) and in order to solve allocation i need to somehow manage to create some kind of cache
    //   0 0 0 1 0 0 0 0
    //   0 0 0 1 0 0 0 0
    //   0 0 0 1 0 0 0 0
    //   0 0 0 1 0 0 0 0
    //   0 1 1 1 1 1 0 0
    //   0 0 0 1 0 0 0 0
    //   0 0 0 1 0 0 0 0
    //   0 0 0 1 0 0 0 0
    //   0 0 0 1 0 0 0 0
    //   0 0 0 1 0 0 0 0
    public struct Match
    {
        public bool IsMatch;
        public Vector2Int Pos;
    }
    public class MatchChecker
    {
        private readonly Board _board;
        private readonly Match[] _horizontalExtensions;
        private readonly Match[] _horizontalMatches;
        private readonly Match[] _verticalExtensions;
        private readonly Match[] _verticalMatches;
        List<Vector2Int> matches = new List<Vector2Int>();
        private int _itemID;
        private Queue<Vector2Int> _itemsToCheckForMatches = new Queue<Vector2Int>();
        public MatchChecker(Board board)
        {
            _board = board;
            _verticalMatches = new Match[10];
            _horizontalMatches =new Match[10];
            _verticalExtensions = new Match[10];
            _horizontalExtensions =new Match[10];
            EventManager.Instance.AddHandler<Vector2Int>(GameEvents.OnItemMovementEnd, AddItemToCheckForMatches);
        }
        public void OnDisable()
        {
            if(EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler<Vector2Int>(GameEvents.OnItemMovementEnd, AddItemToCheckForMatches);
        }
        private void AddItemToCheckForMatches(Vector2Int itemPos)
        {
            if(_itemsToCheckForMatches.Contains(itemPos))
                return;
            _itemsToCheckForMatches.Enqueue(itemPos);
        }
        public bool CheckForMatches()
        {
            int count = _itemsToCheckForMatches.Count;
            bool hasMatch = false;
            while (count > 0)
            {
                count--;
                Vector2Int pos = _itemsToCheckForMatches.Dequeue();

                if (CheckMatch(pos))
                {
                 hasMatch = true;
                }

            }
            return hasMatch;
        }

        public bool  CheckMatch(Vector2Int pos)
        {
            if (!_board.Cells[pos.x,pos.y].HasItem || !_board.GetItem(pos).IsMatchable || _board.GetItem(pos).IsMoving ||
                _board.GetItem(pos).IsExploding||_board.GetItem(pos).IsMatching)
                return false;
            MatchData matchedItems = new MatchData();
            var item = _board.GetItem(pos);
            _itemID = item.ItemID;
            var horizontalMatchCount = CheckMatchesInDirection(_horizontalMatches, pos, new Vector2Int(1, 0));
            var verticalMatchCount = CheckMatchesInDirection(_verticalMatches, pos, new Vector2Int(0, 1));
            var verticalExtensionCount = GetExtension(_horizontalMatches, _verticalExtensions, new Vector2Int(0, 1));
            var horizontalExtensionCount = GetExtension(_verticalMatches, _horizontalExtensions, new Vector2Int(1, 0));
            _board.Cells[pos.x,pos.y].BoardItem.IsMatching = true;
            matches.Add(pos);
            var matchType = CheckForMatchType(horizontalMatchCount, verticalMatchCount, verticalExtensionCount,
                horizontalExtensionCount);
            if (matchType != MatchType.None)
            {
                matchedItems.MatchType = matchType;
                matchedItems.matchID = _itemID;
                matchedItems.Matches.AddRange(matches);
                EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchedItems);
            }
            else
            {
                _board.Cells[pos.x,pos.y].BoardItem.IsMatching = false;
            }
            ClearAllMatchesAndAddThemToArray(_horizontalMatches, false);
            ClearAllMatchesAndAddThemToArray(_verticalMatches, false);
            ClearAllMatchesAndAddThemToArray(_horizontalExtensions, false);
            ClearAllMatchesAndAddThemToArray(_verticalExtensions, false);
            matches.Clear();
            return matchType != MatchType.None;
        }
        private int CheckMatchesInDirection(Match[] matchArray, Vector2Int pos, Vector2Int direction)
        {
            var arrayIndex = 0;
            arrayIndex += CheckMatchesInDirectionHelper(matchArray, pos, direction, 1, arrayIndex);
            arrayIndex = CheckMatchesInDirectionHelper(matchArray, pos, direction, -1, arrayIndex);
            return arrayIndex;
        }

        private int CheckMatchesInDirectionHelper(Match[] matchArray, Vector2Int pos, Vector2Int direction, int start, int arrayIndex)
        {
            var index = start;
            while (_board.IsInBoundaries(pos + direction * index) && _board.GetItem(pos + direction * index) != null)
            {
                Vector2Int cellPos = pos + direction * index;
                var cell = _board.Cells[cellPos.x,cellPos.y];
                if (_itemID == cell.BoardItem.ItemID && !cell.BoardItem.IsMoving && !cell.BoardItem.IsExploding &&
                    !cell.IsGettingFilled && !cell.IsGettingEmptied&&!cell.BoardItem.IsMatching&&cell.BoardItem.IsMatchable&&!cell.IsLocked)
                {
                    matchArray[arrayIndex].Pos = pos + direction * index;
                    matchArray[arrayIndex].IsMatch = true;
                    arrayIndex++;
                }
                else
                {
                    break;
                }
                index += start;
            }
            return arrayIndex;
        }
        Match[] arrayToCheckCopy = new Match[10];

        private int GetExtension(Match[] arrayToCheck, Match[] matchArray, Vector2Int direction)
        {
            int maxMatchCount = 0;
            foreach (var item in arrayToCheck)
            {
                if (!item.IsMatch)
                {
                    break;
                }
                var extensionMatchCount = CheckMatchesInDirection(arrayToCheckCopy, item.Pos, direction);
                if (extensionMatchCount > maxMatchCount)
                {
                    arrayToCheckCopy.CopyTo(matchArray, 0);
                    maxMatchCount = extensionMatchCount;
                }
                arrayToCheckCopy = new Match[10];
            }
            return maxMatchCount;
        }

        private void ClearAllMatchesAndAddThemToArray(Match[] matchArray, bool addToArray = true)
        {
            for (var i = 0; i < matchArray.Length; i++)
            {
                if (matchArray[i].IsMatch && addToArray)
                {
                    matches.Add(matchArray[i].Pos);
                    _board.Cells[matchArray[i].Pos.x,matchArray[i].Pos.y].BoardItem.IsMatching= true;
                }

                matchArray[i].IsMatch = false;
            }
        }
        private MatchType CheckForMatchType(int horizontalMatchCount, int verticalMatchCount, int verticalExtensionCount, int horizontalExtensionCount)
        {
            if (CheckForLightBall(horizontalMatchCount, verticalMatchCount)) return MatchType.LightBall;
            if (CheckForTNT(horizontalMatchCount, verticalMatchCount, verticalExtensionCount, horizontalExtensionCount)) return MatchType.TNT;
            if (CheckForHorizontalRocket(verticalMatchCount)) return MatchType.HorizontalRocket;
            if (CheckForVerticalRocket(horizontalMatchCount)) return MatchType.VerticalRocket;
            if (CheckForMissile(horizontalMatchCount, verticalMatchCount,horizontalExtensionCount,verticalExtensionCount)) return MatchType.Missile;
            if (CheckForNormal(horizontalMatchCount, verticalMatchCount)) return MatchType.Normal;

            return MatchType.None;
        }

        private bool CheckForMissile(int horizontalMatchCount, int verticalMatchCount, int horizontalExtensionCount, int verticalExtensionCount)
        {
            if (horizontalMatchCount < 1 || verticalMatchCount < 1||(verticalExtensionCount==0||horizontalExtensionCount==0)) return false;
            if (_verticalExtensions[0].Pos == _horizontalExtensions[0].Pos)
            {
                ClearAllMatchesAndAddThemToArray(_horizontalMatches);
                ClearAllMatchesAndAddThemToArray(_verticalMatches);
                ClearAllMatchesAndAddThemToArray(_horizontalExtensions);
                return true;
            }

            return false;
        }

        private bool CheckForNormal(int horizontalMatchCount, int verticalMatchCount)
        {
            if (horizontalMatchCount >= 2)
            {
                ClearAllMatchesAndAddThemToArray(_horizontalMatches);
                return true;
            }

            if (verticalMatchCount < 2) return false;
            ClearAllMatchesAndAddThemToArray(_verticalMatches);

            return true;
        }

        private bool CheckForLightBall(int horizontalMatchCount, int verticalMatchCount)
        {
            if (horizontalMatchCount <= 3 && verticalMatchCount <= 3) return false;
            ClearAllMatchesAndAddThemToArray(_horizontalMatches);
            ClearAllMatchesAndAddThemToArray(_verticalMatches);
            if(_horizontalExtensions[0].IsMatch&&_horizontalExtensions[1].IsMatch)
                ClearAllMatchesAndAddThemToArray(_horizontalExtensions);
            if(_verticalExtensions[0].IsMatch&&_verticalExtensions[1].IsMatch)
                ClearAllMatchesAndAddThemToArray(_verticalExtensions);

            return true;
        }

        private bool CheckForTNT(int horizontalMatchCount, int verticalMatchCount, int verticalExtensionCount,
            int horizontalExtensionCount)
        {
            if ((horizontalMatchCount <= 1 || (verticalMatchCount <= 1 && verticalExtensionCount <= 1)) &&
                (verticalMatchCount <= 1 || (horizontalMatchCount <= 1 && horizontalExtensionCount <= 1)))
                return false;
            ClearAllMatchesAndAddThemToArray(_horizontalMatches);
            ClearAllMatchesAndAddThemToArray(_verticalMatches);
            if(_horizontalExtensions[0].IsMatch&&_horizontalExtensions[1].IsMatch)
                ClearAllMatchesAndAddThemToArray(_horizontalExtensions);
            if(_verticalExtensions[0].IsMatch&&_verticalExtensions[1].IsMatch)
                ClearAllMatchesAndAddThemToArray(_verticalExtensions);
            return true;
        }

        private bool CheckForHorizontalRocket(int verticalMatchCount)
        {
            if (verticalMatchCount <= 2) return false;
            ClearAllMatchesAndAddThemToArray(_verticalMatches);
            return true;
        }

        private bool CheckForVerticalRocket(int horizontalMatchCount)
        {
            if (horizontalMatchCount <= 2) return false;
            ClearAllMatchesAndAddThemToArray(_horizontalMatches);
            return true;
        }


    }
}
