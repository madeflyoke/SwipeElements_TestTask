using Gameplay.Blocks;
using UnityEngine;

namespace Gameplay.GameField
{
    public class GridCell : MonoBehaviour
    {
        private Vector2Int _coord;
        private Block _currentBlock;

        public void Initialize(Vector2Int coord)
        {
            _coord = coord;
        }

        public void SetBlock(Block block)
        {
            _currentBlock = block;
        }
    }
}
