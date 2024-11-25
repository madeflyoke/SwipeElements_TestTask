using Gameplay.Levels.Data;
using Gameplay.Levels.Enums;

namespace Signals
{
    public struct GameplayStartedSignal
    {
        public readonly LevelSection RelatedSection;
        public readonly LevelData LevelData;

        public GameplayStartedSignal(LevelSection section, LevelData levelData)
        {
            RelatedSection = section;
            LevelData = levelData;
        }
    }
}