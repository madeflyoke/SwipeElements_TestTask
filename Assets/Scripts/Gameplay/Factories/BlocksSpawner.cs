using Gameplay.Blocks;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.Factories
{
    public class BlocksSpawner : MonoBehaviour
    {
        [SerializeField] private Block _blockPrefab;
        [SerializeField] private BlocksConfig _config;
        
        public Block Spawn(BlockType blockType, Transform parent) //TODO Pool?
        {
            var blockViewData = _config.GetBlockViewData(blockType);

            var block = Instantiate(_blockPrefab, parent);
#if UNITY_EDITOR
            block.gameObject.name = parent.name + "Block";
#endif
            block.Initialize(blockType, blockViewData.AnimatorOverrideController, _config.MovementAnimationData);
            return block;
        }
    }
}
