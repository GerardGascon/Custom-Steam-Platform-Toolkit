using System;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    [Serializable]
    internal class AchievementDefinitionWithNativeId<T> : AchievementDefinition
    {
        public T NativeId => m_NativeId;

        [SerializeField]
        private T m_NativeId;

        public AchievementDefinitionWithNativeId(string id, T nativeId, bool progressive = false, int progressTarget = 1) : base(id, progressive, progressTarget)
        {
            m_NativeId = nativeId;
        }
    }
}
