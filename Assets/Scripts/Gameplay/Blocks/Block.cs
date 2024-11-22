using System.Collections.Generic;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.Blocks
{
    public class Block : MonoBehaviour
    {
        public BlockType Type { get; private set; }

        [SerializeField] private SpriteRenderer _spriteRenderer;
        
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
    }
}
