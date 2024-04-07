#if UNITY_EDITOR
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


//Definitely out of necessity, nothing experimental here
namespace Scripts.Node
{
    public class TileMapToPNG : MonoBehaviour
    {
        [SerializeField] private Tilemap tileMap;
        private Texture2D _imageTexture;
        private float _pixelSize;
        [SerializeField] private string pathToSave = "Assets/Art/Boards/";
        [SerializeField] private string spriteName;


        [Button]
        public void ResetTilemap()
        {
            tileMap.ClearAllTiles();
        }
        [Button]
        public void ExportAsPng()
        {
            Pack();
            byte[] bytes = _imageTexture.EncodeToPNG();
            var dirPath = Application.dataPath + pathToSave;
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + spriteName + ".png", bytes);
            _imageTexture = null;
            AssetDatabase.Refresh();
        }
        private void Pack()
        {

            int spriteWidth=0;
            int spriteHeight=0;
            tileMap.CompressBounds();    
            foreach (Vector3Int pos in tileMap.cellBounds.allPositionsWithin)
            {
                Sprite sprite = tileMap.GetSprite(pos);
                if (sprite == null) continue;
                spriteWidth = (int)sprite.textureRect.width;
                spriteHeight = (int)sprite.textureRect.height;
                _pixelSize = sprite.pixelsPerUnit;
                break;
            }
        
            if (_pixelSize == 0)
            {
                Debug.LogError("No sprites found in the tilemap.");
                return;
            }

            if(spriteHeight == 0 || spriteWidth == 0)
            {
                Debug.LogError("No sprites found in the tilemap.");
                return;
            }

            Texture2D createdImage = new Texture2D(spriteWidth * tileMap.cellBounds.size.x, spriteHeight * tileMap.cellBounds.size.y);

            Color[] clearPixels = new Color[createdImage.width * createdImage.height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.clear;
            }
            createdImage.SetPixels(clearPixels);
            createdImage.Apply();

            foreach (Vector3Int pos in tileMap.cellBounds.allPositionsWithin)
            {
                Sprite sprite = tileMap.GetSprite(pos);

                if (sprite != null)
                {
                    Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, spriteWidth, spriteHeight);
                    createdImage.SetPixels((pos.x - tileMap.cellBounds.min.x) * spriteWidth, (pos.y - tileMap.cellBounds.min.y) * spriteHeight, spriteWidth, spriteHeight, pixels);
                }
            }

            createdImage.Apply();
            _imageTexture = createdImage;
        }

       
    }
}
#endif
