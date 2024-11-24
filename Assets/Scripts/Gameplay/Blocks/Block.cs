using System.Collections.Generic;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.Blocks
{
    public class Block : MonoBehaviour
    {
        public BlockType Type { get; private set; }

        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private bool _isBusy;
        
        public void Initialize(BlockType type)
        {
            Type = type;
        }

        public void SetBusy(bool value)
        {
            _isBusy = value;
            _collider.enabled = !value;
        }
        
        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }
        
        public void DestroyBlock()
        {
            Destroy(gameObject);
        }
    }
}
