using System.Collections.Generic;
using System.Linq;
using _Scripts.Managers;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    //Make LightBall,it would be fun they said.
    //https://youtu.be/juvV9BRF3hI?t=217 as can be seen here when  LB-LB merge destroys everything the other Lightballs get destroyed because there is no item to match.
    public class LightBallBoosterAction : IItemAction
    {
        private const float ExplosionTime = 0.3f;
        public bool IsFinished { get; set; }

        private readonly Board _board;
        private readonly int _boosterID;
        public readonly HashSet<Vector2Int> ItemsToExplode=new HashSet<Vector2Int>();
        private readonly float _lerpTime;
        private readonly int _lightBallID;
        private readonly Vector3 _lightBallPosition;
        private readonly int _matchingItemID;
        private readonly float _noTargetThreshold;
        private readonly Vector2Int _position;
        private readonly bool _shouldSpawnBooster;
        private bool _isBoosterSpawned;
        private float _finishCounter;
        private GameObject _lightBallParticleEffectInstance;
        private GameObject _lightBallExplosionParticles;
        private GameObject _chargingParticleEffects;
        private GameObject _lightBallSprite;
        private List<LightBallRay> _targetedRays=new List<LightBallRay>();
        private List<LightBallRay> _unTargetedRays=new List<LightBallRay>();
        private int _targetCount;
        private int _initialWaitForFrames;
        private AudioSource _audioSource;
        public float _noTargetTime=0.2f;
        private float _noTargetCounter;
        private Queue<Vector2Int> _rayDestinationQueue = new Queue<Vector2Int>();
        private float searchWaitTime = 0.1f;
        private float searchWaitCounter = 0.1f;
        private bool isExplosionInitiated ;

        public LightBallBoosterAction(Board board, int lightBallID, int matchingItemID, Vector2Int pos,
            float noTargetThreshold, float lerpTime, bool shouldSpawnBooster = false, int boosterID = -1)
        {
            _board = board;
            _lightBallID = lightBallID;
            _matchingItemID = matchingItemID;
            _position = pos;
            _noTargetThreshold = noTargetThreshold;
            _lerpTime = lerpTime;
            _shouldSpawnBooster = shouldSpawnBooster;
            _boosterID = boosterID;
            _targetCount = 0;
            _initialWaitForFrames = 10;
            _finishCounter = 0;
            _isBoosterSpawned= false;
            _lightBallPosition = LevelGrid.Instance.GetCellCenterWorld(pos);
        }
        public void InitializeAction()
        {
          _audioSource=AudioManager.Instance.PlaySFX(SFXClips.LightBallPoweringEffect,true);
            SetupParticleEffects();
            _board.GetCell(_position).SetIsLocked(true);
            _lightBallSprite.SetActive(true);
            _chargingParticleEffects.SetActive(true);
            HighlightLightBall(true);
            Search();
            foreach (var ray in _unTargetedRays)
            {
                ray._lightBallBoosterAction= this;
            }
        }
        private void SetupParticleEffects()
        {
            _lightBallParticleEffectInstance = ObjectPool.Instance.GetBoosterParticleEffect(_lightBallID, _lightBallPosition);
            EventManager.Instance.Broadcast(GameEvents.OnBoardLock);
            _lightBallExplosionParticles =
                _lightBallParticleEffectInstance.GetComponent<LightBallParticleEffect>().lightBallExplosionParticle;
            _chargingParticleEffects =_lightBallParticleEffectInstance.GetComponent<LightBallParticleEffect>().chargingParticleEffects;
            _lightBallSprite = _lightBallParticleEffectInstance.GetComponent<LightBallParticleEffect>().lightBallSprite;
            _unTargetedRays = _lightBallParticleEffectInstance.GetComponent<LightBallParticleEffect>().lightBallRays;
            _chargingParticleEffects.SetActive(true);
        }
        private void HighlightLightBall(bool highlight)
        {
            var materialPropertyBlock = new MaterialPropertyBlock();
            _lightBallSprite.GetComponent<SpriteRenderer>().GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_IsOutlineEnabled", highlight ? 1 : 0);
            _lightBallSprite.GetComponent<SpriteRenderer>().SetPropertyBlock(materialPropertyBlock);
        }
        private void TryGiveTargetToRays()
        {
            if (searchWaitCounter < searchWaitTime)
            {
                searchWaitCounter += Time.fixedDeltaTime;
                return;
            }
            if (_rayDestinationQueue.Count == 0||_unTargetedRays.Count==0)
            {
                return;
            }
            searchWaitCounter = 0;
            var target = _rayDestinationQueue.Dequeue();
            _unTargetedRays[^1].SetTarget(_board,_shouldSpawnBooster,_boosterID,target);
            _targetedRays.Add(_unTargetedRays[^1]);
            _unTargetedRays.RemoveAt(_unTargetedRays.Count - 1);
        }
        public void Execute()
        {
            Debug.Log("LightBallBoosterAction: Execute"+_initialWaitForFrames);
            if(_initialWaitForFrames>0)
            {
                _initialWaitForFrames--;
                return;
            }
            TryGiveTargetToRays();
            AnimateAndSearchRays();
            
            if (_rayDestinationQueue.Count == 0 && _targetedRays.Count == 0)
            {
                if(_shouldSpawnBooster)
                {
                    FinishAction();
                }
                else if (_noTargetCounter < _noTargetTime)
                {
                    _noTargetCounter += Time.fixedDeltaTime;
                    Search();
                }
                else
                {
                    FinishAction();
                }
            }
        }
        private void AnimateAndSearchRays()
        {
            Debug.Log("Animating Ray"+_targetedRays.Count);
            foreach (var ray in _targetedRays)
            {
                Debug.Log("Animating Ray"+ray.hasTarget);
                if (ray.hasTarget)
                {
                    ray.AnimateRay();
                }
                else
                {
                    Debug.Log("Ray has no target");
                    _unTargetedRays.Add(ray);
                }
            }
            foreach (var ray in _unTargetedRays)
            {
                _targetedRays.Remove(ray);
            }
        }
        private void Search()
        {
            if (_shouldSpawnBooster)
            {
                HashSet<Vector2Int> randomPositions = LevelManager.Instance.GetRandomSpawnablePos(10);
                Debug.Log("RandomPositions"+randomPositions.Count);
                foreach (var position in randomPositions)
                {
                    Debug.Log("RandomPosition"+position);
                    _rayDestinationQueue.Enqueue(position);
                }
                ItemsToExplode.UnionWith(randomPositions);
            }
            else
            {
                TryGetItemPositionForRay(_matchingItemID);
            }
        }
        private void FinishAction()
        {
            if (!isExplosionInitiated)
            {
                if(_audioSource!=null) 
                    _audioSource.Stop();
                AudioManager.Instance.PlaySFX(SFXClips.MatchSound);
                _chargingParticleEffects.SetActive(false);
                _lightBallSprite.SetActive(false);
                _chargingParticleEffects.SetActive(false);
                HighlightLightBall(false);
                _lightBallExplosionParticles.SetActive(true);
                _lightBallExplosionParticles.GetComponent<ParticleSystem>().Play(true);
                isExplosionInitiated = true;
            }
            if (_shouldSpawnBooster&&!_isBoosterSpawned)
            {
                IBoardItem boardItem = ObjectPool.Instance.GetBoosterItem(_boosterID, LevelGrid.Instance.GetCellCenterWorld(_position), _board);
                boardItem.Transform.parent = _board._boardInstance.transform;
                if (_board.GetCell(_position).HasItem)
                {
                    ObjectPool.Instance.ReturnItem(_board.GetItem(_position), _board.GetItem(_position).ItemID);
                }
                _board.GetCell(_position).SetItem(boardItem);
                ItemsToExplode.Add(_position);
                _isBoosterSpawned = true;
            }
            if (_finishCounter < ExplosionTime)
            {
                _finishCounter += Time.fixedDeltaTime;
                return;
            }
            UnlockBoardAndExplodeItems();
            ReturnParticleEffect();
            _lightBallParticleEffectInstance= null;
            _lightBallExplosionParticles = null;
            _chargingParticleEffects = null;
            _lightBallSprite = null;
            _board.GetCell(_position).SetIsLocked(false);
            IsFinished = true;
            _noTargetCounter = 0;
            if (!_shouldSpawnBooster) LevelManager.Instance.ItemsGettingMatchedByLightBall.Remove(_matchingItemID);
            
        }

        private void UnlockBoardAndExplodeItems()
        {
            Debug.Log("Unlocking Board and Exploding Items"+ItemsToExplode.Count);
            foreach (var itemPosition in ItemsToExplode)
            {
                if (!_board.GetCell(itemPosition).HasItem) continue;
                _board.GetItem(itemPosition).IsActive=true;
                _board.GetItem(itemPosition).OnExplode();
                _board.GetCell(itemPosition).SetIsLocked(false);
                Debug.Log("Exploding"+itemPosition);
                if (!_shouldSpawnBooster)
                { 
                    ExplodeAllDirections(itemPosition);
                }

            }
        }
        private void ReturnParticleEffect()
        {
            _lightBallExplosionParticles.SetActive(false);
            _targetedRays.ForEach(ray => ray.ResetRay());
            _lightBallParticleEffectInstance.SetActive(false);
            ObjectPool.Instance.ReturnBoosterParticleEffect(_lightBallParticleEffectInstance,_lightBallID);
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
        }
        private void TryGetItemPositionForRay(int idToMatch)
        {
            for (var x = 0; x < _board.Width; x++)
            {
                for (var y = 0; y < _board.Height; y++)
                {
                    var cell = _board.GetCell(x, y);
                    if (!cell.HasItem)
                        continue;
                    if (cell.BoardItem.ItemID == idToMatch && !cell.BoardItem.IsExploding && !cell.BoardItem.IsMoving &&
                        !cell.IsLocked && !cell.BoardItem.IsMatching&& !ItemsToExplode.Contains(cell.CellPosition))
                    {
                        cell.BoardItem.IsActive = false;
                        ItemsToExplode.Add(cell.CellPosition);
                        _rayDestinationQueue.Enqueue(cell.CellPosition);
                    }
                }
            }

        }
        private void ExplodeAllDirections(Vector2Int pos)
        {
            foreach (var direction in pos.GetFourDirections())
                if (_board.IsInBoundaries(direction) && _board.GetCell(direction).HasItem &&
                    !_board.GetItem(direction).IsExploding && !_board.GetItem(direction).IsMatching &&
                    _board.GetItem(direction).IsExplodeAbleByNearMatches)
                    _board.GetItem(direction).OnExplode();
        }
    }
}