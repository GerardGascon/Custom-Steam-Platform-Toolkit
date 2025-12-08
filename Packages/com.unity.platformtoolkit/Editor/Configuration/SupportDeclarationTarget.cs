using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class SupportDeclarationTarget
    {
        private SupportDeclarationTargetsManager m_SupportDeclarationTargetsTargetsManager;
        private IPlatformToolkitSupportDeclaration m_Declaration;

        public IReadOnlyList<BuildTarget> SupportedBuildTargets => m_Declaration.SupportedPlatforms.OrderBy(p=> p.ToString()).ToList();

        [CreateProperty]
        public string Name => m_Declaration.DisplayName;

        [CreateProperty]
        public ToggleButtonGroupState CurrentToggleGroupState
        {
            get => GetToggleButtonGroupState();
            set => UpdateTargetPlatform(value);
        }

        public SupportDeclarationTarget(IPlatformToolkitSupportDeclaration declaration, SupportDeclarationTargetsManager supportDeclarationTargetsManager)
        {
            m_SupportDeclarationTargetsTargetsManager =  supportDeclarationTargetsManager;
            m_Declaration = declaration;
        }

        private ToggleButtonGroupState GetToggleButtonGroupState()
        {
            var selectedPlatforms = m_SupportDeclarationTargetsTargetsManager.GetTargetedPlatforms(m_Declaration.Key);
            var options = SupportedBuildTargets.Select(supportedBuildTarget => selectedPlatforms.Contains(supportedBuildTarget)).ToList();
            return ToggleButtonGroupState.CreateFromOptions(options);
        }

        private void UpdateTargetPlatform(ToggleButtonGroupState state)
        {
            var selectedPlatforms = m_SupportDeclarationTargetsTargetsManager.GetTargetedPlatforms(m_Declaration.Key);
            for (var i = 0; i < state.length; i++)
            {
                var platform = SupportedBuildTargets[i];
                if (state[i] && !selectedPlatforms.Contains(platform))
                    m_SupportDeclarationTargetsTargetsManager.TryAddBuildTarget(m_Declaration.Key, platform);
            }
        }
    }
}
