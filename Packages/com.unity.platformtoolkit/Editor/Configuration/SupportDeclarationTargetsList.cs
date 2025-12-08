using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class SupportDeclarationTargetsList : VisualElement
    {
        public SupportDeclarationTargetsList()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/EditorResources/UI/SupportDeclarationTargetsList.uxml");
            uxml.CloneTree(this);

            var declarationsList = this.Q<ScrollView>("declaration-targets-list");
            var targetsManager = PlatformToolkitSettings.instance.SupportDeclarationTargetsManager;

            foreach (var supportDeclaration in SupportDeclarationManager.SupportDeclarations.OrderBy(p => p.ToString()))
            {

                var spt = new SupportDeclarationTarget(supportDeclaration, targetsManager);
                var field = new SupportDeclarationTargetField(spt);
                declarationsList.Add(field);
            }
        }
    }
}
