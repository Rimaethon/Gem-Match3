using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Core
{
    public class SwipeController
    {
        private Vector2Int _swipedItemPos;
        private Vector2Int _swipeTargetPos;
        private bool _isSwipedThisFrame;
        private  Board _board;
        private readonly MatchChecker _matchChecker;
        private readonly int _width;
        private readonly int _height;
        private bool _isSwiping;

        public SwipeController(Board board,MatchChecker matchChecker, int width, int height)
        {
            _board = board;
            _matchChecker = matchChecker;
            _width = width;
            _height = height;
            EventManager.Instance.AddHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnTouch, OnTouch);
        }
        
        
        
        
        public void MakeSwipe()
        {
            if (!_isSwipedThisFrame||_isSwiping)
            {
                return;
            }

            if (!IsInBoundaries(_swipedItemPos) || !IsInBoundaries(_swipeTargetPos) ||
                !_board.GetCell(_swipedItemPos).HasItem || !_board.GetCell(_swipeTargetPos).HasItem)
            {
                _isSwipedThisFrame = false;
                return;
            }
            if (!_board.GetItem(_swipedItemPos).IsSwappable || !_board.GetItem(_swipeTargetPos).IsSwappable||_board.GetItem(_swipeTargetPos).IsMoving||_board.GetItem(_swipedItemPos).IsMoving||_board.GetItem(_swipeTargetPos).IsSwapping||_board.GetItem(_swipedItemPos).IsSwapping||_board.GetItem(_swipeTargetPos).IsExploding||_board.GetItem(_swipedItemPos).IsExploding||_board.GetCell(_swipeTargetPos).IsGettingFilled
                ||_board.GetCell(_swipedItemPos).IsGettingFilled||_board.GetCell(_swipeTargetPos).IsGettingEmptied||_board.GetCell(_swipedItemPos).IsGettingEmptied||_board.GetCell(_swipeTargetPos).IsLocked||_board.GetCell(_swipedItemPos).IsLocked)
            {
                _isSwipedThisFrame = false;
                Debug.Log("Pos is not movable");
                return;
            }
            SwipeItemsAsync(_swipedItemPos, _swipeTargetPos).Forget();
        }
        
  
        private async UniTask SwipeItemsAsync(Vector2Int pos1, Vector2Int pos2)
        {
            _isSwiping = true;
            var item1 = _board.GetItem(pos1);
            var item2 = _board.GetItem(pos2);
            item1.IsSwapping = true;
            item2.IsSwapping = true;
            item1.SwappingFrom = pos1;
            item2.SwappingFrom = pos2;
            // Tween the movement
            var item1Tween = item1.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos2), 0.2f);
            var item2Tween = item2.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos1), 0.2f);
            // Wait for the tweens to complete
            await UniTask.WhenAll(item1Tween.ToUniTask(), item2Tween.ToUniTask());
            SwapItems(pos1,pos2);
            bool swap1 = _matchChecker.CheckMatch(pos1);
            bool swap2 = _matchChecker.CheckMatch(pos2);
            if (!swap1 && !swap2)
            {
                item1Tween = item1.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos1), 0.2f);
                item2Tween = item2.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos2), 0.2f);
                await UniTask.WhenAll(item1Tween.ToUniTask(), item2Tween.ToUniTask());
                SwapItems(pos1,pos2);
            }
            else
            {
                EventManager.Instance.Broadcast(GameEvents.OnSuccessfulSwap);
            }
            item1.IsSwapping = false;
            item2.IsSwapping = false;
            _isSwiping = false;
            _isSwipedThisFrame = false;

        }
        private void SwapItems(Vector2Int pos1, Vector2Int pos2)
        {
            IItem temp = _board.GetItem(pos1);
            _board.GetCell(pos1).SetItem(_board.GetItem(pos2));
            _board.GetCell(pos2).SetItem(temp);
        }
        private  void OnSwipe(Vector2Int direction, Vector2 clickPos)
        {
            if (_isSwipedThisFrame)
            {
                return;
            }
            Vector2Int swipingFrom= LevelGrid.Instance.WorldToCellVector2Int(clickPos);
            Vector2Int swipingTo= swipingFrom + direction;
            if(!IsInBoundaries(swipingFrom)||!IsInBoundaries(swipingTo)||_board.GetItem(swipingFrom)==null||_board.GetItem(swipingTo)==null)
                return;
            if (!_board.GetItem(swipingFrom).IsSwappable || !_board.GetItem(swipingTo).IsSwappable||_board.GetItem(swipingTo).IsMoving||_board.GetItem(swipingFrom).IsMoving||_board.GetItem(swipingTo).IsSwapping||_board.GetItem(swipingFrom).IsSwapping||_board.GetItem(swipingTo).IsExploding||_board.GetItem(swipingFrom).IsExploding||_board.GetCell(swipingTo).IsGettingFilled
                ||_board.GetCell(swipingFrom).IsGettingFilled||_board.GetCell(swipingTo).IsGettingEmptied||_board.GetCell(swipingFrom).IsGettingEmptied||_board.GetCell(swipingTo).IsLocked||_board.GetCell(swipingFrom).IsLocked)
            {
                Debug.Log("Pos is not movable");
                return;
            }
          
            _swipedItemPos = swipingFrom;
            _swipeTargetPos = swipingTo;
            _isSwipedThisFrame = true;
           
            
        }
        private void OnClick(Vector2 clickPos)
        {
            Vector2Int firstCellPos = LevelGrid.Instance.WorldToCellVector2Int(clickPos);
            if (!IsInBoundaries(firstCellPos)) return;
            Debug.Log("Click"+_board.GetCell(firstCellPos).HasItem+" "+_board.GetCell(firstCellPos).IsLocked+" "+
                      _board.GetCell(firstCellPos).IsGettingFilled+" "+_board.GetCell(firstCellPos).IsGettingEmptied);
            if(!_board.GetCell(firstCellPos).HasItem) return;
            if (_board.GetCell(firstCellPos).HasItem)
            {
                _board.GetItem(firstCellPos).OnClick(_board,firstCellPos);
                
            }
            if(_board.GetCell(firstCellPos).HasItem)
                _board.GetItem(firstCellPos).OnTouch();;
                        
        }
        private void OnTouch(Vector2 touchPos)
        {
            Vector2Int firstCellPos = LevelGrid.Instance.WorldToCellVector2Int(touchPos);
            if (IsInBoundaries(firstCellPos)&&_board.GetCell(firstCellPos).HasItem)
            {
                _board.GetItem(firstCellPos).OnTouch();
            }
        }
        private bool IsInBoundaries(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
        }


    }
}