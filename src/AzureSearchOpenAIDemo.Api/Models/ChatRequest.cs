namespace AzureSearchOpenAIDemo.Api.Models;

public class ChatRequest
{
    public string Question { get; set; } = string.Empty;

    public IList<ChatMessage> History { get; set; } = new List<ChatMessage>();

    public int? Top { get; set; }

    public string? OverridePrompt { get; set; }

    public bool UseSemanticCaptions { get; set; } = true;
}
