using System;
using System.Threading.Tasks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	public class SteamPrimaryAccountSystem : IPrimaryAccountSystem {
		public IAccount Current { get; }
		public event Action OnChange;

		public SteamPrimaryAccountSystem(IAccount account) {
			Current = account;
		}

		public Task<IAccount> Establish() {
			return Task.FromResult(Current);
		}
	}
}