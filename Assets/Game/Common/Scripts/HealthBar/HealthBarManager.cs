using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Common.Scripts.Game;
using Game.Stickman.Assets.Scripts;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Game.Common.Scripts.HealthBar
{
    public class HealthBarManager : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private RectTransform _container;
        [SerializeField] private HealthBarUI _healthBarPrefab;
        [SerializeField] private Vector3 _healthBarOffset = new Vector3(0,10,0);

        private ObjectPool<HealthBarUI> _barsPool;

        private readonly Dictionary<EnemyHealth, HealthBarUI> _bars = new();
        private BaseGameSettings _settings;

        [Inject]
        private void Construct(BaseGameSettings settings)
        {
            _settings = settings;
            Init();
        }

        private void Init()
        {
            _barsPool = new ObjectPool<HealthBarUI>(CreateHealthBars, actionOnRelease: OnReleaseHealthBar, defaultCapacity: _settings.EnemyCount);
        }

        private HealthBarUI CreateHealthBars()
        {
            var bar = Instantiate(_healthBarPrefab, _container);
            bar.Hide();

            return bar;
        }

        private void OnReleaseHealthBar(HealthBarUI bar)
        {
            bar.Hide();
        }

        public void Register(EnemyHealth enemy)
        {

            enemy.OnHealthChanged += FirstHealthChangedHandler;
        }

        private void FirstHealthChangedHandler(EnemyHealth enemy, float value)
        {
            enemy.OnHealthChanged -= FirstHealthChangedHandler;
            var bar = _barsPool.Get();
            _bars[enemy] = bar;

            enemy.OnHealthChanged += HealthChangedHandler;
            enemy.OnDeath += Unregister;
            bar.SetValue(value);
            UpdateBarPosition(enemy, bar).Forget();
            bar.Show();
        }

        private void HealthChangedHandler(EnemyHealth enemy, float value)
        {
            _bars[enemy].SetValue(value);
        }

        private void Unregister(EnemyHealth enemy)
        {
            enemy.OnDeath -= Unregister;
            enemy.OnHealthChanged -= HealthChangedHandler;

            if (_bars.TryGetValue(enemy, out var bar))
            {
                _bars.Remove(enemy);
                if (bar != null)
                {
                    _barsPool.Release(bar);
                }
            }
        }

        private async UniTaskVoid UpdateBarPosition(EnemyHealth enemy, HealthBarUI bar)
        {
            while (enemy != null && enemy.gameObject.activeInHierarchy && bar != null)
            {
                var screenPos = _cam.WorldToScreenPoint(enemy.HealthBarPoint.position);
                bar.SetPosition(screenPos + _healthBarOffset);
                await UniTask.Yield();
            }

            if (bar != null)
            {
                bar.Hide();
            }
        }

        public void Reset()
        {
            var barArray = _bars.Keys.ToArray();
            foreach (var enemy in barArray)
            {
                Unregister(enemy);
            }
            _bars.Clear();
        }
    }
}
