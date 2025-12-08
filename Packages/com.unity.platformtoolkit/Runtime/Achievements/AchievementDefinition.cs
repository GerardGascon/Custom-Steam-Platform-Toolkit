using System;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    [Serializable]
    internal class AchievementDefinition
    {
        public string Id => m_Id;

        public bool Progressive => m_Progressive;
        public int ProgressTarget => m_ProgressTarget;

        [SerializeField]
        private string m_Id;
        [SerializeField]
        private bool m_Progressive;
        [SerializeField]
        private int m_ProgressTarget;

        public AchievementDefinition(string id, bool progressive = false, int progressTarget = 1)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Achievement id must be non-empty");

            m_Id = id;
            m_Progressive = progressive;
            if (!progressive)
                m_ProgressTarget = 1;
            else if (progressTarget < 1)
                m_ProgressTarget = 1;
            else
                m_ProgressTarget = progressTarget;
        }

        /// <summary>
        /// Get progress in a different range. Convenient for remapping from pt achievement range to a native range.
        /// </summary>
        /// <param name="targetRangeUpperBound">Upper bound of the range to which progress is remapped.</param>
        /// <param name="progress">Progress in PT range.</param>
        /// <returns>Progress remapped to [0; targetRangeUpperBound] range.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public int RemapProgress(int targetRangeUpperBound, int progress)
        {
            if (progress > ProgressTarget)
                throw new ArgumentOutOfRangeException(nameof(progress));

            var gradation = targetRangeUpperBound / (double)ProgressTarget;
            return (int)(gradation * progress);
        }
    }
}
