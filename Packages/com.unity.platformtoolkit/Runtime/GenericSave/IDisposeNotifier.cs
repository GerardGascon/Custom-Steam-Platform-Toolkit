using System;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Used when object needs to listen for the disposal of another object.
    /// For example saving system listening for save disposal.
    /// </summary>
    internal interface IDisposeListener
    {
        /// <summary>
        /// Called when <see cref="IAsyncDisposable.DisposeAsync"/> is performed.
        /// </summary>
        /// <param name="name">Name that lets the listener identify the disposing object.</param>
        /// <returns>Task which allows awaiting while the listener completes action on dispose.</returns>
        Task OnAsyncDispose(string name);

        /// <summary>
        /// Called when <see cref="IDisposable.Dispose"/> is performed.
        /// </summary>
        /// <param name="name">Name that lets the listener identify the disposing object.</param>
        void OnSyncDispose(string name);
    }
}
