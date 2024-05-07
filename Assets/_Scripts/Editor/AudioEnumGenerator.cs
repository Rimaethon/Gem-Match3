using System.IO;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CustomEditor(typeof(AudioLibrary))]
    public class AudioEnumGenerator : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var sfxLibrary = (AudioLibrary)target;

            if (GUILayout.Button("Generate Enums"))
                GenerateEnums(sfxLibrary);
        }

        private void GenerateEnums(AudioLibrary sfxLibrary)
        {
            foreach (var fieldInfo in sfxLibrary.GetType().GetFields())
                if (fieldInfo.FieldType == typeof(AudioClip[]))
                {
                    var arrayName = fieldInfo.Name;
                    var enumCode = $"public enum {arrayName}\n{{\n";

                    var audioClips = (AudioClip[])fieldInfo.GetValue(sfxLibrary);

                    for (var i = 0; i < audioClips.Length; i++) enumCode += $"\t{audioClips[i].name} = {i},\n";

                    enumCode += "}";

                    Debug.Log($"Generated Enum for {arrayName}:\n{enumCode}");

                    SaveEnumToFile(arrayName, enumCode);
                }
        }

        private void SaveEnumToFile(string arrayName, string enumCode)
        {
            var directoryPath = "Assets/_Scripts/Enums";
            var filePath = $"{directoryPath}/{arrayName}.cs";

            if (!AssetDatabase.IsValidFolder(directoryPath))
                AssetDatabase.CreateFolder("Assets/_Scripts/", "Enums");

            File.WriteAllText(filePath, enumCode);
            AssetDatabase.Refresh();
            Debug.Log($"Saved Enum for {arrayName} to {filePath}");
        }
    }
}