using System;
using UnityEngine;

namespace Game.Stickman.Assets.Scripts
{
    public class EnemyHealth : MonoBehaviour
    {
        public event Action<EnemyHealth, float> OnHealthChanged;
        public event Action<EnemyHealth> OnDeath;

        [field: SerializeField] public Transform HealthBarPoint { get; private set; }

        private float _maxHealth;
        private float _currentHealth;
        private bool _isInited = false;

        public void Init(float enemyHealth)
        {
            _maxHealth = enemyHealth;
            _currentHealth = _maxHealth;
            _isInited = true;
        }

        public void TakeDamage(float amount)
        {
            _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, _maxHealth);
            OnHealthChanged?.Invoke(this, _currentHealth/_maxHealth);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke(this);
            }
        }

        private void OnDisable()
        {
            if (_isInited && _currentHealth > 0)
            {
                OnDeath?.Invoke(this);
                _isInited = false;
            }
        }
    }
}
