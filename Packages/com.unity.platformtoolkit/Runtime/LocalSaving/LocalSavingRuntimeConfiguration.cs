using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.PlatformToolkit.LocalSaving.Editor")]

namespace Unity.PlatformToolkit.LocalSaving
{
    internal class LocalSavingRuntimeConfiguration : BaseRuntimeConfiguration
    {
        public override IPlatformToolkit InstantiatePlatformToolkit()
        {
            return new LocalSavingPlatformToolkit();
        }
    }
}
