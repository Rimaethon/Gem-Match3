using _Scripts.Managers;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts.BoosterActions;
using UnityEngine;

namespace Scripts.Generator_Items
{
    public class GeneratorItem:BoardItemBase
    {
        private bool _isJumping;
        private Vector3 _jumpPos = new Vector3(0, 0.1f, 0);
        protected override void Awake()
        {
            base.Awake();
            isFallAble = false;
            _isMatchable = false;
            _isSwappable = false;
            _isExplodeAbleByNearMatches = true;
            _isGeneratorItem = true;
        }
        public override void OnExplode()
        { 
            if(LevelManager.Instance.IsGoalReached(_itemID))
                return;
            DoJumpAndShake();
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, _position,_itemID,0);
        }
        private void DoJumpAndShake()
        {
            if (_isJumping)
                return;
            _isJumping = true;
            transform.DOPunchPosition(_jumpPos, 0.15f, 0, 0.2f).SetUpdate(UpdateType.Fixed);
            transform.DOShakeScale(0.2f, 0.2f, 0, 90).SetUpdate(UpdateType.Fixed).OnComplete(() =>
            {
                _isJumping = false;
            });
        }
        public override void OnClick(Board board, Vector2Int pos)
        {
            if (IsMoving || _isExploding||_isClicked|| IsMatching)
                return;
            _isClicked = true;
            transform.DOPunchRotation(new Vector3(0, 0, 20), 0.2f).SetUpdate(UpdateType.Fixed).
                OnComplete(() =>
                {
                    _isClicked = false;
                });            
            
        }
    }
}