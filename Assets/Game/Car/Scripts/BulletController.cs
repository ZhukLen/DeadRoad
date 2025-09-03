using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Stickman.Assets.Scripts;
using UnityEngine;

namespace Game.Car.Scripts
{
    public class BulletController : MonoBehaviour
    {
        public event Action<BulletController> OnBulletDestroy;

        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private Renderer _renderer;

        private float _damage = 25f;
        private Vector3 _speed = Vector3.forward;

        private CancellationTokenSource _cancellation;

        private CancellationToken DestroyCancellation => gameObject.GetCancellationTokenOnDestroy();

        public void Init(float speed, float damage)
        {
            _damage = damage;
            _speed = Vector3.forward * speed;
        }

        public void Shoot(Transform firePoint)
        {
            if (_cancellation is { IsCancellationRequested : false }) _cancellation.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
            transform.position = firePoint.position;
            transform.rotation = firePoint.rotation;
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(DestroyCancellation);
            _trail.Clear();
            gameObject.SetActive(true);
            Flight().Forget();
        }

        private async UniTaskVoid Flight()
        {
            await UniTask.NextFrame(_cancellation.Token);
            while (_cancellation is { IsCancellationRequested: false } && _renderer.isVisible && gameObject.activeInHierarchy)
            {
                await UniTask.Yield(_cancellation.Token);
                transform.Translate(_speed * Time.deltaTime);
            }

            DestroyBullet();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(_damage);
                DestroyBullet();
            }
        }

        private void DestroyBullet()
        {
            _cancellation?.Cancel();
            if (gameObject.activeInHierarchy)
            {
                OnBulletDestroy?.Invoke(this);
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
        }
    }
}