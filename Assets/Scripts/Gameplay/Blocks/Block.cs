using System;
using DG.Tweening;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.Blocks
{
    public class Block : MonoBehaviour
    {
        public BlockType Type { get; private set; }
        
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Tween _movingTween;
        private bool _isBusy;
        
        public void Initialize(BlockType type)
        {
            Type = type;
        }

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }
        
        public void DestroyBlock()
        {
            Destroy(gameObject);
        }
        
        public void MoveTo(Vector3 pos,Action onComplete)
        {
            SetBusy(true);
            _movingTween = transform.DOMove(pos, 3f)
                //.SetEase(Ease.OutElastic,amplitude:0.05f,period:0.25f) //TODO Config
                .SetEase(Ease.Linear)
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
    }
}
