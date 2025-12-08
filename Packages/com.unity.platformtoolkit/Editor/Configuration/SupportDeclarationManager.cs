using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.PlatformToolkit.Editor
{
    static internal class SupportDeclarationManager
    {
        private static readonly IPlatformToolkitSupportDeclaration[] s_SupportDeclarations;
        internal static IReadOnlyList<IPlatformToolkitSupportDeclaration> SupportDeclarations => s_SupportDeclarations;

        private static readonly Dictionary<Type, IPlatformToolkitSupportDeclaration> s_DeclarationTypeToDeclaration = new();
        private static readonly Dictionary<string, IPlatformToolkitSupportDeclaration> s_DeclarationKeyToDeclaration = new();
        private static readonly Dictionary<Type, string> s_SettingsTypeToDeclarationKey = new();

        static SupportDeclarationManager()
        {
            var supportDeclarationTypes = TypeCache.GetTypesDerivedFrom<IPlatformToolkitSupportDeclaration>();
            var validDeclarationTypes =
                from type in supportDeclarationTypes
                where type.GetConstructor(Type.EmptyTypes) != null && !type.IsAbstract
                select type;

            foreach (var type in validDeclarationTypes)
            {
                var supportDeclaration = (IPlatformToolkitSupportDeclaration)Activator.CreateInstance(type);
                s_DeclarationKeyToDeclaration.TryAdd(supportDeclaration.Key, supportDeclaration);
            }
            s_SupportDeclarations = s_DeclarationKeyToDeclaration.Values.ToArray();

            foreach (var supportDeclaration in SupportDeclarations)
            {
                s_DeclarationTypeToDeclaration.Add(supportDeclaration.GetType(), supportDeclaration);
                if (supportDeclaration.SettingsProvider != null)
                {
                    s_SettingsTypeToDeclarationKey.TryAdd(supportDeclaration.SettingsProvider.SettingsType, supportDeclaration.Key);
                }
            }
        }

        internal static bool TryGetSupportDeclaration(Type type, out IPlatformToolkitSupportDeclaration supportDeclaration)
        {
            return s_DeclarationTypeToDeclaration.TryGetValue(type, out supportDeclaration);
        }

        internal static bool TryGetSupportDeclaration(string key, out IPlatformToolkitSupportDeclaration supportDeclaration)
        {
            return s_DeclarationKeyToDeclaration.TryGetValue(key, out supportDeclaration);
        }

        internal static bool TryGetDeclarationKey(Type settingsType, out string declarationKey)
        {
            return s_SettingsTypeToDeclarationKey.TryGetValue(settingsType, out declarationKey);
        }
    }
}
