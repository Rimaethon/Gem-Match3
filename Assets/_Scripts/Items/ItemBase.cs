using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour, IItem
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
        set=> _fallSpeed=Mathf.Clamp(value,0,5f);
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
    public float Gravity => _gravity;
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
    protected bool _isBooster = false;
    protected Color _unTouchedColor= new Color(0.88f,0.88f,0.88f,1f);
    private readonly float _gravity = 0.7f; 
    private Material _material;
    private MaterialPropertyBlock _materialPropertyBlock;
    private SpriteRenderer _spriteRenderer;
    protected IItem Item;
    protected bool _isClicked;
    #endregion

    public virtual void SetSortingOrder(int order)
    {
        _spriteRenderer.sortingOrder = order;
    }

   

    protected virtual void Awake()
    {
        Item = GetComponent<IItem>();
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

    public virtual void OnSwap(IItem item, IItem otherItem)
    {
    }
}


