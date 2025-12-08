# Manage user identity and input

The Platform Toolkit package provides APIs to manage user identity and input ownership on supported platforms. These APIs help you access system-level user accounts and associate input devices with specific users.

## Account picker

The Account Picker API provides access to the platform’s native UI for signing in or selecting a user account. After a user selects an account, the API returns an IAccount object which you can use to access account-specific systems, such as save data or achievements.

> [!NOTE]
> The account picker API isn't supported on all platforms. Use [ICapabilities.AccountPicker](xref:Unity.PlatformToolkit.ICapabilities.AccountPicker) to check support for the target platform.

For example, If developing a local multiplayer game, you might use the Account Picker API to prompt each player to sign in with their unique platform account. You can then ensure that all save data and progress are correctly associated with each individual player.

For more information, refer to the [IAccountPickerSystem scripting reference](xref:Unity.PlatformToolkit.IAccountPickerSystem).

## Attributes

Attributes simplify cross-platform development by letting you assign a single, consistent name to a piece of account data, which you then map to the unique API for each platform. This allows you to write platform-agnostic code to retrieve account information.

For more information, refer to [Get started with attributes.](get-started-with-attributes.md)

## Multiple users

On platforms that support simultaneous sign-ins, such as consoles, the Platform Toolkit provides optional access to each user's account. This is useful for managing individual save files and achievements, for example, in a local multiplayer game.

## Input ownership

The input ownership system identifies which user account is associated with a specific input device, such as a game controller. The system also tracks ownership changes, so your application can respond if a controller is disconnected or assigned to a different user during gameplay.

> [!NOTE]
> Input ownership isn't supported on all platforms. Use [ICapabilities.InputOwnership](xref:Unity.PlatformToolkit.ICapabilities.InputOwnership) to check support for the current platform.

The input ownership API triggers an `OnChange` event when:

* A new device is connected and assigned to a user.
* An assigned input device is reassigned to a different user or becomes unassigned.
* An assigned input device is disconnected.
* A previously unassigned device becomes assigned to a user.
* An association is manually set within the Editor’s Play Mode Controls.

For more information, refer to the [IInputOwnershipSystem scripting reference](xref:Unity.PlatformToolkit.IInputOwnershipSystem).

> [!NOTE]
> Input ownership requires the Unity Input System package. For more information, refer to the [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest) package documentation.

## Additional resources

* [Handle platform account systems](handle-platform-account-systems.md)
* [Retrieve account information](retrieve-account-information.md)
