namespace Unity.PlatformToolkit
{
    internal class AchievementProgressState<T> where T : AchievementDefinition
    {
        public readonly T Definition;
        public long NextProgress;
        public bool Unlocked;
        public bool Invalid;

        public AchievementProgressState(T achievement)
        {
            Definition = achievement;
        }

        public bool IsProgressRelevant(int progress)
        {
            if (Invalid || Unlocked)
                return false;
            if (progress < NextProgress)
                return false;
            if (progress < 1)
                return false;

            return true;
        }
    }
}
