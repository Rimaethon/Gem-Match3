using UnityEngine;

namespace Scripts
{
    public class BoosterBoardItem:BoardItemBase
    {
        private void OnEnable()
        {
            Transform.localScale = Vector3.one;
            _isBooster = true;
            _isSwappable = true;
            _isMatchable = false;
            isFallAble = true;
            IsActive = true;
            _isExploding = false;
            _isClicked = false;
            _isHighlightAble = true;
            IsMoving = false;
            IsMatching = false;
            _isShuffleAble = true;
            Highlight(0);
            ObjectPool.Instance.GetBoosterCreationEffect(transform.position).transform.SetParent(transform);
        }
    }
}