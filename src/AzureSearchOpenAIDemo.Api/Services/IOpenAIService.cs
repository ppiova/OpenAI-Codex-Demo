using AzureSearchOpenAIDemo.Api.Models;

namespace AzureSearchOpenAIDemo.Api.Services;

public interface IOpenAIService
{
    Task<ChatMessage> GetChatCompletionAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<float>> GenerateEmbeddingsAsync(string input, CancellationToken cancellationToken = default);
}
