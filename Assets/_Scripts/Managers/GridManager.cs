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
        }

        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector2Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnTouch, OnTouch);
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
                    await HandleMatchWithDelay(firstCellMatches, 0, 0);
                }
                
                if(secondCellMatches.Count>=3)
                {
                    await HandleMatchWithDelay(secondCellMatches, 0, 0);

                }
            }
            _isSwiping = false;
            swappingItems.Remove(firstCellPos);
            swappingItems.Remove(secondCellPos);
            lerpingItems.Remove(firstCellPos);
            lerpingItems.Remove(secondCellPos);
            OnTouch(clickPos);
            
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
               

                int spawnOffset = missingItems[x] ;
                for (int y = 0; y<height; y++)
                {

                    if (_tileItems[x, y] == null)
                    {
                        int posToCheck = y+1;
                        while (posToCheck < height)
                        {
                            if (_tileItems[x, posToCheck] == null)
                            {
                                posToCheck++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        int yOffset = spawnOffset - missingItems[x];

                        if (posToCheck == height)
                        {
                            _tileItems[x, y] = _objectPool.GetRandomItemFromPool(
                                grid.GetCellCenterWorld(new Vector3Int(x, posToCheck+yOffset, 0))).GetComponent<IItem>();
                            Debug.Log(x+" "+y+" has a pos of "+grid.GetCellCenterWorld(new Vector3Int(x, posToCheck+yOffset, 0)));
                            _tileItems[x, y].Transform.parent = transform;
                            missingItems[x]--;
                        }else if(_tileItems[x,posToCheck]!=null )
                        {
                            if (_tileItems[x,posToCheck].IsMovable
                                &&Mathf.Abs(_tileItems[x,posToCheck].Transform.localPosition.x-grid.GetCellCenterLocal(new Vector3Int(x,posToCheck,0)).x)<0.01f)
                                
                            {
                                _tileItems[x, y] = _tileItems[x, posToCheck];

                                _tileItems[x, posToCheck] = null;
                                
                            }else if(x>0&&_tileItems[x-1,y+1]!=null&&_tileItems[x-1,y]!=null&&_tileItems[x-1,y+1].IsMovable
                                     &&!lerpingItems.Contains(new Vector2Int(x-1,y+1))&&!lerpingItems.Contains(new Vector2Int(x-1,y)))
                            {
                                if (y > 0 && _tileItems[x - 1, y - 1] != null)
                                {
                                    if(_tileItems[x,y-1]!=null&&_tileItems[x,y-1].Transform.localPosition==grid.GetCellCenterLocal(new Vector3Int(x,y-1))&&!lerpingItems.Contains(new Vector2Int(x-1,y-1)))
                                    {
                                        _tileItems[x, y] = _tileItems[x - 1,y+1];
                                        missingItems[x-1]++;

                                        _tileItems[x - 1, y+1] = null;
                                    }
                                }
                                else
                                {
                                    _tileItems[x, y] = _tileItems[x - 1,y+1];
                                    missingItems[x-1]++;

                                    _tileItems[x - 1, y+1] = null;
                                }
                                
                            }else if (x<width-1&&_tileItems[x+1,y+1]!=null&&_tileItems[x+1,y]!=null&&_tileItems[x+1,y+1].IsMovable
                                      &&!lerpingItems.Contains(new Vector2Int(x+1,y+1))&&!lerpingItems.Contains(new Vector2Int(x+1,y)))
                            {

                                if (y > 0 && _tileItems[x + 1, y - 1] != null)
                                {
                                    if(_tileItems[x,y-1]!=null&&(_tileItems[x,y-1].Transform.localPosition==grid.GetCellCenterLocal(new Vector3Int(x,y-1))&&!lerpingItems.Contains(new Vector2Int(x+1,y-1))))
                                    {
                                        _tileItems[x, y] = _tileItems[x + 1,y+1];
                                        missingItems[x+1]++;

                                        _tileItems[x + 1, y+1] = null;
                                    }
                                }
                                else
                                {
                                    _tileItems[x, y] = _tileItems[x + 1,y+1];
                                    missingItems[x+1]++;

                                    _tileItems[x + 1, y+1] = null;
                                }
                            }
                            
                        }
                        if(_tileItems[x,y]!=null)_tileItems[x, y].SortingOrder = 5;


                    }else if(Vector3.Distance(_tileItems[x,y].Transform.localPosition,grid.GetCellCenterLocal(new Vector3Int(x,y,0)))>0.01f&&!swappingItems.Contains(new Vector2Int(x,y)))
                    {
                        Vector3 currPos = _tileItems[x, y].Transform.localPosition;
                        Vector3 targetPos = grid.GetCellCenterLocal(new Vector3Int(x, y, 0));

                        // Calculate the direction vector from the current position to the target position

                        _tileItems[x,y].FallSpeed+=_tileItems[x,y].Gravity;
                        
                        // Move the item in the direction of the target position
                        if (Mathf.Abs(targetPos.x- currPos.x)>0.01f&&Mathf.Abs(targetPos.y- currPos.y)<0.5f)
                        {
                            currPos.x += Mathf.Sign(targetPos.x-currPos.x) *(_tileItems[x, y].FallSpeed * Time.deltaTime);
                            currPos.y -=(_tileItems[x, y].FallSpeed * Time.deltaTime);
                         

                        }else
                        {
                            currPos.y -= (_tileItems[x, y].FallSpeed * Time.deltaTime);

                        }
                        if(Vector3.Distance(grid.GetCellCenterLocal(new Vector3Int(x, y, 0)),currPos)<0.01f||grid.GetCellCenterLocal(new Vector3Int(x, y, 0)).y>currPos.y)
                        {
                            Debug.Log("Lerping is done since " + Vector3.Distance(grid.GetCellCenterLocal(new Vector3Int(x, y, 0)),currPos)+" "+x+" "+y);
                            _tileItems[x, y].FallSpeed = 0f;
                            _tileItems[x,y].Transform.localPosition =  grid.GetCellCenterLocal(new Vector3Int(x, y, 0));
                            Debug.Log(_tileItems[x,y].Transform.localPosition+" "+grid.GetCellCenterLocal(new Vector3Int(x, y, 0)));
                            lerpingItems.Remove(new Vector2Int(x, y)); 
                            _tileItems[x, y].SortingOrder = 4;

                        }else
                        {
                            _tileItems[x, y].Transform.localPosition = currPos;
                        }
                       

                    }
                    
                   
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
                _tileItems[item.x, item.y].OnMatch();
                _objectPool.GetParticleEffectFromPool(_tileItems.GetTypeID(item), grid.GetCellCenterWorldVector2(item));
                await UniTask.Delay((int)(delayBetweenMatches * 1000));
            }
            await UniTask.Delay((int)(delayToFillBoard * 1000));
            foreach (var item in matchedItems)
            {
                _tileItems[item.x, item.y] = null;
                missingItems[item.x]++;
                lerpingItems.Add(item);


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