namespace AzureSearchOpenAIDemo.Api.Options;

public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    public string? Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public string? DeploymentName { get; set; }

    public string? EmbeddingDeploymentName { get; set; }

    public string? ApiVersion { get; set; }

    public float Temperature { get; set; } = 0.3f;

    public float TopP { get; set; } = 0.95f;

    public int MaxTokens { get; set; } = 1024;

    public bool StreamResponses { get; set; }
}
