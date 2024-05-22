using _Scripts.Utility;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.DownWardItems
{
    //TODO: Implement DownWardItemAction 
    public class DownWardItemAction: IItemAction
    {
        private Vector2Int _pos;
        private Vector3 _goalUIPos;
        private readonly Vector3 _moveDownOffset = new(0, 0.1f, 0);
        private GameObject _particleEffect;
        public bool IsFinished => _isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        private bool _isFinished;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            _pos = pos;
            Board = board;
            _goalUIPos = InGameUIManager.Instance.GetGoalPosition(board.GetItem(pos).ItemID);
            _particleEffect =
                ObjectPool.Instance.GetItemParticleEffect(Board.GetItem(_pos).ItemID,
                    Board.GetItem(_pos).Transform.position);
            _particleEffect.transform.DOMove(_particleEffect.transform.position - _moveDownOffset, 0.3f)
                .SetUpdate(UpdateType.Fixed);
            _particleEffect.transform.DOScale(Vector3.one, 0.3f).SetUpdate(UpdateType.Fixed).OnComplete(() =>
            {
                _particleEffect.transform.DOMove(_goalUIPos, 0.7f).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                {
                    _particleEffect.transform.DOScale(Vector3.zero, 0.15f).SetUpdate(UpdateType.Fixed)
                        .OnComplete(() =>
                        {
                            EventManager.Instance.Broadcast(GameEvents.OnItemExplosion, _pos, Board.GetItem(_pos).ItemID);
                            _isFinished = true;
                            _particleEffect = null;
                        });
                });
            });
        }

        public void Execute()
        {
        }
    }
}