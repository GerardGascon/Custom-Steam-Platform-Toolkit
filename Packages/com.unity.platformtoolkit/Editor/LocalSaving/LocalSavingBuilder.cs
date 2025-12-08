using System;
using System.Collections.Generic;
using System.Linq;
using Unity.PlatformToolkit.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.PlatformToolkit.LocalSaving.Editor
{
    internal class LocalSavingBuilder : IPlatformToolkitBuilder
    {
        public void PostBuild(BuildReport buildReport) { }

        BaseRuntimeConfiguration IPlatformToolkitBuilder.PrepareBuild(BuildReport buildReport)
        {
            return ScriptableObject.CreateInstance<LocalSavingRuntimeConfiguration>();
        }
    }
}
