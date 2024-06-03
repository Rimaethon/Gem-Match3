using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using Scripts.BoosterActions;
using UnityEngine;

namespace _Scripts.Items.ItemActions.MergeActions
{
    public class TntTntMergeAction : ExplodeAction, IItemMergeAction
    {
        private const float Delay = 0.55f;
        private float _counter;
        private bool _isTntInitialized;
        private Vector2Int _position;
        public bool IsFinished=>_isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public int Item1ID { get; set; }
        public int Item2ID { get; set; }

        private GameObject _tntExplosionEffect;
        public void InitializeMergeAction(Board board, int item1ID, int item2ID, Vector2Int position1,
            Vector2Int position2)
        {
            _board = board;
            _position = position2;
            board.Cells[_position.x, _position.y].SetIsLocked(true);
            _counter = 0.0f;
            _tntExplosionEffect =
                ObjectPool.Instance.tntTntEffect;
            _tntExplosionEffect.transform.position = LevelGrid.Instance.GetCellCenterWorld(_position);
            _tntExplosionEffect.SetActive(true);
            
            _tntExplosionEffect.GetComponent<TntTntMergeParticleEffect>().PlayExplosionEffect();
            _isTntInitialized = false;
            _isFinished = false;
        }




        public void Execute()
        {
            if (_counter < Delay)
            {
                _counter += Time.fixedDeltaTime;
            }
            else
            {
                if (!_isTntInitialized)
                {
                    AudioManager.Instance.PlaySFX(SFXClips.TntTntMergeExplosionSound);
                    InitializeExplode(_board,4,_position);
                    _isTntInitialized = true;
                }
                HandleExplosion();
                if (_isFinished)
                {
                    _board.Cells[_position.x,_position.y].SetIsLocked(false);
                    _tntExplosionEffect.SetActive(false);
                    _tntExplosionEffect = null;
                }
               
            }
        }

    }
}