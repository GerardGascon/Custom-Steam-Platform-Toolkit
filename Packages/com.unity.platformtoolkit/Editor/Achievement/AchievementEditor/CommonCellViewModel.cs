using System;
using System.Collections.Generic;
using Unity.Properties;

namespace Unity.PlatformToolkit.Editor
{
    internal class CommonCellViewModel
    {
        [CreateProperty]
        public StoredAchievement StoredAchievement { get; set; }

        [CreateProperty]
        public IReadOnlyList<string> Warnings { get; set; }
    }
}
