# Unlock achievements

Use the following workflow to initialize and unlock player achievements:

1) Initialize the achievement system using the following code.

```csharp
if (PlatformToolkit.Capabilities.AccountAchievements)
{
   try
   {
       IAchievementSystem achievementSystem;
       achievementSystem = await account.GetAchievementSystem();
   }
   catch (InvalidAccountException e)
   {
       // Handle signed out account
   }
}

```

2) Unlock an achievement when the player reaches a significant milestone.

The following example demonstrates how to unlock two different types of achievements:

* A **Single** unlock type named `EGGCITEMENT` for when a player collects their first egg.
* A **Progressive** unlock type named `THIRTY_EGGS`, for when a player collects 30 eggs.

```csharp
if (PlatformToolkit.Capabilities.AccountAchievements)
{
   try
   {
       IAchievementSystem achievementSystem;
       achievementSystem = await account.GetAchievementSystem();

       achievementSystem.Unlock("EGGCITEMENT");
       achievementSystem.UpdateProgress("THIRTY_EGGS", 30);
   }
   catch (InvalidAccountException e)
   {
       // Handle signed out account
   }
}

```

> [!NOTE]
> You can adjust and unlock player achievement data using [Play Mode Controls](../playmodecontrols/play-mode-controls.md) when not in Play mode.

## Handle failed unlock requests

Network interruptions and system errors can sometimes prevent a successful unlock request on the first attempt. To handle these cases, it's recommended to:

* Unlock achievements immediately when the player earns them.
* Implement a retry system for requests that might have failed. Configure this system to send unlock requests for earned achievements at logical points in your application, such as when returning to a main menu. This ensures your application successfully unlocks any achievements that previously failed.

> [!NOTE]
> Some platforms might impose limits on how frequently you can send unlock requests. Ensure that retry requests send at reasonable intervals to avoid reaching any limits.

## Additional resources

* [Achievement Editor](achievements-editor-reference.md)
* [Play Mode Controls](../playmodecontrols/play-mode-controls.md)

