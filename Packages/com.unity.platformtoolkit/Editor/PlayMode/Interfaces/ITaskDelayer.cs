using System;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit.PlayMode
{
    internal interface ITaskDelayer
    {
        /// <summary>
        /// Wait if paused for a time given by <see cref="TimeSpan"/>
        /// </summary>
        /// <param name="waitFor"> The <see cref="TimeSpan"/> that should be waited for if paused </param>
        /// <returns> A <see cref="Task"/> that can be awaited upon </returns>
        Task Delay(TimeSpan waitFor);
    }
}
