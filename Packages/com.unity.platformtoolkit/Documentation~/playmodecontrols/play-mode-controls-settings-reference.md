# Play Mode Controls window reference

Reference documentation for the Play Mode Controls window.

## Play Mode Controls Settings asset

Select the Play Mode Controls Settings asset to use with this project.

## Behavior

Select the desired platform behavior type for the Play Mode Controls window. For more information, refer to [Simulate platform system behaviors](simulate-platform-system-behaviors.md).

## Play Mode Controls

### System Controls

#### Network

| Property | Description |
| :---- | :---- |
| **Simulate Offline** | Simulate a scenario where the application loses network connections. |

#### Storage

| Property | Description |
| :---- | :---- |
| **Simulate Full Storage** | Simulate a scenario where the device storage capacity is reached. |

#### API Tasks

| Property | Description |
| :---- | :---- |
| **Simulate Long Running Tasks** | Simulate a series of long running tasks in an allotted time. Select from `1`, `3`, or `5` seconds. |

### Account Controls

Use Account Controls to set a test account as the Primary account and optionally sign an account out if required. This is useful when testing how your game responds when a user signs out unexpectedly. For more information, refer to [Respond to sign-in events](respond-to-sign-event.md).

### Input Device Mapping

Displays a list of connected input devices, allowing you to assign each device to an active test account.

## Test Account data

### Accounts

A list of configurable test accounts that you can use to simulate different user scenarios in Play Mode.

| Property | Description |
| :---- | :---- |
| **Name** | The name of the test account. |
| **Picture** | An optional picture to associate with the test account. |
| **Achievements** | A list of configured achievements that can be unlocked or locked as required.|
| **Saves** | Save files specific to each test account. This option is available when the selected **Behavior** supports account-based saves. For more information, refer to [Saves](#saves).

<a name="saves"></a>

### Saves

Use the **Import** option to import local save data. Save data must be in a `.zip` archive format.

> [!NOTE]
> This option is supported only when the selected **Behavior** supports local saving. For example, **Generic Local Saving**.

Once imported, use the following options to manage save data for each test account:

* **Export**: Export the save data to a local archive file.
* **Delete**: Remove the imported save data.

> [!NOTE]
>  When you import a save archive on macOS, ensure the archive doesn't contain meta files, such as those in the `__MACOSX` folder, as these will cause the import to fail. To create a valid archive for import, run the `zip -r <filename>.zip .` command in the terminal, replacing `<filename>` with your desired archive name.

## Additional resources

* [Handle platform account systems](../accounts/handle-platform-account-systems.md)
