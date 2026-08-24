using UnityEngine;

namespace Plugins.BFixedSlider.Scripts
{
    public class SliderHandleFixer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _handle;
        [SerializeField] private RectTransform _handleArea;
    
        [Header("Settings")]
        [SerializeField, Min(0)] private float _handleWidth;

        private void OnValidate()
        {
            _handleArea.offsetMax = new Vector2(-_handleWidth, _handleArea.offsetMax.y);
            _handle.sizeDelta = new Vector2(_handleWidth, _handle.sizeDelta.y);
        }
    }
}