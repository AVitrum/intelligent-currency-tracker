using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces.Services;

public interface IPostService
{
    Task<BaseResult> CreateAsync(CreatePostRequest request);
    Task<BaseResult> GetById(Guid id);
    Task<BaseResult> GetAttachmentsById(Guid id);
    Task<BaseResult> GetAllAsync(string language, int page, int pageSize);
}