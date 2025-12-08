using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>Abstraction of platform-specific account.</summary>
    /// <remarks>
    /// <para>Use <see cref="ICapabilities.Accounts"/> to check if accounts are supported.</para>
    /// <para>After an account becomes signed out, operations on it will throw an <see cref="InvalidAccountException"/>.</para>
    /// <para>IAccount operations can throw an <see cref="InvalidAccountException"/> before <see cref="State"/> is set to <see cref="AccountState.SignedOut"/> and before <see cref="IAccountSystem.OnChange"/> is invoked.</para>
    /// <para>Through <see cref="GetAttribute{T}"/> and <see cref="HasAttribute{T}"/> methods accounts expose various attributes. Attributes can be things like a username, a user picture, IDs, or handles.</para>
    /// </remarks>
    public interface IAccount
    {
        /// <summary>Current account state, which can be either <see cref="Unity.PlatformToolkit.AccountState.SignedIn"/> if the account is valid for use or <see cref="Unity.PlatformToolkit.AccountState.SignedOut"/> if the account has become invalid for use.</summary>
        /// <remarks>
        /// <para>Once an account instance becomes SignedOut, it will remain so. If the same platform account signs in again, a new instance of IAccount is created.</para>
        /// <para><see cref="IAccountSystem.OnChange"/> is invoked after <see cref="State"/> changes.</para>
        /// </remarks>
        AccountState State { get; }

        /// <summary>
        /// Manually SignOut an account. Use <see cref="ICapabilities.AccountManualSignOut"/> for a given account type
        /// to check if sign out is supported.
        /// </summary>
        /// <exception cref="InvalidOperationException">Sign out isn't supported as indicated by ICapabilities.</exception>
        /// <returns>A Task containing a result of the sign out.</returns>
        Task<bool> SignOut();

        /// <summary>
        /// Provides a non-specific account name that can be used to display a string identifier for a user.
        /// </summary>
        /// <remarks>
        /// <para>Some platforms can provide multiple types of name, in which case it may be more appropriate to use <see cref="HasAttribute"/> and <see cref="GetAttribute"/> to retrieve specific types of names for the account.</para>
        /// <para>This method should not throw platform-specific exceptions.</para>
        /// </remarks>
        /// <returns>A Task containing the name.</returns>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        Task<string> GetName();

        /// <summary>
        /// Provides a non-specific account picture.
        /// </summary>
        /// <remarks>
        /// <para>Some platforms can provide multiple types of account picture, in which case it may be more appropriate to use <see cref="HasAttribute"/> and <see cref="GetAttribute"/> to retrieve specific types of picture for the account.</para>
        /// <para>This method doesn't throw platform-specific exceptions.</para>
        /// </remarks>
        /// <returns>A Task containing the picture or null.</returns>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        Task<Texture2D> GetPicture();

        /// <summary>Check if an attribute with a given type and name is defined.</summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <typeparam name="T">Type of the attribute.</typeparam>
        /// <remarks>Attributes are given a unique name, that is set in the editor.</remarks>
        /// <returns>True, if the attribute is defined, false otherwise.</returns>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        bool HasAttribute<T>(string attributeName);

        /// <summary>
        /// Get the value of an attribute.
        /// This method can throw various exceptions depending on the attribute specified.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <typeparam name="T">Type of the attribute.</typeparam>
        /// <returns>Attribute value.</returns>
        /// <exception cref="InvalidOperationException">Attribute with a given type and name combination is not defined. Call <see cref="HasAttribute{T}"/> to make sure the attribute is defined.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        Task<T> GetAttribute<T>(string attributeName)
        {
            throw new InvalidOperationException("Attributes are not supported");
        }

        /// <summary>Get the <see cref="ISavingSystem"/> belonging to the <see cref="IAccount"/>.</summary>
        /// <remarks>
        /// <para>Saves within this system are stored in dedicated account storage: they are not visible or accessible using saving systems belonging to other accounts or using the local saving system.</para>
        /// <para>When the account's <see cref="GetSavingSystem"/> is called, the saving system may need to be created. While creating the save system, some platforms might perform long-running operations such as downloading cloud saves. During saving system creation, OS-level UI prompts may be displayed.</para>
        /// </remarks>
        /// <returns>Task containing the saving system for the account.</returns>
        /// <exception cref="InvalidOperationException"><see cref="IAccount"/> doesn't support saving. See <see cref="ICapabilities.AccountSaving"/>.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="UserRefusalException">User chose not to give access to the saving system.</exception>
        /// <exception cref="NotEnoughSpaceException">User does not have enough disc space left on device for saving operation.</exception>
        /// <seealso cref="PlatformToolkit.LocalSaving"/>
        Task<ISavingSystem> GetSavingSystem();

        /// <summary>Get the <see cref="IAchievementSystem"/> belonging to the <see cref="IAccount"/>.</summary>
        /// <returns>Task containing the achievement system for the account.</returns>
        /// <exception cref="InvalidOperationException"><see cref="IAccount"/> doesn't support achievements. See <see cref="ICapabilities.AccountAchievements"/>.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        Task<IAchievementSystem> GetAchievementSystem();
    }
}
