using Unity.PlatformToolkit;
using UnityEngine;

public class AchievementUnlockTest : MonoBehaviour {
	private async void Start() {
		await PlatformToolkit.Initialize();
	}

	private async void OnGUI() {
		if (GUI.Button(new Rect(10, 10, 350, 100), "Unlock Achievement")) {
			IAchievementSystem system = await PlatformToolkit.Accounts.Primary.Current.GetAchievementSystem();
			system.Unlock("ACH_WIN_ONE_GAME");
		}
	}
}