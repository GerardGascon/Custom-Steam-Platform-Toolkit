using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Provides achievement editing capabilities in the Achievement Editor Window.</summary>
    internal interface IAchievementEditorProvider
    {
        /// <summary>Make a header visual element for a column in the Achievement Editor window.</summary>
        /// <returns>Header visual element.</returns>
        public VisualElement MakeHeader();

        /// <summary>Make a cell for the Achievement Editor window.</summary>
        /// <returns>Cell visual element.</returns>
        /// <seealso cref="ListView.makeItem"/>
        /// <seealso cref="Column.makeCell"/>
        public VisualElement MakeCell();

        /// <summary>Bind a cell in the Achievement editor window.</summary>
        /// <param name="cell">VisualElement previously created in the <see cref="MakeCell"/> call.</param>
        /// <param name="achievement">Achievement bound to the cell.</param>
        /// <seealso cref="ListView.bindItem"/>
        /// <seealso cref="Column.bindCell"/>
        public void BindCell(VisualElement cell, IAchievement achievement);

        /// <summary>Unbind a cell in the Achievement editor window.</summary>
        /// <param name="cell">VisualElement previously created in the <see cref="MakeCell"/> call.</param>
        /// <seealso cref="ListView.unbindItem"/>
        /// <seealso cref="Column.unbindCell"/>
        public void UnbindCell(VisualElement cell);
    }
}
