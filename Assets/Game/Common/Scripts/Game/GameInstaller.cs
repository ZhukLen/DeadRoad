using Game.Car.Scripts;
using Game.Common.Scripts.HealthBar;
using Game.Ground.Assets.Scripts;
using UnityEngine;
using Zenject;

namespace Game.Common.Scripts.Game
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private BaseGameSettings _baseGameSettings;
        [SerializeField] private SceneController _sceneController;
        [SerializeField] private CarController _carController;
        [SerializeField] private AreaManager _areaManager;
        [SerializeField] private HealthBarManager _healthBarManager;
        [SerializeField] private UIManager _uiManager;

        public override void InstallBindings()
        {
            Container.Bind<BaseGameSettings>().FromInstance(_baseGameSettings).AsSingle();
            Container.Bind<CarController>().FromInstance(_carController).AsSingle();
            Container.Bind<HealthBarManager>().FromInstance(_healthBarManager).AsSingle();
            Container.Bind<AreaManager>().FromInstance(_areaManager).AsSingle();
            Container.Bind<UIManager>().FromInstance(_uiManager).AsSingle();
            Container.Bind<SceneController>().FromInstance(_sceneController).AsSingle();

            Container.BindInterfacesAndSelfTo<GameManager>().AsSingle().NonLazy();;
        }
    }
}
