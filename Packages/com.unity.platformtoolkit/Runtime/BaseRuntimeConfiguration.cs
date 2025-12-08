using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    internal abstract class BaseRuntimeConfiguration : ScriptableObject
    {
#if !UNITY_EDITOR
        private void Awake()
        {
            if (InstantiatePlatformToolkit() is { } implementation)
                PlatformToolkit.InjectImplementation(implementation);
        }
#endif
        public abstract IPlatformToolkit InstantiatePlatformToolkit();
    }
}
