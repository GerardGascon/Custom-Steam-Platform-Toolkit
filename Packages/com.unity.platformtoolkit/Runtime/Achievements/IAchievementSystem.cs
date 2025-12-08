using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>Provides access to achievement functionality.</summary>
    /// <remarks>
    /// <para>Achievements are managed in the Achievement Editor window (Window -> Platform Toolkit -> Achievement Editor). In the editor, achievements must be created and a unique string id must be set for each achievement. This name is then used to update achievements with <see cref="Unlock"/> and <see cref="UpdateProgress"/> methods.</para>
    /// <para>In addition, the Achievement Editor allows setting the unlock type for each achievement to either single or progressive. </para>
    /// <para>If the platform natively supports both progressive and non-progressive achievements, the Platform Toolkit achievement type must match the platform type, otherwise the achievement might not unlock when expected.</para>
    /// <para>Progressive achievements must have a progress target set. If the platform supports custom progress targets natively, the native progress target and the progress target set in the achievement editor must match, or the achievement might not unlock when expected.</para>
    /// <para>The exact behavior when achievement type or progress target is mismatched or is undefined and can differ between platforms.</para>
    /// <para>On platforms where progressive achievements have a fixed target, for example when progress is tracked in percentages, the Platform Toolkit progress target can be set to any value and the progress will be remapped proportionally.</para>
    /// <para>Achievement Editor does not register achievements with platform services: that has to be done manually for each platform service. After achievements are registered in platform services, their native IDs have to be entered in the Achievement Editor. See platform package documentation to find out which achievement ID should be used for each platform and where to find them.</para>
    /// <para>Achievement updates that are initiated by calling <see cref="Unlock"/> and <see cref="UpdateProgress"/> are unreliable. Achievements might not get updated due to network issues, or because network event limits were exceeded. The achievement system retries failed achievement updates internally, but it can only do so while the account is signed in and while the game is still running. If achievement is still not updated by the time the game is turned off or the account signs out, the updates can be lost. While some platforms maintain their own local cache of unlocked achievements, some platforms do not or don’t do it reliably.</para>
    /// <para><see cref="IAchievementSystem"/> will internally fetch platform achievement state and use that information to ignore redundant <see cref="Unlock"/> and <see cref="UpdateProgress"/> calls. So for example if on the platform side the achievement progress is set to 50, when attempting to call <see cref="UpdateProgress"/> with progress less than or equal to 50, the call will be completely ignored by the achievement system. This is a measure to reduce the chance of hitting network event limits that can be imposed by platforms.</para>
    /// </remarks>
    public interface IAchievementSystem
    {
        /// <summary>Unlock achievement.</summary>
        /// <remarks>When called on a progressive unlock achievement, will set the progress to the target progress value.</remarks>
        /// <param name="id">Achievement id defined in the Achievement Editor.</param>
        /// <exception cref="System.ArgumentException">Parameter <see cref="id"/> is null, empty or not defined in the Achievement Editor.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        void Unlock(string id);

        /// <summary>Update achievement progress and unlock it when the progress is updated to its target.</summary>
        /// <remarks>
        /// <para>Progress cannot be lowered. Attempts to set progress to a lower value than the current one will be ignored.</para>
        /// </remarks>
        /// <param name="id">Achievement id defined in the Achievement Editor.</param>
        /// <param name="progress">New achievement progress in the range between 0 and the progress target defined in the Achievement Editor window.</param>
        /// <exception cref="System.ArgumentException">
        /// <para>Parameter <see cref="id"/> is null, empty or not defined in the Achievement Editor.</para>
        /// <para>Parameter <see cref="progress"/> value is less than 0. </para>
        /// </exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        void UpdateProgress(string id, int progress);
    }
}
