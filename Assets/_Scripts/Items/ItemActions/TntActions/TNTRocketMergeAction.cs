using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions.MergeActions
{
    public class TNTRocketMergeAction : IItemMergeAction
    {
        private const float Delay = 1f;
        private const float ParticleEffectDuration = 0.2f;
        private float _counter;
        private GameObject _tntRocketExplosionEffect;
        private TntRocketMergeParticleEffect _tntRocketMergeParticleEffect;
        private Vector2Int _position1;
        private Board _board;
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }
        Vector2Int _position2;
        private bool _isActionsInitialized;
        public int Item1ID { get; set; }
        public int Item2ID { get; set; }
        private AudioSource _audioSource;
        public void InitializeMergeAction(Board board, int item1ID, int item2ID, Vector2Int position1, Vector2Int position2)
        {
            _position1 = position1;
            _position2 = position2;
            _board=board;
            _tntRocketExplosionEffect = ObjectPool.Instance.GetTntRocketMergeParticleEffect(LevelGrid.Instance.GetCellCenterWorld(_position2));
            _tntRocketMergeParticleEffect = _tntRocketExplosionEffect.GetComponent<TntRocketMergeParticleEffect>();
            _audioSource=AudioManager.Instance.PlaySFX(SFXClips.MergeRotatingSound,true);
            _tntRocketMergeParticleEffect.InitializeEffect(ParticleEffectDuration);
            _board.Cells[_position1.x,_position1.y].SetIsLocked(true);
            _board.Cells[_position2.x,_position2.y].SetIsLocked(true);
            _counter = 0.0f;
        }
        private void InitializeRockets()
        {
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position2,101,-1);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position2,102,-1);

            if (_board.IsInBoundaries(_position2.x, _position2.y + 1))
            {

                EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, new Vector2Int(_position2.x, _position2.y + 1),101,-1);

            }
            if (_board.IsInBoundaries(_position2.x, _position2.y - 1))
            {

                EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, new Vector2Int(_position2.x, _position2.y - 1),101,-1);

            }
            if (_board.IsInBoundaries(_position2.x+1, _position2.y))
            {

                EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, new Vector2Int(_position2.x+1, _position2.y ),102,-1);

            }
            if (_board.IsInBoundaries(_position2.x-1, _position2.y ))
            {

                EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, new Vector2Int(_position2.x-1, _position2.y ),102,-1);

            }

        }


        public void Execute()
        {
            if (_counter < Delay)
            {
                _counter += Time.fixedDeltaTime;
                return;
            }
            if (!_isActionsInitialized)
            {
                _audioSource.Stop();
                 InitializeRockets();
                _board.Cells[_position1.x,_position1.y].SetIsLocked(false);
                _board.Cells[_position2.x,_position2.y].SetIsLocked(false);
                _isActionsInitialized = true;
            }
            IsFinished = true;

        }

    }
}
