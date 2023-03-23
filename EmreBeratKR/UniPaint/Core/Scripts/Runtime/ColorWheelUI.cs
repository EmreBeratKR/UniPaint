using System;
using UnityEngine;

namespace UniPaint
{
    public class ColorWheelUI : MonoBehaviour
    {
        [SerializeField] private HueWheelUI hueWheel;
        [SerializeField] private SVRectUI svRect;


        private UniPaintCanvas m_Canvas;


        private void Awake()
        {
            m_Canvas = FindObjectOfType<UniPaintCanvas>();
            m_Canvas.OnColorPicked += OnColorPicked;
        }

        private void OnDestroy()
        {
            m_Canvas.OnColorPicked -= OnColorPicked;
        }

        
        private void OnColorPicked(float h, float s, float v, float a)
        {
            hueWheel.SetHue(h);
            svRect.SetSaturation(s);
            svRect.SetValue(v);
        }
        

        public Color GetColor()
        {
            var h = hueWheel.GetHue();
            var s = svRect.GetSaturation();
            var v = svRect.GetValue();
            return Color.HSVToRGB(h, s, v, false);
        }
    }
}