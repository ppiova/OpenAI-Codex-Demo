using System.Linq;
using Azure;
using Azure.AI.OpenAI;
using AzureSearchOpenAIDemo.Api.Models;
using AzureSearchOpenAIDemo.Api.Options;
using Microsoft.Extensions.Options;

namespace AzureSearchOpenAIDemo.Api.Services;

public class OpenAIService : IOpenAIService
{
    private readonly AzureOpenAIOptions _options;
    private readonly OpenAIClient _client;

    public OpenAIService(IOptions<AzureOpenAIOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI API key is not configured.");
        }

        var credential = new AzureKeyCredential(_options.ApiKey);
        _client = new OpenAIClient(new Uri(_options.Endpoint), credential);
    }

    public async Task<ChatMessage> GetChatCompletionAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI chat deployment name is not configured.");
        }

        var chatOptions = new ChatCompletionsOptions
        {
            MaxTokens = _options.MaxTokens,
            Temperature = _options.Temperature,
            TopP = _options.TopP
        };

        foreach (var message in messages)
        {
            chatOptions.Messages.Add(new Azure.AI.OpenAI.ChatMessage(ToChatRole(message.Role), message.Content));
        }

        var response = await _client.GetChatCompletionsAsync(_options.DeploymentName, chatOptions, cancellationToken).ConfigureAwait(false);
        var choice = response.Value.Choices.FirstOrDefault();

        if (choice?.Message is null)
        {
            throw new InvalidOperationException("Azure OpenAI did not return any completions.");
        }

        string content = choice.Message.Content ?? string.Empty;

        return new ChatMessage
        {
            Role = "assistant",
            Content = content
        };
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingsAsync(string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.EmbeddingDeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI embedding deployment name is not configured.");
        }

        var embeddingOptions = new EmbeddingsOptions();
        embeddingOptions.Input.Add(input);

        var response = await _client.GetEmbeddingsAsync(_options.EmbeddingDeploymentName, embeddingOptions, cancellationToken).ConfigureAwait(false);
        var vector = response.Value.Data.FirstOrDefault()?.Embedding;

        if (vector is null)
        {
            throw new InvalidOperationException("Azure OpenAI did not return any embeddings.");
        }

        return vector.ToArray();
    }

    private static ChatRole ToChatRole(string? role)
    {
        return role?.ToLowerInvariant() switch
        {
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            _ => ChatRole.User
        };
    }
}
