using UnityEngine;
using UnityEngine.UI;

namespace UniPaint
{
    public class ColorWheelUI : MonoBehaviour
    {
        [SerializeField] private RawImage hueWheel;
        [SerializeField] private RawImage svRect;
        [SerializeField, Range(0f, 1f)] private float testHue;
        
        


        private void Awake()
        {
            InitializeHueWheel();
        }

        private void Update()
        {
            UpdateSVRect(testHue);
        }


        private void InitializeHueWheel()
        {
            const int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.ARGB32, false);

            var center = new Vector2(size, size) * 0.5f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var distance = new Vector2(x - center.x, y - center.y).magnitude;
                    var angleInDegrees = Mathf.Atan2(y - center.y, center.x - x) * Mathf.Rad2Deg;
                    var h = Mathf.InverseLerp(-180f, 180f, angleInDegrees);
                    var color = distance >= size * 0.38f && distance <= size * 0.5f
                        ? Color.HSVToRGB(h, 1f, 1f, false)
                        : Color.clear;
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();
            hueWheel.texture = texture;
        }

        private void UpdateSVRect(float hue)
        {
            const int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.ARGB32, false);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var s = Mathf.InverseLerp(0f, size - 1, x);
                    var v = Mathf.InverseLerp(0f, size - 1, y);
                    var color = Color.HSVToRGB(testHue, s, v, false);
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();
            svRect.texture = texture;
        }
    }
}