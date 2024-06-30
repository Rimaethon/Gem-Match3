using _Scripts.Utility;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public enum MatchState
    {
        ScaleDown,
        Explode
    }

    public class MatchItemAction : IItemAction
    {
        private IBoardItem _boardItem;
        private Vector2Int _pos;
        private float _explodeTime=0.15f;
        private float _scaleDownTime=0.15f;
        private float _counter;
        private bool _isExplodeInitiated;
        private MatchState _state=MatchState.Explode;
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            _state = value2==0 ? MatchState.ScaleDown : MatchState.Explode;
            _boardItem = board.GetItem(pos);
            ItemID = _boardItem.ItemID;
            Board = board;
            _pos = pos;
            IsFinished = false;
            _counter = 0;
            _isExplodeInitiated = false;
        }

        public void Execute()
        {
            switch (_state)
            {
                case MatchState.ScaleDown:
                    ScaleDown();
                    break;
                case MatchState.Explode:
                    Explode();
                    break;
            }
        }


        private void ScaleDown()
        {
            if (_counter < _scaleDownTime)
            {
                _counter += Time.deltaTime;
                _boardItem.Transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, _counter / _scaleDownTime);
                return;
            }

            _counter = 0;
            _state = MatchState.Explode;
        }

        private void Explode()
        {
            if (!_isExplodeInitiated)
            {
               ObjectPool.Instance.GetItemParticleEffect(ItemID, LevelGrid.Instance.GetCellCenterWorld(_pos));
                _isExplodeInitiated = true;
                Board.Cells[_pos.x,_pos.y].SetIsLocked(true);
                _boardItem.Transform.gameObject.SetActive(false);
            }

            if (_counter < _explodeTime)
            {
                _counter += Time.deltaTime;
                return;
            }
            Board.Cells[_pos.x,_pos.y].SetIsLocked(false);
            _boardItem.OnRemove();
            IsFinished = true;
        }


    }
}
