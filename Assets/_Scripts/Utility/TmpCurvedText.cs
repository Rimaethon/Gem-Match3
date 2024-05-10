using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Rimaethon.Runtime.UI
{
    public class TmpCurvedText:MonoBehaviour
    {
        private TMP_Text textComponent;
        
        [Button]
        public void UpdateText()
        {
            textComponent = gameObject.GetComponent<TMP_Text>();
            Vector3[] vertices;
            Matrix4x4 matrix;
            textComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = textComponent.textInfo;
            int characterCount = textInfo.characterCount;
            if (characterCount == 0)
                return;
            float boundsMinX = textComponent.bounds.min.x;
            float boundsMaxX = textComponent.bounds.max.x;
            float boundsMinY = textComponent.bounds.min.y;
            float boundsMaxY = textComponent.bounds.max.y;

            // Calculate the area of the text's bounding box
            float area = (boundsMaxX - boundsMinX) * (boundsMaxY - boundsMinY);

            // Set the frequency and amplitude based on the area
            frequency = area / 1000f; // Adjust the divisor as needed
            amplitude = area / 500f; // Adjust the divisor as needed

            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 charMidBaselinePos = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);

                vertices[vertexIndex + 0] += -charMidBaselinePos;
                vertices[vertexIndex + 1] += -charMidBaselinePos;
                vertices[vertexIndex + 2] += -charMidBaselinePos;
                vertices[vertexIndex + 3] += -charMidBaselinePos;
                float zeroToOnePos = (charMidBaselinePos.x - boundsMinX) / (boundsMaxX - boundsMinX);
                matrix = ComputeTransformationMatrix(charMidBaselinePos, zeroToOnePos, textInfo, i);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);
            }

            textComponent.UpdateVertexData();
        }
        [SerializeField] private float startRotation = 45f; // Start rotation value
        [SerializeField] private float endRotation = -45f; // End rotation value
        [SerializeField] private float amplitude = 1f; // Amplitude of the sine wave
        [SerializeField] private float frequency = 1f; // Frequency of the sine wave

        private Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, float zeroToOnePos, TMP_TextInfo textInfo, int charIdx)
        {
            // Calculate the rotation for this character
            float rotation = Mathf.Lerp(startRotation, endRotation, zeroToOnePos);

            // Calculate the y-position for this character
            float yPos = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * zeroToOnePos);

            // Compute the new position of the character
            float x0 = charMidBaselinePos.x;
            Vector2 newMideBaselinePos = new Vector2(x0, yPos);

            // Compute the transformation matrix: move the points to the just found position, then rotate the character to fit the angle of the curve
            return Matrix4x4.TRS(new Vector3(newMideBaselinePos.x, newMideBaselinePos.y, 0), Quaternion.Euler(0, 0, rotation), Vector3.one);
        }
    }
}
