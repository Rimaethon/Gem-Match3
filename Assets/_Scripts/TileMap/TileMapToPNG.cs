#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


//Definitely out of necessity, nothing experimental here
namespace Scripts.Node
{
    public class TileMapToPNG : MonoBehaviour
    {
        private Tilemap _tileMap;
        public Texture2D img;
        private float _pixelSize;

        public void Pack()
        {
            _tileMap = GetComponent<Tilemap>();

            int spriteWidth=0;
            int spriteHeight=0;
            _tileMap.CompressBounds();    
            foreach (Vector3Int pos in _tileMap.cellBounds.allPositionsWithin)
            {
                Sprite sprite = _tileMap.GetSprite(pos);
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

            Texture2D createdImage = new Texture2D(spriteWidth * _tileMap.cellBounds.size.x, spriteHeight * _tileMap.cellBounds.size.y);

            Color[] clearPixels = new Color[createdImage.width * createdImage.height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.clear;
            }
            createdImage.SetPixels(clearPixels);
            createdImage.Apply();

            foreach (Vector3Int pos in _tileMap.cellBounds.allPositionsWithin)
            {
                Sprite sprite = _tileMap.GetSprite(pos);

                if (sprite != null)
                {
                    Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, spriteWidth, spriteHeight);
                    createdImage.SetPixels((pos.x - _tileMap.cellBounds.min.x) * spriteWidth, (pos.y - _tileMap.cellBounds.min.y) * spriteHeight, spriteWidth, spriteHeight, pixels);
                }
            }

            createdImage.Apply();
            img = createdImage;
        }

        public void ExportAsPng(string textureName)
        {
            byte[] bytes = img.EncodeToPNG();
            var dirPath = Application.dataPath + "/Exported Tilemaps/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + textureName + ".png", bytes);
            img = null;
            AssetDatabase.Refresh();
        }
    }
}
#endif
