# Access a save system

The Platform Toolkit package can help manage save files when accessing an available save system. Save systems are available from user accounts on most common platforms.

## Access a platform-specific save system

Use the following example to access a platform save system that's connected to a system-level user account:

```csharp
ISavingSystem savingSystem;

if (PlatformToolkit.Capabilities.Accounts)
{
   try
   {
       savingSystem = await account.GetSavingSystem();
   }
   catch (InvalidAccountException e)
   {
       // Handle signed out account
   }
}
else if (PlatformToolkit.Capabilities.LocalSaving)
{
   savingSystem = PlatformToolkit.LocalSaving;
}
```

For more information, refer to the [GetSavingSystem](xref:Unity.PlatformToolkit.IAccount.GetSavingSystem) API reference.

## Access a local save system

If developing for a platform without a system-level user account, you can use the local saving system instead.

```csharp
var savingSystem = PlatformToolkit.LocalSaving;
```

> [!NOTE]
> Not all platforms support a local saving system. Check the capabilities of your target platform service to ensure you use the most appropriate system for your application.

Use the local saving system for platforms that don't support accounts, or for platforms where a user can decline to sign in but still continue to play the game.

When developing for mobile platforms, you can use the local saving system when a user declines to sign into an account and then migrate the data later to an account saving system. Both local and account-based saving systems are accessible at the same time on mobile platforms.

For more information, refer to the [LocalSaving](xref:Unity.PlatformToolkit.PlatformToolkit.LocalSaving) scripting API reference.

## Additional resources

* [Manage save files](manage-save-files.md)
* [GetSavingSystem scripting API reference](xref:Unity.PlatformToolkit.IAccount.GetSavingSystem)
* [LocalSaving scripting API reference](xref:Unity.PlatformToolkit.PlatformToolkit.LocalSaving)