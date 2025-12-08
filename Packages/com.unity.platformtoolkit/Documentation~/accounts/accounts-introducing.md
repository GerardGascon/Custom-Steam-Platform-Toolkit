# Introducing the Platform Toolkit account system

Many platforms support system-level accounts and gate features such as storage behind account ownership. You can access accounts and their associated features, for example save data, with the Platform Toolkit account API. For multi-user scenarios, such as a local multiplayer game, the Account API allows you to access the unique account information for each signed-in player.

## Platform account systems

Platform account systems typically fall into one of three categories:

* No account system is present.
* An account is required to start a game.
* A game can start without an account, but when interacting with other system level features, an account is required to continue.

The Platform Toolkit package allows you to create games that handle each of these categories. For more information on handling each of these types, refer to [Handle platform account systems](handle-platform-account-systems.md).

It's also common for platform account systems to use the concept of a primary account. A primary account is the main user profile that's signed in at the system level of a device or platform when an app is launched.

## Additional resources

* [Retrieve account information](retrieve-account-information.md)
* [Handle platform account systems](handle-platform-account-systems.md)
* [IAccount API scripting reference](xref:Unity.PlatformToolkit.IAccount)
* [IAccountPickerSystem scripting reference](xref:Unity.PlatformToolkit.IAccountPickerSystem)