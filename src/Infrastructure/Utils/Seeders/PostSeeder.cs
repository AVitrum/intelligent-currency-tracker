using System.Text.Json;
using Application.Common.Interfaces.Repositories;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shared.Payload.Requests;

namespace Infrastructure.Utils.Seeders;

public static class PostSeeder
{
    public static async Task SeedPostsAsync(IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        IPostRepository repository = serviceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        
        UserManager<ApplicationUser> userManager =
            serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        ApplicationUser? publisher = await userManager.FindByEmailAsync("publisher@gmail.com");
        if (publisher == null)
        {
            throw new Exception("Public user not found");
        }
        
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp/posts_ua.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("posts.json file not found at " + filePath);
        }

        string json = await File.ReadAllTextAsync(filePath);
        List<CreatePostRequest>? requests = JsonSerializer.Deserialize<List<CreatePostRequest>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (requests == null || requests.Count == 0)
        {
            throw new Exception("No posts found in the JSON file");
        }
        
        foreach (CreatePostRequest request in requests)
        {
            await repository.AddAsync(new Post
            {
                Title = request.Title,
                Content = request.Content,
                Category = Enum.Parse<PostCategory>(request.Category,
                    true),
                UserId = publisher.Id,
                Language = Language.Ua
            });
        }
    }
}