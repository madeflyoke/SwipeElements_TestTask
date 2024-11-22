using System.Collections.Generic;
using System.Linq;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.Blocks
{
    [CreateAssetMenu(fileName = "BlocksConfig", menuName = "Gameplay/Blocks/BlocksConfig")]
    public class BlocksConfig : ScriptableObject
    {
        [SerializeField] private List<BlockViewData> _blocksViewData;

        public BlockViewData GetBlockViewData(BlockType blockType)
        {
            return _blocksViewData.FirstOrDefault(x=>x.Type==blockType);
        }
    }
}