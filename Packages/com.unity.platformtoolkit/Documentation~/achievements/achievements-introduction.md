# Introduction to achievements

Achievements are milestones players earn by completing tasks in an application. They track progress, encourage exploration, and add challenges, enhancing the overall gaming experience. Many platforms mandate the implementation of platform-specific achievements for users before you can pass certification. The Platform Toolkit package provides support for creating achievements across multiple platforms at the same time.

The Platform Toolkit package supports two types of achievements:

* **Single**: The achievement is unlocked only once. For example, collecting your first crystal.
* **Progressive**: The achievement unlocks at a specific progress point. For example, collecting 30 crystals.

> [!NOTE]
> For platforms that don't support progressive achievements, this behavior is handled by the Platform Toolkit API. Refer to the [scripting documentation](xref:Unity.PlatformToolkit.IAchievementSystem) for more information.

For testing, generic achievement data can be configured from the **Achievement Editor.** Access the **Achievement Editor** from  **Window** > **Platform Toolkit** > **Achievement Editor**. When configuring achievement data, a common ID unlocks the achievement using the Platform Toolkit API.

> [!NOTE]
> The achievement system is obtained from a user account like other account-related systems, such as storage.

## Additional resources

* [Configure achievements with the Achievement Editor](configure-achievements.md)

