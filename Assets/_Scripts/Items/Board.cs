using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using UnityEngine;

namespace Scripts
{
    [Serializable]
    public class Board
    {
        public Cell[,] Cells;
        public int Width => _width;
        public int Height=> _height;
        private int _width;
        private int _height;
        public Sprite BoardSprite;
        public Vector3 boardPosition;
        public readonly GameObject _boardInstance;
        public readonly List<int> _spawnAbleFillerItemIds;

        public Board(BoardSpriteSaveData boardSpriteSaveData, BoardData boardData, GameObject boardInstance,List<int> spawnAbleFillerItemIds)
        {
            _boardInstance = boardInstance;
            _width = boardSpriteSaveData.Width;
            _height = boardSpriteSaveData.Height;
            Cells = new Cell[_width, _height];
            BoardSprite = boardSpriteSaveData.Sprite;
            boardPosition = boardData.BoardPosition;
            _spawnAbleFillerItemIds = spawnAbleFillerItemIds;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Cells[x, y] = new Cell(new Vector2Int(x, y));
                    if (boardSpriteSaveData.BlankCells.Contains(new Vector2Int(x, y)))
                    {
                        Cells[x, y].IsNotInBoard = true;
                        continue;
                    }
                    if(boardData.NormalItemIds[x,y]!=-1)
                        Cells[x, y].SetItem( ObjectPool.Instance
                            .GetItem(boardData.NormalItemIds[x,y], LevelGrid.Instance.GetCellCenterWorld(new Vector2Int(x,y)),this));
                    if (boardData.UnderlayItemIds.TryGetValue(new Vector2Int(x, y), out var itemID))
                    {
                        Cells[x, y].SetUnderLayItem( ObjectPool.Instance
                            .GetItem(itemID, LevelGrid.Instance.GetCellCenterWorld(new Vector2Int(x,y)),this));
                    }
                     
                    if (boardData.OverlayItemIds.TryGetValue(new Vector2Int(x, y), out itemID))
                        Cells[x, y].SetOverLayItem( ObjectPool.Instance
                            .GetItem(itemID, LevelGrid.Instance.GetCellCenterWorld(new Vector2Int(x,y)),this));
                }
            }

        }

    }
}