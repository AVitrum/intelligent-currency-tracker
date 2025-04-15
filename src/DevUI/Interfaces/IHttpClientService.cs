namespace DevUI.Interfaces;

public interface IHttpClientService
{
    Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc);
}