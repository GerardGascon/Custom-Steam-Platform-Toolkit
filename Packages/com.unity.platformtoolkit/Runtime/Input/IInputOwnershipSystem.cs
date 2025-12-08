using System;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Allows finding which account owns an input device and track when ownership changes.
    /// </summary>
    /// <remarks>Use <see cref="ICapabilities.InputOwnership"/> capability to check if input ownership is supported.</remarks>
    /// <seealso cref="PlatformToolkit.Accounts"/>
    /// <seealso cref="IAccountSystem.InputOwnership"/>
    public interface IInputOwnershipSystem
    {
        /// <summary>
        /// Event invoked after one or more account to input pairings has changed.
        /// Pairing change occurs when:
        /// 1. Input device previously paired to an account is paired to a different account or is unpaired.
        /// 2. Input device previously paired to an account is disconnected.
        /// 3. Input device that was unpaired is paired to an account.
        /// 4. Input device is connected and paired to an account.
        /// </summary>
        public event Action OnChange;

        /// <summary>
        /// Get the owner of a given input device.
        /// </summary>
        /// <param name="inputDevice">Input device, for which the owner is to be returned.</param>
        /// <returns>Returns the owner of the given input device, or null if the device has no owner.</returns>
        public IAccount GetOwner(IInputDevice inputDevice);

        /// <summary>
        /// Get the owner of a given input device.
        /// </summary>
        /// <param name="inputDevice">Input device, for which the owner is to be returned.</param>
        /// <returns>Returns the owner of the given input device, or null if the device has no owner.</returns>
        public IAccount GetOwner(object inputDevice);

        /// <summary>
        /// Register converter, which converts input system specific input device into platform specific IInputDevice.
        /// </summary>
        /// <param name="converter">Converter from input system device type T.</param>
        /// <typeparam name="T">Type which given converter can convert.
        /// Converter will be used for type T and any types derived from T, if a more specific converter is not available.</typeparam>
        public void RegisterInputDeviceConverter<T>(Func<T, IInputDevice> converter);
    }
}
