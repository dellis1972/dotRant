using System.IO;
using System.Threading.Tasks;

namespace dotRant
{
    public interface IConnectionFactory
    {
        Task<Stream> Connect(string hostname, int port, bool useSsl);
    }
}
