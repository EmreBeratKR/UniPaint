using UnityEngine;
using UnityEngine.UI;

namespace UniPaint
{
    [RequireComponent(typeof(UniPaintCanvas))]
    public class UniPaintCanvasUI : MonoBehaviour
    {
        [SerializeField] private Button circlePenButton;
        [SerializeField] private Button circleEraseButton;
        [SerializeField] private Button squarePenButton;
        [SerializeField] private Button squareEraseButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button colorPickerButton;
        [SerializeField] private Button colorBucketButton;
        [SerializeField] private Slider toolSizeSlider;


        private UniPaintCanvas m_Canvas;
        
        
        private void Start()
        {
            m_Canvas = GetComponent<UniPaintCanvas>();
            Initialize();
        }

        
        private void Initialize()
        {
            m_Canvas.SetToolSizeToDefault();

            if (circlePenButton)
            {
                circlePenButton.onClick.AddListener(m_Canvas.SetToolCirclePen);
            }

            if (circleEraseButton)
            {
                circleEraseButton.onClick.AddListener(m_Canvas.SetToolCircleEraser);
            }

            if (squarePenButton)
            {
                squarePenButton.onClick.AddListener(m_Canvas.SetToolSquarePen);
            }

            if (squareEraseButton)
            {
                squareEraseButton.onClick.AddListener(m_Canvas.SetToolSquareEraser);
            }

            if (clearButton)
            {
                clearButton.onClick.AddListener(m_Canvas.Clear);
            }

            if (colorPickerButton)
            {
                colorPickerButton.onClick.AddListener(m_Canvas.SetToolColorPicker);
            }

            if (colorBucketButton)
            {
                colorBucketButton.onClick.AddListener(m_Canvas.SetToolColorBucket);
            }
            
            if (toolSizeSlider)
            {
                toolSizeSlider.value = m_Canvas.GetDefaultToolSize();
                toolSizeSlider.onValueChanged.AddListener(m_Canvas.SetToolSize);
            }
        }
    }
}