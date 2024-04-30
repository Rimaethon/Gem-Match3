using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Scripts.Data_Classes
{
  [CreateAssetMenu(fileName = "ItemTile", menuName = "Tiles/ItemTile")]
  public class ItemTileDataSO : TileBase
  {
    [SerializeField]
    private Sprite m_Sprite;
    [SerializeField]
    private Color m_Color = Color.white;
    [SerializeField]
    private Matrix4x4 m_Transform = Matrix4x4.identity;
    [SerializeField]
    private GameObject m_InstancedGameObject;
    [SerializeField]
    private TileFlags m_Flags = TileFlags.LockColor;
    [SerializeField]
    private Tile.ColliderType m_ColliderType = Tile.ColliderType.Sprite;

    /// <summary>
    ///   <para>Sprite to be rendered at the Tile.</para>
    /// </summary>
    public Sprite sprite
    {
      get => this.m_Sprite;
      set => this.m_Sprite = value;
    }

    /// <summary>
    ///   <para>Color of the Tile.</para>
    /// </summary>
    public Color color
    {
      get => this.m_Color;
      set => this.m_Color = value;
    }

    /// <summary>
    ///   <para>Matrix4x4|Transform matrix of the Tile.</para>
    /// </summary>
    public Matrix4x4 transform
    {
      get => this.m_Transform;
      set => this.m_Transform = value;
    }

    /// <summary>
    ///   <para>GameObject of the Tile.</para>
    /// </summary>
    public GameObject gameObject
    {
      get => this.m_InstancedGameObject;
      set => this.m_InstancedGameObject = value;
    }

    /// <summary>
    ///   <para>TileFlags of the Tile.</para>
    /// </summary>
    public TileFlags flags
    {
      get => this.m_Flags;
      set => this.m_Flags = value;
    }

    public Tile.ColliderType colliderType
    {
      get => this.m_ColliderType;
      set => this.m_ColliderType = value;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
      tileData.sprite = this.m_Sprite;
      tileData.color = this.m_Color;
      tileData.transform = this.m_Transform;
      tileData.gameObject = this.m_InstancedGameObject;
      tileData.flags = this.m_Flags;
      tileData.colliderType = this.m_ColliderType;
    }

    /// <summary>
    ///   <para>Enum for determining what collider shape is generated for this Tile by the TilemapCollider2D.</para>
    /// </summary>
    public enum ColliderType
    {
      /// <summary>
      ///   <para>No collider shape is generated for the Tile by the TilemapCollider2D.</para>
      /// </summary>
      None,
      /// <summary>
      ///   <para>The Sprite outline is used as the collider shape for the Tile by the TilemapCollider2D.</para>
      /// </summary>
      Sprite,
      /// <summary>
      ///   <para>The grid layout boundary outline is used as the collider shape for the Tile by the TilemapCollider2D.</para>
      /// </summary>
      Grid,
    }
  }
}
