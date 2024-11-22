namespace Signals
{
    public struct GameplayStartedSignal
    {
        public readonly int LevelId;

        public GameplayStartedSignal(int levelId)
        {
            LevelId = levelId;
        }
    }
}