using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Core
{
    public class InputHandler
    {
        private Vector2Int _swipedItemPos;
        private Vector2Int _swipeTargetPos;
        private bool _isSwipedThisFrame;
        private  Board _board;
        private readonly MatchChecker _matchChecker;
        private bool _isSwiping;
        private MergeHandler _mergeHandler;

        public InputHandler(Board board,MatchChecker matchChecker,MergeHandler mergeHandler)
        {
            _board = board;
            _matchChecker = matchChecker;
            _mergeHandler= mergeHandler;
            EventManager.Instance.AddHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnTouch, OnTouch);
        }
      
        public void HandleInputs()
        {
            if (_isClickedThisFrame)
            {
                HandleClick();
            }
            if (!_isSwipedThisFrame||_isSwiping)
            {
                return;
            }

            if (!_board.IsInBoundaries(_swipedItemPos) ||!_board.IsInBoundaries(_swipeTargetPos) ||
                !_board.GetCell(_swipedItemPos).HasItem || !_board.GetCell(_swipeTargetPos).HasItem)
            {
                _isSwipedThisFrame = false;
                return;
            }
            if (!_board.GetItem(_swipedItemPos).IsSwappable || !_board.GetItem(_swipeTargetPos).IsSwappable||_board.GetItem(_swipeTargetPos).IsMoving||_board.GetItem(_swipedItemPos).IsMoving||_board.GetItem(_swipeTargetPos).IsSwapping||_board.GetItem(_swipedItemPos).IsSwapping||_board.GetItem(_swipeTargetPos).IsExploding||_board.GetItem(_swipedItemPos).IsExploding||_board.GetCell(_swipeTargetPos).IsGettingFilled
                ||_board.GetCell(_swipedItemPos).IsGettingFilled||_board.GetCell(_swipeTargetPos).IsGettingEmptied||_board.GetCell(_swipedItemPos).IsGettingEmptied||_board.GetCell(_swipeTargetPos).IsLocked||_board.GetCell(_swipedItemPos).IsLocked)
            {
                _isSwipedThisFrame = false;
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
            AudioManager.Instance.PlaySFX(SFXClips.SwapForward);

            if(item1.IsBooster&&item2.IsBooster)
            {
                var itemTween = item1.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos2), 0.2f);
                await UniTask.WhenAll(itemTween.ToUniTask());
                item1.IsActive = false;
                item2.IsActive = false;
                item1.IsSwapping = false;
                item2.IsSwapping = false;
                _isSwiping = false;
                _isSwipedThisFrame = false;
                _mergeHandler.MergeItems(item1,item2);
                return;
            }
            var item1Tween = item1.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos2), 0.2f);
            var item2Tween = item2.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos1), 0.2f);
            await UniTask.WhenAll(item1Tween.ToUniTask(), item2Tween.ToUniTask());
            SwapItems(pos1,pos2);
            if (item1.IsBooster || item2.IsBooster)
            {
                _matchChecker.CheckMatch(!item1.IsBooster ? pos2 : pos1);
                item1.OnSwap(item1,item2);
               item2.OnSwap(item2,item1);
               EventManager.Instance.Broadcast<int>(GameEvents.OnMoveCountChanged,-1);

            }
            else
            {
                bool swap1 = _matchChecker.CheckMatch(pos1);
                bool swap2 = _matchChecker.CheckMatch(pos2);
                if (!swap1 && !swap2)
                {
                    item1Tween = item1.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos1), 0.2f);
                    item2Tween = item2.Transform.DOLocalMove(LevelGrid.Instance.GetCellCenterLocalVector2(pos2), 0.2f);
                    AudioManager.Instance.PlaySFX(SFXClips.SwapBackWardSound);
                    await UniTask.WhenAll(item1Tween.ToUniTask(), item2Tween.ToUniTask());
                    SwapItems(pos1,pos2);
                }
                else
                {
                    EventManager.Instance.Broadcast<int>(GameEvents.OnMoveCountChanged,-1);
                }
            }
           
            item1.IsSwapping = false;
            item2.IsSwapping = false;
            _isSwiping = false;
            _isSwipedThisFrame = false;

        }
        private void SwapItems(Vector2Int pos1, Vector2Int pos2)
        {
            IBoardItem temp = _board.GetItem(pos1);
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
            if(!_board.IsInBoundaries(swipingFrom)||!_board.IsInBoundaries(swipingTo)||_board.GetItem(swipingFrom)==null||_board.GetItem(swipingTo)==null)
                return;
            if (!_board.GetItem(swipingFrom).IsSwappable || !_board.GetItem(swipingTo).IsSwappable||_board.GetItem(swipingTo).IsMoving||_board.GetItem(swipingFrom).IsMoving||_board.GetItem(swipingTo).IsSwapping||_board.GetItem(swipingFrom).IsSwapping||_board.GetItem(swipingTo).IsExploding||_board.GetItem(swipingFrom).IsExploding||_board.GetCell(swipingTo).IsGettingFilled
                ||_board.GetCell(swipingFrom).IsGettingFilled||_board.GetCell(swipingTo).IsGettingEmptied||_board.GetCell(swipingFrom).IsGettingEmptied||_board.GetCell(swipingTo).IsLocked||_board.GetCell(swipingFrom).IsLocked||!_board.GetItem(swipingFrom).IsActive||!_board.GetItem(swipingTo).IsActive)
            {
                return;
            }
            _swipedItemPos = swipingFrom;
            _swipeTargetPos = swipingTo;
            _isSwipedThisFrame = true;
        }
        private bool _isClickedThisFrame;
        private Vector2Int _clickPos;
        private void HandleClick()
        {
            if (!_isClickedThisFrame)
            {
                return;
            }
            _isClickedThisFrame = false;
            if (!_board.IsInBoundaries(_clickPos)) return;
            if(!_board.GetCell(_clickPos).HasItem) return;
            if (_board.GetCell(_clickPos).HasItem)
            {
                _board.GetItem(_clickPos).OnClick(_board,_clickPos);
                if (_board.GetItem(_clickPos).IsBooster)
                {
                    EventManager.Instance.Broadcast<int>(GameEvents.OnMoveCountChanged,-1);
                }
                
            }
            if(_board.GetCell(_clickPos).HasItem)
                _board.GetItem(_clickPos).OnTouch();;
        }
        private void OnClick(Vector2 clickPosition)
        {
            if (_isClickedThisFrame)
            {
                return;
            }
            
            _clickPos= LevelGrid.Instance.WorldToCellVector2Int(clickPosition);
            _isClickedThisFrame = true;
                        
        }
        private void OnTouch(Vector2 touchPos)
        {
            Vector2Int firstCellPos = LevelGrid.Instance.WorldToCellVector2Int(touchPos);
            if (_board.IsInBoundaries(firstCellPos)&&_board.GetCell(firstCellPos).HasItem)
            {
                _board.GetItem(firstCellPos).OnTouch();
            }
        }

    }
}