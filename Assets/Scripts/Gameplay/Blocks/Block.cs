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
        private bool _isBusy;
        
        public void Initialize(BlockType type, AnimatorOverrideController controller)
        {
            Type = type;
            _animator.runtimeAnimatorController = controller;
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

        public void MoveTo(Vector3 pos,Action onComplete)
        {
            _movingTween?.Kill(true);
            SetBusy(true);
            _movingTween = transform.DOMove(pos, 0.3f)
                .SetEase(Ease.OutElastic,amplitude:0.05f,period:0.25f) //TODO Config
                .OnComplete(() =>
                {
                    SetBusy(false);
                    onComplete?.Invoke();
                });
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
    }
}
