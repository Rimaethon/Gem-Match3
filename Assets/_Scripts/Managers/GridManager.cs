using System.Collections.Generic;
using System.Linq;
using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;
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
        
        private HashSet<Vector2Int> lerpingItems=new HashSet<Vector2Int>();
        private HashSet<Vector2Int> swappingItems=new HashSet<Vector2Int>();
        private RandomBoardGenerator _randomBoardGenerator;
        
        private int[] missingItems=new int[8];
        
        private void OnEnable()
        {
            EventManager.Instance.AddHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnTouch, OnTouch);
            EventManager.Instance.AddHandler<List<Vector2Int>>(GameEvents.OnBomb, HandleBombMatch);
            EventManager.Instance.AddHandler<List<Vector2Int>>(GameEvents.OnHorizontalMatch, HandleHorizontalMatch);
            EventManager.Instance.AddHandler<List<Vector2Int>>(GameEvents.OnVerticalMatch, HandleVerticalMatch);
            
        }

        

        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnTouch, OnTouch);
            EventManager.Instance.RemoveHandler<List<Vector2Int>>(GameEvents.OnBomb, HandleBombMatch);
            EventManager.Instance.RemoveHandler<List<Vector2Int>>(GameEvents.OnHorizontalMatch, HandleHorizontalMatch);
            EventManager.Instance.RemoveHandler<List<Vector2Int>>(GameEvents.OnVerticalMatch, HandleVerticalMatch);
        }

        private void HandleVerticalMatch(List<Vector2Int> positions)
        {
            for (int i=1;i<positions.Count;i++)
            {
                if (_tileItems.GetItem(positions[i]) != null)
                {
                    _tileItems.GetItem(positions[i]).OnClick(_tileItems,positions[i]);
                    
                }
            }
            HandleMatchWithDelay(positions).Forget();
            _objectPool.GetParticleEffectFromPool(7,grid.GetCellCenterWorldVector2(positions[0]));
        }


        private void HandleBombMatch(List<Vector2Int> positions)
        {
            for (int i=1;i<positions.Count;i++)
            {
                if (_tileItems.GetItem(positions[i]) != null)
                {
                    _tileItems.GetItem(positions[i]).OnClick(_tileItems,positions[i]);
                    
                }
            }
            _tileItems.GetItem(positions[0]).OnMatch();
            _tileItems[positions[0].x,positions[0].y]=null;
            HandleMatchWithDelay(positions,0,0.3f).Forget();
            _objectPool.GetParticleEffectFromPool(5,grid.GetCellCenterWorldVector2(positions[0]));
            BoardShake().Forget();
        }
        private void HandleHorizontalMatch(List<Vector2Int> positions)
        {
            for (int i=1;i<positions.Count;i++)
            {
                if (_tileItems.GetItem(positions[i]) != null)
                {
                    _tileItems.GetItem(positions[i]).OnClick(_tileItems,positions[i]);
                    
                }
            }
            HandleMatchWithDelay(positions,0.05f,0.2f).Forget();
            _objectPool.GetParticleEffectFromPool(6,grid.GetCellCenterWorldVector2(positions[0]));
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
        }

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
          
        }
        

        private void OnClick(Vector2 clickPos)
        {
            Vector2Int firstCellPos = grid.WorldToCellVector2Int(clickPos);
            if (!IsMovable(firstCellPos))
                return;
            _tileItems.GetItem(firstCellPos).OnClick(_tileItems,firstCellPos);
            OnTouch(clickPos);
        }

        private async void  OnSwipe(Vector2Int direction, Vector2 clickPos)
        {

            if (_isSwiping)
            {
                Debug.Log("Im swiping");
                return;
            }
            Vector2Int firstCellPos = grid.WorldToCellVector2Int(clickPos);
            Vector2Int secondCellPos = firstCellPos + direction;

            if (!IsMovable(firstCellPos) || !IsMovable(secondCellPos))
            {
                Debug.Log(lerpingItems.Contains(firstCellPos));
                Debug.Log(lerpingItems.Contains(secondCellPos));
                
                Debug.Log("Pos is not movable");
                return;
            }
            _isSwiping = true;

            Debug.Log(lerpingItems.Contains(firstCellPos));
            swappingItems.Add(firstCellPos);
            swappingItems.Add(secondCellPos);
            lerpingItems.Add(firstCellPos);
            lerpingItems.Add(secondCellPos);
            _tileItems[firstCellPos.x, firstCellPos.y].SortingOrder= 5;
            _tileItems[secondCellPos.x, secondCellPos.y].SortingOrder = 4;
            await ChangeItemPositions(firstCellPos, secondCellPos);
            
            List<Vector2Int> firstCellMatches=_tileItems.CheckMatches(firstCellPos,lerpingItems, width, height);
            List<Vector2Int> secondCellMatches=_tileItems.CheckMatches(secondCellPos,lerpingItems, width, height);
            
            
            
            if (firstCellMatches.Count<3 && secondCellMatches.Count<3)
            {
                await ChangeItemPositions(secondCellPos, firstCellPos); 
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
            _isSwiping = false;
            swappingItems.Remove(firstCellPos);
            swappingItems.Remove(secondCellPos);
            lerpingItems.Remove(firstCellPos);
            lerpingItems.Remove(secondCellPos);
            OnTouch(clickPos);
            
        }
        
        private async UniTask HandleMatch(List<Vector2Int> matchedItems,Vector2Int targetPos)
        {
            if (matchedItems.Count==3)
            {
                await HandleMatchWithDelay(matchedItems,0.1f,0.1f);
            }else if (matchedItems.Count == 4)
            {
                await LerpAllItemsToPosition(matchedItems,targetPos);
                _objectPool.GetParticleEffectFromPool(8,grid.GetCellCenterWorldVector2(targetPos));
                _tileItems[targetPos.x, targetPos.y]= _objectPool.GetItemFromPool(6,grid.GetCellCenterWorld(new Vector3Int(targetPos.x, targetPos.y, 0))).GetComponent<IItem>();
                _tileItems[targetPos.x, targetPos.y].Transform.parent = transform;
            }else if (matchedItems.Count == 5)
            {
                await LerpAllItemsToPosition(matchedItems,targetPos);
                _objectPool.GetParticleEffectFromPool(8,grid.GetCellCenterWorldVector2(targetPos));
                _tileItems[targetPos.x, targetPos.y]= _objectPool.GetItemFromPool(5,grid.GetCellCenterWorld(new Vector3Int(targetPos.x, targetPos.y, 0))).GetComponent<IItem>();
                _tileItems[targetPos.x, targetPos.y].Transform.parent = transform;
            }
        }
        private bool IsMovable(Vector2Int pos)
        {
            return IsInBoundaries(pos) && _tileItems[pos.x,pos.y]!=null && !lerpingItems.Contains(pos) &&
                   _tileItems[pos.x, pos.y].IsMovable;
        }

        private bool IsInBoundaries(Vector2Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
        }

        private async UniTask ChangeItemPositions(Vector2Int pos1, Vector2Int pos2, float lerpSpeed = 0.15f)
        {
            Vector2 pos1InitialPosition = grid.GetCellCenterLocalVector2(pos1);
            Vector2 pos2InitialPosition = grid.GetCellCenterLocalVector2(pos2);
            Sequence sequence = DOTween.Sequence();
            sequence.Insert(0,_tileItems[pos1.x, pos1.y].Transform.DOLocalMove(pos2InitialPosition, lerpSpeed));
            sequence.Insert(0,_tileItems[pos2.x, pos2.y].Transform.DOLocalMove(pos1InitialPosition, lerpSpeed));

            await sequence.AwaitForComplete();    
            SwapItemsInArray(pos1, pos2);
          
           
        }
      
        private void SwapItemsInArray(Vector2Int pos1, Vector2Int pos2)
        {
            (_tileItems[pos1.x, pos1.y], _tileItems[pos2.x, pos2.y]) = (_tileItems[pos2.x, pos2.y], _tileItems[pos1.x, pos1.y]);
        }
        
         private void FixedUpdate()
        {
            for (int x = 0; x < width; x++)
            {
                int spawnOffset = missingItems[x];
                for (int y = 0; y < height; y++)
                {
                    if (_tileItems[x, y] != null)
                    {
                        HandleNonEmptyTile(x, y);
                        continue;
                    }

                    int posToCheck = FindNextNonEmptyPosition(x, y);
                    int yOffset = spawnOffset - missingItems[x];

                    if (posToCheck == height)
                    {
                        SpawnNewItem(x, y, posToCheck + yOffset);
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

        private int FindNextNonEmptyPosition(int x, int y)
        {
            int posToCheck = y + 1;
            while (posToCheck < height && _tileItems[x, posToCheck] == null)
            {
                posToCheck++;
            }
            return posToCheck;
        }

        private void SpawnNewItem(int x, int y, int yOffset)
        {
            _tileItems[x, y] = _objectPool.GetRandomItemFromPool(
                grid.GetCellCenterWorld(new Vector3Int(x, yOffset, 0))).GetComponent<IItem>();
            _tileItems[x, y].Transform.parent = transform;
            missingItems[x]--;
        }

        private void MoveItem(int x, int y, int posToCheck)
        {
            _tileItems[x, y] = _tileItems[x, posToCheck];
            _tileItems[x, posToCheck] = null;
        }

        private bool CanMoveFromLeft(int x, int y)
        {
            return _tileItems[x - 1, y + 1] != null && _tileItems[x - 1, y] != null && IsMovable(new Vector2Int(x - 1, y + 1)) &&
                   !lerpingItems.Contains(new Vector2Int(x - 1, y + 1)) && !lerpingItems.Contains(new Vector2Int(x - 1, y));
        }

        private void MoveItemFromLeft(int x, int y)
        {
            _tileItems[x, y] = _tileItems[x - 1, y + 1];
            missingItems[x - 1]++;
            _tileItems[x - 1, y + 1] = null;
        }

        private bool CanMoveFromRight(int x, int y)
        {
            return _tileItems[x + 1, y + 1] != null && _tileItems[x + 1, y] != null && IsMovable(new Vector2Int(x + 1, y + 1)) &&
                   !lerpingItems.Contains(new Vector2Int(x + 1, y + 1)) && !lerpingItems.Contains(new Vector2Int(x + 1, y));
        }

        private void MoveItemFromRight(int x, int y)
        {
            _tileItems[x, y] = _tileItems[x + 1, y + 1];
            missingItems[x + 1]++;
            _tileItems[x + 1, y + 1] = null;
        }

        private void HandleNonEmptyTile(int x, int y)
        {
            if (IsItemMoving(x, y) && !swappingItems.Contains(new Vector2Int(x, y)))
            {
                MoveItemTowardsTarget(x, y);
            }
        }

        private bool IsItemMoving(int x, int y)
        {
            return Vector3.Distance(_tileItems[x, y].Transform.localPosition, grid.GetCellCenterLocal(new Vector3Int(x, y, 0))) > 0.01f;
        }

        private void MoveItemTowardsTarget(int x, int y)
        {
            Vector3 currPos = _tileItems[x, y].Transform.localPosition;
            Vector3 targetPos = grid.GetCellCenterLocal(new Vector3Int(x, y, 0));

            _tileItems[x, y].FallSpeed += _tileItems[x, y].Gravity;

            if (ShouldMoveDiagonally(currPos, targetPos))
            {
                currPos.x += Mathf.Sign(targetPos.x - currPos.x) * (_tileItems[x, y].FallSpeed * Time.deltaTime);
                currPos.y -= (_tileItems[x, y].FallSpeed * Time.deltaTime);
            }
            else
            {
                currPos.y -= (_tileItems[x, y].FallSpeed * Time.deltaTime);
            }

            if (IsItemCloseToTarget(currPos, targetPos))
            {
                FinishItemMovement(x, y, currPos);
            }
            else
            {
                _tileItems[x, y].Transform.localPosition = currPos;
            }
        }

        private bool ShouldMoveDiagonally(Vector3 currPos, Vector3 targetPos)
        {
            return Mathf.Abs(targetPos.x - currPos.x) > 0.01f && Mathf.Abs(targetPos.y - currPos.y) < 0.5f;
        }

        private bool IsItemCloseToTarget(Vector3 currPos, Vector3 targetPos)
        {
            return Vector3.Distance(targetPos, currPos) < 0.01f || targetPos.y > currPos.y;
        }

        private void FinishItemMovement(int x, int y, Vector3 currPos)
        {
            _tileItems[x, y].FallSpeed = 0f;
            _tileItems[x, y].Transform.localPosition = grid.GetCellCenterLocal(new Vector3Int(x, y, 0));
            lerpingItems.Remove(new Vector2Int(x, y));
            _tileItems[x, y].SortingOrder = 4;

            List<Vector2Int> cellMatches = _tileItems.CheckMatches(new Vector2Int(x, y), lerpingItems, width, height);

            if (cellMatches.Count >= 3)
            {
                HandleMatch(cellMatches, new Vector2Int(x, y)).Forget();
            }
        }



        private async UniTask LerpAllItemsToPosition(List<Vector2Int> combinedItems, Vector2Int targetPos, float lerpSpeed = 0.15f)
        {
            // Convert the target position to world coordinates
            Vector2 targetPosWorld = grid.GetCellCenterLocalVector2(targetPos);

            // Create a list to store all the lerp tasks
            List<UniTask> lerpTasks = new List<UniTask>();

            foreach (var item in combinedItems)
            {
                if (_tileItems[item.x, item.y] != null)
                {
                            // Start the lerp and add the task to the list
                            lerpTasks.Add(_tileItems[item.x, item.y].Transform.DOLocalMove(targetPosWorld, lerpSpeed).ToUniTask());
                }
            }

            // Wait for all the lerp tasks to complete
            await UniTask.WhenAll(lerpTasks);
            foreach (var item in combinedItems)
            {
                if (_tileItems[item.x, item.y] != null)
                {
                    _tileItems[item.x, item.y].OnMatch();
                    _tileItems[item.x, item.y] = null;
                }
            }
            
            
        }
        private async UniTask HandleMatchWithDelay(List<Vector2Int> matchedItems, float delayBetweenMatches = 0.1f,float delayToFillBoard=0.1f)
        {
            foreach (var item in matchedItems)
            {
                if (_tileItems[item.x, item.y] == null)
                {
                    Debug.Log("There is a null item in the matched items");
                    return;
                }
                lerpingItems.Add(item);
                _tileItems[item.x, item.y].OnMatch();
                _objectPool.GetParticleEffectFromPool(_tileItems.GetTypeID(item), grid.GetCellCenterWorldVector2(item));
                await UniTask.Delay((int)(delayBetweenMatches * 1000));
            }
            await UniTask.Delay((int)(delayToFillBoard * 1000));
            foreach (var item in matchedItems)
            {
                _tileItems[item.x, item.y] = null;
                missingItems[item.x]++;


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