using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Implement this interface to declare achievement support and integrate with the Achievement editor window.</summary>
    /// <seealso cref="BaseEditorConfiguration.GetAchievementConfiguration"/>
    internal interface IAchievementConfiguration
    {
        /// <summary>Allows editing achievement data in the Achievement Editor Window.</summary>
        public IAchievementEditorProvider EditorProvider { get; }

        /// <summary>The import and export provider interface for achievements for a specific platform <see cref="IAchievementImportExport"/>.</summary>
        IAchievementImportExport ImportExportProvider { get; }

        /// <summary>The name of the platform this achievement configuration is attached to. </summary>
        public string PlatformName { get; }
    }
}
