# Flow: ai-agent-with-kb

## Trigger

User wants to create a fully configured AI agent with a knowledge base and tools.

> "create an AI agent with a knowledge base"
> "set up a chatbot with document search"
> "build an AI assistant that can answer questions from our docs"

---

## Pre-flight Questions

1. What is the agent's name and purpose?
2. What persona/system prompt should the agent use?
3. What knowledge sources? (files, text, links, Q&A pairs)
4. Should the agent have tool access? (API tools, MCP tools)
5. Which AI model to use?

---

## Cross-Domain Dependencies

| Step | Domain | Action |
|------|--------|--------|
| 1 | ai-services | Create agent |
| 2 | ai-services | Update agent persona |
| 3 | ai-services | Update agent AI config (model) |
| 4 | ai-services | Create KB folder |
| 5 | ai-services | Ingest KB content (files/text/links) |
| 6 | ai-services | Create tools (optional) |
| 7 | ai-services | Publish agent |
| 8 | ai-services | Test with chat |

---

## Flow Steps

### Step 1 — Create Agent
```
Action: ai-services/actions/create-agent
Input:
  name        = "Support Bot"
  description = "Answers customer questions from documentation"
  projectKey  = $PROJECT_SLUG
Output:
  agentId → needed for all subsequent steps
```

### Step 2 — Set Persona
```
Action: ai-services/actions/update-agent-persona
Input:
  agentId  = (from Step 1)
  persona  = "You are a helpful support assistant. Answer questions based on the provided knowledge base. Be concise and accurate."
  projectKey = $PROJECT_SLUG
```

### Step 3 — Configure AI Model
```
Action: ai-services/actions/update-agent-ai-config
Input:
  agentId    = (from Step 1)
  modelId    = (from get-models or create-model)
  projectKey = $PROJECT_SLUG
```

### Step 4 — Create KB Folder
```
Action: ai-services/actions/create-kb-folder
Input:
  agentId  = (from Step 1)
  name     = "documentation"
  projectKey = $PROJECT_SLUG
Output:
  folderId → needed for Step 5
```

### Step 5 — Ingest Knowledge
Use one or more of:
- `ai-services/actions/upload-kb-file` for documents
- `ai-services/actions/ingest-kb-text` for raw text
- `ai-services/actions/ingest-kb-link` for web URLs
- `ai-services/actions/ingest-kb-qa` for Q&A pairs

### Step 6 — Add Tools (Optional)
```
Action: ai-services/actions/create-api-tool or create-mcp-tool
```

### Step 7 — Publish Agent
```
Action: ai-services/actions/publish-agent
Input:
  agentId    = (from Step 1)
  projectKey = $PROJECT_SLUG
```

### Step 8 — Test
```
Action: ai-services/actions/initiate-conversation → chat-agent
```

---

## Error Handling

| Error | Step | Action |
|-------|------|--------|
| Agent creation fails | Step 1 | Check name uniqueness |
| Model not found | Step 3 | List available models with get-models first |
| KB ingestion fails | Step 5 | Check file size limits, URL accessibility |
| Publish fails | Step 7 | Ensure persona and model are configured |
| 401 on any step | Any | Refresh token and retry |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/ai-services/pages/agent-wizard/agent-wizard-page.tsx` | Multi-step wizard: basic info → persona → model → knowledge base → tools → publish |
| `modules/ai-services/components/kb-uploader/kb-uploader.tsx` | File/text/link/QA ingestion UI |
| `modules/ai-services/components/agent-tester/agent-tester.tsx` | Chat interface for testing the agent |
