using System;
using _Scripts.Managers;
using _Scripts.Utility;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class MissileBoosterAction : IItemAction
    {
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public bool IsFinished => _isFinished;

        private const float RotationSpeed = 0.5f;
        private const float MovementSpeed = 2.7f;
        private const float ExplosionDelay = 0.2f;
        private int _carriedBoosterID;
        private bool _isCarryingAnotherBooster;
        private Vector2Int _pos;
        private Vector2Int _target;
        private float _counter;
        private Vector3 _hitPointPos;
        private bool _isHitInitiated;
        private GameObject _itemParticleEffect;
        private GameObject _itemShadow;
        private GameObject _missileHitPoint;
        private Transform _itemTransform;
        private Transform _missileCompanyTransform;
        private SpriteRenderer _missileCompanySpriteRenderer;
        private Vector3 _itemPosition;
        private Vector3 _itemShadowPos;
        private Vector3 _missileCompanyInitialPos;
        private Vector3 _missileCompanyTargetPos;
        private Vector3 _targetPos;
        private bool _isFinished;
        private AudioSource _audioSource;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            _target = LevelManager.Instance.GetRandomGoalPos();
            _pos = pos;
            Board = board;
            _isCarryingAnotherBooster = value2 != -1;
            _carriedBoosterID = value2;
            if (_target is { x: -1, y: -1 }) return;
            InitializeMissile(_pos);
            ExplodeAllDirections();
            _isFinished = false;
            _isHitInitiated = false;
        }

        public void Execute()
        {
            if (_target is { x: -1, y: -1 })
            {
                Debug.Log("Target is null");
                return;
            }
            if (IsFinished)
                return;
            MoveToTarget(_targetPos);
        }

        private void InitializeMissile(Vector2Int pos)
        {
            _audioSource=AudioManager.Instance.PlaySFX(SFXClips.MissileFlyingSound);
            _itemPosition = LevelGrid.Instance.GetCellCenterWorld(pos);
            _itemParticleEffect = ObjectPool.Instance.GetBoosterParticleEffect(ItemID, _itemPosition);
            _itemParticleEffect.SetActive(true);
            var missileParticleEffect = _itemParticleEffect.GetComponent<MissileParticleEffect>();
            _itemShadow = missileParticleEffect.missileShadow;
            _missileHitPoint = missileParticleEffect.hitPoint;
            _itemTransform = _itemParticleEffect.transform;
            _itemShadowPos = _itemShadow.transform.localPosition;
            if (_isCarryingAnotherBooster && _carriedBoosterID != -1)
            {
                _missileCompanyTransform = missileParticleEffect.missileCompany.transform;
                _missileCompanySpriteRenderer = missileParticleEffect.missileCompany.GetComponent<SpriteRenderer>();
                _missileCompanySpriteRenderer.sprite = ObjectPool.Instance.GetBoosterSprite(_carriedBoosterID);
                _missileCompanyInitialPos = _missileCompanyTransform.localPosition;
                _missileCompanyTargetPos = _missileCompanyInitialPos * -1 + new Vector3(0, 0, -0.1f);
                _missileCompanyTransform.DOScale(0.95f, 0.3f).SetLoops(-1, LoopType.Yoyo);
                _missileCompanyTransform.DOLocalMove(_missileCompanyTargetPos, 0.3f).SetLoops(-1, LoopType.Yoyo);
            }

            _targetPos = LevelGrid.Instance.GetCellCenterWorld(_target);
        }

        private void MoveToTarget(Vector3 target)
        {
            _hitPointPos = _missileHitPoint.transform.position;
            var distance = Vector2.Distance(target, _hitPointPos);
            if (distance > 0.1f)
            {
                UpdateMissilePositionAndRotation(target);
            }
            else
            {
                HandleMissileHit();
            }
        }

        private void UpdateMissilePositionAndRotation(Vector3 target)
        {
            _itemPosition = _itemTransform.position;
            var dir = target - _hitPointPos;
            var angleDifference = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg-39f;
            var rotationTarget = Quaternion.AngleAxis(angleDifference, Vector3.forward);
            _itemTransform.rotation = Quaternion.RotateTowards(_itemTransform.rotation, rotationTarget,
                Math.Abs(RotationSpeed * Math.Clamp(Math.Abs(angleDifference), 1, 20)));
            _itemPosition += _missileHitPoint.transform.up * (MovementSpeed * Time.fixedDeltaTime);
            _itemTransform.position = _itemPosition;
        }

        private void HandleMissileHit()
        {
            if (!_isHitInitiated) InitiateMissileHit();

            if (_counter < ExplosionDelay)
            {
                _counter += Time.fixedDeltaTime;
                return;
            }

            Board.Cells[_pos.x,_pos.y].SetIsLocked(false);
            ResetMissile();
            _isFinished = true;
        }

        private void InitiateMissileHit()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            if (_isCarryingAnotherBooster)
            {
                EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, _target, _carriedBoosterID, -1);
            }
            _audioSource=AudioManager.Instance.PlaySFX(SFXClips.MissileExplosionSound);
            ObjectPool.Instance.GetMissileHitEffect(_targetPos);
            _itemParticleEffect.SetActive(false);
            if (Board.GetItem(_target) != null) Board.GetItem(_target).OnExplode();
            _isHitInitiated = true;
        }

        private void ResetMissile()
        {
            if (_isCarryingAnotherBooster && _carriedBoosterID != -1)
            {
                _missileCompanyTransform.DOKill();
                _missileCompanyTransform.localScale = Vector3.one;
                _missileCompanyTransform.localPosition = _missileCompanyInitialPos;
                _missileCompanySpriteRenderer.sprite = null;
            }

            _itemParticleEffect.transform.localPosition = Vector3.zero;
            _itemParticleEffect.transform.localRotation = Quaternion.Euler(0, 0, 0);
            ObjectPool.Instance.ReturnBoosterParticleEffect(_itemParticleEffect, ItemID);

            _itemParticleEffect=null;
            _itemShadow=null;
            _missileHitPoint=null;
            _itemTransform=null;
            _missileCompanyTransform=null;
            _missileCompanySpriteRenderer=null;
            Board=null;
        }

        private void ExplodeAllDirections()
        {
            ObjectPool.Instance.GetMissileExplosionEffect(_itemPosition);
            foreach (var direction in _pos.GetFourDirections())
            {
                if (Board.GetItem(direction) != null && !Board.GetItem(direction).IsExploding &&
                    !Board.GetItem(direction).IsMatching)
                    Board.GetItem(direction).OnExplode();
            }

        }
    }
}
