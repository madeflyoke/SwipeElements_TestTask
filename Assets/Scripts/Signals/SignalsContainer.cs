using Gameplay.Blocks.Enums;
using Gameplay.Levels.Data;
using Gameplay.Levels.Enums;

namespace Signals
{
    public struct LevelStartedSignal
    {
        public readonly LevelSection RelatedSection;
        public readonly LevelData LevelData;

        public LevelStartedSignal(LevelSection section, LevelData levelData)
        {
            RelatedSection = section;
            LevelData = levelData;
        }
    }

    public struct LevelCompletedSignal
    {
        
    }

    public struct GameFieldChangedSignal
    {
        public readonly BlockType[,] GridBlocksState;
        
        public GameFieldChangedSignal(BlockType[,] gridBlocksState)
        {
            GridBlocksState = gridBlocksState;
        }
    }
}