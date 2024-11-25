using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Input.Enums;
using Services.Interfaces;
using Signals;
using UnityEngine;
using Zenject;

namespace Services.Input
{
    public class InputService : IService
    {
        [Inject] private SignalBus _signalBus;
        
        public event Action<Vector3, SwipeDirection> SwipePerformed;

        private InputSwipe _inputSwipe;

        public UniTask Initialize(CancellationTokenSource cts)
        {
            _signalBus.Subscribe<GameplayStartedSignal>(OnGameplayStarted);
            
            _inputSwipe = new InputSwipe();
            _inputSwipe.SwipePerformed += OnSwipePerformed;
            return UniTask.CompletedTask;
        }

        private void OnSwipePerformed(Vector3 startPos, SwipeDirection swipeDirection)
        {
            SwipePerformed?.Invoke(startPos, swipeDirection);
        }
        
        private void OnGameplayStarted()
        {
            _inputSwipe.SetActive(true);
        }
        
        //gameplay stopped (menu etc)  _inputSwipe.SetActive(false);
        
        public void Dispose()
        {
            _inputSwipe.SwipePerformed -= OnSwipePerformed;
            _signalBus.Unsubscribe<GameplayStartedSignal>(OnGameplayStarted);

        }
    }
}
