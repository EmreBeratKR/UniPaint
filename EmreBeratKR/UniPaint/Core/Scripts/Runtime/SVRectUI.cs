using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniPaint
{
    public class SVRectUI : MonoBehaviour, 
        IPointerDownHandler,
        IDragHandler
    {
        [SerializeField] private RawImage image;
        [SerializeField] private Image selector;


        private Texture2D m_Texture;
        private float m_Saturation;
        private float m_Value;


        private void Awake()
        {
            InitializeTexture();
            SelectDefaultPosition();
        }
        

        public void OnPointerDown(PointerEventData eventData)
        {
            SelectPosition(eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            SelectPosition(eventData.position);
        }

        public void SetHue(float hue)
        {
            var width = m_Texture.width;
            var height = m_Texture.height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var s = Mathf.InverseLerp(0f, width - 1, x);
                    var v = Mathf.InverseLerp(0f, height - 1, y);
                    var color = Color.HSVToRGB(hue, s, v, false);
                    m_Texture.SetPixel(x, y, color);
                }
            }
            
            m_Texture.Apply();
        }

        public float GetSaturation()
        {
            return m_Saturation;
        }

        public void SetSaturation(float saturation)
        {
            m_Saturation = saturation;
            var localPosition = SVToLocalPosition(m_Saturation, m_Value);
            SelectPosition(GetImagePosition() + localPosition);
        }

        public float GetValue()
        {
            return m_Value;
        }

        public void SetValue(float value)
        {
            m_Value = value;
            var localPosition = SVToLocalPosition(m_Saturation, m_Value);
            SelectPosition(GetImagePosition() + localPosition);
        }
        

        private void SelectDefaultPosition()
        {
            var defaultPosition = GetImagePosition();
            var halfSize = GetImageSize() * 0.5f;
            defaultPosition.x -= halfSize.x;
            defaultPosition.y += halfSize.y;
            SelectPosition(defaultPosition);
        }
        
        private void InitializeTexture()
        {
            const int size = 64;
            
            m_Texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
            image.texture = m_Texture;
        }

        private void SelectPosition(Vector2 position)
        {
            var imagePosition = GetImagePosition();
            var imageHalfSize = GetImageSize() * 0.5f;
            var localPosition = position - imagePosition;
            m_Saturation = Mathf.InverseLerp(-imageHalfSize.x, imageHalfSize.x, localPosition.x);
            m_Value = Mathf.InverseLerp(-imageHalfSize.y, imageHalfSize.y, localPosition.y);
            var x = Mathf.Clamp(position.x, imagePosition.x - imageHalfSize.x, imagePosition.x + imageHalfSize.x);
            var y = Mathf.Clamp(position.y, imagePosition.y - imageHalfSize.y, imagePosition.y + imageHalfSize.y);
            selector.rectTransform.position = new Vector3(x, y);
            selector.color = m_Saturation > 0.5f || m_Value < 0.5f
                ? Color.white
                : Color.black;
        }

        private Vector2 SVToLocalPosition(float s, float v)
        {
            var imageHalfSize = GetImageSize() * 0.5f;
            var x = Mathf.Lerp(-imageHalfSize.x, imageHalfSize.x, s);
            var y = Mathf.Lerp(-imageHalfSize.y, imageHalfSize.y, v);
            return new Vector2(x, y);
        }

        private Vector2 GetImagePosition()
        {
            return image.rectTransform.position;
        }

        private Vector2 GetImageSize()
        {
            return image.rectTransform.sizeDelta * GetCanvasScaleFactor();
        }

        private float GetCanvasScaleFactor()
        {
            return image.canvas.scaleFactor;
        }
    }
}