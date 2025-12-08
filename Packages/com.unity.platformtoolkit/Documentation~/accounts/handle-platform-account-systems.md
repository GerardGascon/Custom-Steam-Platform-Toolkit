# Handle platform account systems

Use the following examples to handle different account systems and scenarios:

* [Initialize a primary account and access the save system](#initialize-a-primary-account-and-access-the-save-system)
* [Manage optional account systems](#manage-optional-account-systems)
* [Platforms with no account support](#platforms-with-no-account-support)
* [Use a combined approach to account handling](#use-a-combined-approach-to-account-handling)
* [Further considerations](#further-considerations)

## Initialize a primary account and access the save system

The following example initializes the primary account and enables access to its saving system on platforms that require mandatory accounts. This is typical for console platforms where the player must sign in to an account in order to save progress.

```csharp
await PlatformToolkit.Initialize();

// The following example assumes that the target platform has primary account functionality. While some platforms can be
// configured to function without a primary account, these modes are meant for advanced
// use cases, which are supported but not recommended for the vast majority of games.


// EstablishLimited being false indicates that PlatformToolkit.Accounts.Primary.Establish()
// can be called as many times as needed. This is typical of consoles and platforms where
// an account is always signed in.
if (PlatformToolkit.Capabilities.PrimaryAccountEstablishLimited is false)
{
    try
    {
        // Attempt to get or sign in the primary account.
        var primaryAccount = await PlatformToolkit.Accounts.Primary.Establish();
        // Get the primary account saving system.
        var savingSystem = await primaryAccount.GetSavingSystem();
    }
    catch (UserRefusalException)
    {
        // UserRefusalException indicates that the player was presented with a sign-in
        // dialogue, which they then dismissed without signing in. Because EstablishLimited
        // capability is false, it's safe to call Establish() again until a player signs in.
        // This is typically done in the game's title screen, where the player is asked to
        // Press A or something similar.
    }
    catch (InvalidAccountException)
    {
        // InvalidAccountException indicates that account was signed out before
        // GetSavingSystem() method completed. You should always expect that any operation
        // on an IAccount object or any of its systems can fail in this manner, since on
        // many platforms players can sign out at any time. When this happens it's
        // recommended to return back to the title screen.
    }
}
```

## Manage optional account systems

The previous example works for many platforms, but doesn't consider platforms where user accounts are optional. Console players expect to be signed into an account when they launch a game. However, mobile players expect to be able to dismiss an account sign-in request, continue to play the game, and have the option to sign in to an account later if they wish.

The following example modifies the previous example to use the local saving system when an account isn't available:

```csharp
await PlatformToolkit.Initialize();

// EstablishLimited being true indicates that PlatformToolkit.Accounts.Primary.Establish()
// can be disallowed by the operating system, meaning that there can be times when account
// sign-in is impossible. This is typical of mobile platforms, where use of an account is optional.
if (PlatformToolkit.Capabilities.PrimaryAccountEstablishLimited is true)
{
    try
    {
        // Attempt to get or sign in the primary account.
        var primaryAccount = await PlatformToolkit.Accounts.Primary.Establish();
        // Get the primary account saving system.
        var savingSystem = await primaryAccount.GetSavingSystem();
    }
    catch (Exception e) when (e is UserRefusalException or TemporarilyUnavailableException)
    {
        // When EstablishLimited capability is true and Establish() throws either
        // UserRefusalException or TemporarilyUnavailableException, it's recommended to
        // proceed without an account and use the local saving system instead. In order to
        // allow players to sign in later a Sign In button should be added somewhere in
        // the game. Pressing that button would then call Establish() again.
        if (PlatformToolkit.Capabilities.LocalSaving)
        {
            var savingSystem = PlatformToolkit.LocalSaving;
        }
        else
        {
            // Not currently possible as all platforms that have EstablishLimited
            // capability support the local saving system.
        }
    }
    catch (InvalidAccountException)
    {
        // InvalidAccountException indicates that account was signed out before
        // GetSavingSystem() method completed.
    }
}
```

## Platforms with no account support

The following example describes how to use the local save system when the target platform doesn't support an account system. For example, standalone platforms such as Windows.

```csharp
await PlatformToolkit.Initialize();
if (PlatformToolkit.Capabilities.Accounts is false && PlatformToolkit.Capabilities.LocalSaving)
{
    // Some platforms don't have accounts, such as standalone platforms. In that case
    // saving is performed via the local saving system. Account-based functionality is not
    // available.
    var savingSystem = PlatformToolkit.LocalSaving;
}
```

## Use a combined approach to account handling

It's recommended to use a combined approach to support the different ways platforms can handle user accounts. Consider whether the platform:

* Has a primary account system.
* Supports multiple user accounts.
* Has no account system.

The following example explains how you can support these considerations in your game:

```csharp
await PlatformToolkit.Initialize();

if (PlatformToolkit.Capabilities.Accounts is false && PlatformToolkit.Capabilities.LocalSaving)
{
    var savingSystem = PlatformToolkit.LocalSaving;
}
else if (PlatformToolkit.Capabilities.PrimaryAccount)
{
    try
    {
        var primaryAccount = await PlatformToolkit.Accounts.Primary.Establish();
        var savingSystem = await primaryAccount.GetSavingSystem();
    }
    catch (Exception e) when (e is UserRefusalException or TemporarilyUnavailableException)
    {
        if (PlatformToolkit.Capabilities.PrimaryAccountEstablishLimited)
        {
            var savingSystem = PlatformToolkit.LocalSaving;
        }
        else
        {
            // Wait for user interaction and call Establish() again.
        }
    }
    catch (InvalidAccountException)
    {
        // Go to the title screen.
    }
}
else
{
    // Scenarios that aren't demonstrated in this sample:
    // Platforms with account support, but no support for a primary account.
    // The Platform Toolkit supports this scenario but you will need to implement additional handling.
}
```

## Further considerations

The previous code examples cover the majority of use cases with user accounts. However, there are other use cases that you might need to consider:

**Note**: The following considerations are dependent on the platform you're targeting and their configuration.

* Platforms with support for accounts but have no primary account system.
* Platforms with no support for additional accounts, but have a primary account system.
* Platforms with no account support, and therefore no account-based save systems.
* Platforms where accounts can sign-out.
* Platforms where account usage is optional
* Platforms with no local save system.
* Platforms with no achievement system.

## Additional resources

* [IAccount Scripting API reference](xref:Unity.PlatformToolkit.IAccount)
* [Play Mode Controls](../playmodecontrols/play-mode-controls.md)