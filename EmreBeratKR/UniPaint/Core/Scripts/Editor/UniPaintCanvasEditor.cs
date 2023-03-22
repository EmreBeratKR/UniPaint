using UnityEditor;
using UnityEngine;

namespace UniPaint
{
    [CustomEditor(typeof(UniPaintCanvas))]
    public class UniPaintCanvasEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Exit Play Mode to modify values!", MessageType.Info);
                return;
            }
            
            base.OnInspectorGUI();
        }
    }
}