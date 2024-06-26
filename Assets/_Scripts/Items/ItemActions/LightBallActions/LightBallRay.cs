﻿using System;
using _Scripts.Utility;
using Scripts.BoosterActions;
using UnityEngine;

namespace Scripts
{
    public class LightBallRay:MonoBehaviour
    {
        public LightBallBoosterAction _lightBallBoosterAction;
        public LineRenderer lineRenderer;
        public bool hasTarget;
        public Vector2Int _targetCell;
        public IBoardItem _targetItem;
        private Vector3 startPosition;
        private readonly float _rayStretchSpeed = 15f;
        private readonly float _rayAlphaDecreaseSpeed =25f;
        private bool _isTargetHighlighted ;
        private MaterialPropertyBlock _materialPropertyBlock;
        private float _rayAlpha = 15f;
        private bool _shouldSpawnBooster;
        private int _boosterID;
        private Board _board;
        private bool _isTargetRemoved;
        private bool isSoundPlayed;
        private bool isMovementPrevented;
        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void AnimateRay()
        {
            if(!isSoundPlayed)
            {
                AudioManager.Instance.PlaySFX(SFXClips.RaySound);
                isSoundPlayed = true;
            }
            if(!hasTarget)
            {
                return;
            }

            _targetCell = _targetItem.Position;
            Vector3 localTargetPosition = transform.InverseTransformPoint(_targetItem.Transform.position);

            if (Vector3.Distance(lineRenderer.GetPosition(1), localTargetPosition) > 0.1f)
            {
                lineRenderer.SetPosition(1,
                    Vector3.MoveTowards(lineRenderer.GetPosition(1), localTargetPosition,
                        _rayStretchSpeed * Time.fixedDeltaTime));
                return;
            }
            if (!_isTargetRemoved && _shouldSpawnBooster)
            {
                IBoardItem boardItem = ObjectPool.Instance.GetBoosterItem(_boosterID,
                    LevelGrid.Instance.GetCellCenterWorld(_targetCell), _board);
                boardItem.Transform.parent = _board._boardInstance.transform;
                if (_board.Cells[_targetCell.x,_targetCell.y].HasItem)
                {
                    ObjectPool.Instance.ReturnItem(_board.GetItem(_targetCell), _board.GetItem(_targetCell).ItemID);
                }

                _board.Cells[_targetCell.x,_targetCell.y].SetItem(boardItem);
                _isTargetRemoved = true;
                _targetItem = boardItem;
            }
            if (!_isTargetHighlighted)
            {
                _lightBallBoosterAction.ItemsToExplode.Add(_targetItem);
                _targetItem.Highlight(1);
                _isTargetHighlighted = true;
            }

            if(_rayAlpha>0)
            {
                _rayAlpha = Math.Clamp(_rayAlpha-Time.fixedDeltaTime* _rayAlphaDecreaseSpeed,0,15);
                lineRenderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetFloat("_Emission",_rayAlpha);
                lineRenderer.SetPropertyBlock(_materialPropertyBlock);
                return;
            }
            ResetRay();
        }

        public void ResetRay()
        {
            lineRenderer.SetPosition(0,Vector3.zero);
            lineRenderer.SetPosition(1,Vector3.zero);
            lineRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat("_Emission",15);
            lineRenderer.SetPropertyBlock(_materialPropertyBlock);
            _rayAlpha = 15f;
            _isTargetHighlighted = false;
            hasTarget = false;
            isSoundPlayed = false;
            _isTargetRemoved = false;
        }

        public void SetTarget(Board board,bool shouldSpawnBooster,int boosterID,IBoardItem item)
        {
            _shouldSpawnBooster = shouldSpawnBooster;
            _boosterID = boosterID;
            _board = board;
            startPosition = transform.position;
            _isTargetHighlighted = false;
            _targetItem = item;
            _targetCell = item.Position;
            _rayAlpha = 15f;
            lineRenderer.SetPosition(0,Vector3.zero);
            lineRenderer.SetPosition(1,Vector3.zero);
            lineRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat("_Emission",15);
            lineRenderer.SetPropertyBlock(_materialPropertyBlock);
            hasTarget = true;
        }
    }
}
