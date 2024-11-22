using Gameplay.Blocks;
using Gameplay.Blocks.Enums;
using UnityEngine;
using Utility;

namespace Gameplay.Factories
{
    public class BlocksSpawner : MonoBehaviour //factory?
    {
        [SerializeField] private Block _blockPrefab;
        [SerializeField] private BlocksConfig _config;
        
        public Block Spawn(BlockType blockType, Vector2 position, Transform parent)
        {
            var blockViewData = _config.GetBlockViewData(blockType);
            
            var block = Instantiate(_blockPrefab, position, Quaternion.identity, parent);
            block.Initialize(blockType);
            block.SetSprite(blockViewData.BaseSprite);
            return block;
        }

        public Block Spawn(BlockType blockType, Transform parent)
        {
            var blockViewData = _config.GetBlockViewData(blockType);

            var block = Instantiate(_blockPrefab, parent);
            block.Initialize(blockType);
            block.SetSprite(blockViewData.BaseSprite);
            return block;
        }
    }
}
