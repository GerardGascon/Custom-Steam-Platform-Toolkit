using UnityEngine;

internal interface IPlayModeCapability
{
    /// <summary>
    /// Title used to present platform in UI
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Maximum number of accounts that can be signed in at the same time.
    /// </summary>
    public int MaxSignedInAccounts { get; }

    /// <summary>
    /// Describes the behavior of the primary account system, detailing whether and when a primary account can be signed in.
    /// </summary>
    public PrimaryAccountBehaviour PrimaryAccountBehaviour { get; }

    /// <summary>
    /// Describes the how and when additional accounts can be signed in.
    /// </summary>
    public AdditionalAccountBehaviour AdditionalAccountBehaviour { get; }

    /// <summary>
    /// Determines whether the platform allows multiple sign-in attempts. If false, the platform will only allow one sign-in attempt.
    /// </summary>
    public bool AllowMultipleSignInAttempts { get; }

    /// <summary>
    /// Determines whether the platform supports an achievement system.
    /// </summary>
    public bool SupportsAchievements { get; }

    /// <summary>
    /// Determines whether the platform allows manual sign out of accounts via the SignOut function.
    /// </summary>
    public bool AccountsCanManuallySignOut { get; }

    /// <summary>
    /// Determines whether the platform supports an account to input pairing relationship system.
    /// </summary>
    public bool SupportsAccountInputOwnership { get; }

    /// <summary>
    /// Determines whether the platform supports a local saving system not tied to any account.
    /// </summary>
    public bool SupportsLocalSaving { get; }
}

/// <summary>
/// Enumerates the different behaviors for the primary account system.
/// </summary>
internal enum PrimaryAccountBehaviour
{
    /// <summary>
    /// The system does not support primary accounts.
    /// </summary>
    NotSupported = 0,

    /// <summary>
    /// The account must be signed in before the application is launched and cannot be changed afterward.
    /// </summary>
    AlwaysSignedIn = 1,

    /// <summary>
    /// Signing in a primary account is not required, but if an account is signed in, it cannot be changed afterward.
    /// </summary>
    OptionalAndImmutable = 2,

    /// <summary>
    /// The primary account can be signed in or out at any time and it can be changed.
    /// </summary>
    OptionalAndMutable = 3
}

/// <summary>
/// Enumerates the different behaviors for additional accounts.
/// </summary>
internal enum AdditionalAccountBehaviour
{
    /// <summary>
    /// The system does not support additional accounts.
    /// </summary>
    NotSupported = 0,

    /// <summary>
    /// Additional accounts can be signed in or out at any time.
    /// </summary>
    SignInAndOutAnytime = 1,

    /// <summary>
    /// Additional accounts can only be signed in after the application has launched using IAccountPickerSystem only,
    /// they cannot be signed in from the system environment,
    /// however, they can be signed out afterwards (after sign-in) from the system environment.
    /// All additional account will be automatically signed out when the application is closed.
    /// </summary>
    SignInOnGameRequestAndSignOutAnytime = 2
}
