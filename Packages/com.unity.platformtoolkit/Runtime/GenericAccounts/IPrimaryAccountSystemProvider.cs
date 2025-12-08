using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    internal interface IPrimaryAccountSystemProvider
    {
        Task<IAccount> Establish();
    }
}
