using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using Scripts.BoosterActions;
using UnityEngine;

namespace _Scripts.Items.ItemActions.MergeActions
{
    public class LightBallLightBallMergeAction :ExplodeAction, IItemMergeAction
    {

        public bool IsFinished => _isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public int Item1ID { get; set; }
        public int Item2ID { get; set; }

        private const float Delay = 2.5f;
        private const float ParticleEffectDuration = 0.4f;
        private float _counter;
        private bool _isTntInitialized;
        private Vector2Int _position;
        private GameObject _lightBallExplosionEffect;
        private LightBallLightBallMergeParticleEffect _lightBallLightBallMergeParticleEffect;
        private AudioSource _audioSource;

        public void InitializeMergeAction(Board board, int item1ID, int item2ID, Vector2Int position1, Vector2Int position2)
        {
            _board = board;
            _position = position2;
            board.Cells[position2.x,position2.y].SetIsLocked(true);
            _audioSource=AudioManager.Instance.PlaySFX(SFXClips.MergeRotatingSound,true);
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
            _counter = 0.0f;
            _lightBallExplosionEffect =
                ObjectPool.Instance.lightBallLightBallEffect;
            _lightBallExplosionEffect.transform.position = LevelGrid.Instance.GetCellCenterWorld(_position);
            _lightBallExplosionEffect.SetActive(true);
            _lightBallLightBallMergeParticleEffect = _lightBallExplosionEffect.GetComponent<LightBallLightBallMergeParticleEffect>();
            _lightBallLightBallMergeParticleEffect.InitializeEffect(ParticleEffectDuration);
            _isTntInitialized = false;
            _isFinished = false;
        }

        public void Execute()
        {
            if (_counter < Delay)
            {
                _counter += Time.fixedDeltaTime;
                return;
            }
            if (!_isTntInitialized)
            {
                InitializeExplode(_board,10,_position);
                _audioSource.Stop();
                _isTntInitialized = true;
            }
            HandleExplosion();
        }
    }
}
