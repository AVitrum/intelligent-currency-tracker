namespace UI.Common.Interfaces;

public interface IHttpClientService
{
    Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc);
}