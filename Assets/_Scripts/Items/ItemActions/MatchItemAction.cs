using UnityEngine;

namespace Scripts.BoosterActions
{
    public class MatchItemAction:IItemAction
    {
        public bool IsFinished=>_isFinished;
        private IItem _item;
        private readonly Board _board;
        private bool _isFinished;
        private bool _isMatch;
        private float _counter = 0.0f;
        private readonly float _scaleDownTime;
        public MatchItemAction(Board board,IItem item,float scaleDownTime, bool isMatch)
        {
            _item = item;
            _board = board;
            _scaleDownTime = scaleDownTime;
            _isMatch = isMatch;
        }
        public void Execute()
        {
            if (_isMatch)
            {
                while(_counter < _scaleDownTime)
                {
                    _counter += Time.deltaTime;
                    _item.Transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, _counter / _scaleDownTime);
                    return;
                }
                ObjectPool.Instance.GetItemParticleEffect(_item.ItemID, _item.Transform.position);
                ObjectPool.Instance.GetMatchEffect(_item.Transform.position);
                _item.OnRemove();
                
            }
            else
            {
                ObjectPool.Instance.GetItemParticleEffect(_item.ItemID, _item.Transform.position);
                _item.OnRemove();
            }
            _isFinished = true;
        }

    }
}