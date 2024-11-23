using Gameplay.Levels.Data;

namespace Signals
{
    public struct GameplayStartedSignal
    {
        public readonly LevelData LevelData;

        public GameplayStartedSignal(LevelData levelData)
        {
            LevelData = levelData;
        }
    }
}