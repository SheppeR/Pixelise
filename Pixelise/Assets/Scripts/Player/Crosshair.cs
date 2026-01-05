using UnityEngine;

namespace Player
{
    public class Crosshair : MonoBehaviour
    {
        public int size = 12;
        public int thickness = 2;
        public Color color = Color.white;

        private void OnGUI()
        {
            var x = Screen.width / 2f;
            var y = Screen.height / 2f;

            GUI.color = color;

            // ligne horizontale
            GUI.DrawTexture(
                new Rect(x - size / 2, y - thickness / 2, size, thickness),
                Texture2D.whiteTexture
            );

            // ligne verticale
            GUI.DrawTexture(
                new Rect(x - thickness / 2, y - size / 2, thickness, size),
                Texture2D.whiteTexture
            );

            GUI.color = Color.white;
        }
    }
}