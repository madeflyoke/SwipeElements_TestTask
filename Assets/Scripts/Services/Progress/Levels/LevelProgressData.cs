using System;
using Gameplay.Blocks.Enums;
using Gameplay.Levels.Data;
using Gameplay.Levels.Enums;

namespace Services.Progress.Levels
{
    [Serializable]
    public class LevelProgressData
    {
        public int LevelId;
        public bool IsCompleted;
    }

    [Serializable]
    public class LevelProgressDataExtended : LevelProgressData
    {
        public LevelSection RelatedSection;
        public BlockType[,] GridState;
        public bool IsStarted;

        public LevelData ConvertToLevelData()
        {
            return new LevelData()
            {
                BlocksData = GridState,
                LevelId = this.LevelId
            };
        }
    }
}
