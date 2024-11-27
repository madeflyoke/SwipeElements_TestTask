using System;
using DG.Tweening;
using Gameplay.Blocks.Enums;
using UnityEngine;
using Utility;

namespace Gameplay.Blocks
{
    public class Block : MonoBehaviour
    {
        public event Action BlockDestroyed;
        public BlockType Type { get; private set; }

        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        private Tween _movingTween;
        private MovementAnimationData _movementAnimationData;
        private bool _isBusy;

        public void Initialize(BlockType type, AnimatorOverrideController controller,
            MovementAnimationData movementAnimationData)
        {
            Type = type;
            _animator.runtimeAnimatorController = controller;
            _movementAnimationData = movementAnimationData;
        }

        public void SetSortingOrder(int index)
        {
            _spriteRenderer.sortingOrder = index;
        }

        public void StartDestroyingBlock()
        {
            SetBusy(true);
            _animator.SetTrigger(Constants.Animations.BLOCK_DESTROY_TRIGGER);
        }

        public void MoveTo(Vector3 pos, Action onComplete)
        {
            _movingTween?.Kill(true);
            SetBusy(true);
            
            _movingTween = transform
                .DOMove(pos, _movementAnimationData.Duration) //maybe speed based? for constant speed
                .OnComplete(() =>
                {
                    SetBusy(false);
                    onComplete?.Invoke();
                });
            
            if (_movementAnimationData.WithCustomEaseParameters)
            {
                _movingTween.SetEase(_movementAnimationData.Ease, _movementAnimationData.CustomAmplitude,
                    _movementAnimationData.CustomPeriod);
            }
            else
            {
                _movingTween.SetEase(_movementAnimationData.Ease);
            }
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;
            _collider.enabled = !value;
        }

        private void OnDisable()
        {
            _movingTween?.Kill();
        }

        private void OnDeathAnimation() //animation event
        {
            BlockDestroyed?.Invoke();
            Destroy(gameObject);
        }
        
        [Serializable]
        public struct MovementAnimationData
        {
            public float Duration;
            public Ease Ease;
            public bool WithCustomEaseParameters;
            public float CustomAmplitude;
            public float CustomPeriod;
        }
    }
}