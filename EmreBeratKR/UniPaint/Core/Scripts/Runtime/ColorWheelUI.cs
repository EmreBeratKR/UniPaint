using UnityEngine;
using UnityEngine.UI;

namespace UniPaint
{
    public class ColorWheelUI : MonoBehaviour
    {
        [SerializeField] private RawImage hueWheel;
        [SerializeField] private RawImage svRect;
        [SerializeField, Range(0f, 1f)] private float testHue;


        private Texture2D m_SVTexture;
        

        private void Awake()
        {
            InitializeHueWheel();
            InitializeSVTexture();
        }

        private void Update()
        {
            UpdateSVRect(testHue);
        }


        private void InitializeHueWheel()
        {
            const int size = 512;
            const float innerRadius = 0.38f;
            
            var texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            var center = new Vector2(size, size) * 0.5f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var distance = new Vector2(x - center.x, y - center.y).magnitude;
                    var angleInDegrees = Mathf.Atan2(y - center.y, center.x - x) * Mathf.Rad2Deg;
                    var h = Mathf.InverseLerp(-180f, 180f, angleInDegrees);
                    var color = distance >= size * innerRadius && distance <= size * 0.5f
                        ? Color.HSVToRGB(h, 1f, 1f, false)
                        : Color.clear;
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();
            hueWheel.texture = texture;
        }

        private void InitializeSVTexture()
        {
            const int size = 64;
            
            m_SVTexture = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
            svRect.texture = m_SVTexture;
        }

        private void UpdateSVRect(float hue)
        {
            var width = m_SVTexture.width;
            var height = m_SVTexture.height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var s = Mathf.InverseLerp(0f, width - 1, x);
                    var v = Mathf.InverseLerp(0f, height - 1, y);
                    var color = Color.HSVToRGB(hue, s, v, false);
                    m_SVTexture.SetPixel(x, y, color);
                }
            }
            
            m_SVTexture.Apply();
        }
    }
}