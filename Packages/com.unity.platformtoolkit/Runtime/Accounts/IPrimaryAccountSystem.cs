using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>Provides access to all primary account related functionality.</summary>
    /// <remarks>
    /// <para>Primary account is the account that players will expect most actions and data to be associated with, e.g. save games and achievements. The selection of the primary account is dictated by the platform. Usually this is the account which launched the game. On platforms that support only one signed in account at a time, that account is the primary.</para>
    /// <para>On some platforms the primary account might not be available even if accounts are supported. Use <see cref="ICapabilities.PrimaryAccount"/> capability to check if the primary account is supported.</para>
    /// </remarks>
    /// <seealso cref="IAccountSystem.Primary"/>
    /// <seealso cref="ICapabilities.PrimaryAccount"/>
    public interface IPrimaryAccountSystem
    {
        /// <summary>Current primary account.</summary>
        IAccount Current { get; }

        /// <summary>Invoked after primary account changes.</summary>
        /// <remarks>
        /// <para>Check <see cref="Current"/> to get the current primary account.</para>
        /// <para>Exactly if and when the primary account can change is platform dependent. On some platforms the same primary account is guaranteed to remain signed in throughout the entire game session, while on other platforms the primary account can sign in and sign out at any moment. Refer to platform support package documentation to find out how to handle changes to the primary account.</para>
        /// <para>Will not get invoked if the primary account is set during <see cref="PlatformToolkit.Initialize"/>.</para>
        /// </remarks>
        event Action OnChange;

        /// <summary>Sign in the primary account if one is not currently signed in, then return the current primary account.</summary>
        /// <remarks>
        /// <para><see cref="Establish"/> might show a system sign in prompt. There is no way to tell how long the prompt will be shown, so make sure to only call <see cref="Establish"/> when appropriate.</para>
        /// <para>Some platforms impose a limit to how many times a user can be presented with a sign in prompt. If the primary account is not signed in and the platform does not allow prompting the user to sign in, <see cref="Establish"/> will throw a <see cref="TemporarilyUnavailableException"/>. Check <see cref="IPrimaryAccountSystemCapabilities.EstablishLimited"/> to find out if <see cref="Establish"/> can throw a <see cref="TemporarilyUnavailableException"/>.</para>
        /// <para>On platforms where <see cref="IPrimaryAccountSystemCapabilities.EstablishLimited"/> capability is true, it’s recommended to treat accounts as optional and design your game to work both with an account and without one.</para>
        /// </remarks>
        /// <returns>
        /// <para>Task that contains the current primary account. The same account can be read from <see cref="Current"/>.</para>
        /// <para>If the primary account is already signed in when calling <see cref="Establish"/>, result will contain the same account.</para>
        /// </returns>
        /// <exception cref="UserRefusalException">The user has canceled the sign in prompt.</exception>
        /// <exception cref="TemporarilyUnavailableException">Sign in prompt could not be shown.</exception>
        Task<IAccount> Establish();
    }
}
