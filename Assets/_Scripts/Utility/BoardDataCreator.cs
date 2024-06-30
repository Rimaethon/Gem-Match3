using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Scripts.Utility
{
    [RequireComponent(typeof(SpriteRenderer))][ExecuteInEditMode]
    public class BoardDataCreator:MonoBehaviour
    {
        public int BoardID;
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [SerializeField] ItemIDMatrixCreator normalItemIDMatrixCreator;
        [SerializeField] ItemIDMatrixCreator underlayItemIDMatrixCreator;
        [SerializeField] ItemIDMatrixCreator overlayItemIDMatrixCreator;
        [ShowInInspector][ReadOnly] private int _boardWidth;
        [ShowInInspector][ReadOnly] private int _boardHeight;
        private SpriteRenderer _spriteRenderer;
        private BoardSpriteSaveData _boardSpriteSaveData;
        private BoardData _board;
        private void Awake()
        {
            if(itemDatabase==null)
                throw new Exception("Item Database is null");
            _spriteRenderer = GetComponent<SpriteRenderer>();
            GetBoardSpriteData();
            InitializeItemMatrices();
        }
        [Button]
        public void GetBoardSpriteData()
        {
            _boardSpriteSaveData= itemDatabase.GetBoardSpriteData(BoardID);
            _spriteRenderer.sprite=_boardSpriteSaveData.Sprite;
            _boardWidth = _boardSpriteSaveData.Width;
            _boardHeight = _boardSpriteSaveData.Height;
            normalItemIDMatrixCreator.boardSpriteSaveData = _boardSpriteSaveData;
            underlayItemIDMatrixCreator.boardSpriteSaveData = _boardSpriteSaveData;
            overlayItemIDMatrixCreator.boardSpriteSaveData = _boardSpriteSaveData;
        }
        [Button]
        public void ResetAllTilemaps()
        {
            normalItemIDMatrixCreator.ResetTilemap();
            underlayItemIDMatrixCreator.ResetTilemap();
            overlayItemIDMatrixCreator.ResetTilemap();
        }
        [Button]
        public void InitializeItemMatrices()
        {
            normalItemIDMatrixCreator.InitializeItemIDMatrix(_boardWidth, _boardHeight);
            underlayItemIDMatrixCreator.InitializeItemIDMatrix(_boardWidth, _boardHeight);
            overlayItemIDMatrixCreator.InitializeItemIDMatrix(_boardWidth, _boardHeight);
        }
        public BoardData CreateBoardData()
        {
            int[,] normalItemIds = normalItemIDMatrixCreator.ItemIDMatrix;
            Dictionary<Vector2Int,int> underlayItemIds= new Dictionary<Vector2Int,int>();
            Dictionary<Vector2Int,int> overlayItemIds= new Dictionary<Vector2Int,int>();
            CreateDictionaryFromMatrix(overlayItemIDMatrixCreator.ItemIDMatrix,overlayItemIds);
            CreateDictionaryFromMatrix(underlayItemIDMatrixCreator.ItemIDMatrix,underlayItemIds);
            _board = new BoardData(BoardID,Vector3.zero, normalItemIds, underlayItemIds, overlayItemIds);
            return _board;
        }
        private void CreateDictionaryFromMatrix(int[,] matrix, Dictionary<Vector2Int,int> dictionary)
        {
            dictionary.Clear();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] != -1)
                    {
                        dictionary.Add( new Vector2Int(i,j),matrix[i, j]);
                    }
                }
            }
        }
    }
}
