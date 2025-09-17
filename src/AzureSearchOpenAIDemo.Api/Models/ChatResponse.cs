namespace AzureSearchOpenAIDemo.Api.Models;

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;

    public IList<Citation> Citations { get; set; } = new List<Citation>();

    public IList<ChatMessage> History { get; set; } = new List<ChatMessage>();

    public IList<SearchDocumentResult> SourceDocuments { get; set; } = new List<SearchDocumentResult>();
}
