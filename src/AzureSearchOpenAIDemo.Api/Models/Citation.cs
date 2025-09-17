namespace AzureSearchOpenAIDemo.Api.Models;

public class Citation
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Source { get; set; }

    public string? Url { get; set; }

    public string? Content { get; set; }
}
