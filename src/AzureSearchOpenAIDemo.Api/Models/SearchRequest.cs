namespace AzureSearchOpenAIDemo.Api.Models;

public class SearchRequest
{
    public string Query { get; set; } = string.Empty;

    public int? Top { get; set; }

    public string? Filter { get; set; }

    public bool UseVector { get; set; }

    public bool UseSemantic { get; set; } = true;
}
