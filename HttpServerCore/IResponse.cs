using System.Net;

namespace HttpServerCore
{
    public interface IResponse
    {
        HttpStatusCode StatusCode { get; set; }
        //TODO: string instead of stream? Good idea?
        string Content { get; set; }
    }
}