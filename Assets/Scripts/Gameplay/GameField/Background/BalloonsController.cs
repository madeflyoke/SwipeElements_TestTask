using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EasyButtons;
using Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.GameField.Background
{
    [RequireComponent(typeof(BalloonsSpawner))]
    public class BalloonsController : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        [SerializeField] private BalloonsConfig _balloonsConfig;
        [SerializeField] private BalloonsSpawner _balloonsSpawner;
        [SerializeField] private bool _autoLaunch= true;
        private CancellationTokenSource _cts = new();

        private void Awake()
        {
            if (_autoLaunch)
            {
                _signalBus.Subscribe<LevelStartedSignal>(StartLaunchingBalloons);
            }
        }

        [Button]
        private void StartLaunchingBalloons()
        {
            _balloonsSpawner.Clear();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            LaunchBalloons();
        }
        
        private async void LaunchBalloons()
        {
            for (int i = 0; i < _balloonsConfig.MaxBalloonsCount; i++)
            {
                SpawnBalloon();
                var canceled =await UniTask.Delay(TimeSpan.FromSeconds(_balloonsConfig.SpawnDelaySeconds), cancellationToken:_cts.Token)
                    .SuppressCancellationThrow();
                if (canceled)
                {
                    return;
                }
            }
        }

        private void SpawnBalloon()
        {
            var balloon = _balloonsSpawner.SpawnRandomBalloon();
            balloon.OutOfScreen += OnBalloonOutOfScreen;
            balloon.SetMovementState(true);
        }

        private void OnBalloonOutOfScreen(Balloon balloon)
        {
            balloon.OutOfScreen -= OnBalloonOutOfScreen;
            SpawnBalloon(); //dont need spawn delay - balloons have random speed so it will be random-looking as well
        }
        
        private void OnDisable()
        {
            _cts?.Cancel();
            _signalBus.TryUnsubscribe<LevelStartedSignal>(StartLaunchingBalloons);
        }
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            _balloonsSpawner ??= GetComponent<BalloonsSpawner>();
        }

#endif
    }
}