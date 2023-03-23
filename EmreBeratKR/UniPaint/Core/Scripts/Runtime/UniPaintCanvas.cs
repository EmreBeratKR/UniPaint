using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UniPaint
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class UniPaintCanvas : MonoBehaviour
    {
        private static readonly int MainTexID = Shader.PropertyToID("_MainTex");


        public event Action<float, float, float, float> OnColorPicked; 


        private delegate void ToolMethod(Vector2 position);
        
        
        [SerializeField, Min(1f)] private Vector2Int resolution = new Vector2Int(256, 256);
        [SerializeField, Min(0f)] private float size = 5f;
        [SerializeField, Min(0f)] private float maxToolSize = 25f;
        [SerializeField, Min(0f)] private float minToolSize = 1f;
        [SerializeField, Range(0f, 1f)] private float defaultToolSize = 0.2f;


        private Color[] m_TextureColors;
        private Vector3 m_PreviousFrameMousePosition;
        private ColorWheelUI m_ColorWheel;
        private ToolMethod m_SelectedTool;
        private Texture2D m_CanvasTexture;
        private Material m_CanvasMaterial;
        private MeshRenderer m_MeshRenderer;
        private MeshFilter m_MeshFilter;
        private Mesh m_CanvasMesh;
        private Camera m_Camera;
        private float m_ToolSize;
        private int m_Width;
        private int m_Height;
        private bool m_IsDirty;
        private bool m_IsDragging;
        private bool m_IsClickOnlyTool;


        private void Awake()
        {
            m_Width = resolution.x;
            m_Height = resolution.y;
            m_Camera = Camera.main;
            m_ColorWheel = FindObjectOfType<ColorWheelUI>();
            InitializeMeshRenderer();
            InitializeMesh();
            InitializeTexture();
            m_SelectedTool = CircleDrawTool;
        }

        private void Update()
        {
            HandleInput();
            HandleTool();
            TryApplyChanges();
        }


        public void Clear()
        {
            for (var i = 0; i < m_TextureColors.Length; i++)
            {
                SetPixelColorAtIndex(i, Color.clear);
            }
            
            SetDirty();
        }
        
        public void SetToolSize(float sizeNormalized)
        {
            m_ToolSize = Mathf.Lerp(minToolSize, maxToolSize, sizeNormalized);
        }

        public void SetToolSizeToDefault()
        {
            SetToolSize(defaultToolSize);
        }

        public float GetDefaultToolSize()
        {
            return defaultToolSize;
        }

        public void SetToolCirclePen()
        {
            m_IsClickOnlyTool = false;
            m_SelectedTool = CircleDrawTool;
        }
        
        public void SetToolCircleEraser()
        {
            m_IsClickOnlyTool = false;
            m_SelectedTool = CircleEraseTool;
        }
        
        public void SetToolSquarePen()
        {
            m_IsClickOnlyTool = false;
            m_SelectedTool = SquareDrawTool;
        }
        
        public void SetToolSquareEraser()
        {
            m_IsClickOnlyTool = false;
            m_SelectedTool = SquareEraseTool;
        }

        public void SetToolColorPicker()
        {
            m_IsClickOnlyTool = true;
            m_SelectedTool = ColorPickerTool;
        }
        

        private bool TryApplyChanges()
        {
            if (!m_IsDirty) return false;

            m_IsDirty = false;
            ApplyChanges();

            return true;
        }

        private void InitializeMeshRenderer()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_MeshFilter = GetComponent<MeshFilter>();

            m_MeshRenderer.material = GetDefaultMaterial();
            m_CanvasMaterial = m_MeshRenderer.material;
        }
        
        private void InitializeMesh()
        {
            m_CanvasMesh = new Mesh();
            m_CanvasMesh.name = "Canvas Mesh";
            var aspectRatio = GetAspectRatio();
            m_CanvasMesh.vertices = new Vector3[]
            {
                new Vector3(-aspectRatio, +1) * size,
                new Vector3(-aspectRatio, -1) * size,
                new Vector3(+aspectRatio, -1) * size,
                new Vector3(+aspectRatio, +1) * size
            };
            m_CanvasMesh.uv = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };
            m_CanvasMesh.triangles = new int[]
            {
                0, 3, 1, 1, 3, 2
            };
            m_MeshFilter.mesh = m_CanvasMesh;
        }
        
        private void InitializeTexture()
        {
            m_TextureColors = new Color[GetPixelCount()];
            m_CanvasTexture = new Texture2D(m_Width, m_Height, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
            m_CanvasMaterial.SetTexture(MainTexID, m_CanvasTexture);

            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    SetPixelColor(x, y, Color.clear);
                }
            }
            
            ApplyChanges();
        }

        private float GetAspectRatio()
        {
            return (float) m_Width / m_Height;
        }

        private Vector2 GetTotalSize()
        {
            var scale = transform.lossyScale;
            var aspectRatio = GetAspectRatio();
            return new Vector2(scale.x * aspectRatio, scale.y) * (size * 2f);
        }

        private Vector3 GetOrigin()
        {
            return transform.position;
        }

        private void SetDirty()
        {
            m_IsDirty = true;
        }
        
        private void ApplyChanges()
        {
            m_CanvasTexture.SetPixels(m_TextureColors);
            m_CanvasTexture.Apply(false);
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (m_IsClickOnlyTool)
                    {
                        var position = MousePositionToCanvasPosition(Input.mousePosition);
                        m_SelectedTool(position);
                        return;
                    }
                    
                    m_IsDragging = true;
                    m_PreviousFrameMousePosition = Input.mousePosition;
                    return;
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                m_IsDragging = false;
            }
        }
        
        private void HandleTool()
        {
            if (m_IsClickOnlyTool) return;
            
            if (!m_IsDragging) return;
            
            if (!Input.GetMouseButton(0)) return;

            var mousePosition = Input.mousePosition;
            var deltaDistance = (mousePosition - m_PreviousFrameMousePosition).magnitude;

            for (var i = 0f; i <= deltaDistance; i += m_ToolSize * 1f)
            {
                var t = Mathf.InverseLerp(0, deltaDistance, i);
                var lerpPosition = Vector3.Lerp(mousePosition, m_PreviousFrameMousePosition, t);
                var position = MousePositionToCanvasPosition(lerpPosition);
                m_SelectedTool(position);
            }
            
            m_PreviousFrameMousePosition = mousePosition;
        }

        private void SquareColorTool(Vector2 position, Color color)
        {
            var posX = Mathf.FloorToInt(position.x);
            var posY = Mathf.FloorToInt(position.y);
            
            if (posX < 0 || posX >= m_Width) return;
            
            if (posY < 0 || posY >= m_Height) return;
            
            var radiusInt = Mathf.CeilToInt(m_ToolSize);
            var left = Mathf.Max(0, posX - radiusInt);
            var right = Mathf.Min(m_Width, posX + radiusInt + 1);
            var bottom = Mathf.Max(0, posY - radiusInt);
            var top = Mathf.Min(m_Height, posY + radiusInt + 1);
            
            for (int x = left; x < right; x++)
            {
                for (int y = bottom; y < top; y++)
                {
                    SetPixelColor(x, y, color);
                }
            }
            
            SetDirty();
        }

        private void CircleColorTool(Vector2 position, Color color)
        {
            var posX = Mathf.FloorToInt(position.x);
            var posY = Mathf.FloorToInt(position.y);
            
            if (posX < 0 || posX >= m_Width) return;
            
            if (posY < 0 || posY >= m_Height) return;
            
            var radiusInt = Mathf.CeilToInt(m_ToolSize);
            var left = Mathf.Max(0, posX - radiusInt);
            var right = Mathf.Min(m_Width, posX + radiusInt + 1);
            var bottom = Mathf.Max(0, posY - radiusInt);
            var top = Mathf.Min(m_Height, posY + radiusInt + 1);
            var sqrPenSize = m_ToolSize * m_ToolSize;
            
            for (int x = left; x < right; x++)
            {
                for (int y = bottom; y < top; y++)
                {
                    var deltaX = position.x - x;
                    var deltaY = position.y - y;
                    var sqrDistance = deltaX * deltaX + deltaY * deltaY;
                    
                    if (sqrDistance > sqrPenSize) continue;
                    
                    SetPixelColor(x, y, color);
                }
            }
            
            SetDirty();
        }

        private void ColorPickerTool(Vector2 position)
        {
            var x = Mathf.FloorToInt(position.x);
            var y = Mathf.FloorToInt(position.y);
            var color = m_TextureColors[GetPixelIndex(x, y)];
            Color.RGBToHSV(color, out var h, out var s, out var v);
            OnColorPicked?.Invoke(h, s, v, color.a);
        }

        private void SquareDrawTool(Vector2 position)
        {
            SquareColorTool(position, GetSelectedColor());
        }
        
        private void CircleDrawTool(Vector2 position)
        {
            CircleColorTool(position, GetSelectedColor());
        }

        private void SquareEraseTool(Vector2 position)
        {
            SquareColorTool(position, Color.clear);
        }

        private void CircleEraseTool(Vector2 position)
        {
            CircleColorTool(position, Color.clear);
        }

        private Vector2 MousePositionToCanvasPosition(Vector3 mousePosition)
        {
            var totalSize = GetTotalSize();
            var halfSize = GetTotalSize() * 0.5f;
            var leftBottomPosition = GetOrigin() - new Vector3(halfSize.x, halfSize.y);
            var mouseWorldPosition = m_Camera.ScreenToWorldPoint(mousePosition);
            var worldPosition = mouseWorldPosition - leftBottomPosition;
            var tX = InverseLerpUnclamped(0f, totalSize.x, worldPosition.x);
            var tY = InverseLerpUnclamped(0f, totalSize.y, worldPosition.y);
            return new Vector2(tX * m_Width, tY * m_Height);
        }

        private Color GetSelectedColor()
        {
            return m_ColorWheel.GetColor();
        }

        private int GetPixelCount()
        {
            return m_Width * m_Height;
        }
        
        private int GetPixelIndex(int x, int y)
        {
            return x + y * m_Width;
        }

        private void SetPixelColor(int x, int y, Color color)
        {
            var index = GetPixelIndex(x, y);
            SetPixelColorAtIndex(index, color);
        }

        private void SetPixelColorAtIndex(int index, Color color)
        {
            m_TextureColors[index] = color;
        }
        
        
        private static float InverseLerpUnclamped(float a, float b, float value)
        {
            return (double) a != (double) b 
                ? (float) (((double) value - (double) a) / ((double) b - (double) a)) 
                : 0.0f;
        }
        
        private static Material GetDefaultMaterial()
        {
            return Resources.Load<Material>("Default-UniPaint-Material");
        }


#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            OnDrawCanvasBorderGizmos();
        }

        private void OnDrawCanvasBorderGizmos()
        {
            var origin = GetOrigin();
            var totalSize = GetTotalSize();
            var bottomLeft = origin + new Vector3(-totalSize.x, -totalSize.y) * 0.5f;
            var bottomRight = origin + new Vector3(totalSize.x, -totalSize.y) * 0.5f;
            var topLeft = origin + new Vector3(-totalSize.x, totalSize.y) * 0.5f;
            var topRight = origin + new Vector3(totalSize.x, totalSize.y) * 0.5f;

            Gizmos.color = Color.black;
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }

#endif
    }
}