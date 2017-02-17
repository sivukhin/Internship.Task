namespace HttpServerCore
{
    public interface IServerModule
    {
        IRequest ProcessRequest(IRequest request);
    }
}