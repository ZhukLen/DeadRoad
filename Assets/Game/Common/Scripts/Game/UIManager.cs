using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Common.Scripts.Game
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _overlay;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _button;
        [SerializeField] private AnimationCurve _textSizeAnimation;

        private CancellationTokenSource _cancellation;
        private float _animationDuration;

        private void Start()
        {
            _animationDuration = _textSizeAnimation.keys.Last().time;
        }

        public void ShowWinLose(bool win, Action onTap)
        {
            _overlay.SetActive(true);
            _messageText.SetText(win ? "You WIN" : "You Lose");
            Animate().Forget();

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                onTap?.Invoke();
                if (_cancellation is {IsCancellationRequested: false}) _cancellation.Cancel();
                _messageText.transform.localScale = Vector3.one;
            });
        }

        public void ShowTapToStart(Action onTap)
        {
            _overlay.SetActive(true);
            _messageText.SetText("Tap to START");

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                onTap?.Invoke();
            });
        }

        public void HideOverlay()
        {
            _overlay.SetActive(false);
        }

        private async UniTaskVoid Animate()
        {
            if (_cancellation is {IsCancellationRequested: false}) _cancellation.Cancel();
            var time = 0F;
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(gameObject.GetCancellationTokenOnDestroy());

            while (_cancellation is {IsCancellationRequested: false} && _animationDuration >= time)
            {
                var size = _textSizeAnimation.Evaluate(time);
                _messageText.transform.localScale = new Vector3(size, size, 1F);
                await UniTask.NextFrame(_cancellation.Token);
                time += Time.deltaTime;
            }

            _messageText.transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            if (_cancellation is {IsCancellationRequested: false}) _cancellation.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
        }
    }
}
