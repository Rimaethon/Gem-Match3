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
        public int ItemID { get; set; }
        public Board Board { get; set; }
        private int _boosterID;
        public readonly HashSet<IBoardItem> ItemsToExplode=new HashSet<IBoardItem>();
        private int _matchingItemID;
        private readonly float _noTargetThreshold=0.2f;
        private Vector2Int _position;
        private bool _shouldSpawnBooster;
        private bool _isBoosterSpawned;
        private float _finishCounter;
        private GameObject _lightBallParticleEffectInstance;
        private ParticleSystem _lightBallExplosionParticles;
        private GameObject _chargingParticleEffects;
        private GameObject _lightBallSprite;
        private List<LightBallRay> _targetedRays=new List<LightBallRay>();
        private List<LightBallRay> _unTargetedRays=new List<LightBallRay>();
        private readonly int _targetCount=10;
        private float _initialWaitTime=0.4f;
        private AudioSource _audioSource;
        private float _noTargetCounter;
        private Queue<IBoardItem> _rayDestinationQueue = new Queue<IBoardItem>();
        private const float SearchWaitTime = 0.1f;
        private float _searchWaitCounter = 0.1f;
        private bool _isExplosionInitiated ;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            Board = board;
            _position = pos;
            _initialWaitTime = 15;
            _finishCounter = 0;
            _isBoosterSpawned= false;
            _shouldSpawnBooster = value2>99;
            _initialWaitTime=0.4f;
            _matchingItemID = value2;
            _boosterID = value2;
            IsFinished = false;
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
            _audioSource=AudioManager.Instance.PlaySFX(SFXClips.LightBallPoweringEffect,true);
            SetupParticleEffects();
            Board.Cells[_position.x,_position.y].SetIsLocked(true);
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
            _lightBallParticleEffectInstance = ObjectPool.Instance.GetBoosterParticleEffect(ItemID, LevelGrid.Instance.GetCellCenterWorld(_position));
            _lightBallExplosionParticles =
                _lightBallParticleEffectInstance.GetComponent<LightBallParticleEffect>().lightBallExplosionParticles;
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
            if (_searchWaitCounter < SearchWaitTime)
            {
                _searchWaitCounter += Time.fixedDeltaTime;
                return;
            }
            if (_rayDestinationQueue.Count == 0||_unTargetedRays.Count==0)
            {
                return;
            }
            _searchWaitCounter = 0;
            var target = _rayDestinationQueue.Dequeue();
            _unTargetedRays[^1].SetTarget(Board,_shouldSpawnBooster,_boosterID,target);
            _targetedRays.Add(_unTargetedRays[^1]);
            _unTargetedRays.RemoveAt(_unTargetedRays.Count - 1);
        }


        public void Execute()
        {
            if(_initialWaitTime>0)
            {
                _initialWaitTime -= Time.fixedDeltaTime;
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
                else if (_noTargetCounter < _noTargetThreshold)
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
            foreach (var ray in _targetedRays)
            {
                if (ray.hasTarget)
                {
                    ray.AnimateRay();
                }
                else
                {
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
                HashSet<Vector2Int> randomPositions = LevelManager.Instance.GetRandomSpawnablePos(_targetCount);
                foreach (var position in randomPositions)
                {
                    _rayDestinationQueue.Enqueue(Board.GetItem(position));
                }
            }
            else
            {
                TryGetItemPositionForRay(_matchingItemID);
            }
        }
        private void FinishAction()
        {
            if (!_isExplosionInitiated)
            {
                if(_audioSource!=null) 
                    _audioSource.Stop();
                AudioManager.Instance.PlaySFX(SFXClips.MatchSound);
                _chargingParticleEffects.SetActive(false);
                _lightBallSprite.SetActive(false);
                HighlightLightBall(false);
                _lightBallExplosionParticles.Play(true);
                _isExplosionInitiated = true;
            }
            if (_shouldSpawnBooster&&!_isBoosterSpawned)
            {
                IBoardItem boardItem = ObjectPool.Instance.GetBoosterItem(_boosterID, LevelGrid.Instance.GetCellCenterWorld(_position), Board);
                boardItem.Transform.parent = Board._boardInstance.transform;
                if (Board.Cells[_position.x,_position.y].HasItem)
                {
                    ObjectPool.Instance.ReturnItem(Board.GetItem(_position), Board.GetItem(_position).ItemID);
                }
                Board.Cells[_position.x,_position.y].SetItem(boardItem);
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
            Board.Cells[_position.x,_position.y].SetIsLocked(false);
            IsFinished = true;
            _noTargetCounter = 0;
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
            if (!_shouldSpawnBooster) LevelManager.Instance.ItemsGettingMatchedByLightBall.Remove(_matchingItemID);
            
        }
        private void UnlockBoardAndExplodeItems()
        {
            foreach (var itemPosition in ItemsToExplode)
            {
                if (itemPosition==null) continue;
                itemPosition.IsActive=true;
               
                Board.Cells[itemPosition.Position.x,itemPosition.Position.y].SetIsLocked(false);
                itemPosition.OnExplode();
                if (!_shouldSpawnBooster)
                { 
                    ExplodeAllDirections(itemPosition.Position);
                }

            }
        }
        private void ReturnParticleEffect()
        {
            _targetedRays.ForEach(ray => ray.ResetRay());
            _lightBallParticleEffectInstance.SetActive(false);
            ObjectPool.Instance.ReturnBoosterParticleEffect(_lightBallParticleEffectInstance,ItemID);
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
        }
        private void TryGetItemPositionForRay(int idToMatch)
        {
            for (var x = 0; x < Board.Width; x++)
            {
                for (var y = 0; y < Board.Height; y++)
                {
                    var cell = Board.Cells[x,y];
                    if (!cell.HasItem)
                        continue;
                    if (cell.BoardItem.ItemID == idToMatch && !cell.BoardItem.IsExploding && !cell.BoardItem.IsMoving &&
                        !cell.IsLocked && !cell.BoardItem.IsMatching&& !ItemsToExplode.Contains(cell.BoardItem)&&cell.BoardItem.IsActive)
                    {
                        _rayDestinationQueue.Enqueue(Board.GetItem(new Vector2Int(x,y)));
                        ItemsToExplode.Add(cell.BoardItem);
                        Board.Cells[x,y].SetIsLocked(true);
                        cell.BoardItem.IsActive = false;
                    }
                }
            }

        }
        private void ExplodeAllDirections(Vector2Int pos)
        {
            foreach (var direction in pos.GetFourDirections())
                if (Board.IsInBoundaries(direction) && Board.Cells[direction.x,direction.y].HasItem &&
                    !Board.GetItem(direction).IsExploding && !Board.GetItem(direction).IsMatching &&
                    Board.GetItem(direction).IsExplodeAbleByNearMatches)
                    Board.GetItem(direction).OnExplode();
        }
    }
}