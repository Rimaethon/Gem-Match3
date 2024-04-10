using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    enum LightBallState
    {
        Search,
        Animate,
        Finish
    }
    public class LightBallBoosterAction :IItemAction
    {
        public bool IsFinished=>_isFinished;
        private bool _isFinished;
        private readonly LineRenderer _lineRenderer;
        private readonly float _searchTime ;
        private float _counter= 0.0f;
        private readonly float _lerpSpeed ;
        private readonly GameObject _itemParticleEffect;
        private readonly GameObject _lightBallExplosionParticle;
        private readonly GameObject _lightBallSprite;
        private readonly Vector3 _lightBallPosition;
        private readonly Board _board;
        private readonly int _matchingItemID;
        private readonly Vector2Int _position;
        private readonly int _lightBallID;
        private MatchData matchData;
        private int _matchIndex = 0;
        private LightBallState _state;
        private IItem _itemToSendRay;
        private float _lineLerpTime;
        
        public LightBallBoosterAction(Board board,int lightBallID,int matchingItemID, Vector2Int pos, float searchTime, float lerpSpeed)
        {
            _board = board;
            _lightBallID = lightBallID;
            _matchingItemID = matchingItemID;
            _position = pos;
            _searchTime = searchTime;
            _lerpSpeed = lerpSpeed;
            matchData = new MatchData();
            matchData.MatchType = MatchType.SingleExplosion;
            matchData.Matches = new Match[30];
            _lightBallPosition = LevelGrid.Instance.GetCellCenterWorld(pos);
            _itemParticleEffect = ObjectPool.Instance.GetBoosterParticleEffect(_lightBallID, _lightBallPosition);
            EventManager.Instance.Broadcast(GameEvents.OnBoardLock);
            _lightBallExplosionParticle = _itemParticleEffect.GetComponent<LightBallParticleEffect>().lightBallExplosionParticle;
            _lightBallSprite = _itemParticleEffect.GetComponent<LightBallParticleEffect>().lightBallSprite;
            _lineRenderer = _itemParticleEffect.GetComponent<LineRenderer>();
            _board.GetCell(pos).SetIsLocked( true);
            HighlightLightBall();
            
        }
      
       

        private void HighlightLightBall()
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            _lightBallSprite.GetComponent<SpriteRenderer>().GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_IsOutlineEnabled",1f);
            _lightBallSprite.GetComponent<SpriteRenderer>().SetPropertyBlock(materialPropertyBlock);
        }
        
        public void Execute()
        {
            switch (_state)
            {
                case LightBallState.Search:
                    Search();
                    break;
                case LightBallState.Animate:
                    AnimateRay(_itemToSendRay);
                    break;
                case LightBallState.Finish:
                    FinishAction();
                    break;
                
            }
        }

        private void Search()
        {
            while (_counter<_searchTime)
            {
                _counter += Time.fixedDeltaTime;
                if (!TryGetItemToMatch(_board, _matchingItemID))
                {
                    _state = LightBallState.Finish;
                    return;
                }
                matchData.Matches[_matchIndex].Pos=_itemToSendRay.Position;
                matchData.Matches[_matchIndex].IsMatch= true;
                _matchIndex++;
                _itemToSendRay.IsMatching = true;
                _state = LightBallState.Animate;                
               return;
            }
        
        }
        private void FinishAction()
        {
                
            _lightBallExplosionParticle.SetActive(true);
            EventManager.Instance.Broadcast<MatchData>(GameEvents.AddMatchToHandle,matchData);
            _board.GetCell(_position).SetIsLocked( false);
            foreach (var match in matchData.Matches)
            {
                if(match.IsMatch)
                    _board.GetCell(match.Pos).SetIsLocked(false);
            }
            ObjectPool.Instance.ReturnBoosterParticleEffect(_itemParticleEffect, _lightBallID);
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
        }
       
        private void AnimateRay(IItem itemToSendRay)
        {
            Vector3 endPosition=itemToSendRay.Transform.position;
            Vector3 currentEndPos = _lightBallPosition;
            Vector3 currentStartPos = _lightBallPosition;
            _lineRenderer.SetPosition(0, currentStartPos);
            float lerpTime = 0.0f;
            while (Vector3.Distance(endPosition, currentEndPos) > 0.1f)
            {
                lerpTime += Time.fixedDeltaTime* _lerpSpeed;
                currentEndPos = Vector3.Lerp(currentEndPos, endPosition, lerpTime);
                _lineRenderer.SetPosition(1, currentEndPos);
                return;
            }
            itemToSendRay.Highlight(1);
            lerpTime = 0.0f;
            while (Vector3.Distance(endPosition, currentStartPos) > 0.1f)
            {
                lerpTime += Time.fixedDeltaTime;
                currentStartPos= Vector3.Lerp(currentStartPos, endPosition, lerpTime);
                _lineRenderer.SetPosition(0, currentStartPos);
                return;
            }
            
            
        }

        private bool TryGetItemToMatch(Board board,int idToMatch)
        {

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.GetCell(x,y).HasItem && board.GetItem(x,y).ItemID ==idToMatch && !board.GetItem(x,y).IsExploding&&!board.GetItem(x,y).IsMoving&&!board.GetCell(x,y).IsLocked)
                    {
                        Cell cell = board.GetCell(x, y);
                        cell.SetIsLocked( true);
                        _itemToSendRay = cell.Item;
                        return true;
                    }
                }
            }

            return false;
        } 
      
        
    }
}