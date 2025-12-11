# Capabilities

A description of the features supported indicated under `PlatformToolkit.Capabilities`

## Supported Capabilities

### Accounts

This integration only supports one account signed in at a time. Meaning that querying for accounts will return only one account.

### PrimaryAccount

Primary account is fully supported, this is the account who is signed in on Steam when the Platform Toolkit is initialized.

### Account Saving

All save data is stored under `Application.persistentDataPath/{steam user id}`. There are settings on Steamworks Config to use auto cloud sync with files/folders depending on the steam user id in their name (see [Section 2 of Auto-Cloud Setup](https://partner.steamgames.com/doc/features/cloud)).

### Account Achievements

You can unlock both Single and Progressive achievements.

#### Progressive achievements

For progressive achievements, you can only use integer values (it's a Platform Toolkit limitation)

Steam allows different names for the achievement and stat IDs. Therefore, if possible, try to stick with a single ID for both elements. But, if that's not a possibility, I've added a way to pass both values to Platform Toolkit.

Use the following format: `ACH_ID|STAT_ID`.

```cs
IAchievementSystem achievementSystem = await PlatformToolkit.Accounts.Primary.Current.GetAchievementSystem();
achievementSystem.UpdateProgress("ACH_TRAVEL_FAR_ACCUM|FeetTraveled", 100);
```

## Unsupported Capabilities

### Account Picker

Steam doesn't support the ability to select which account is playing the game from within the game.

### Input Ownership

Steam doesn't support the ability to assign a different user for each controller connected.

### Primary Account Establish Limited

Steam doesn't have a limit on how many times you can query the user data from within the game.

### Account Manual Sign Out

Steam doesn't have the ability to sign out from the account from within the game, the account is logged in the entire playtime.

### Local Saving

Even though PCs support local save systems, I won't add support for now as you can simply use an account-based save system, and it will just work.

Quoting Unity's documentation:

> On platforms that support both an account saving system and a local saving system, it's recommended to use the account saving system and only fall back on the local saving system if an account saving system is unavailable.

I won't be adding support for that as Steam can store data for different accounts.
