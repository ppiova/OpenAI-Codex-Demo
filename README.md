# Azure Cognitive Search + Azure OpenAI Demo (.NET)

This repository contains a C#/.NET implementation of the experience provided in the original [azure-search-openai-demo](https://github.com/Azure-Samples/azure-search-openai-demo). It exposes a Web API that orchestrates Azure Cognitive Search and Azure OpenAI to answer questions with cited references.

## Features

- **Retrieval augmented generation** powered by Azure Cognitive Search (semantic + vector search) and Azure OpenAI chat completions.
- **Citation tracking** – responses include identifiers that map back to the retrieved documents.
- **Configurable prompts** and conversation history so that the assistant can maintain context across turns.
- **REST endpoints** for both chat completions and raw search results so the API can be consumed from any client (web, desktop, mobile, bots, etc.).

## Project structure

```
src/
  AzureSearchOpenAIDemo.sln           # Solution file
  AzureSearchOpenAIDemo.Api/          # ASP.NET Core Web API project
    Controllers/                      # REST endpoints for chat and search
    Models/                           # DTOs used by the API
    Options/                          # Configuration bindings
    Orchestration/                    # Chat orchestration logic
    Services/                         # Azure Cognitive Search and Azure OpenAI integrations
```

## Prerequisites

- .NET 8 SDK or later
- An [Azure Cognitive Search](https://learn.microsoft.com/azure/search/search-what-is-azure-search) service with an index containing the documents you want to ground answers on
- An [Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/) resource with:
  - A chat completion deployment (for example, `gpt-35-turbo` or `gpt-4o-mini`)
  - An embeddings deployment (for example, `text-embedding-ada-002` or `text-embedding-3-large`)

## Configuration

Copy `src/AzureSearchOpenAIDemo.Api/appsettings.json` and provide the settings for your environment (you can use `appsettings.Development.json` locally). The main configuration sections are:

- `AzureOpenAI`
  - `Endpoint`: Base URL of your Azure OpenAI resource
  - `ApiKey`: API key for the resource
  - `DeploymentName`: Name of the chat completion deployment
  - `EmbeddingDeploymentName`: Name of the embedding deployment (needed for vector search)
  - `Temperature`, `TopP`, `MaxTokens`: Generation controls
- `AzureSearch`
  - `Endpoint`: Base URL of your Cognitive Search service
  - `IndexName`: Name of the search index that stores your documents
  - `ApiKey`: Admin or query API key
  - `SemanticConfiguration`: Semantic configuration defined on your index
  - `SearchFields`: Fields searched for keyword/semantic queries
  - `VectorFieldName`: Name of the vector field on the index (optional but recommended)
  - `DefaultTop`: Default number of results to retrieve
  - `MinimumSemanticScore`: Filter out low relevance results when using semantic ranking

> **Tip**: The API expects the index to contain fields such as `title`, `content`, and `source` (similar to the official sample). Adjust `AzureSearchOptions` or the mapping logic in `AzureSearchService` if your schema differs.

## Running the API

```bash
cd src
# Restore dependencies (optional if you open the solution in Visual Studio/VS Code)
dotnet restore

# Run the ASP.NET Core Web API
dotnet run --project AzureSearchOpenAIDemo.Api
```

By default the API listens on `https://localhost:7240` and `http://localhost:5240`. Swagger UI is enabled in development for quick testing.

## Endpoints

- `POST /api/search` – accepts a `SearchRequest` payload and returns the raw search results used for grounding.
- `POST /api/chat` – accepts a `ChatRequest` payload and returns a `ChatResponse` containing:
  - `Answer`: Assistant response text with citations like `[doc1]`
  - `Citations`: Structured information about the cited documents
  - `History`: Updated conversation history including the assistant reply
  - `SourceDocuments`: Raw search results with content, scores, and highlights

### Sample request body (`/api/chat`)

```json
{
  "question": "What does the user guide say about warranty coverage?",
  "top": 5,
  "history": [],
  "useSemanticCaptions": true
}
```

### Sample response body (`/api/chat`)

```json
{
  "answer": "The warranty covers factory defects for one year from the purchase date [doc1].",
  "citations": [
    {
      "id": "doc1",
      "title": "Contoso Warranty Policy",
      "source": "https://contoso.com/warranty.pdf"
    }
  ],
  "history": [
    { "role": "user", "content": "What does the user guide say about warranty coverage?" },
    { "role": "assistant", "content": "The warranty covers factory defects..." }
  ],
  "sourceDocuments": [
    {
      "citationId": "doc1",
      "title": "Contoso Warranty Policy",
      "content": "..."
    }
  ]
}
```

## Extending the sample

- **Front-end**: Pair the API with any web or native client. The original sample uses Next.js + React – you can reuse that UI with this backend.
- **Data ingestion**: Use Azure Cognitive Search indexers, Azure Functions, Logic Apps, or custom code to populate the search index with your content.
- **Authentication**: The API currently assumes trusted callers. Add Azure AD authentication/authorization for production scenarios.

## Troubleshooting

- Ensure your Azure resources allow requests from your network.
- Confirm that the search index schema matches the fields referenced in `AzureSearchService`.
- If you do not have an embeddings deployment, set `UseVector` to `false` in requests or remove the vector configuration.

## License

This project is provided under the [MIT License](LICENSE).
