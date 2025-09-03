using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Common.Scripts.Game
{
    public class DistanceTracker<T> : MonoBehaviour where T : DistanceTracker<T>
    {
        public event Action<T> OnDestroy;

        [SerializeField] private float _destroyDistance = 100f;

        private readonly TimeSpan _delay = TimeSpan.FromSeconds(1d);
        protected Transform _target;
        protected CancellationTokenSource _cancellation;

        private bool IsBack => _target.position.z - transform.position.z > _destroyDistance;

        public virtual void Init(Transform target)
        {
            _target = target;
        }

        protected virtual void OnEnable()
        {
            if (_cancellation is { IsCancellationRequested: false }) _cancellation.Cancel();

            _cancellation?.Dispose();
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(gameObject.GetCancellationTokenOnDestroy());
            TrackingDistance().Forget();
        }

        private async UniTaskVoid TrackingDistance()
        {
            await UniTask.NextFrame(_cancellation.Token);
            while (_cancellation is {IsCancellationRequested: false} && !IsBack && gameObject.activeInHierarchy)
            {
                await UniTask.Delay(_delay, cancellationToken: _cancellation.Token);
            }

            DestroyObject();
        }

        protected virtual void DestroyObject()
        {
            gameObject.SetActive(false);
            OnDestroy?.Invoke((T)this);
            if (_cancellation is { IsCancellationRequested: false })
            {
                _cancellation.Cancel();
            }
        }

        private void OnDisable()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
        }
    }
}