# Simulate platform system behaviors

Use **Play Mode Controls** to simulate different platform behaviors directly in the Unity Editor.

You can use the Behaviour dropdown in the Play Mode Controls window to select different platform behaviors. Each behavior simulates a specific set of platform capabilities, such as local saving or multiple user accounts. For example, you can use behaviors to test how your game might run on a mobile platform with a single user whilst using local storage.

The behaviors aren't full platform simulations, but they provide the necessary tools for in-Editor testing.

## Select a behavior

Use the following steps to select a behavior in the Play Mode Controls window:

1. Open the Play Mode Controls window.
2. In the **Behavior** dropdown, select the desired behavior.

The following behaviors are available in the [Play Mode Controls](play-mode-controls-settings-reference.md) window:

| **Behavior** | **Description** |
| :---- | :---- |
| **Generic Local Saving (Desktop-like)** | Simulates a platform that supports local saving without user accounts. Use this behavior to test save and load functionality in your game. |
| **Generic Multiple User (Console-like)** | Simulates a platform that supports multiple user accounts. Use this behavior to test how your game handles different user profiles and associated data. |
| **Generic Single User (Mobile-like)** | Simulates a platform that supports a single user account with local saving. Use this behavior to test how your game handles user data and preferences in a mobile environment. |

> [!Note]
> Additional behaviors will appear as you install additional Platform Toolkit platform service packages. These additional behaviors correspond to the capabilities of each installed platform service package. For more information on installing platform service packages, refer to [Install platform modules and packages](../install-platform-modules.md).

## Additional resources

* [Play Mode Controls window reference](play-mode-controls-settings-reference.md)
* [Play Mode Controls](play-mode-controls.md)