using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Provides access to a system prompt for signing in or selecting an account.
    /// </summary>
    /// <remarks>Use <see cref="ICapabilities.AccountPicker"/> capability to check if account picker is supported.</remarks>
    /// <seealso cref="PlatformToolkit.Accounts"/>
    /// <seealso cref="IAccountSystem.Picker"/>
    public interface IAccountPickerSystem
    {
        /// <summary>Shows a system prompt for signing in or selecting an account.</summary>
        /// <remarks>If a previously signed out account is selected, the account will be signed in.</remarks>
        /// <returns>Task which represents the lifetime of the account picker prompt. TResult contains the selected <see cref="IAccount"/>.</returns>
        /// <exception cref="UserRefusalException">User has canceled the account picker prompt.</exception>
        Task<IAccount> Show();
    }
}
