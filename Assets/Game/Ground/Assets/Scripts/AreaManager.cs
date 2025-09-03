using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Car.Scripts;
using Game.Common.Scripts.Game;
using Game.Common.Scripts.HealthBar;
using Game.Stickman.Assets.Scripts;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Game.Ground.Assets.Scripts
{
    public class AreaManager : MonoBehaviour
    {
        [SerializeField] private GroundController _groundPrefab;
        [SerializeField] private EnemyController _enemyPrefab;
        [SerializeField] private int _initialGroundCount  = 3;
        [SerializeField] private Vector2 _enemyRangeX = new Vector2(-25f, 25f);

        private HealthBarManager _healthBarManager;
        private CarController _carController;
        private BaseGameSettings _settings;
        private ObjectPool<EnemyController> _enemyPool;
        private ObjectPool<GroundController> _groundPool;
        private List<EnemyController> _spawnedEnemy = new List<EnemyController>();
        private List<GroundController> _spawnedGround = new List<GroundController>();
        private float _halfGroundLength;
        private int _groundsSpawned = 0;

        public float GetFullRoadLength() => _settings is null ? 0 : _groundPrefab.BoxSize.y * (_settings.RoadLength - 1);

        [Inject]
        private void Construct(BaseGameSettings settings, CarController carController, HealthBarManager healthBarManager)
        {
            _healthBarManager = healthBarManager;
            _carController = carController;
            _settings = settings;
            Init();
        }

        private void Init()
        {
            _groundPool = new ObjectPool<GroundController>(CreateGround, OnGroundGet, OnGroundRelease, defaultCapacity: _initialGroundCount);
            _enemyPool = new ObjectPool<EnemyController>(CreateEnemy, OnEnemyGet, OnEnemyRelease, defaultCapacity: _settings.EnemyCount * (_initialGroundCount - 1));
            _halfGroundLength = _groundPrefab.BoxSize.y * .5f;
        }

        public void InitialSpawn()
        {
            ResetArea();
            for (int i = 0; i < _initialGroundCount; i++)
            {
                SpawnNextGround();
            }
        }

        private EnemyController CreateEnemy()
        {
            var enemy = Instantiate(_enemyPrefab, transform);
            enemy.Init(_carController.Car);
            enemy.OnDestroy += _enemyPool.Release;
            enemy.gameObject.SetActive(false);
            return enemy;
        }

        private void OnEnemyGet(EnemyController enemy)
        {
            enemy.gameObject.SetActive(true);
            _spawnedEnemy.Add(enemy);
        }

        private void OnEnemyRelease(EnemyController enemy)
        {
            _spawnedEnemy.Remove(enemy);
            enemy.gameObject.SetActive(false);
        }

        private GroundController CreateGround()
        {
            var ground = Instantiate(_groundPrefab, transform);
            ground.Init(_carController.Car);
            ground.OnDestroy += _groundPool.Release;
            ground.gameObject.SetActive(false);
            return ground;
        }

        private void OnGroundGet(GroundController ground)
        {
            ground.gameObject.SetActive(true);
            _spawnedGround.Add(ground);
        }

        private void OnGroundRelease(GroundController ground)
        {
            _spawnedGround.Remove(ground);
            ground.gameObject.SetActive(false);
            if (_groundsSpawned > 0 && _groundsSpawned < _settings.RoadLength)
            {
                UniTask.NextFrame().ContinueWith(SpawnNextGround).Forget();
            }
        }

        private void SpawnNextGround()
        {
            var ground = _groundPool.Get();
            ground.transform.position = new Vector3(0, 0, _groundsSpawned * ground.BoxSize.y);

            if (_groundsSpawned > 0 && _groundsSpawned < _settings.RoadLength)
            {
                SpawnEnemiesOnGround(ground.transform.position);
            }

            _groundsSpawned++;
        }

        private void SpawnEnemiesOnGround(Vector3 groundPosition)
        {
            for (int i = 0; i < _settings.EnemyCount; i++)
            {
                var enemy = _enemyPool.Get();
                float x = Random.Range(_enemyRangeX.x, _enemyRangeX.y);
                float z = Random.Range(-_halfGroundLength,  _halfGroundLength);
                enemy.transform.position = groundPosition + new Vector3(x, 0, z);
                enemy.transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
                enemy.Init(_settings);
                _healthBarManager.Register(enemy.Health);
            }
        }

        private void ResetArea()
        {
            _groundsSpawned = 0;
            var groundArray = _spawnedGround.ToArray();
            foreach (var ground in groundArray)
            {
                _groundPool.Release(ground);
            }

            var enemyArray = _spawnedEnemy.ToArray();
            foreach (var enemy in enemyArray)
            {
                _enemyPool.Release(enemy);
            }

            _healthBarManager.Reset();
        }
    }
}
