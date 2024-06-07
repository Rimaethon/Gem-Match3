using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Core
{
    public class InputHandler
    {
        private Vector2Int _swipedItemPos;
        private Vector2Int _swipeTargetPos;
        private bool _isSwipedThisFrame;
        private  Board _board;
        private  MatchChecker _matchChecker;
        private bool _isSwiping;
        private bool _isDisabled;

        public InputHandler(Board board,MatchChecker matchChecker)
        {
            _board = board;
            _matchChecker = matchChecker;
            
            EventManager.Instance.AddHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnTouch, OnTouch);
        }
        public void OnDisable()
        {
            _isDisabled = true;
            _board = null;
            _matchChecker = null;
            _isSwipedThisFrame = false;
            _isClickedThisFrame = false;
            if (EventManager.Instance == null)
            {
                Debug.Log("Event Manager is null");
                return;
            }
            EventManager.Instance.RemoveHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnTouch, OnTouch);
            
        }
        public void HandleInputs()
        {
            if (_isDisabled)
            {
                return;
            }
            if (_isClickedThisFrame&&!_isSwiping)
            {
                HandleClick();
            }
           
            if (!_isSwipedThisFrame||_isSwiping)
            {
                return;
            }
            if (!_board.IsInBoundaries(_swipedItemPos) ||!_board.IsInBoundaries(_swipeTargetPos) ||
                !_board.Cells[_swipedItemPos.x,_swipedItemPos.y].HasItem || !_board.Cells[_swipeTargetPos.x,_swipeTargetPos.y].HasItem)
            {
                _isSwipedThisFrame = false;
                return;
            }
            if (!_board.GetItem(_swipedItemPos).IsSwappable || !_board.GetItem(_swipeTargetPos).IsSwappable||_board.GetItem(_swipeTargetPos).IsMoving||_board.GetItem(_swipedItemPos).IsMoving||_board.GetItem(_swipeTargetPos).IsSwapping||_board.GetItem(_swipedItemPos).IsSwapping||_board.GetItem(_swipeTargetPos).IsExploding||_board.GetItem(_swipedItemPos).IsExploding||_board.Cells[_swipeTargetPos.x, _swipeTargetPos.y].IsGettingFilled
                ||_board.Cells[_swipedItemPos.x, _swipedItemPos.y].IsGettingFilled||_board.Cells[_swipeTargetPos.x, _swipeTargetPos.y].IsGettingEmptied||_board.Cells[_swipedItemPos.x, _swipedItemPos.y].IsGettingEmptied||_board.Cells[_swipeTargetPos.x, _swipeTargetPos.y].IsLocked||_board.Cells[_swipedItemPos.x, _swipedItemPos.y].IsLocked)
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
                await MoveItemAsync(item1.Transform, LevelGrid.Instance.GetCellCenterLocalVector2(pos2), 0.2f);
                item1.IsActive = false;
                item2.IsActive = false;
                item1.IsSwapping = false;
                item2.IsSwapping = false;
                _isSwiping = false;
                _isSwipedThisFrame = false;
                //SMALL BUT DOES A LOT
                EventManager.Instance.Broadcast(GameEvents.AddMergeActionToHandle, pos1, pos2);
                item1.OnRemove();
                item2.OnRemove();
                return;
            }
            
            await UniTask.WhenAll(MoveItemAsync(item1.Transform,LevelGrid.Instance.GetCellCenterLocalVector2(pos2),0.2f),MoveItemAsync(item2.Transform,LevelGrid.Instance.GetCellCenterLocalVector2(pos1),0.2f));
            SwapItems(pos1,pos2);
            if ((item1.IsBooster && _board.Cells[pos2.x,pos2.y].HasItem )|| (item2.IsBooster&& _board.Cells[pos1.x,pos1.y].HasItem))
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
                    AudioManager.Instance.PlaySFX(SFXClips.SwapBackWardSound);
                    await UniTask.WhenAll(MoveItemAsync(item1.Transform,LevelGrid.Instance.GetCellCenterLocalVector2(pos1),0.2f),MoveItemAsync(item2.Transform,LevelGrid.Instance.GetCellCenterLocalVector2(pos2),0.2f));
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
        private async UniTask MoveItemAsync(Transform itemTransform, Vector3 targetPos, float duration)
        {
            float elapsedTime = 0;

            Vector3 startingPos = itemTransform.localPosition;

            while (elapsedTime < duration)
            {
                itemTransform.localPosition = Vector3.Lerp(startingPos, targetPos, elapsedTime / duration);
                elapsedTime += Time.fixedDeltaTime;
                await UniTask.Yield();
            }

            itemTransform.localPosition = targetPos;
        }
        
        private void SwapItems(Vector2Int pos1, Vector2Int pos2)
        {
            IBoardItem temp = _board.GetItem(pos1);
            _board.Cells[pos1.x,pos1.y].SetItem(_board.GetItem(pos2));
            _board.Cells[pos2.x,pos2.y].SetItem(temp);
        }
        private  void OnSwipe(Vector2Int direction, Vector2 clickPos)
        {
            if (_isDisabled)
            {
                return;
            }
            if (_isSwipedThisFrame)
            {
                return;
            }
            Vector2Int swipingFrom= LevelGrid.Instance.WorldToCellVector2Int(clickPos);
            Vector2Int swipingTo= swipingFrom + direction;
            if(!_board.IsInBoundaries(swipingFrom)||!_board.IsInBoundaries(swipingTo)||_board.GetItem(swipingFrom)==null||_board.GetItem(swipingTo)==null)
                return;
            if (!_board.GetItem(swipingFrom).IsSwappable || !_board.GetItem(swipingTo).IsSwappable||_board.GetItem(swipingTo).IsMoving||_board.GetItem(swipingFrom).IsMoving||_board.GetItem(swipingTo).IsSwapping||_board.GetItem(swipingFrom).IsSwapping||_board.GetItem(swipingTo).IsExploding||_board.GetItem(swipingFrom).IsExploding||_board.Cells[swipingTo.x,swipingTo.y].IsGettingFilled
                ||_board.Cells[swipingFrom.x,swipingFrom.y].IsGettingFilled||_board.Cells[swipingTo.x,swipingTo.y].IsGettingEmptied||_board.Cells[swipingFrom.x,swipingFrom.y].IsGettingEmptied||_board.Cells[swipingTo.x,swipingTo.y].IsLocked||_board.Cells[swipingFrom.x,swipingFrom.y].IsLocked||!_board.GetItem(swipingFrom).IsActive||!_board.GetItem(swipingTo).IsActive)
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
            if(!_board.Cells[_clickPos.x,_clickPos.y].HasItem) return;
            if (_board.Cells[_clickPos.x,_clickPos.y].HasItem)
            {
                _board.GetItem(_clickPos).OnClick(_board,_clickPos);
                if (_board.GetItem(_clickPos).IsBooster)
                {
                    EventManager.Instance.Broadcast<int>(GameEvents.OnMoveCountChanged,-1);
                }
                
            }
            if(_board.Cells[_clickPos.x,_clickPos.y].HasItem)
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
            if (_isDisabled)
            {
                return;
            }
            Vector2Int firstCellPos = LevelGrid.Instance.WorldToCellVector2Int(touchPos);
            if (_board.IsInBoundaries(firstCellPos)&&_board.Cells[firstCellPos.x,firstCellPos.y].HasItem)
            {
                _board.GetItem(firstCellPos).OnTouch();
            }
        }

    }
}