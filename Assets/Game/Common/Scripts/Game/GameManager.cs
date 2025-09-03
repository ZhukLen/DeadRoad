using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Car.Scripts;
using Game.Ground.Assets.Scripts;
using Zenject;

namespace Game.Common.Scripts.Game
{
    public class GameManager : IInitializable, System.IDisposable
    {
        private readonly CarController _carController;
        private readonly AreaManager _areaManager;
        private readonly UIManager _uiManager;
        private readonly SceneController _sceneController;

        private CancellationTokenSource _cancellation;
        private bool _isGameStarted;

        [Inject]
        public GameManager(CarController carController, AreaManager areaManager, UIManager uiManager, SceneController sceneController)
        {
            _sceneController = sceneController;
            _carController = carController;
            _areaManager = areaManager;
            _uiManager = uiManager;
        }

        public void Initialize()
        {
            Prepare();
        }

        public void Dispose()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
        }

        private void Prepare()
        {
            if (_cancellation is {IsCancellationRequested: false}) _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
            _sceneController.PrepareScene();
            _uiManager.ShowTapToStart(StartGame);
        }

        private void StartGame()
        {
            StartGameAsync().Forget();
        }

        private async UniTaskVoid StartGameAsync()
        {
            if (_isGameStarted) return;
            _isGameStarted = true;

            _cancellation = new CancellationTokenSource();
            _uiManager.HideOverlay();
            _sceneController.StartGameplay();
            _carController.Run();

            await  WaitGameEnd();
        }

        private async UniTask WaitGameEnd()
        {
            while (_isGameStarted)
            {
                if (_carController.IsDestroy)
                {
                    GameOver(false);
                    return;
                }

                if (_carController.Car.position.z >= _areaManager.GetFullRoadLength())
                {
                    GameOver(true);
                    return;
                }

                await UniTask.Yield(_cancellation.Token);
            }
        }

        private void GameOver(bool win)
        {
            if (!_isGameStarted) return;
            _isGameStarted = false;
            _cancellation?.Cancel();
            _carController.Stop();
            _uiManager.ShowWinLose(win, Prepare);
        }
    }
}