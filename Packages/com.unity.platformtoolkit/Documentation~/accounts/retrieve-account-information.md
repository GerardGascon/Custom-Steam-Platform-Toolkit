# Retrieve account information

Understand how to access and retrieve account information.

## Access an account

The following example is the minimum amount of code required to access the primary user account:

```csharp
try
{
    if (PlatformToolkit.Capabilities.PrimaryAccount)
    {
        IAccount account = await PlatformToolkit.Accounts.Primary.Establish();
    }
}
catch (UserRefusalException e)
{
    // User refused to sign in
}
catch (TemporarilyUnavailableException e)
{
    // Either a network error or sign in limit was exceeded
}
```

## Retreive account data

Use the IAccount API to retrieve specific account information such as a players user name, profile picture, or specific attributes. For more information, refer to [IAccount API scripting reference](xref:Unity.PlatformToolkit.IAccount).

For example, use the following code to retrieve the player name associated with an account:

```csharp
var name = await account.GetName();
```

User accounts also allow access to the save system associated with that account:

```csharp
var savingSystem = await account.GetSavingSystem();
```

For more information about the save system, refer to [Save systems](../savedata/save-systems.md).

## Account states

On some platforms, accounts can be signed out at any time for several reasons. Exceptions can occur when accessing account data if a user is signed out of the system.

You can react to a change in account state by using the following callback:

```csharp
PlatformToolkit.Accounts.OnChange += Accounts_OnChange;
```

You might also want to take action when the primary account signs out. For example, returning to the title screen when `AccountState.SignedOut` is returned.

```csharp
private void Accounts_OnChange(IAccount account, AccountState newState)
{
    if (account == myPrimaryAccount && newState == AccountState.SignedOut)
        // back to the title screen...
}
```

## Additional resources

* [IAccount API scripting reference](xref:Unity.PlatformToolkit.IAccount)
* [Introducing the Platform Toolkit account system](accounts-introducing)