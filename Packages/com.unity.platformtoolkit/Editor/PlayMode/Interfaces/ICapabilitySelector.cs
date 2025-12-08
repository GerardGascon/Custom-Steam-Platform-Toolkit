using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Interface for interacting with play mode platform capabilities.
    /// Allows setting and getting current capability.
    /// Provides events when capabilities change.
    /// Provides the set of all available capabilities.
    /// </summary>
    internal interface ICapabilitySelector
    {
        /// <summary>
        /// Currently selected capability for the play mode platform.
        /// </summary>
        IPlayModeCapability CurrentCapability { get; set; }

        /// <summary>
        /// Invoked after the value of <see cref="CurrentCapability"/> changes.
        /// </summary>
        event Action CurrentCapabilityChanged;

        /// <summary>
        /// All play mode platform capabilities currently available in the project. Can change if a new capability is added or deleted.
        /// </summary>
        IEnumerable<IPlayModeCapability> Capabilities { get; }

        /// <summary>
        /// Invoked after capability set returned by <see cref="Capabilities"/> changes.
        /// Typically IPlayModeCapability is backed by an asset, this event is called when one of these assets is created or deleted.
        /// </summary>
        event Action CapabilitiesCreatedOrDeleted;
    }
}
