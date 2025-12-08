using System.Collections.Generic;
using Unity.Properties;

namespace Unity.PlatformToolkit.Editor
{
    internal class HeaderViewModel
    {
        [CreateProperty] public string HeaderText;

        [CreateProperty]
        public IReadOnlyList<string> Warnings { get; set; }
    }
}
