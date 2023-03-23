using UnityEngine;

namespace UniPaint
{
    public class ColorWheelUI : MonoBehaviour
    {
        [SerializeField] private HueWheelUI hueWheel;
        [SerializeField] private SVRectUI svRect;
        
        
        
        public Color GetColor()
        {
            var h = hueWheel.GetHue();
            var s = svRect.GetSaturation();
            var v = svRect.GetValue();
            return Color.HSVToRGB(h, s, v, false);
        }
    }
}