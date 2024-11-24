using Gameplay.Blocks;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.GameField
{
    public class GridCell : MonoBehaviour
    {
        public BlockType RelatedBlockType => IsEmpty? BlockType.NONE : CurrentBlock.Type;
        public bool IsEmpty => CurrentBlock == null;
        public Vector2Int Coord { get; private set; }
        public Block CurrentBlock { get; private set; }

        public void Initialize(Vector2Int coord)
        {
            Coord = coord;
        }

        public void SetBlock(Block block)
        {
            CurrentBlock = block;
            if (block!=null)
            {
                CurrentBlock.transform.SetParent(transform);
                CurrentBlock.transform.localPosition = Vector3.zero;
            }
        }

        public void Clear()
        {
            CurrentBlock.DestroyBlock();
            CurrentBlock = null;
        }
    }
}
