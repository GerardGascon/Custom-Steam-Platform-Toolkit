using System.Threading.Tasks;

namespace Unity.PlatformToolkit.LocalSaving
{
    internal class LocalSavingPlatformToolkit : IPlatformToolkit
    {
        public LocalSavingPlatformToolkit()
        {
            var capabilityBuilder = new CapabilityBuilder
            {
                AccountSupport = false,
                PrimaryAccount = false,
                PrimaryAccountEstablishLimited = false,
                AccountName = false,
                AccountPicture = false,
                AccountAchievementSystem = false,
                AccountSavingSystem = false,
                AccountManualSignOut = false,
                AccountInputPairingSystem = false,
                AdditionalAccountSystem = false,
                LocalSavingSystem = true
            };

            Capabilities = capabilityBuilder.ToCapabilities();
        }

        public Task Initialize()
        {
            LocalSavingSystem = new GenericSavingSystem(new GenericLocalStorageSystem());
            return Task.CompletedTask;
        }

        public ICapabilities Capabilities { get; }
        public ISavingSystem LocalSavingSystem { get; private set; }
    }
}
