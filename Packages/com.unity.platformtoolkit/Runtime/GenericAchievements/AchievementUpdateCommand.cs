using System;

namespace Unity.PlatformToolkit
{
    internal class AchievementUpdateCommand<T> where T : AchievementDefinition
    {
        public readonly AchievementProgressState<T> Achievement;

        private int m_Progress;
        public int Progress
        {
            get => m_Progress;
            set
            {
                if (value > m_Progress)
                {
                    m_Progress = value;
                }
            }
        }

        public DateTime TimeAdded { get; private set; }

        public AchievementUpdateCommand(AchievementProgressState<T> achievement, int progress)
        {
            Achievement = achievement;
            Progress = progress;
        }

        public void SetTimeAdded()
        {
            TimeAdded = DateTime.Now;
        }

        public bool IsCommandRelevant()
        {
            return Achievement.IsProgressRelevant(Progress);
        }
    }
}
