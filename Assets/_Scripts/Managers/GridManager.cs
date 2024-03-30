using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;
using Vector3Int = UnityEngine.Vector3Int;

namespace Scripts
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private ObjectPool _objectPool;
        private bool _isLerping;
        private int[,] gameBoard;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int itemTypeStart=0;
        [SerializeField] private int itemTypeEnd=4;
        private IItem[,] _tileItems;
        //Im only using local position to 
        private Vector2[,] _cellPositions;
        private bool _isSwiping;
        
        private RandomBoardGenerator _randomBoardGenerator;
        
       private bool[] isTopCellFit=new bool[8];
        
        private void OnEnable()
        {
            EventManager.Instance.AddHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnTouch, OnTouch);
            EventManager.Instance.AddHandler<HashSet<Vector2Int>,Vector2Int>(GameEvents.OnBomb, HandleBombMatch);
            EventManager.Instance.AddHandler<HashSet<Vector2Int>,Vector2Int>(GameEvents.OnHorizontalMatch, HandleHorizontalMatch);
            EventManager.Instance.AddHandler<HashSet<Vector2Int>,Vector2Int>(GameEvents.OnVerticalMatch, HandleVerticalMatch);
            
        }

        

        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnTouch, OnTouch);
            EventManager.Instance.RemoveHandler<HashSet<Vector2Int>,Vector2Int>(GameEvents.OnBomb, HandleBombMatch);
            EventManager.Instance.RemoveHandler<HashSet<Vector2Int>,Vector2Int>(GameEvents.OnHorizontalMatch, HandleHorizontalMatch);
            EventManager.Instance.RemoveHandler<HashSet<Vector2Int>,Vector2Int>(GameEvents.OnVerticalMatch, HandleVerticalMatch);
            _cts.Cancel();
        }
        private void OnApplicationQuit()
        {
            _cts.Cancel();
        }
        private void HandleVerticalMatch(HashSet<Vector2Int> positions,Vector2Int targetPos)
        {
            HashSet<Vector2Int> positionsToDestroy = new HashSet<Vector2Int>();
            positionsToDestroy.Add(targetPos);
            positionsToDestroy.UnionWith(positions);
            foreach (Vector2Int pos in positions)
            {
                if (_tileItems.GetItem(pos) != null)
                {
                    positionsToDestroy.UnionWith(_tileItems.GetItem(pos).OnClick(_tileItems,pos,false));
                }
            }
            HandleMatchWithDelay(positionsToDestroy,0.05f,0.2f).Forget();
        }
        private void HandleHorizontalMatch(HashSet<Vector2Int>  positions,Vector2Int targetPos)
        {
            HashSet<Vector2Int> positionsToDestroy = new HashSet<Vector2Int>();
            positionsToDestroy.Add(targetPos);
            positionsToDestroy.UnionWith(positions);
            foreach (Vector2Int pos in positions)
            {
                if (_tileItems.GetItem(pos) != null)
                {
                    positionsToDestroy.UnionWith(_tileItems.GetItem(pos).OnClick(_tileItems,pos,false));
                }
            }
            HandleMatchWithDelay(positionsToDestroy,0.05f,0.2f).Forget();
        }

        private void HandleBombMatch(HashSet<Vector2Int>  positions,Vector2Int targetPos)
        {
            HashSet<Vector2Int> positionsToDestroy = new HashSet<Vector2Int>();
            positionsToDestroy.Add(targetPos);
            positionsToDestroy.UnionWith(positions);
            foreach (Vector2Int pos in positions)
            {
                if (_tileItems.GetItem(pos) != null)
                {
                    positionsToDestroy.UnionWith(_tileItems.GetItem(pos).OnClick(_tileItems,pos,false));
                }
            }
            HandleMatchWithDelay(positionsToDestroy,0,0.1f).Forget();
            BoardShake().Forget();
        }

   
        private void OnTouch(Vector2 touchPos)
        {
            Vector3Int firstCellPos = grid.WorldToCell(touchPos);
            if (IsInsideGrid(firstCellPos)&&_tileItems[firstCellPos.x, firstCellPos.y]!=null)
            {
                _tileItems[firstCellPos.x, firstCellPos.y].OnTouch();
            }
        }
        
        private bool IsInsideGrid(Vector3Int pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }
    
    
        private void Start()
        {
            Application.targetFrameRate = 60;
            _randomBoardGenerator = new RandomBoardGenerator();
            gameBoard = _randomBoardGenerator.GenerateRandomBoard(width, height,itemTypeStart,itemTypeEnd);
            Generate();
            UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

        }
        private CancellationTokenSource _cts;
        private void Generate()
        {

            _tileItems = new IItem[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    
                    _tileItems[i,j] =_objectPool.GetItemFromPool(gameBoard[i, j], grid.GetCellCenterWorld(new Vector3Int(i, j, 0))).GetComponent<IItem>();
                    
                    _tileItems[i, j].Transform.parent = transform;
                }
                

            }
            _cts = new CancellationTokenSource();
            MovementUpdate(_cts.Token).Forget();
            MatchCheckingUpdate(_cts.Token).Forget();
          
        }
        

        private void OnClick(Vector2 clickPos)
        {
            Vector2Int firstCellPos = grid.WorldToCellVector2Int(clickPos);
            if (!IsMovable(firstCellPos))
                return;
            _tileItems.GetItem(firstCellPos).OnClick(_tileItems,firstCellPos,true);
            OnTouch(clickPos);
        }

        private async void  OnSwipe(Vector2Int direction, Vector2 clickPos)
        {

            Vector2Int firstCellPos = grid.WorldToCellVector2Int(clickPos);
            Vector2Int secondCellPos = firstCellPos + direction;

            if (!IsMovable(firstCellPos) || !IsMovable(secondCellPos)||IsItemMoving(firstCellPos.x,firstCellPos.y)||IsItemMoving(secondCellPos.x,secondCellPos.y))
            {
                
                Debug.Log("Pos is not movable");
                return;
            }
            _tileItems[firstCellPos.x, firstCellPos.y].SortingOrder= 5;
            _tileItems[secondCellPos.x, secondCellPos.y].SortingOrder = 4;
           
            _tileItems[firstCellPos.x, firstCellPos.y].MovementQueue.Enqueue(secondCellPos);
            _tileItems[secondCellPos.x, secondCellPos.y].MovementQueue.Enqueue(firstCellPos);
            while(_tileItems[firstCellPos.x,firstCellPos.y].MovementQueue.Count>0||_tileItems[secondCellPos.x,secondCellPos.y].MovementQueue.Count>0)
            {
                await UniTask.Yield();
            }
            SwapItemsInArray(firstCellPos,secondCellPos);
            List<Vector2Int> firstCellMatches=_tileItems.CheckMatches(firstCellPos,width, height);
            List<Vector2Int> secondCellMatches=_tileItems.CheckMatches(secondCellPos, width, height);
            
            
            
            if (firstCellMatches.Count<3 && secondCellMatches.Count<3)
            {
                _tileItems[firstCellPos.x,firstCellPos.y].MovementQueue.Enqueue(secondCellPos);
                _tileItems[secondCellPos.x,secondCellPos.y].MovementQueue.Enqueue(firstCellPos);
                while(_tileItems[firstCellPos.x,firstCellPos.y].MovementQueue.Count>0||_tileItems[secondCellPos.x,secondCellPos.y].MovementQueue.Count>0)
                {
                    await UniTask.Yield();
                }
                SwapItemsInArray(firstCellPos,secondCellPos);
                
            }
            else
            {
                
                if (firstCellMatches.Count >= 3)
                {
                    await HandleMatch(firstCellMatches,firstCellPos);
                }
                
                if(secondCellMatches.Count>=3)
                {
                  
                    await HandleMatch(secondCellMatches,secondCellPos);
                }
            }


            OnTouch(clickPos);
            
        }
        
        private async UniTask HandleMatch(List<Vector2Int> matchedItems,Vector2Int targetPos)
        {
            if (matchedItems.Count==3)
            {
                await HandleMatchWithDelay(matchedItems,0f,0f);
                Debug.Log($"HandleMatch called with target position {targetPos} and matched items {string.Join(", ", matchedItems)}");

            }else if (matchedItems.Count == 4)
            {
                 await LerpAllItemsToPosition(matchedItems,targetPos,0.1f,0f);
                _objectPool.GetParticleEffectFromPool(8,grid.GetCellCenterWorldVector2(targetPos));
                _tileItems[targetPos.x, targetPos.y]= _objectPool.GetItemFromPool(7,grid.GetCellCenterWorld(new Vector3Int(targetPos.x, targetPos.y, 0))).GetComponent<IItem>();
                _tileItems[targetPos.x, targetPos.y].Transform.parent = transform;
            }else if (matchedItems.Count == 5)
            {
                await LerpAllItemsToPosition(matchedItems,targetPos,0f,0.1f);
                _objectPool.GetParticleEffectFromPool(8,grid.GetCellCenterWorldVector2(targetPos));
                _tileItems[targetPos.x, targetPos.y]= _objectPool.GetItemFromPool(5,grid.GetCellCenterWorld(new Vector3Int(targetPos.x, targetPos.y, 0))).GetComponent<IItem>();
                _tileItems[targetPos.x, targetPos.y].Transform.parent = transform;
            }
        }
        
        private bool IsInBoundaries(Vector2Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
        }

        /*private async UniTask ChangeItemPositions(Vector2Int pos1, Vector2Int pos2, float lerpSpeed = 0.15f)
        {
            Vector2 pos1InitialPosition = grid.GetCellCenterLocalVector2(pos1);
            Vector2 pos2InitialPosition = grid.GetCellCenterLocalVector2(pos2);
            Sequence sequence = DOTween.Sequence();
            sequence.Insert(0,_tileItems[pos1.x, pos1.y].Transform.DOLocalMove(pos2InitialPosition, lerpSpeed));
            sequence.Insert(0,_tileItems[pos2.x, pos2.y].Transform.DOLocalMove(pos1InitialPosition, lerpSpeed));

            await sequence.AwaitForComplete();    
            SwapItemsInArray(pos1, pos2);
          
           
        }*/
        
        private bool IsItemMoving(int x, int y)
        {
            return _tileItems[x,y]!=null&&_tileItems[x, y].MovementQueue.Count > 0;
        }
      
        private void SwapItemsInArray(Vector2Int pos1, Vector2Int pos2)
        {
            (_tileItems[pos1.x, pos1.y], _tileItems[pos2.x, pos2.y]) = (_tileItems[pos2.x, pos2.y], _tileItems[pos1.x, pos1.y]);
        }
        
         private async UniTaskVoid MatchCheckingUpdate(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await CheckMatchesAsync();
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    // Handle any exceptions here
                    Debug.LogError(ex);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        private async UniTaskVoid MovementUpdate(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await MoveItemsAsync();
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    // Handle any exceptions here
                    Debug.LogError(ex);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private async UniTask CheckMatchesAsync()
        {
            for (int x = width-1; x >=0; x--)
            {
                for (int y = height-1; y >=0 ; y--)
                {
                    if (!IsItemMoving(x, y)) continue;
                    Vector3 currPos = _tileItems[x, y].Transform.localPosition;

                    Vector3 targetPos = grid.GetCellCenterLocalVector2(_tileItems[x,y].MovementQueue.Peek());

                    if (IsItemCloseToTarget(currPos, targetPos))
                    {
                        await FinishItemMovement(x, y, currPos);
                            
                    }
                }
            }
            
           
        }

        private async UniTask MoveItemsAsync()
        {
            
            for (int x = width-1; x >=0; x--)
            {
                
                for (int y = height-1; y >=0 ; y--)
                {
                    if (_tileItems[x, y] != null)
                    {
                        if (IsItemMoving(x, y))
                        {
                            await MoveItemTowardsTarget(x, y);
                        }
                        continue;
                    }

                    int posToCheck = FindNextNonEmptyPosition(x, y);

                    if (posToCheck == height)
                    {
                        if (!isTopCellFit[x])
                        {
                            SpawnNewItem(x, y, height);
                            isTopCellFit[x] = true;
                        }
                        continue;
                    }

                    if (_tileItems[x, posToCheck] != null && IsMovable(new Vector2Int(x, posToCheck)))
                    {
                        MoveItem(x, y, posToCheck);
                        continue;
                    }

                    if (x > 0 && CanMoveFromLeft(x, y))
                    {
                        MoveItemFromLeft(x, y);
                        continue;
                    }

                    if (x < width - 1 && CanMoveFromRight(x, y))
                    {
                        MoveItemFromRight(x, y);
                    }
                }

                
            }
        }

        private async UniTask MoveItemTowardsTarget(int x, int y)
        {
            Vector3 currPos = _tileItems[x, y].Transform.localPosition;
            Vector3 targetPos = grid.GetCellCenterLocalVector2(_tileItems[x,y].MovementQueue.Peek());
            _tileItems[x, y].FallSpeed+=_tileItems[x, y].Gravity;
            if(y>0&&_tileItems[x,y-1]!=null&&_tileItems[x,y-1].FallSpeed>_tileItems[x,y].FallSpeed)
            {
                _tileItems[x, y].FallSpeed = _tileItems[x, y-1].FallSpeed-_tileItems[x, y].Gravity;
            }
            // Calculate the maximum distance the item can move in this frame
            float maxDistance = _tileItems[x, y].FallSpeed * Time.deltaTime;
            Debug.Log($"MoveItemTowardsTarget called for tile at position ({x}, {y})"+_tileItems[x,y].Transform.localPosition+" "+targetPos+" "+maxDistance);

            // Move the item towards the target
            Vector3 newPos = Vector3.MoveTowards(currPos, targetPos, maxDistance);

            // Update the item's position
            _tileItems[x, y].Transform.localPosition = newPos;
        }
        private bool IsMovable(Vector2Int pos)
        {
            return IsInBoundaries(pos) && _tileItems[pos.x,pos.y]!=null&&_tileItems[pos.x, pos.y].IsMovable;
        }


        private async UniTask FinishItemMovement(int x, int y, Vector3 currPos)
        {
            
            if(_tileItems[x,y].MovementQueue.Peek().y==height-1)
            {
                isTopCellFit[x] = false;
            }

            _tileItems[x, y].Transform.localPosition = grid.GetCellCenterLocalVector2(_tileItems[x,y].MovementQueue.Dequeue());
            Debug.Log($"FinishItemMovement called for tile at position ({x}, {y})"+_tileItems[x,y].Transform.localPosition);

            if (_tileItems[x,y].MovementQueue.Count == 0)
            {
                _tileItems[x, y].FallSpeed = 0f;
                _tileItems[x, y].SortingOrder = 4;

                List<Vector2Int> cellMatches =
                    _tileItems.CheckMatches(new Vector2Int(x, y),width, height);
                Debug.Log("CheckMatchesAsync called for tile at position (" + x + ", " + y +" "+cellMatches.Count+" matches found"+_tileItems[x,y].Transform.localPosition);

                if (cellMatches.Count >= 3)
                {
                    Debug.Log(x+" "+y+" "+cellMatches.Count+" matches found");
                    await HandleMatch(cellMatches, new Vector2Int(x, y));
                }
            }
        }

        private int FindNextNonEmptyPosition(int x, int y)
        {
            int posToCheck = y + 1;
            while (posToCheck < height && (_tileItems[x, posToCheck] == null))
            {
                posToCheck++;
            }
            return posToCheck;
        }

        private void SpawnNewItem(int x, int y, int yOffset)
        {
            _tileItems[x, y]=_objectPool.GetRandomItemFromPool(
                grid.GetCellCenterWorld(new Vector3Int(x, yOffset, 0))).GetComponent<IItem>();
            _tileItems[x, y].Transform.parent = transform;
            if (y != height - 1)
            {
                _tileItems[x,y].MovementQueue.Enqueue(new Vector2Int(x, height-1));
            }

            _tileItems[x,y].MovementQueue.Enqueue(new Vector2Int(x, y));
        }

        private void MoveItem(int x, int y, int posToCheck)
        {
            if (!_tileItems[x,posToCheck].MovementQueue.Contains(new Vector2Int(x, y)))
            {
                _tileItems[x,posToCheck].MovementQueue.Enqueue(new Vector2Int(x, y));
            }
            _tileItems[x,y]=_tileItems[x, posToCheck];
            _tileItems[x, posToCheck] = null;
        }

        private bool CanMoveFromLeft(int x, int y)
        {
            return !IsItemMoving(x - 1, y) && !IsItemMoving(x - 1, y + 1) &&_tileItems[x-1,y+1]!=null&& _tileItems[x - 1, y + 1].IsMovable;
        }

        private void MoveItemFromLeft(int x, int y)
        {
            if(!_tileItems[x-1,y+1].MovementQueue.Contains(new Vector2Int(x, y)))
            {
                _tileItems[x-1,y+1].MovementQueue.Enqueue(new Vector2Int(x , y ));

                _tileItems[x, y] = _tileItems[x - 1, y + 1];
                if(y==height-1)
                {
                    isTopCellFit[x] = false;
                }
                _tileItems[x - 1, y + 1] = null;

            }
            
        }

        private bool CanMoveFromRight(int x, int y)
        {
            return !IsItemMoving(x + 1, y) && !IsItemMoving(x + 1, y + 1) &&_tileItems[x+1,y+1]!=null&&  _tileItems[x + 1, y + 1].IsMovable;

        }

        private void MoveItemFromRight(int x, int y)
        {
            if(!_tileItems[x+1,y+1].MovementQueue.Contains(new Vector2Int(x, y)))
            {
                _tileItems[x+1,y+1].MovementQueue.Enqueue(new Vector2Int(x, y));
                _tileItems[x, y] = _tileItems[x + 1, y + 1];
                if(y==height-1)
                {
                    isTopCellFit[x] = false;
                }
                _tileItems[x + 1, y + 1] = null;
            }
    
         
        }

      

  
        private bool IsItemCloseToTarget(Vector3 currPos, Vector3 targetPos)
        {
            return Mathf.Abs(targetPos.y - currPos.y) < 0.05f&&Mathf.Abs(targetPos.x - currPos.x) < 0.05f;
        }

       

      
        private async UniTask WaitForQueueToEmpty(IItem item)
        {
            while (item.MovementQueue.Count > 0)
            {
                await UniTask.Yield();
            }
        }
        private async UniTask LerpAllItemsToPosition(List<Vector2Int> combinedItems, Vector2Int targetPos, float lerpSpeed = 0.15f,float delayAfterLerp=0.1f)
        {

            foreach (var item in combinedItems)
            {
                if (_tileItems[item.x, item.y] != null)
                {
                    _tileItems[item.x, item.y].MovementQueue.Enqueue(targetPos);

                }
            }

            await UniTask.Delay((int)(delayAfterLerp * 1000));
            foreach (var item in combinedItems)
            {
                if(item.y==height-1)
                {
                    isTopCellFit[item.x] = false;
                }
                if (_tileItems[item.x, item.y] != null)
                {
                    _tileItems[item.x,item.y].MovementQueue.Clear();
                    _tileItems[item.x, item.y].OnMatch();
                    _tileItems[item.x, item.y] = null;
                    
                }
               
            }
            
            
        }
        
        private SemaphoreSlim _matchSemaphoreHash = new SemaphoreSlim(1, 1);

        private async UniTask HandleMatchWithDelay(HashSet<Vector2Int> matchedItems, float delayBetweenMatches = 0.1f,float delayToFillBoard=0.1f)
        {
            await _matchSemaphoreHash.WaitAsync();
            try
            {
                foreach (var item in matchedItems)
                {
                    if (_tileItems[item.x, item.y] == null)
                    {
                        return;
                    }
                    _tileItems[item.x, item.y].OnMatch();
                    _objectPool.GetParticleEffectFromPool(_tileItems.GetTypeID(item), grid.GetCellCenterWorldVector2(item));
                    Debug.Log($"HandleMatchWithDelay called with matched items {string.Join(", ", matchedItems)}, delayBetweenMatches {delayBetweenMatches}, and delayToFillBoard {delayToFillBoard}");
                    await UniTask.Delay((int)(delayBetweenMatches * 1000));
                }
                await UniTask.Delay((int)(delayToFillBoard * 1000));
                foreach (var item in matchedItems)
                {
                    _tileItems[item.x, item.y] = null;
                    if(item.y==height-1)
                    {
                        isTopCellFit[item.x] = false;
                    }
                    
                      
                   
                }
            }
            finally
            {
                _matchSemaphoreHash.Release();
            }
                
           
           
        }
        private SemaphoreSlim _matchSemaphore = new SemaphoreSlim(1, 1);

        private async UniTask HandleMatchWithDelay(List<Vector2Int> matchedItems, float delayBetweenMatches = 0.1f,float delayToFillBoard=0.1f)
        {
            await _matchSemaphore.WaitAsync();


            try
            {
                foreach (var item in matchedItems)
                {
                    if (_tileItems[item.x, item.y] == null)
                    {
                        return;
                    }

                    _tileItems[item.x, item.y].OnMatch();
                    _objectPool.GetParticleEffectFromPool(_tileItems.GetTypeID(item),
                        grid.GetCellCenterWorldVector2(item));
                    Debug.Log(
                        $"HandleMatchWithDelay called with matched items {string.Join(", ", matchedItems)}, delayBetweenMatches {delayBetweenMatches}, and delayToFillBoard {delayToFillBoard}");
                    await UniTask.Delay((int)(delayBetweenMatches * 1000));
                }

                await UniTask.Delay((int)(delayToFillBoard * 1000));
                foreach (var item in matchedItems)
                {
                    _tileItems[item.x, item.y] = null;
                    if (item.y == height - 1)
                    {
                        isTopCellFit[item.x] = false;
                    }
                }
            }
            finally
            {
                _matchSemaphore.Release();
            }
           
           
           
        }
  

        // Screen shake on board but particle effects dont change their position
        //Board items are also changing their position so making grid their parent might be a good idea
        //first board goes to -1,-1(left,bottom) and then goes to 0,0
        public float shakeDuration = 0.5f;
        public float shakeMagnitude = 0.1f;

        private async UniTaskVoid BoardShake()
        {
            Vector3 originalPosition = transform.position;
            float elapsed = 0.0f;
            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                transform.position = new Vector3(originalPosition.x+x,originalPosition.y+ y, originalPosition.z);
                elapsed += Time.deltaTime;

                await UniTask.Yield();
            }

            transform.position = originalPosition;
        }
     
    }
    
}
