using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    internal interface IAccountPickerSystemProvider
    {
        Task<IAccount> Show();
    }
}
