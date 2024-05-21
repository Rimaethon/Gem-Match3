using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

public abstract class BoardItemBase : MonoBehaviour, IBoardItem
{
    #region Properties
    public  int ItemID
    {
        get => _itemID;
        set => _itemID = value;
    }
    public float FallSpeed
    {
        get => _fallSpeed;
        set=> _fallSpeed=Mathf.Clamp(value,1.5f, 5f);
    }
    public bool IsActive
    {
        get => _isActive;
        set=> _isActive=value; 
    }
    public bool IsFallAble 
    { 
        get=> isFallAble;
        set=> isFallAble=value;
    }

    public bool IsExplodeAbleByNearMatches => _isExplodeAbleByNearMatches;

    public bool IsGeneratorItem => _isGeneratorItem;
    public bool IsShuffleAble => _isShuffleAble;
    public bool IsProtectingUnderIt => _isProtectingUnderIt;
    public bool IsMatchable
    {
        get=> _isMatchable;
        set=> _isMatchable=value;
    }
  
    public bool IsSwappable
    {
        get=> _isSwappable;
        set=> _isSwappable=value;
    }
    public bool IsSwapping
    {
        get; 
        set;
    }
    public Vector2Int SwappingFrom
    {
        get;
        set;
    }
    public bool IsMoving
    {
        get;
        set;
    }
    public Vector2Int TargetToMove
    {
        get => _targetPosition;
        set=> _targetPosition=value;
    }

    public Vector2Int Position
    {
        get=> _position;
        set=> _position=value;
    }
    public bool IsBooster => _isBooster;
    public Transform Transform => transform;
    public bool IsHighlightAble => _isHighlightAble;

    public Board Board
    {
        get;
        set;
    }
    public bool IsExploding=>_isExploding;
    public bool IsMatching { get; set; }

    #endregion
    
    #region Fields
    public Vector2Int _position;

    [SerializeField] private Vector2Int _targetPosition;
    [SerializeField] protected int _itemID;
    [SerializeField] protected bool isFallAble = true;
    [SerializeField] protected bool _isMatchable = true;
    [SerializeField] protected bool _isSwappable = true;
    [SerializeField] protected bool _isExploding = false;
    [SerializeField] protected bool _isActive = true;
    [SerializeField] private float _fallSpeed;
    [SerializeField] protected bool _isHighlightAble = true;
    [SerializeField] protected bool _isExplodeAbleByNearMatches = false;
    [SerializeField] protected bool _isGeneratorItem = false;
    [SerializeField] protected bool _isShuffleAble;
    protected bool _isProtectingUnderIt=false;
    protected bool _isBooster = false;
    protected Color _unTouchedColor= new Color(1f,1f,1f,1f);
    private Material _material;
    private MaterialPropertyBlock _materialPropertyBlock;
    private SpriteRenderer _spriteRenderer;
    protected IBoardItem BoardItem;
    protected bool _isClicked;
    private Tween _highlightShake;
    #endregion

    public virtual void SetSortingOrder(int order)
    {
        _spriteRenderer.sortingOrder = order;
    }
    protected virtual void Awake()
    {
        BoardItem = GetComponent<IBoardItem>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        if (gameObject.TryGetComponent(out _spriteRenderer))
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
            _spriteRenderer.color = _unTouchedColor;
        }
    }
    public void Highlight(float value)
    {
        value=value>=0.5f?1f:0f;
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat("_IsOutlineEnabled",value);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
        if (value == 1)
        {
            _highlightShake=transform.DOShakeScale(0.8f, 0.08f, 5,90).SetLoops(-1,LoopType.Yoyo).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
        }
        else
        {
            _highlightShake.Kill();
        }
    }
    public virtual void OnExplode()
    {
    }
    public virtual void OnRemove()
    {
    }
    public virtual void OnTouch()
    {
    }
    public virtual void OnMatch()
    {
    }
    public virtual void OnClick(Board board, Vector2Int pos)
    {
    }
    public virtual void OnSwap(IBoardItem boardItem, IBoardItem otherBoardItem)
    {
    }
}


