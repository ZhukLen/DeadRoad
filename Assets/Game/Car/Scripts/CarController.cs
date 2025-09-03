using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Common.Scripts.Game;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game.Car.Scripts
{
    public class CarController : MonoBehaviour
    {
        [SerializeField] private CarHealth _carHealth;
        [SerializeField] private TurretController _turret;

        public UnityEvent _onRun;
        public UnityEvent _onDeath;
        public bool IsDestroy { get; private set; } = false;

        private BaseGameSettings _settings;
        private CancellationTokenSource _cancellation;

        public Transform Car => this.transform;

        [Inject]
        private void Construct(BaseGameSettings settings)
        {
            _settings = settings;
            _turret.Init(_settings);
        }

        public void Reset()
        {
            Car.position = Vector3.zero;
        }

        public void Run()
        {
            IsDestroy = false;
            _carHealth.Init(_settings.CarHealth);
            _carHealth.OnDeath += DeathHandler;
            _carHealth.Show();
            if (_cancellation is {IsCancellationRequested: false}) _cancellation.Cancel();
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(gameObject.GetCancellationTokenOnDestroy());
            _turret.MouseTracking(_cancellation).Forget();
            _turret.Fire(_cancellation).Forget();
            Move().Forget();
            _onRun?.Invoke();
        }

        public void Stop()
        {
            if (_cancellation is {IsCancellationRequested: false}) _cancellation.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
            _carHealth.Hide();
        }

        private void DeathHandler()
        {
            _carHealth.OnDeath -= DeathHandler;
            _onDeath?.Invoke();
            IsDestroy = true;
        }

        private async UniTaskVoid Move()
        {
            while (_cancellation is {IsCancellationRequested: false})
            {
                transform.Translate(Vector3.forward * _settings.CarSpeed * Time.deltaTime);
                await UniTask.Yield(_cancellation.Token);
            }
        }
    }
}