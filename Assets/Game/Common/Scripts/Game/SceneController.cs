using Cinemachine;
using Game.Car.Scripts;
using Game.Common.Scripts.HealthBar;
using Game.Ground.Assets.Scripts;
using UnityEngine;
using Zenject;

namespace Game.Common.Scripts.Game
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _idleCamera;
        [SerializeField] private CinemachineVirtualCamera _carCamera;

        private CarController _carController;
        private AreaManager _areaManager;

        [Inject]
        private void Construct(CarController carController, AreaManager areaManager)
        {
            _carController = carController;
            _areaManager = areaManager;
        }

        public void PrepareScene()
        {
            _carController.Reset();
            _areaManager.InitialSpawn();

            _idleCamera.Priority = 100;
            _carCamera.Priority = 0;
        }

        public void StartGameplay()
        {
            _idleCamera.Priority = 0;
            _carCamera.Priority = 100;
        }
    }
}
