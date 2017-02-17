using System.Threading.Tasks;

namespace HttpServerCore
{
    public interface IServerModule
    {
        Task<IRequest> ProcessRequest(IRequest request);
    }
}