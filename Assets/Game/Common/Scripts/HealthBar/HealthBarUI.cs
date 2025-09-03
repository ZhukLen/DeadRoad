using UnityEngine;
using UnityEngine.UI;

namespace Game.Common.Scripts.HealthBar
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public void SetValue(float value)
        {
            _slider.value = value;
        }

        public void SetPosition(Vector3 screenPosition)
        {
            transform.position = screenPosition;
        }
    }
}
