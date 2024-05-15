using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using _Scripts.Utility;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Scripts.Editor
{
    [CustomGridBrush(false, false, false, "Level Creator Brush")]
    public class CustomGridBrush : GridBrushBase
    {
        private static readonly Matrix4x4 SClockwise = new(new Vector4(0f, -1f, 0f, 0f),
            new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0f, 0f, 1f));

        private static readonly Matrix4x4 SCounterClockwise = new(new Vector4(0f, 1f, 0f, 0f),
            new Vector4(-1f, 0f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0f, 0f, 1f));

        private static readonly Matrix4x4 S180Rotate = new(new Vector4(-1f, 0f, 0f, 0f),
            new Vector4(0f, -1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0f, 0f, 1f));

        [SerializeField] [HideInInspector] private BrushCell[] mCells;

        [SerializeField] [HideInInspector] private Vector3Int mSize;

        [SerializeField] [HideInInspector] private Vector3Int mPivot;

        [SerializeField] [HideInInspector] private bool mCanChangeZPosition;

        [SerializeField] private bool mFloodFillContiguousOnly = true;
        private BrushCell[] _mStoredCells;
        private Vector3Int _mStoredPivot;

        private Vector3Int _mStoredSize;

        [SerializeField] [HideInInspector] private List<TileChangeData> _mTileChangeDataList;

        public CustomGridBrush()
        {
            Init(Vector3Int.one, Vector3Int.zero);
            SizeUpdated();
        }

        /// <summary>Size of the brush in cells. </summary>
        public Vector3Int size
        {
            get => mSize;
            set
            {
                mSize = value;
                SizeUpdated();
            }
        }

        /// <summary>Pivot of the brush. </summary>
        public Vector3Int Pivot
        {
            get => mPivot;
            set => mPivot = value;
        }

        /// <summary>All the brush cells the brush holds. </summary>
        public BrushCell[] Cells => mCells;

        /// <summary>Number of brush cells in the brush.</summary>
        public int CellCount => mCells != null ? mCells.Length : 0;

        /// <summary>Whether the brush can change Z Position</summary>
        public bool CanChangeZPosition
        {
            get => mCanChangeZPosition;
            set => mCanChangeZPosition = value;
        }

        /// <summary>Clears all data of the brush.</summary>
        public void Reset()
        {
            UpdateSizeAndPivot(Vector3Int.one, Vector3Int.zero);
        }

        /// <summary>Initializes the content of the GridBrush.</summary>
        /// <param name="size">Size of the GridBrush.</param>
        public void Init(Vector3Int size)
        {
            Init(size, Vector3Int.zero);
            SizeUpdated();
        }

        /// <summary>Initializes the content of the GridBrush.</summary>
        /// <param name="size">Size of the GridBrush.</param>
        /// <param name="pivot">Pivot point of the GridBrush.</param>
        public void Init(Vector3Int size, Vector3Int pivot)
        {
            mSize = size;
            mPivot = pivot;
            SizeUpdated();
        }

        /// <summary>Paints tiles and GameObjects into a given position within the selected layers.</summary>
        /// <param name="gridLayout">Grid used for layout.</param>
        /// <param name="brushTarget">Target of the paint operation. By default the currently selected GameObject.</param>
        /// <param name="position">The coordinates of the cell to paint data to.</param>
        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            var tilemap = brushTarget.GetComponent<Tilemap>();
            var matrixCreator = tilemap.gameObject.GetComponent<ItemIDMatrixCreator>();
            if (matrixCreator != null && !matrixCreator.IsInBounds(position))
                return;
            var min = position - Pivot;
            var bounds = new BoundsInt(min, mSize);
            BoxFill(gridLayout, brushTarget, bounds);
            // Access the RuleTile's GameObject
            if (tilemap != null)
            {
                var tileBase = tilemap.GetTile(position);
                var tile = tileBase as ItemTileDataSO;
                if(matrixCreator.ItemIDMatrix==null)
                    matrixCreator.GetComponentInParent<BoardDataCreator>().InitializeItemMatrices();
                matrixCreator.ItemIDMatrix[position.x, position.y] = tile.gameObject.GetComponent<BoardItemBase>().ItemID;
                Debug.Log("RuleTile GameObject: " + tile.name + " " + position);
            }
        }

        /// <summary>Erases tiles and GameObjects in a given position within the selected layers.</summary>
        /// <param name="gridLayout">Grid used for layout.</param>
        /// <param name="brushTarget">Target of the erase operation. By default the currently selected GameObject.</param>
        /// <param name="position">The coordinates of the cell to erase data from.</param>
        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            var tilemap = brushTarget.GetComponent<Tilemap>();
            var matrixCreator = tilemap.gameObject.GetComponent<ItemIDMatrixCreator>();
            if (matrixCreator != null && !matrixCreator.IsInBounds(position))
                return;

            var min = position - Pivot;
            var bounds = new BoundsInt(min, mSize);
            BoxErase(gridLayout, brushTarget, bounds);
            matrixCreator.ItemIDMatrix[position.x, position.y] = -1;
            Debug.Log("Object Erased At: " + position);
        }


        /// <summary>Box fills tiles and GameObjects into given bounds within the selected layers.</summary>
        /// <param name="gridLayout">Grid to box fill data to.</param>
        /// <param name="brushTarget">Target of the box fill operation. By default the currently selected GameObject.</param>
        /// <param name="position">The bounds to box fill data into.</param>
        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            if (brushTarget == null)
                return;

            var map = brushTarget.GetComponent<Tilemap>();
            if (map == null)
                return;

            var count = 0;
            var listSize = position.size.x * position.size.y * position.size.z;
            if (_mTileChangeDataList == null || _mTileChangeDataList.Capacity != listSize)
                _mTileChangeDataList = new List<TileChangeData>(listSize);
            _mTileChangeDataList.Clear();
            foreach (var location in position.allPositionsWithin)
            {
                var local = location - position.min;
                var cell = mCells[GetCellIndexWrapAround(local.x, local.y, local.z)];
                if (cell.tile == null)
                    continue;

                var tcd = new TileChangeData
                    { position = location, tile = cell.tile, transform = cell.matrix, color = cell.color };
                _mTileChangeDataList.Add(tcd);
                count++;
            }

            // Duplicate empty slots in the list, as ExtractArrayFromListT returns full list
            if (0 < count && count < listSize)
            {
                var tcd = _mTileChangeDataList[count - 1];
                for (var i = count; i < listSize; ++i) _mTileChangeDataList.Add(tcd);
            }

            var tileChangeData = _mTileChangeDataList.ToArray();
            map.SetTiles(tileChangeData, false);
        }

        /// <summary>Erases tiles and GameObjects from given bounds within the selected layers.</summary>
        /// <param name="gridLayout">Grid to erase data from.</param>
        /// <param name="brushTarget">Target of the erase operation. By default the currently selected GameObject.</param>
        /// <param name="position">The bounds to erase data from.</param>
        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            if (brushTarget == null)
                return;

            var map = brushTarget.GetComponent<Tilemap>();
            if (map == null)
                return;

            var identity = Matrix4x4.identity;
            var listSize = Math.Abs(position.size.x * position.size.y * position.size.z);
            if (_mTileChangeDataList == null || _mTileChangeDataList.Capacity != listSize)
                _mTileChangeDataList = new List<TileChangeData>(listSize);
            _mTileChangeDataList.Clear();
            foreach (var location in position.allPositionsWithin)
                _mTileChangeDataList.Add(new TileChangeData
                    { position = location, tile = null, transform = identity, color = Color.white });

            var tileChangeData = _mTileChangeDataList.ToArray();
            map.SetTiles(tileChangeData, false);
        }

        /// <summary>Flood fills tiles and GameObjects starting from a given position within the selected layers.</summary>
        /// <param name="gridLayout">Grid used for layout.</param>
        /// <param name="brushTarget">Target of the flood fill operation. By default the currently selected GameObject.</param>
        /// <param name="position">Starting position of the flood fill.</param>
        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (CellCount == 0)
                return;

            if (brushTarget == null)
                return;

            var map = brushTarget.GetComponent<Tilemap>();
            if (map == null)
                return;

            if (mFloodFillContiguousOnly)
            {
                map.FloodFill(position, Cells[0].tile);
            }
            else
            {
                var tile = map.GetTile(position);
                if (tile != null && tile != Cells[0].tile)
                    map.SwapTile(tile, Cells[0].tile);
                else
                    map.FloodFill(position, Cells[0].tile);
            }
        }

        /// <summary>Rotates the brush by 90 degrees in the given direction.</summary>
        /// <param name="direction">Direction to rotate by.</param>
        /// <param name="layout">Cell Layout for rotating.</param>
        public override void Rotate(RotationDirection direction, GridLayout.CellLayout layout)
        {
            switch (layout)
            {
                case GridLayout.CellLayout.Hexagon:
                    RotateHexagon(direction);
                    break;
                case GridLayout.CellLayout.Isometric:
                case GridLayout.CellLayout.IsometricZAsY:
                case GridLayout.CellLayout.Rectangle:
                {
                    var oldSize = mSize;
                    var oldCells = mCells.Clone() as BrushCell[];
                    size = new Vector3Int(oldSize.y, oldSize.x, oldSize.z);
                    var oldBounds = new BoundsInt(Vector3Int.zero, oldSize);

                    foreach (var oldPos in oldBounds.allPositionsWithin)
                    {
                        var newX = direction == RotationDirection.Clockwise ? oldPos.y : oldSize.y - oldPos.y - 1;
                        var newY = direction == RotationDirection.Clockwise ? oldSize.x - oldPos.x - 1 : oldPos.x;
                        var toIndex = GetCellIndex(newX, newY, oldPos.z);
                        var fromIndex = GetCellIndex(oldPos.x, oldPos.y, oldPos.z, oldSize.x, oldSize.y, oldSize.z);
                        mCells[toIndex] = oldCells[fromIndex];
                    }

                    var newPivotX = direction == RotationDirection.Clockwise ? Pivot.y : oldSize.y - Pivot.y - 1;
                    var newPivotY = direction == RotationDirection.Clockwise ? oldSize.x - Pivot.x - 1 : Pivot.x;
                    Pivot = new Vector3Int(newPivotX, newPivotY, Pivot.z);

                    var rotation = direction == RotationDirection.Clockwise ? SClockwise : SCounterClockwise;
                    var counterRotation =
                        direction != RotationDirection.Clockwise ? SClockwise : SCounterClockwise;
                    foreach (var cell in mCells)
                    {
                        var oldMatrix = cell.matrix;
                        var counter = (oldMatrix.lossyScale.x < 0) ^ (oldMatrix.lossyScale.y < 0);
                        cell.matrix = oldMatrix * (counter ? counterRotation : rotation);
                    }
                }
                    break;
            }
        }

        private static Vector3Int RotateHexagonPosition(RotationDirection direction, Vector3Int position)
        {
            var cube = HexagonToCube(position);
            var rotatedCube = Vector3Int.zero;
            if (RotationDirection.Clockwise == direction)
            {
                rotatedCube.x = -cube.y;
                rotatedCube.y = -cube.z;
                rotatedCube.z = -cube.x;
            }
            else
            {
                rotatedCube.x = -cube.z;
                rotatedCube.y = -cube.x;
                rotatedCube.z = -cube.y;
            }

            return CubeToHexagon(rotatedCube);
        }

        private void RotateHexagon(RotationDirection direction)
        {
            var oldCells = mCells.Clone() as BrushCell[];
            var oldPivot = new Vector3Int(Pivot.x, Pivot.y, Pivot.z);
            var oldSize = new Vector3Int(size.x, size.y, size.z);
            var minSize = Vector3Int.zero;
            var maxSize = Vector3Int.zero;
            var oldBounds = new BoundsInt(Vector3Int.zero, oldSize);
            foreach (var oldPos in oldBounds.allPositionsWithin)
            {
                if (oldCells[GetCellIndex(oldPos.x, oldPos.y, oldPos.z, oldSize.x, oldSize.y, oldSize.z)].tile == null)
                    continue;
                var pos = RotateHexagonPosition(direction, oldPos - oldPivot);
                minSize.x = Mathf.Min(minSize.x, pos.x);
                minSize.y = Mathf.Min(minSize.y, pos.y);
                maxSize.x = Mathf.Max(maxSize.x, pos.x);
                maxSize.y = Mathf.Max(maxSize.y, pos.y);
            }

            var newSize = new Vector3Int(1 + maxSize.x - minSize.x, 1 + maxSize.y - minSize.y, oldSize.z);
            var newPivot = new Vector3Int(-minSize.x, -minSize.y, oldPivot.z);
            UpdateSizeAndPivot(newSize, new Vector3Int(newPivot.x, newPivot.y, newPivot.z));
            foreach (var oldPos in oldBounds.allPositionsWithin)
            {
                if (oldCells[GetCellIndex(oldPos.x, oldPos.y, oldPos.z, oldSize.x, oldSize.y, oldSize.z)].tile == null)
                    continue;
                var newPos =
                    RotateHexagonPosition(direction, new Vector3Int(oldPos.x, oldPos.y, oldPos.z) - oldPivot) +
                    newPivot;
                mCells[GetCellIndex(newPos.x, newPos.y, newPos.z)] =
                    oldCells[GetCellIndex(oldPos.x, oldPos.y, oldPos.z, oldSize.x, oldSize.y, oldSize.z)];
            }
            // Do not rotate hexagon cell matrix, as hexagon cells are not perfect hexagons
        }

        private static Vector3Int HexagonToCube(Vector3Int position)
        {
            var cube = Vector3Int.zero;
            cube.x = position.x - (position.y - (position.y & 1)) / 2;
            cube.z = position.y;
            cube.y = -cube.x - cube.z;
            return cube;
        }

        private static Vector3Int CubeToHexagon(Vector3Int position)
        {
            var hexagon = Vector3Int.zero;
            hexagon.x = position.x + (position.z - (position.z & 1)) / 2;
            hexagon.y = position.z;
            hexagon.z = 0;
            return hexagon;
        }

        /// <summary>Flips the brush in the given axis.</summary>
        /// <param name="flip">Axis to flip by.</param>
        /// <param name="layout">Cell Layout for flipping.</param>
        public override void Flip(FlipAxis flip, GridLayout.CellLayout layout)
        {
            if (flip == FlipAxis.X)
                FlipX(layout);
            else
                FlipY(layout);
        }

        /// <summary>Picks tiles from selected Tilemaps and child GameObjects, given the coordinates of the cells.</summary>
        /// <param name="gridLayout">Grid to pick data from.</param>
        /// <param name="brushTarget">Target of the picking operation. By default the currently selected GameObject.</param>
        /// <param name="position">The coordinates of the cells to paint data from.</param>
        /// <param name="pickStart">Pivot of the picking brush.</param>
        public override void Pick(GridLayout gridLayout, GameObject brushTarget, BoundsInt position,
            Vector3Int pickStart)
        {
            Reset();
            UpdateSizeAndPivot(new Vector3Int(position.size.x, position.size.y, 1),
                new Vector3Int(pickStart.x, pickStart.y, 0));

            if (brushTarget == null)
                return;

            var tilemap = brushTarget.GetComponent<Tilemap>();
            foreach (var pos in position.allPositionsWithin)
            {
                var brushPosition = new Vector3Int(pos.x - position.x, pos.y - position.y, 0);
                PickCell(pos, brushPosition, tilemap);
            }
        }

        private void PickCell(Vector3Int position, Vector3Int brushPosition, Tilemap tilemap)
        {
            if (tilemap == null)
                return;

            SetTile(brushPosition, tilemap.GetTile(position));
            SetMatrix(brushPosition, tilemap.GetTransformMatrix(position));
            SetColor(brushPosition, tilemap.GetColor(position));
        }

        private void StoreCells()
        {
            _mStoredSize = mSize;
            _mStoredPivot = mPivot;
            if (mCells != null)
            {
                _mStoredCells = new BrushCell[mCells.Length];
                for (var i = 0; i < mCells.Length; ++i) _mStoredCells[i] = mCells[i];
            }
            else
            {
                _mStoredCells = new BrushCell[0];
            }
        }

        private void RestoreCells()
        {
            mSize = _mStoredSize;
            mPivot = _mStoredPivot;
            if (_mStoredCells != null)
            {
                mCells = new BrushCell[_mStoredCells.Length];
                _mTileChangeDataList = new List<TileChangeData>(_mStoredCells.Length);
                for (var i = 0; i < _mStoredCells.Length; ++i) mCells[i] = _mStoredCells[i];
            }
        }

        /// <summary>MoveStart is called when user starts moving the area previously selected with the selection marquee.</summary>
        /// <param name="gridLayout">Grid used for layout.</param>
        /// <param name="brushTarget">Target of the move operation. By default the currently selected GameObject.</param>
        /// <param name="position">Position where the move operation has started.</param>
        public override void MoveStart(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            var tilemap = brushTarget.GetComponent<Tilemap>();
            if (tilemap == null)
                return;

            StoreCells();
            Reset();
            UpdateSizeAndPivot(new Vector3Int(position.size.x, position.size.y, 1), Vector3Int.zero);

            foreach (var pos in position.allPositionsWithin)
            {
                var brushPosition = new Vector3Int(pos.x - position.x, pos.y - position.y, 0);
                PickCell(pos, brushPosition, tilemap);
                tilemap.SetTile(pos, null);
            }
        }

        /// <summary>MoveEnd is called when user has ended the move of the area previously selected with the selection marquee.</summary>
        /// <param name="gridLayout">Grid used for layout.</param>
        /// <param name="brushTarget">Target of the move operation. By default the currently selected GameObject.</param>
        /// <param name="position">Position where the move operation has ended.</param>
        public override void MoveEnd(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            Paint(gridLayout, brushTarget, position.min);
            Reset();
            RestoreCells();
        }

        private void FlipX(GridLayout.CellLayout layout)
        {
            var oldCells = mCells.Clone() as BrushCell[];
            var oldBounds = new BoundsInt(Vector3Int.zero, mSize);

            foreach (var oldPos in oldBounds.allPositionsWithin)
            {
                var newX = mSize.x - oldPos.x - 1;
                var toIndex = GetCellIndex(newX, oldPos.y, oldPos.z);
                var fromIndex = GetCellIndex(oldPos);
                mCells[toIndex] = oldCells[fromIndex];
            }

            var newPivotX = mSize.x - Pivot.x - 1;
            Pivot = new Vector3Int(newPivotX, Pivot.y, Pivot.z);
            FlipCells(ref mCells, new Vector3(-1f, 1f, 1f), layout == GridLayout.CellLayout.Hexagon);
        }

        private void FlipY(GridLayout.CellLayout layout)
        {
            var oldCells = mCells.Clone() as BrushCell[];
            var oldBounds = new BoundsInt(Vector3Int.zero, mSize);

            foreach (var oldPos in oldBounds.allPositionsWithin)
            {
                var newY = mSize.y - oldPos.y - 1;
                var toIndex = GetCellIndex(oldPos.x, newY, oldPos.z);
                var fromIndex = GetCellIndex(oldPos);
                mCells[toIndex] = oldCells[fromIndex];
            }

            var newPivotY = mSize.y - Pivot.y - 1;
            Pivot = new Vector3Int(Pivot.x, newPivotY, Pivot.z);
            FlipCells(ref mCells, new Vector3(1f, -1f, 1f), layout == GridLayout.CellLayout.Hexagon);
        }

        private static void FlipCells(ref BrushCell[] cells, Vector3 scale, bool skipRotation)
        {
            var flip = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
            foreach (var cell in cells)
            {
                var oldMatrix = cell.matrix;
                if (skipRotation ||
                    Mathf.Approximately(
                        oldMatrix.rotation.x + oldMatrix.rotation.y + oldMatrix.rotation.z + oldMatrix.rotation.w,
                        1.0f))
                    cell.matrix = oldMatrix * flip;
                else
                    cell.matrix = oldMatrix * S180Rotate * flip;
            }
        }

        /// <summary>Updates the size, pivot and the number of layers of the brush.</summary>
        /// <param name="size">New size of the brush.</param>
        /// <param name="pivot">New pivot of the brush.</param>
        public void UpdateSizeAndPivot(Vector3Int size, Vector3Int pivot)
        {
            mSize = size;
            mPivot = pivot;
            SizeUpdated();
        }

        /// <summary>Sets a Tile at the position in the brush.</summary>
        /// <param name="position">Position to set the tile in the brush.</param>
        /// <param name="tile">Tile to set in the brush.</param>
        public void SetTile(Vector3Int position, TileBase tile)
        {
            if (ValidateCellPosition(position))
                mCells[GetCellIndex(position)].tile = tile;
        }

        /// <summary>
        ///     Sets a transform matrix at the position in the brush. This matrix is used specifically for tiles on a Tilemap
        ///     and not GameObjects of the brush cell.
        /// </summary>
        /// <param name="position">Position to set the transform matrix in the brush.</param>
        /// <param name="matrix">Transform matrix to set in the brush.</param>
        public void SetMatrix(Vector3Int position, Matrix4x4 matrix)
        {
            if (ValidateCellPosition(position))
                mCells[GetCellIndex(position)].matrix = matrix;
        }

        /// <summary>Sets a tint color at the position in the brush.</summary>
        /// <param name="position">Position to set the color in the brush.</param>
        /// <param name="color">Tint color to set in the brush.</param>
        public void SetColor(Vector3Int position, Color color)
        {
            if (ValidateCellPosition(position))
                mCells[GetCellIndex(position)].color = color;
        }

        /// <summary>Gets the index to the GridBrush::ref::BrushCell based on the position of the BrushCell.</summary>
        /// <param name="brushPosition">Position of the BrushCell.</param>
        /// <returns>The index to the GridBrush::ref::BrushCell.</returns>
        public int GetCellIndex(Vector3Int brushPosition)
        {
            return GetCellIndex(brushPosition.x, brushPosition.y, brushPosition.z);
        }

        /// <summary>Gets the index to the GridBrush::ref::BrushCell based on the position of the BrushCell.</summary>
        /// <param name="x">X Position of the BrushCell.</param>
        /// <param name="y">Y Position of the BrushCell.</param>
        /// <param name="z">Z Position of the BrushCell.</param>
        /// <returns>The index to the GridBrush::ref::BrushCell.</returns>
        public int GetCellIndex(int x, int y, int z)
        {
            return x + mSize.x * y + mSize.x * mSize.y * z;
        }

        /// <summary>Gets the index to the GridBrush::ref::BrushCell based on the position of the BrushCell.</summary>
        /// <param name="x">X Position of the BrushCell.</param>
        /// <param name="y">Y Position of the BrushCell.</param>
        /// <param name="z">Z Position of the BrushCell.</param>
        /// <param name="sizex">X Size of Brush.</param>
        /// <param name="sizey">Y Size of Brush.</param>
        /// <param name="sizez">Z Size of Brush.</param>
        /// <returns>The index to the GridBrush::ref::BrushCell.</returns>
        public int GetCellIndex(int x, int y, int z, int sizex, int sizey, int sizez)
        {
            return x + sizex * y + sizex * sizey * z;
        }

        /// <summary>
        ///     Gets the index to the GridBrush::ref::BrushCell based on the position of the BrushCell. Wraps each coordinate
        ///     if it is larger than the size of the GridBrush.
        /// </summary>
        /// <param name="x">X Position of the BrushCell.</param>
        /// <param name="y">Y Position of the BrushCell.</param>
        /// <param name="z">Z Position of the BrushCell.</param>
        /// <returns>The index to the GridBrush::ref::BrushCell.</returns>
        public int GetCellIndexWrapAround(int x, int y, int z)
        {
            return x % mSize.x + mSize.x * (y % mSize.y) + mSize.x * mSize.y * (z % mSize.z);
        }

        private bool ValidateCellPosition(Vector3Int position)
        {
            var valid =
                position.x >= 0 && position.x < size.x &&
                position.y >= 0 && position.y < size.y &&
                position.z >= 0 && position.z < size.z;
            if (!valid)
                throw new ArgumentException(string.Format(
                    "Position {0} is an invalid cell position. Valid range is between [{1}, {2}).", position,
                    Vector3Int.zero, size));
            return valid;
        }

        private void SizeUpdated()
        {
            var cellSize = mSize.x * mSize.y * mSize.z;
            mCells = new BrushCell[cellSize];
            _mTileChangeDataList = new List<TileChangeData>(cellSize);
            var bounds = new BoundsInt(Vector3Int.zero, mSize);
            foreach (var pos in bounds.allPositionsWithin) mCells[GetCellIndex(pos)] = new BrushCell();
        }

        /// <summary>
        ///     Returns a HashCode for the GridBrush based on its contents.
        /// </summary>
        /// <returns>A HashCode for the GridBrush based on its contents.</returns>
        public override int GetHashCode()
        {
            var hash = 0;
            unchecked
            {
                foreach (var cell in Cells) hash = hash * 33 + cell.GetHashCode();
            }

            return hash;
        }

        /// <summary>Brush Cell stores the data to be painted in a grid cell.</summary>
        [Serializable]
        public class BrushCell
        {
            [SerializeField] private TileBase m_Tile;
            [SerializeField] private Matrix4x4 m_Matrix = Matrix4x4.identity;
            [SerializeField] private Color m_Color = Color.white;

            /// <summary>Tile to be placed when painting.</summary>
            public TileBase tile
            {
                get => m_Tile;
                set => m_Tile = value;
            }

            /// <summary>The transform matrix of the brush cell.</summary>
            public Matrix4x4 matrix
            {
                get => m_Matrix;
                set => m_Matrix = value;
            }

            /// <summary>Color to tint the tile when painting.</summary>
            public Color color
            {
                get => m_Color;
                set => m_Color = value;
            }

            /// <summary>
            ///     Returns a HashCode for the BrushCell based on its contents.
            /// </summary>
            /// <returns>A HashCode for the BrushCell based on its contents.</returns>
            public override int GetHashCode()
            {
                int hash;
                unchecked
                {
                    hash = tile != null ? tile.GetInstanceID() : 0;
                    hash = hash * 33 + matrix.GetHashCode();
                    hash = hash * 33 + matrix.rotation.GetHashCode();
                    hash = hash * 33 + color.GetHashCode();
                }

                return hash;
            }
        }
    }
}