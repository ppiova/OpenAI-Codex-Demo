using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AzureSearchOpenAIDemo.Api.Models;
using AzureSearchOpenAIDemo.Api.Options;
using AzureSearchOpenAIDemo.Api.Services;
using Microsoft.Extensions.Options;

namespace AzureSearchOpenAIDemo.Api.Orchestration;

public class ChatOrchestrator
{
    private const string DefaultPrompt = """
You are an intelligent assistant helping users with information retrieved from Azure Cognitive Search.
Answer the user's question using only the provided sources. Always cite sources in square brackets using their identifiers (for example, [doc1]).
If the answer cannot be determined from the sources, say you do not know.
""";

    private static readonly Regex CitationRegex = new("\\[(doc\\d+)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IAzureSearchService _searchService;
    private readonly IOpenAIService _openAiService;
    private readonly AzureOpenAIOptions _openAiOptions;

    public ChatOrchestrator(IAzureSearchService searchService, IOpenAIService openAiService, IOptions<AzureOpenAIOptions> openAiOptions)
    {
        _searchService = searchService;
        _openAiService = openAiService;
        _openAiOptions = openAiOptions.Value;
    }

    public async Task<ChatResponse> GetResponseAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException("The question cannot be empty.", nameof(request));
        }

        var searchResults = await _searchService.SearchAsync(new SearchRequest
        {
            Query = request.Question,
            Top = request.Top,
            UseSemantic = request.UseSemanticCaptions,
            UseVector = !string.IsNullOrWhiteSpace(_openAiOptions.EmbeddingDeploymentName)
        }, cancellationToken);

        string context = BuildContextMessage(searchResults);

        var conversation = new List<ChatMessage>
        {
            new()
            {
                Role = "system",
                Content = string.IsNullOrWhiteSpace(request.OverridePrompt) ? DefaultPrompt : request.OverridePrompt!
            },
            new()
            {
                Role = "system",
                Content = context
            }
        };

        if (request.History.Count > 0)
        {
            conversation.AddRange(request.History.Select(m => new ChatMessage
            {
                Role = m.Role,
                Content = m.Content
            }));
        }

        conversation.Add(new ChatMessage
        {
            Role = "user",
            Content = request.Question
        });

        var assistantMessage = await _openAiService.GetChatCompletionAsync(conversation, cancellationToken);

        var updatedHistory = new List<ChatMessage>(request.History)
        {
            new() { Role = "user", Content = request.Question },
            assistantMessage
        };

        var citations = ExtractCitations(assistantMessage.Content, searchResults);

        return new ChatResponse
        {
            Answer = assistantMessage.Content,
            Citations = citations,
            History = updatedHistory,
            SourceDocuments = searchResults.ToList()
        };
    }

    private static string BuildContextMessage(IReadOnlyList<SearchDocumentResult> documents)
    {
        if (documents.Count == 0)
        {
            return "No sources were found for this query.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("The following documents are available as reference sources:");
        builder.AppendLine();

        foreach (var document in documents)
        {
            builder.AppendLine($"[{document.CitationId}] {document.Title}");
            if (!string.IsNullOrWhiteSpace(document.Source))
            {
                builder.AppendLine($"Source: {document.Source}");
            }

            builder.AppendLine(document.Content);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static IList<Citation> ExtractCitations(string answer, IReadOnlyList<SearchDocumentResult> documents)
    {
        var citations = new List<Citation>();

        if (string.IsNullOrWhiteSpace(answer))
        {
            return citations;
        }

        var matches = CitationRegex.Matches(answer);
        foreach (Match match in matches)
        {
            var citationId = match.Groups[1].Value;
            var document = documents.FirstOrDefault(d => string.Equals(d.CitationId, citationId, StringComparison.OrdinalIgnoreCase));

            if (document is not null && citations.All(c => !string.Equals(c.Id, document.CitationId, StringComparison.OrdinalIgnoreCase)))
            {
                citations.Add(new Citation
                {
                    Id = document.CitationId,
                    Title = document.Title,
                    Source = document.Source,
                    Content = document.Content
                });
            }
        }

        return citations;
    }
}
