using AzureSearchOpenAIDemo.Api.Models;

namespace AzureSearchOpenAIDemo.Api.Services;

public interface IAzureSearchService
{
    Task<IReadOnlyList<SearchDocumentResult>> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
}
