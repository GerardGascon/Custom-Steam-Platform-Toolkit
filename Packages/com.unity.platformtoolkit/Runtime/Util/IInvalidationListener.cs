using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Used when a dependency needs to listen when an object becomes invalid. For example,
    /// saves listening when a saving system becomes invalid.
    /// </summary>
    internal interface IInvalidationListener
    {
        Task OnInvalidation();
    }
}
