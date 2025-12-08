using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// A static entry-point interface for using PlatformToolkit.
    ///
    /// This interface is used to initialize the toolkit as well as get handles to major subsystems.
    /// </summary>
    public static class PlatformToolkit
    {
        private static IPlatformToolkit s_Instance;
        private static IPlatformToolkit s_InitialisingInstance;
        private static bool s_IsInitialized;
        private static Action s_Initialized;

        internal static IPlatformToolkit Instance => s_Instance;

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
        {
            s_Instance = null;
            s_InitialisingInstance = null;
            s_IsInitialized = false;
            s_Initialized = null;
        }
#endif

        /// <summary>
        /// Event for registering systems dependent on a Platform Toolkit implementation. Called after <see cref="Initialize"/>
        /// successfully initializes the Platform Toolkit implementation. If the delegate is added after Platform Toolkit is initialized, it will be called shortly after adding.
        /// </summary>
        public static event Action Initialized
        {
            add
            {
                s_Initialized += value;
                if (s_IsInitialized)
                {
#pragma warning disable CS4014
                    SafeInvoker.InvokeOnMainThread(value);
#pragma warning restore CS4014
                }
            }
            remove
            {
                s_Initialized -= value;
            }
        }

        /// <summary>
        /// Most likely a temporary way to inject PT implementations. Users should never call this method, unless they are injecting their own custom PT implementations.
        /// </summary>
        static internal void InjectImplementation(IPlatformToolkit implementation)
        {
            s_Instance = new NullToolkit();
            s_InitialisingInstance = implementation;
            s_Initialized = null;
            s_IsInitialized = false;
        }

        /// <summary>
        /// Initialize must be called to initialize Platform Toolkit. Platform Toolkit functionality will not work before Initialize completes.
        /// Games should call Initialize as early as possible, for example during the initial loading screen.
        /// </summary>
        /// <returns>Task which returns when the toolkit is initialized.</returns>
        public async static Task Initialize()
        {
            if (!s_IsInitialized)
            {
                if (s_InitialisingInstance == null)
                {
#if UNITY_EDITOR
                    throw new InvalidOperationException("No PlatformToolkit implementation available. Play Mode requires a Play Mode Controls Settings asset to be configured in Play Mode Controls.");
#else
                    throw new InvalidOperationException("No PlatformToolkit implementation available.");
#endif
                }

                await s_InitialisingInstance.Initialize();
                await Awaitable.MainThreadAsync();

                s_Instance = s_InitialisingInstance;
                s_InitialisingInstance = null;
                s_IsInitialized = true;
                SafeInvoker.Invoke(s_Initialized);
            }
        }

        /// <summary>
        /// Access to capabilities of the current platform.
        /// </summary>
        public static ICapabilities Capabilities => s_Instance.Capabilities;

        /// <summary>Get the <see cref="IAccountSystem"/>.</summary>
        /// <exception cref="InvalidOperationException">
        /// <para>Thrown if accounts aren't supported in the current implementation. Check that <see cref="ICapabilities.Accounts"/> isn't null to make sure that accounts are supported.</para>
        /// <para>Also thrown if <see cref="PlatformToolkit.Initialize"/> wasn't called or completed.</para>
        /// </exception>
        public static IAccountSystem Accounts => s_Instance.Accounts;

        /// <summary>
        /// <para>A saving system that's unrelated to any account. Saves are stored locally on the device.</para>
        /// <para>On platforms that support both an account saving system and a local saving system, it's recommended
        /// to use the account saving system and only fall back on the local saving system if an account saving system is unavailable. That way different
        /// players will be able to keep their saves separate. Use <see cref="IAccount.GetSavingSystem"/> to access the account saving system.</para>
        /// <para>Use <see cref="ICapabilities.LocalSaving"/> to check if the current platform supports local saving system.</para>
        /// </summary>
        public static ISavingSystem LocalSaving => s_Instance.LocalSavingSystem;
    }

    /// <summary>Platform Toolkit implementation. Accessed through <see cref="PlatformToolkit"/> class.</summary>
    /// <seealso cref="BaseRuntimeConfiguration"/>
    public interface IPlatformToolkit
    {
        /// <summary>
        /// Platform-specific initialization method.
        /// </summary>
        /// <returns>Task that returns when completed.</returns>
        Task Initialize();

        /// <summary>Get the <see cref="ICapabilities"/>. Exposed through <see cref="PlatformToolkit.Capabilities"/>.</summary>
        ICapabilities Capabilities { get; }

        /// <summary>Get the <see cref="IAccountSystem"/>. Exposed through <see cref="PlatformToolkit.Accounts"/>.</summary>
        IAccountSystem Accounts => throw new InvalidOperationException("Accounts are not supported on this platform.");

        /// <summary>
        /// Get the local saving system on platforms that support it.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when platform does not support the local saving system.
        /// </exception>
        ISavingSystem LocalSavingSystem => throw new InvalidOperationException("Local saving is not supported on this platform.");
    }

    internal class NullToolkit : IPlatformToolkit
    {
        private const string k_UninitialisedError = "PlatformToolkit is not initialized. Call and await PlatformToolkit.Initialize() before using this API.";

        public Task Initialize() => throw new NotImplementedException();

        public ICapabilities Capabilities => throw new InvalidOperationException(k_UninitialisedError);
        public IAccountSystem Accounts => throw new InvalidOperationException(k_UninitialisedError);
        public ISavingSystem LocalSavingSystem => throw new InvalidOperationException(k_UninitialisedError);
    }

    #region Exceptions


    // exceptions derived from InvalidOperationException

    /// <summary>
    /// Thrown when an operation fails due to user choice. For example, a user cancels the sign-in UI without signing in.
    /// </summary>
    public class UserRefusalException : InvalidOperationException
    {
        /// <summary>
        /// Construct a UserRefusalException with no message.
        /// </summary>
        public UserRefusalException()
        { }

        /// <summary>
        /// Construct a UserRefusalException with a message.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public UserRefusalException(string message) : base(message)
        { }
    }

    /// <summary>
    /// Thrown when attempting to use a signed out account.
    /// </summary>
    public class InvalidAccountException : InvalidOperationException
    {
        /// <summary>
        /// Construct an InvalidAccountException with no message.
        /// </summary>
        public InvalidAccountException()
        { }

        /// <summary>
        /// Construct an InvalidAccountException with a message.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public InvalidAccountException(string message) : base(message)
        { }
    }

    /// <summary>
    /// Thrown when a system in use is no longer valid and needs to be reinitialized.
    /// For rare edge cases where we can’t safely and silently reinitialize a system.
    /// If a system is invalid because its account is invalid, throw InvalidAccountException instead.
    /// </summary>
    public class InvalidSystemException : InvalidOperationException
    {
        /// <summary>
        /// Construct an InvalidSystemException with no message.
        /// </summary>
        public InvalidSystemException()
        { }

        /// <summary>
        /// Construct an InvalidSystemException with a message.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public InvalidSystemException(string message) : base(message)
        { }
    }

    // exceptions derived from IOException

    /// <summary>
    /// Thrown when attempting to write more data than is available to a user or is available on the system.
    /// </summary>
    public class NotEnoughSpaceException : IOException
    {
        /// <summary>
        /// Construct a NotEnoughSpaceException with no message.
        /// </summary>
        public NotEnoughSpaceException()
        { }

        /// <summary>
        /// Construct a NotEnoughSpaceException with a message.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public NotEnoughSpaceException(string message) : base(message)
        { }
    }

    /// <summary>
    /// Thrown when hitting some limit of a saving system other than not enough storage space.
    /// </summary>
    public class SaveSystemLimitException : IOException
    {
        /// <summary>
        /// Construct a SaveSystemLimitException with no message.
        /// </summary>
        public SaveSystemLimitException()
        { }

        /// <summary>
        /// Construct a SaveSystemLimitException with a message.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public SaveSystemLimitException(string message) : base(message)
        { }
    }

    /// <summary>
    /// Thrown when a corrupted save has been found.
    /// </summary>
    public class CorruptedSaveException : IOException
    {
        /// <summary>
        /// Construct a CorruptedSaveException with no message.
        /// </summary>
        public CorruptedSaveException()
        { }

        /// <summary>
        /// Construct a CorruptedSaveException with a message.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public CorruptedSaveException(string message) : base(message)
        { }
    }

    /// <summary>
    /// Thrown when an operation failed due to some temporary state, like no network connection.
    /// </summary>
    public class TemporarilyUnavailableException : IOException
    {
        /// <summary>
        /// Construct a TemporarilyUnavailableException with no message.
        /// </summary>
        public TemporarilyUnavailableException()
        { }

        /// <summary>
        /// Construct a TemporarilyUnavailableException with a message.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public TemporarilyUnavailableException(string message) : base(message)
        { }
    }

    #endregion
}
