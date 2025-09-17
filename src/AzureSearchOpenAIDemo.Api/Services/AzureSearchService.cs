using System.Linq;
using System.Text;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using AzureSearchOpenAIDemo.Api.Models;
using AzureSearchOpenAIDemo.Api.Options;
using Microsoft.Extensions.Options;

namespace AzureSearchOpenAIDemo.Api.Services;

public class AzureSearchService : IAzureSearchService
{
    private readonly AzureSearchOptions _options;
    private readonly IOpenAIService _openAiService;
    private readonly SearchClient _searchClient;

    public AzureSearchService(IOptions<AzureSearchOptions> options, IOpenAIService openAiService)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _openAiService = openAiService;

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException("Azure Search endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.IndexName))
        {
            throw new InvalidOperationException("Azure Search index name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Azure Search API key is not configured.");
        }

        _searchClient = new SearchClient(new Uri(_options.Endpoint), _options.IndexName, new AzureKeyCredential(_options.ApiKey));
    }

    public async Task<IReadOnlyList<SearchDocumentResult>> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return Array.Empty<SearchDocumentResult>();
        }

        int top = request.Top ?? _options.DefaultTop;

        var options = new SearchOptions
        {
            Size = top,
            IncludeTotalCount = true
        };

        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            options.Filter = request.Filter;
        }

        if (_options.SearchFields is { Length: > 0 })
        {
            foreach (var field in _options.SearchFields)
            {
                options.SearchFields.Add(field);
            }
        }

        bool useSemanticSearch = request.UseSemantic && !string.IsNullOrWhiteSpace(_options.SemanticConfiguration);
        if (useSemanticSearch)
        {
            options.QueryType = SearchQueryType.Semantic;
            options.SemanticConfigurationName = _options.SemanticConfiguration;
            options.QueryLanguage = QueryLanguage.EnUs;
            options.QueryCaption = QueryCaptionType.Extractive;
            options.QueryAnswer = QueryAnswerType.Extractive;
        }

        if (request.UseVector && !string.IsNullOrWhiteSpace(_options.VectorFieldName))
        {
            var embedding = await _openAiService.GenerateEmbeddingsAsync(request.Query, cancellationToken);
            options.VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    new VectorQuery(new ReadOnlyMemory<float>(embedding.ToArray()))
                    {
                        KNearestNeighborsCount = top,
                        Fields = { _options.VectorFieldName }
                    }
                }
            };
        }

        var response = await _searchClient.SearchAsync<SearchDocument>(request.Query, options, cancellationToken);

        var documents = new List<SearchDocumentResult>();
        int index = 1;

        await foreach (var result in response.Value.GetResultsAsync())
        {
            if (_options.MinimumSemanticScore.HasValue && result.RerankerScore.HasValue && result.RerankerScore.Value < _options.MinimumSemanticScore.Value)
            {
                continue;
            }

            var document = result.Document;

            string? title = TryGetString(document, "title", "metadata_title", "name");
            string? content = TryGetString(document, "content", "chunks", "text");

            if (useSemanticSearch && result.SemanticCaptions is { Count: > 0 })
            {
                content = string.Join(Environment.NewLine, result.SemanticCaptions.Select(c => c.Text));
            }

            documents.Add(new SearchDocumentResult
            {
                Id = TryGetString(document, "id", "metadata_storage_path") ?? index.ToString(),
                Title = title ?? $"Document {index}",
                Content = content ?? string.Empty,
                Source = TryGetString(document, "source", "url", "filepath"),
                Score = result.Score ?? 0,
                CitationId = $"doc{index}",
                Highlights = result.Highlights?.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<string>)kvp.Value.ToList()) ?? new Dictionary<string, IReadOnlyList<string>>()
            });

            index++;
        }

        return documents;
    }

    private static string? TryGetString(SearchDocument document, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (document.TryGetValue(key, out var value) && value is not null)
            {
                switch (value)
                {
                    case string s:
                        return s;
                    case IEnumerable<string> strings:
                        return string.Join(Environment.NewLine, strings);
                    case IEnumerable<object> objects:
                        var builder = new StringBuilder();
                        foreach (var item in objects)
                        {
                            if (item is string str)
                            {
                                builder.AppendLine(str);
                            }
                        }

                        if (builder.Length > 0)
                        {
                            return builder.ToString();
                        }

                        break;
                }
            }
        }

        return null;
    }
}
