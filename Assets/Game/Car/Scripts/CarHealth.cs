using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Car.Scripts
{
    public class CarHealth : MonoBehaviour
    {
        public event Action OnDeath;

        [SerializeField] private Slider _carHealthBar;

        private float _maxHealth;
        private float _currentHealth;

        public void Show() => _carHealthBar.gameObject.SetActive(true);

        public void Hide() => _carHealthBar.gameObject.SetActive(false);

        public void Init(float carHealth)
        {
            _maxHealth = carHealth;
            _currentHealth = _maxHealth;
            _carHealthBar.value = 1F;
        }

        public void TakeDamage(float amount)
        {
            _currentHealth -= amount;
            _carHealthBar.value = _currentHealth / _maxHealth;
            if (_currentHealth <= 0F)
            {
                OnDeath?.Invoke();
            }
        }
    }
}
