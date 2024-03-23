#if UNITY_EDITOR

using Scripts.Node;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TileMapToPNG))]
    public class TileMapToPNGConverter : UnityEditor.Editor
    {
        public string textureName = ""; 

        public override void OnInspectorGUI ()
        {
            TileMapToPNG tileMapToConvert = (TileMapToPNG)target;

            // If the image is not ready
            if (tileMapToConvert.img == null)
            {
                if (GUILayout.Button("Create png"))
                {
                    tileMapToConvert.Pack();
                }
            }
            else
            {
                GUILayout.Label("File name");
                name = GUILayout.TextField(name);
                if(name.Length > 0)
                {
                    if (GUILayout.Button("Export png"))
                    {
                        tileMapToConvert.ExportAsPng(name);
                    }
                }
            
            }
            
        
        }

    }
  
}
#endif
