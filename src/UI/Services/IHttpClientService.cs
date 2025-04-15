namespace UI.Services;

public interface IHttpClientService
{
    Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc);
}