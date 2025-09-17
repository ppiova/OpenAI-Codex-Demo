namespace AzureSearchOpenAIDemo.Api.Options;

public class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";

    public string? Endpoint { get; set; }

    public string? IndexName { get; set; }

    public string? ApiKey { get; set; }

    public string? SemanticConfiguration { get; set; }

    public string[]? SearchFields { get; set; }

    public string? VectorFieldName { get; set; }

    public int VectorDimensions { get; set; } = 1536;

    public int DefaultTop { get; set; } = 3;

    public double? MinimumSemanticScore { get; set; }
}
