using System;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Capabilities system specifies what capabilities are available in the current implementation.
    /// </summary>
    public interface ICapabilities
    {
        /// <summary>
        /// Indicates, if accounts are supported.
        /// </summary>
        bool Accounts => false;

        /// <summary>
        /// Indicates, if the primary account is supported.
        /// </summary>
        bool PrimaryAccount => false;

        /// <summary>
        /// Indicates, if account picker is supported.
        /// </summary>
        bool AccountPicker => false;

        /// <summary>
        /// Indicates, if input ownership is supported.
        /// </summary>
        bool InputOwnership => false;

        /// <summary>
        /// Indicates, if platform has limits on calling <see cref="IPrimaryAccountSystem.Establish"/>.
        /// </summary>
        /// <remarks>
        /// <para>On platforms where the value is true, it's recommended to treat accounts as optional.</para>
        /// <para>On platforms where the value is false, it's safe to repeatedly call <see cref="IPrimaryAccountSystem.Establish"/> until player signs in.</para>
        /// </remarks>
        bool PrimaryAccountEstablishLimited => false;

        /// <summary>
        /// Indicates, if accounts support saving.
        /// </summary>
        bool AccountSaving => false;

        /// <summary>
        /// Indicates, if accounts support achievements.
        /// </summary>
        bool AccountAchievements => false;

        /// <summary>
        /// Indicates if account can be manually signed out by calling <see cref="IAccount.SignOut"/>.
        /// </summary>
        bool AccountManualSignOut => false;

        /// <summary>
        /// Indicates, if <see cref="PlatformToolkit.LocalSaving"/> is supported.
        /// </summary>
        bool LocalSaving => false;
    }
}
