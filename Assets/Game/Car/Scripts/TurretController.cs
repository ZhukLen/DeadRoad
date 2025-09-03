using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Common.Scripts.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Car.Scripts
{
    public class TurretController : MonoBehaviour
    {
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        [SerializeField] private BulletController _bulletPrefab;
        [SerializeField] private Transform _turretTransform;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private int _bulletsPoolCapacity = 10;

        private Camera _mainCamera;
        private BaseGameSettings _settings;
        private ObjectPool<BulletController> _bulletsPool;
        private TimeSpan _fireDelay;
        private int _layerMask;

        public void Init(BaseGameSettings settings)
        {
            _settings = settings;
            _mainCamera = Camera.main;
            _layerMask = LayerMask.GetMask("Ground");
            _fireDelay = TimeSpan.FromSeconds(1 / _settings.FireRate);
            _bulletsPool = new ObjectPool<BulletController>(CreateBullet, OnBulletGet, defaultCapacity: _bulletsPoolCapacity);
        }

        private BulletController CreateBullet()
        {
            var bullet = Instantiate<BulletController>(_bulletPrefab);
            bullet.Init(_settings.BulletSpeed, _settings.BulletDamage);
            bullet.OnBulletDestroy += _bulletsPool.Release;
            return bullet;
        }

        private void OnBulletGet(BulletController bullet)
        {
            bullet.Shoot(_firePoint);
        }

        public async UniTaskVoid Fire(CancellationTokenSource cancellation)
        {
            while (cancellation is {IsCancellationRequested: false})
            {
                _bulletsPool.Get();
                await UniTask.Delay(_fireDelay, cancellationToken: cancellation.Token);
            }
        }

        public async UniTaskVoid MouseTracking(CancellationTokenSource cancellation)
        {
            while (cancellation is {IsCancellationRequested: false})
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.RaycastNonAlloc(ray, _raycastHits, 1000f, _layerMask) > 0)
                {
                    Vector3 lookDir = _raycastHits[0].point - _turretTransform.position;
                    lookDir.y = 0;
                    _turretTransform.forward = lookDir;
                }
                await UniTask.Yield(cancellation.Token);
            }
        }
    }
}
