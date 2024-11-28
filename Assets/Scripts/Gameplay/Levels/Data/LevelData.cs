using System;
using Gameplay.Blocks.Enums;
using Newtonsoft.Json;

namespace Gameplay.Levels.Data
{
    [Serializable]
    public class LevelData
    {
        [JsonIgnore] public int GridWidth => BlocksData.GetLength(0);
        [JsonIgnore] public int GridHeight => BlocksData.GetLength(1);
        
        [JsonIgnore] public BlockType[,] BlocksData => _blocksData;
        public int LevelId;

        [JsonProperty] private BlockType[,] _blocksData;
        
        [JsonConstructor]
        public LevelData(int levelId, BlockType[,] blocksData)
        {
            LevelId = levelId;
            _blocksData = blocksData;
        }

        public LevelData(int levelId)
        {
            LevelId = levelId;
            _blocksData = new BlockType[0,0];
        }
        
        public LevelData Copy()
        {
            return new LevelData(this.LevelId, this._blocksData);
        }

#if UNITY_EDITOR
        
        public void SaveDataFromEditor(BlockType[,] blocksEditorData)
        {
            _blocksData = blocksEditorData;
        }

        public void ClearDataFromEditor()
        {
            _blocksData = new BlockType[0, 0];
        }
       
#endif
    }
}
