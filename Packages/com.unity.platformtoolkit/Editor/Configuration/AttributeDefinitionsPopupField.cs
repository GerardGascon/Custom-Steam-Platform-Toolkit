using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    [UxmlElement]
    internal partial class AttributeDefinitionsPopupField : PopupField<IAttributeDefinition>
    {
        public AttributeDefinitionsPopupField()
        {
            formatSelectedValueCallback = FormatItem;
            formatListItemCallback = FormatItem;
        }

        static string FormatItem(IAttributeDefinition attributeDefinition)
        {
            if (attributeDefinition == null)
                return "None";
            return $"{attributeDefinition.Name} ({attributeDefinition.Type})";
        }
    }

    // This class is not required, it only exists because it prevents an error when importing the package.
    internal class AttributeDefinitionConverter : UxmlAttributeConverter<IAttributeDefinition>
    {
        public override IAttributeDefinition FromString(string value)
        {
            return null;
        }
    }
}
