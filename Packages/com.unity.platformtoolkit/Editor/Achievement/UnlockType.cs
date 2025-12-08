namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Defines what state achievement can be in.</summary>
    public enum UnlockType
    {
        /// <summary>Achievement can be either locked or unlocked.</summary>
        Single,
        /// <summary>Achievement has progress value from 0 to <see cref="IAchievement.ProgressTarget"/>.</summary>
        Progressive
    }
}
