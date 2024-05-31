using System.Collections.Generic;
using _Scripts.Data_Classes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Scripts.Utility
{
    public class ItemIDMatrixCreator:MonoBehaviour
    {
        [ShowInInspector] [TableMatrix( DrawElementMethod = "DrawElement",RowHeight = 20,IsReadOnly = true,HideColumnIndices = true,HideRowIndices = true)]
        public int[,] ItemIDMatrix;
        public HashSet<Vector2Int> BlankCells;
        [ShowInInspector][ReadOnly]
        private int _boardWidth;
        [ShowInInspector][ReadOnly]
        private  int _boardHeight;
        private Tilemap _tilemap;
        
        
        [Button]
        public void ResetTilemap()
        {
            _tilemap = GetComponent<Tilemap>();
            _tilemap.ClearAllTiles();
        }

        [Button]
        public void GetItemIDFromTilemap()
        {
            _tilemap = GetComponent<Tilemap>();
            _tilemap.CompressBounds();
            InitializeItemIDMatrix(_boardWidth, _boardHeight);
            foreach (Vector3Int pos in _tilemap.cellBounds.allPositionsWithin)
            {

                TileBase tile = _tilemap.GetTile(pos);
                if(tile==null)
                    continue;
                var tileItem=tile as ItemTileDataSO;
                Debug.Log("Tile Item: "+pos);
                ItemIDMatrix[pos.x, pos.y] = tileItem.gameObject.GetComponent<BoardItemBase>().ItemID;
            }
        }
        public void InitializeItemIDMatrix(int width, int height)
        {
            _boardWidth = width;
            _boardHeight = height;
            ItemIDMatrix = new int[_boardWidth, _boardHeight];
            for (int i = 0; i < _boardWidth; i++)
            {
                for (int j = 0; j < _boardHeight; j++)
                {
                    ItemIDMatrix[i, j] = -1;
                }
            }
        }

        public bool IsInBounds(Vector3Int position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= _boardWidth || position.y >= _boardHeight||(BlankCells!=null&&BlankCells.Contains(new Vector2Int(position.x,position.y))))
                return false;
            return true;
        }
        private int DrawElement(Rect rect, int x, int y)
        {
            GUI.Box(rect,ItemIDMatrix[x,_boardHeight-y-1].ToString());
            return 1;

        }
    }
}