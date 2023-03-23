using UnityEngine;

namespace UniPaint
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class UniPaintCanvas : MonoBehaviour
    {
        private static readonly int MainTexID = Shader.PropertyToID("_MainTex");


        private delegate void ToolMethod(Vector2 position);
        
        
        [SerializeField, Min(1f)] private Vector2Int resolution = new Vector2Int(256, 256);
        [SerializeField, Min(0f)] private float size = 5f;
        [SerializeField, Min(0f)] private float penSize = 10f;
        [SerializeField] private Color selectedColor = Color.white;


        private ToolMethod m_SelectedTool;
        private Texture2D m_CanvasTexture;
        private Material m_CanvasMaterial;
        private MeshRenderer m_MeshRenderer;
        private MeshFilter m_MeshFilter;
        private Mesh m_CanvasMesh;
        private Camera m_Camera;
        private bool m_IsDirty;


        private void Awake()
        {
            m_Camera = Camera.main;
            InitializeMeshRenderer();
            InitializeMesh();
            InitializeTexture();
            m_SelectedTool = CircleDrawTool;
        }

        private void Update()
        {
            HandleTool();
            TryApplyChanges();
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
            m_CanvasTexture = new Texture2D(resolution.x, resolution.y, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
            m_CanvasMaterial.SetTexture(MainTexID, m_CanvasTexture);

            var center = new Vector2(resolution.x, resolution.y) * 0.5f;
            
            for (int x = 0; x < resolution.x; x++)
            {
                for (int y = 0; y < resolution.y; y++)
                {
                    m_CanvasTexture.SetPixel(x, y, Color.clear);
                }
            }
            
            ApplyChanges();
        }
        
        private float GetAspectRatio()
        {
            return (float) resolution.x / resolution.y;
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
            m_CanvasTexture.Apply();
        }

        private void HandleTool()
        {
            if (!Input.GetMouseButton(0)) return;

            var position = MousePositionToCanvasPosition(Input.mousePosition);
            m_SelectedTool(position);
        }

        private void SquareColorTool(Vector2 position, Color color)
        {
            var posX = Mathf.FloorToInt(position.x);
            var posY = Mathf.FloorToInt(position.y);
            
            if (posX < 0 || posX >= resolution.x) return;
            
            if (posY < 0 || posY >= resolution.y) return;
            
            var radiusInt = Mathf.CeilToInt(penSize);
            var left = Mathf.Max(0, posX - radiusInt);
            var right = Mathf.Min(resolution.x, posX + radiusInt + 1);
            var bottom = Mathf.Max(0, posY - radiusInt);
            var top = Mathf.Min(resolution.y, posY + radiusInt + 1);
            
            for (int x = left; x < right; x++)
            {
                for (int y = bottom; y < top; y++)
                {
                    m_CanvasTexture.SetPixel(x, y, color);
                }
            }
            
            SetDirty();
        }

        private void CircleColorTool(Vector2 position, Color color)
        {
            var posX = Mathf.FloorToInt(position.x);
            var posY = Mathf.FloorToInt(position.y);
            
            if (posX < 0 || posX >= resolution.x) return;
            
            if (posY < 0 || posY >= resolution.y) return;
            
            var radiusInt = Mathf.CeilToInt(penSize);
            var left = Mathf.Max(0, posX - radiusInt);
            var right = Mathf.Min(resolution.x, posX + radiusInt + 1);
            var bottom = Mathf.Max(0, posY - radiusInt);
            var top = Mathf.Min(resolution.y, posY + radiusInt + 1);
            var sqrPenSize = penSize * penSize;
            
            for (int x = left; x < right; x++)
            {
                for (int y = bottom; y < top; y++)
                {
                    var sqrDistance = Vector2.SqrMagnitude(position - new Vector2(x, y));
                    
                    if (sqrDistance > sqrPenSize) continue;
                    
                    m_CanvasTexture.SetPixel(x, y, color);
                }
            }
            
            SetDirty();
        }

        private void SquareDrawTool(Vector2 position)
        {
            SquareColorTool(position, selectedColor);
        }
        
        private void CircleDrawTool(Vector2 position)
        {
            CircleColorTool(position, selectedColor);
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
            return new Vector2(tX * resolution.x, tY * resolution.y);
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