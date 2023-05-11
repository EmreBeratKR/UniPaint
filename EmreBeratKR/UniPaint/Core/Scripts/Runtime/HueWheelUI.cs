using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniPaint
{
    public class HueWheelUI : MonoBehaviour, 
        IPointerDownHandler,
        IPointerUpHandler,
        IDragHandler
    {
        [SerializeField] private SVRectUI svRect;
        [SerializeField] private RawImage image;
        [SerializeField] private Image selector;


        private Texture2D m_Texture;
        private float m_Hue;
        private bool m_IsDragging;
        

        private void Awake()
        {
            InitializeTexture();
            SelectDefaultPosition();
        }

        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsValidPosition(eventData.position)) return;
            
            SelectPosition(eventData.position);
            m_IsDragging = true;
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            m_IsDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_IsDragging) return;
            
            SelectPosition(eventData.position);
        }

        public float GetHue()
        {
            return m_Hue;
        }

        public void SetHue(float hue)
        {
            var localPosition = HueToLocalPosition(hue);
            SelectLocalPosition(localPosition);
        }


        private void SelectDefaultPosition()
        {
            var defaultPosition = GetImagePosition() + Vector2.right * (GetImageSize().x * 0.5f); 
            SelectPosition(defaultPosition);
        }
        
        private void InitializeTexture()
        {
            const int size = 512;
            const float innerRadius = 0.38f;
            
            m_Texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            var center = new Vector2(size, size) * 0.5f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var distance = new Vector2(x - center.x, y - center.y).magnitude;
                    var delta = new Vector2(x - center.x, y - center.y);
                    var h = LocalPositionToHue(delta);
                    var color = distance >= size * innerRadius && distance <= size * 0.5f
                        ? Color.HSVToRGB(h, 1f, 1f, false)
                        : Color.clear;
                    m_Texture.SetPixel(x, y, color);
                }
            }
            
            m_Texture.Apply();
            image.texture = m_Texture;
        }

        private void SelectPosition(Vector2 position)
        {
            var localPosition = ScreenPointToLocalPosition(position);
            SelectLocalPosition(localPosition);
        }

        private void SelectLocalPosition(Vector2 localPosition)
        {
            var localPositionNormalized = localPosition.normalized;
            var distance = GetImageSize() * 0.5f - GetSelectorSize() * 0.5f;
            selector.rectTransform.position = GetImagePosition() + localPositionNormalized * distance;
            m_Hue = LocalPositionToHue(localPosition);
            svRect.SetHue(m_Hue);
        }

        private Vector2 HueToLocalPosition(float hue)
        {
            var angleInRadians = Mathf.Lerp(Mathf.PI, -Mathf.PI, hue);
            var x = Mathf.Cos(angleInRadians);
            var y = Mathf.Sin(angleInRadians);
            return new Vector2(-x, y);
        }
        
        private float LocalPositionToHue(Vector2 localPosition)
        {
            var angleInRadians = Mathf.Atan2(localPosition.y, -localPosition.x);
            return Mathf.InverseLerp(Mathf.PI, -Mathf.PI, angleInRadians);
        }

        private bool IsValidPosition(Vector2 position)
        {
            var localPosition = ScreenPointToLocalPosition(position);
            var imageHalfSize = GetImageSize() * 0.5f;
            var tX = Mathf.InverseLerp(-imageHalfSize.x, imageHalfSize.x, localPosition.x);
            var tY = Mathf.InverseLerp(-imageHalfSize.y, imageHalfSize.y, localPosition.y);
            var textureX = Mathf.RoundToInt(m_Texture.width * tX);
            var textureY = Mathf.RoundToInt(m_Texture.height * tY);
            return m_Texture.GetPixel(textureX, textureY).a > 0f;
        }

        private Vector2 GetImagePosition()
        {
            return image.rectTransform.position;
        }

        private Vector2 GetImageSize()
        {
            return image.rectTransform.sizeDelta * GetCanvasScaleFactor();
        }

        private Vector2 GetSelectorSize()
        {
            return selector.rectTransform.sizeDelta * GetCanvasScaleFactor();
        }

        private Vector2 ScreenPointToLocalPosition(Vector2 screenPoint)
        {
            return screenPoint - GetImagePosition();
        }

        private float GetCanvasScaleFactor()
        {
            return image.canvas.scaleFactor;
        }
    }
}