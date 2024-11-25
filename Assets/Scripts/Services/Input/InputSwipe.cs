using System;
using Services.Input.Enums;
using UniRx;
using UnityEngine;
using Input = UnityEngine.Input;

namespace Services.Input
{
    public class InputSwipe : IDisposable
    {
        public event Action<Vector3, SwipeDirection> SwipePerformed; 
        
        private float _minSwipeDistance = 100f;
        private float _maxSwipeTime = 0.5f;

        private Vector2 _startPosition;
        private float _startTime;
        private bool _isDragging;

        private bool _isActive;
        private readonly IDisposable _disposable;

        public InputSwipe()
        {
            SetActive(false);
            _disposable = Observable.EveryUpdate().Where(x=>_isActive).Subscribe(_ => ManualUpdate());
        }

        public void SetActive(bool value)
        {
            _isActive = value;
        }

        private void ManualUpdate()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _startPosition =UnityEngine.Input.mousePosition;
                _startTime = Time.time;
                _isDragging = true;
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) && _isDragging)
            {
                _isDragging = false;
                Vector2 endPosition = UnityEngine.Input.mousePosition;
                var endTime = Time.time;

                if (endTime - _startTime <= _maxSwipeTime)
                {
                    var dir = endPosition - _startPosition;

                    if (dir.magnitude >= _minSwipeDistance)
                    {
                        var swipeDir = GetSwipeDirection(dir);
                        SwipePerformed?.Invoke(_startPosition, swipeDir);
                    }
                }
            }
        }

        private SwipeDirection GetSwipeDirection(Vector2 swipe)
        {
            swipe = swipe.normalized;

            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                return swipe.x > 0 ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
            }
            else
            {
                return swipe.y > 0 ? SwipeDirection.UP : SwipeDirection.DOWN;
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}