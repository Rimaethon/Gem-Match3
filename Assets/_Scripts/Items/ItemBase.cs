using System;
using DG.Tweening;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour, IItem
{

    public int ItemType
    {
        get => _itemType;
        set => _itemType = value;
    }

    public float FallSpeed
    {
     get => _fallSpeed;
     set=> _fallSpeed=Mathf.Clamp(value,0,6f);
    }

    public virtual int SortingOrder { get=> _spriteRenderer.sortingOrder; set=> _spriteRenderer.sortingOrder=value; }

    public bool IsMovable { get=> _isMovable; set=> _isMovable=value; }
    private bool _isMovable = true;
    
    public float Gravity => _gravity;
    protected Color _touchedColor= new Color(0.88f,0.88f,0.88f,1f);

    private float _gravity = 0.4f;
    private float _fallSpeed = 0f;
    public bool IsMoving { get; set; }
    private SpriteRenderer _spriteRenderer;
    protected virtual void Awake()
    {
        if (gameObject.TryGetComponent<SpriteRenderer>(out  _spriteRenderer))
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _spriteRenderer.color = _touchedColor;
        }
       

    }

    public Transform Transform => transform;

    [SerializeField] private int _itemType;
    
    
    public virtual void OnMatch()
    {
        gameObject.SetActive(false);
        
    }
    public virtual void OnTouch()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _spriteRenderer.color==_touchedColor?Color.white:_touchedColor;           
        }
    }

    public virtual void OnClick(IItem[,] board,Vector2Int pos)
    {
        
    }
        

}

public interface IItem
{
    public int ItemType { get; set; }
    public bool IsMoving { get; set; }
    public float FallSpeed { get; set; }
    public int SortingOrder { get; set; }
    public bool IsMovable { get; set; }
    public float Gravity { get; }
    public void OnMatch();
    public void OnTouch();
    public void OnClick(IItem[,] board,Vector2Int pos);
    public Transform Transform { get; }
}
