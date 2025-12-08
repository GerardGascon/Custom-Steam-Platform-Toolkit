using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.Editor
{
    [Serializable]
    internal class SupportDeclarationTargetsManager
    {
        [SerializeField]
        private SerializableDictionary<BuildTarget, string> m_BuildTargetToDeclarationMappings = new ();

        private IReadOnlyList<IPlatformToolkitSupportDeclaration> m_Declarations;

        public event Action SupportDeclarationTargetChanged;

        public void SetSupportDeclarations(IReadOnlyList<IPlatformToolkitSupportDeclaration> declarations)
        {
            m_Declarations = declarations ?? throw new ArgumentNullException("The given declarations list is null.");

            var mappingsToRemove = m_BuildTargetToDeclarationMappings.Where(m => declarations.All(d => d.Key != m.Value)).ToList();
            foreach (var mappingToRemove in mappingsToRemove)
            {
                TryRemoveBuildTarget(mappingToRemove.Key, out var dKey);
            }

            // TODO: Ideally this list should already come in ordered. Possibly, define category enum for the IPlatformToolkitSupportDeclaration
            var localSavingPlatform = declarations.FirstOrDefault(d => d.Key == "Unity.LocalSaving");
            foreach (BuildTarget bt in Enum.GetValues(typeof(BuildTarget)))
            {
                if (m_BuildTargetToDeclarationMappings.ContainsKey(bt)) continue;
                if (localSavingPlatform != null && localSavingPlatform.SupportedPlatforms.Contains(bt))
                    TryAddBuildTarget(localSavingPlatform.Key, bt);
                else
                {
                    var declaration = m_Declarations.FirstOrDefault(d => d.SupportedPlatforms.Contains(bt));
                    if (declaration != null)
                        TryAddBuildTarget(declaration.Key, bt);
                }
            }
        }

        public bool TryAddBuildTarget(string declarationKey, BuildTarget buildTarget)
        {
            if (!m_Declarations.Any(d => d.Key == declarationKey && d.SupportedPlatforms.Contains(buildTarget)))
                return false;
            if (m_BuildTargetToDeclarationMappings.ContainsKey(buildTarget) &&
                m_BuildTargetToDeclarationMappings[buildTarget] == declarationKey)
                return false;
            if (m_BuildTargetToDeclarationMappings.ContainsKey(buildTarget) &&
                m_BuildTargetToDeclarationMappings[buildTarget] != declarationKey)
                TryRemoveBuildTarget(buildTarget, out var oldKey);

            m_BuildTargetToDeclarationMappings[buildTarget] = declarationKey;
            SupportDeclarationTargetChanged?.Invoke();
            return true;
        }

        public bool TryRemoveBuildTarget(BuildTarget buildTarget, out string declarationKey)
        {
            if (!m_BuildTargetToDeclarationMappings.Remove(buildTarget, out declarationKey))
                return false;

            SupportDeclarationTargetChanged?.Invoke();
            return true;
        }

        public IReadOnlyList<BuildTarget> GetTargetedPlatforms(string declarationKey)
        {
            if (m_BuildTargetToDeclarationMappings.All(pair => pair.Value != declarationKey))
                return new List<BuildTarget>();
            return m_BuildTargetToDeclarationMappings.Where(pair => pair.Value == declarationKey).Select(pair => pair.Key).ToList();
        }

        public bool TryGetDeclarationForBuildTarget(BuildTarget buildTarget, out string declarationKey)
        {
            return m_BuildTargetToDeclarationMappings.TryGetValue(buildTarget, out declarationKey);
        }
    }
}
