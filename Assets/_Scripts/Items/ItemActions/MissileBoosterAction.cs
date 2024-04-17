using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class MissileBoosterAction:IItemAction
    {
        private Vector2Int _target;
        private const float RotationSpeed = 0.4f;
        private const int MovementSpeed =2;
     
        private GameObject _itemParticleEffect;
        private GameObject _itemSprite;
        private GameObject _itemShadow;
        private GameObject _itemHitEffect;
        private Transform _spriteTransform;  
        private Vector2Int _itemCellPos;
        private Vector3 _itemPos;
        private Vector3 _hitPointPos;
        private Vector3 _targetPos;
        private Vector3 _itemShadowPos;
        private int _itemID;
        private Board _board;

        public MissileBoosterAction (Vector2Int target)
        {
            _target = target;
        }
        
 
        public void Execute(Board board, Vector2Int pos, int itemID)
        {
            
            _board = board;
            _itemCellPos = pos;
            _itemPos= LevelGrid.Instance.GetCellCenterWorld(pos);
            _itemParticleEffect = ObjectPool.Instance.GetBoosterParticleEffect(itemID, _itemPos);
            _itemShadow = _itemParticleEffect.GetComponent<MissileParticleEffect>().missileShadow;
            _itemHitEffect = _itemParticleEffect.GetComponent<MissileParticleEffect>().missileHitEffect;
            _itemSprite = _itemParticleEffect.GetComponent<MissileParticleEffect>().missileSprite;
            _spriteTransform = _itemSprite.transform;
            _itemSprite.transform.localPosition = Vector3.zero;
            _itemSprite.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _itemShadowPos=_itemShadow.transform.localPosition;
            _targetPos = LevelGrid.Instance.GetCellCenterWorld(_target);
            Debug.Log("Missile Booster Action"+_targetPos);
            ExplodeAllDirections(board,pos);
            MoveToTarget(_targetPos).Forget();
            _itemID = itemID;
           
        }
        private async UniTask MoveToTarget(Vector3 target)
        {
            _hitPointPos=_itemHitEffect.transform.position;
            float distance = Vector2.Distance(target, _hitPointPos);
            while (distance> 0.05f)
            {
                Debug.Log("Distance: "+distance);
                _itemPos = _spriteTransform.localPosition;
                Vector3 dir = target - _hitPointPos;
                var angleDifference = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 40;
                var rotationTarget = Quaternion.AngleAxis(angleDifference, Vector3.forward);
                _itemPos += (_spriteTransform.up + _spriteTransform.right) * (MovementSpeed * Time.fixedDeltaTime);
                _spriteTransform.localPosition = _itemPos;
                _spriteTransform.localRotation = Quaternion.RotateTowards(_spriteTransform.localRotation, rotationTarget,
                    Mathf.Abs(RotationSpeed * angleDifference*distance));
                _hitPointPos=_itemHitEffect.transform.position;
                distance = Vector2.Distance(target, _hitPointPos);
                await UniTask.Yield();
            }
            _itemHitEffect.SetActive(true);
            MatchData matchData = new MatchData();
            matchData.Matches = new Match[1];
            matchData.Matches[0].Pos = _target;
            matchData.Matches[0].IsMatch = true;
            matchData.MatchType = MatchType.SingleExplosion;
            if (_board.GetCell(_target).HasItem)
            {
                _board.GetItem(_target).IsMatching = true;
            }
            if(_board.GetCell(_target).HasUnderLayItem)
            {
                _board.GetCell(_target).UnderLayItem.IsMatching = true;
            }
            EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchData);
           
            await UniTask.Delay(300);
            _itemParticleEffect.SetActive(false);
            _itemSprite.transform.localPosition = Vector3.zero;
            _itemSprite.transform.localRotation = Quaternion.Euler(0, 0, 0);
            ObjectPool.Instance.ReturnBoosterParticleEffect(_itemParticleEffect, _itemID);
            
            
        }

        private void ExplodeAllDirections(Board board,Vector2Int pos)
        {
            MatchData matchData = new MatchData();
            matchData.Matches = new Match[4];
            matchData.MatchType = MatchType.SingleExplosion;
            int index = 0;
            foreach (Vector2Int direction in pos.GetFourDirections())
            {
                if(board.GetItem(direction)!=null&&!board.GetItem(direction).IsExploding&&!board.GetItem(direction).IsMatching)
                {
                    matchData.Matches[index].Pos = direction;
                    matchData.Matches[index].IsMatch = true;
                    index++;
                    board.GetItem(direction).IsMatching = true;
                }

            }
           
            EventManager.Instance.Broadcast(GameEvents.AddMatchToHandle, matchData);
            
        }


        public void Execute()
        {
            
        }

        public bool IsFinished { get; }
    }
}