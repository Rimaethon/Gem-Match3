﻿using System;
using System.Collections.Generic;
using _Scripts.Core;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Managers.Matching
{
    //I made this for checking shapes but there are some shapes that can happen at runtime and this class is not enough to handle all of them.
    //Since items are dropping randomly there can be some instances such as + shape or 5+ same items in a row or column 
    //Such as : https://youtu.be/KjkjjvClTGU?t=230   https://youtu.be/KjkjjvClTGU?t=222  

    // Also sometimes match logic doesn't work as I assumed it would be. such as : https://youtu.be/KjkjjvClTGU?t=280  Which should be a TNT but it merges to a Rocket instead. 
    /// <summary>
    /// HOW CAN THIS BE SO HARD
    /// </summary>

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
        private readonly Match[] _allMatches;

        private readonly Board _board;
        private readonly int _height;
        private readonly Match[] _horizontalExtensions;
        private readonly Match[] _horizontalMatches;
        private readonly Match[] _verticalExtensions;
        private readonly Match[] _verticalMatches;
        private readonly int _width;
        private int _matchCount;
        private int _itemID;
        private readonly HashSet<Vector2Int> _itemsToCheckForMatches = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> _itemsToCheckForMatchesThisFrame = new HashSet<Vector2Int>();
        public MatchChecker(Board board, int width, int height)
        {
            _board = board;
            _width = width;
            _height = height;
            _verticalMatches = new Match[height];
            _horizontalMatches = new Match[width];
            _verticalExtensions = new Match[height];
            _horizontalExtensions = new Match[width];
            _allMatches = new Match[14];
            EventManager.Instance.AddHandler<Vector2Int>(GameEvents.OnItemMovementEnd, AddItemToCheckForMatches);            
        }
        private void AddItemToCheckForMatches(Vector2Int itemPos)
        {
            _itemsToCheckForMatches.Add(itemPos); 
        }
        
        public void CheckForMatches()
        {
            _itemsToCheckForMatchesThisFrame.UnionWith(_itemsToCheckForMatches);
            foreach (Vector2Int pos in _itemsToCheckForMatchesThisFrame)
            {
                CheckMatch(pos);
                _itemsToCheckForMatches.Remove(pos);
            }
            _itemsToCheckForMatchesThisFrame.Clear();

        }

       public bool  CheckMatch(Vector2Int pos)
        {
            if (!_board.GetCell(pos).HasItem || !_board.GetItem(pos).IsMatchable || _board.GetItem(pos).IsMoving ||
                _board.GetItem(pos).IsExploding||_board.GetItem(pos).IsMatching)
                return false;
            MatchData matchedItems = new MatchData();
//            Debug.Log("Checking Matches for "+pos);
            var item = _board.GetItem(pos);
            _itemID = item.ItemID;

            var horizontalMatchCount = CheckMatchesInDirection(_horizontalMatches, pos, new Vector2Int(1, 0));
            var verticalMatchCount = CheckMatchesInDirection(_verticalMatches, pos, new Vector2Int(0, 1));
            var verticalExtensionCount = GetExtension(_horizontalMatches, _verticalExtensions, new Vector2Int(0, 1));
            var horizontalExtensionCount = GetExtension(_verticalMatches, _horizontalExtensions, new Vector2Int(1, 0));

            _matchCount = 0;
            _allMatches[_matchCount].Pos = pos;
            _allMatches[_matchCount].IsMatch = true;
            _board.GetCell(pos).Item.IsMatching = true;
            _matchCount++;

            var matchType = CheckForMatchType(horizontalMatchCount, verticalMatchCount, verticalExtensionCount,
                horizontalExtensionCount);
            if (matchType != MatchType.None)
            {
                Array.Copy(_allMatches, matchedItems.Matches, 14);
                matchedItems.MatchType = matchType;
                EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchedItems);
            }
            else
            {
                _board.GetCell(pos).Item.IsMatching = false;
            }
            ClearAllMatchesAndAddThemToArray(_horizontalMatches, false);
            ClearAllMatchesAndAddThemToArray(_verticalMatches, false);
            ClearAllMatchesAndAddThemToArray(_horizontalExtensions, false);
            ClearAllMatchesAndAddThemToArray(_verticalExtensions, false);
            ClearAllMatchesAndAddThemToArray(_allMatches, false);
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
            while (IsWithinBounds(pos + direction * index) && _board.GetItem(pos + direction * index) != null)
            {
                var cell = _board.GetCell(pos + direction * index);
                if (_itemID == cell.Item.ItemID && !cell.Item.IsMoving && !cell.Item.IsExploding &&
                    !cell.IsGettingFilled && !cell.IsGettingEmptied&&!cell.Item.IsMatching)
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

        private bool IsWithinBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
        }

        private int GetExtension(Match[] arrayToCheck, Match[] matchArray, Vector2Int direction, int minMatchCount = 2)
        {
            foreach (var item in arrayToCheck)
            {
                if (!item.IsMatch) return 0;
                var extensionMatchCount = CheckMatchesInDirection(matchArray, item.Pos, direction);
                if (extensionMatchCount > minMatchCount) return extensionMatchCount;
            }

            return 0;
        }

        private void ClearAllMatchesAndAddThemToArray(Match[] matchArray, bool addToArray = true)
        {
            for (var i = 0; i < matchArray.Length; i++)
            {
                if (matchArray[i].IsMatch && addToArray)
                {
                    _allMatches[_matchCount] = matchArray[i];
                    _allMatches[_matchCount].IsMatch = true;
                    _board.GetCell(matchArray[i].Pos).Item.IsMatching= true;
                    _matchCount++;
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
            if (CheckForMissile(horizontalMatchCount, verticalMatchCount)) return MatchType.Missile;
            if (CheckForNormal(horizontalMatchCount, verticalMatchCount)) return MatchType.Normal;
            
            return MatchType.None;
        }

        private bool CheckForMissile(int horizontalMatchCount, int verticalMatchCount)
        {
            if (horizontalMatchCount < 1 || verticalMatchCount < 1) return false;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if(!_horizontalMatches[i].IsMatch||!_verticalMatches[j].IsMatch) continue;
                    var posToCheck= new Vector2Int(_horizontalMatches[i].Pos.x, _verticalMatches[j].Pos.y);
                    if (IsWithinBounds(posToCheck) && _board.GetCell(posToCheck).HasItem && _board.GetItem(posToCheck).ItemID == _itemID)
                    {
                        _allMatches[_matchCount].Pos = posToCheck;
                        _allMatches[_matchCount].IsMatch = true;
                        _matchCount++;
                        _board.GetItem(posToCheck).IsMatching = true;
                        ClearAllMatchesAndAddThemToArray(_horizontalMatches);
                        ClearAllMatchesAndAddThemToArray(_verticalMatches);
                        return true;
                    }
                }
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
            ClearAllMatchesAndAddThemToArray(_horizontalExtensions);
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
            ClearAllMatchesAndAddThemToArray(_horizontalExtensions);
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