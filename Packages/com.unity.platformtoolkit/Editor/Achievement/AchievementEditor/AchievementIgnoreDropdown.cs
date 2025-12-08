using System;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    [UxmlElement]
    internal partial class AchievementIgnoreDropdown : Button
    {
        GenericDropdownMenu m_DropdownMenu;
        public IAchievement Achievement { get; set; }
        public AchievementIgnoreDropdown()
        {
            m_DropdownMenu = new GenericDropdownMenu();
            m_DropdownMenu.AddItem("Toggle Ignore", false, OnIgnoreToggle, null);

            clickable.clicked += () => { m_DropdownMenu.DropDown(worldBound, this, DropdownMenuSizeMode.Auto); };
        }

        private void OnIgnoreToggle(object e)
        {
            Achievement.ImplementationData.Ignore = !Achievement.ImplementationData.Ignore;
        }
    }
}
