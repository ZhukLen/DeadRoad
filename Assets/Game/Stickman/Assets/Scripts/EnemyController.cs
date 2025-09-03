using Cysharp.Threading.Tasks;
using Game.Car.Scripts;
using Game.Common.Scripts.Game;
using UnityEngine;

namespace Game.Stickman.Assets.Scripts
{
    public class EnemyController : DistanceTracker<EnemyController>
    {
        private static readonly int Run = Animator.StringToHash("Run");
        private static readonly int Multiplier = Animator.StringToHash("Multiplier");

        [SerializeField] private Animator _animator;
        private float _speedMultiplier = 2f;
        private float _damage = 1f;
        private float _detectRange = 25f;
        [field: SerializeField] public EnemyHealth Health { get; private set; }

        private bool _isRunning = false;

        public void Init(BaseGameSettings settings)
        {
            _speedMultiplier = settings.EnemySpeed;
            _damage = settings.EnemyDamage;
            _detectRange = settings.EnemyDetectRange;
            Health.Init(settings.EnemyHealth);
            Health.OnDeath += DeathHandler;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _animator.SetFloat(Multiplier, _speedMultiplier);
            _isRunning = false;
            _animator.SetBool(Run, false);
            TrackingTarget().Forget();
        }

        private async UniTaskVoid TrackingTarget()
        {
            if (_cancellation == null || _cancellation.IsCancellationRequested)
                await UniTask.WaitUntil(() => _cancellation is { IsCancellationRequested: false });
            await UniTask.NextFrame(_cancellation.Token);

            while (_cancellation is { IsCancellationRequested: false })
            {
                float distance = Vector3.Distance(transform.position, _target.position);

                if (!_isRunning)
                {
                    if (distance <= _detectRange)
                    {
                        _animator.SetBool(Run, true);
                        _isRunning = true;
                    }
                }
                else
                {
                    Vector3 direction = _target.position - transform.position;
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                await UniTask.Yield(_cancellation.Token);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CarHealth car))
            {
                car.TakeDamage(_damage);
                DestroyObject();
            }
        }

        private void DeathHandler(EnemyHealth health)
        {
            Health.OnDeath -= DeathHandler;
            if (gameObject.activeInHierarchy) DestroyObject();
        }
    }
}
