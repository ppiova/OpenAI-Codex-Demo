namespace AzureSearchOpenAIDemo.Api.Models;

public class SearchDocumentResult
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? Source { get; set; }

    public double Score { get; set; }

    public string CitationId { get; set; } = string.Empty;

    public IDictionary<string, IReadOnlyList<string>> Highlights { get; set; } = new Dictionary<string, IReadOnlyList<string>>();
}
