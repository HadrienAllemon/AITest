# Elara — private AI journaling companion

## What this is and why

Elara is a personal project, not a product. The author has ADHD and has
found that free-form written journaling doesn't stick, but journaling
where "the journal talks back" does. Off-the-shelf AI chat tools solve
the "talks back" part but not the privacy part — journaling into a
service that logs/trains on your data defeats the point of journaling
somewhere honest. Elara is an attempt to get both: a conversational
journaling companion that runs entirely locally (local LLM via Ollama,
local SQL Server), so the content never leaves the machine.

Practically, that local-first constraint is a real design driver, not
just a preference — it's why the chat model is Ollama-hosted rather than
a hosted API, and it should keep informing choices going forward (e.g.
don't casually introduce a cloud AI dependency into this project without
flagging the tradeoff).

The tone the assistant should take with the *journal's user* is
deliberately not the standard supportive-chatbot tone: see
`Elara1/Prompts/SystemPrompt.cs` → `PersonaClearCut` (the persona
currently wired into `ChatService`). It's built to push back and name
avoidance/patterns rather than default to validation. If you're touching
prompt content, preserve that intent rather than softening it toward
generic assistant warmth. Older persona drafts (`Persona`,
`PersonaDetailed`, `FormattingRules`) are kept in the same file as
comments-explained fallbacks, not dead code to delete casually.

## Architecture

Three projects:

- **`Elara1`** — ASP.NET Core minimal API backend (.NET 8). Owns the AI
  integration (`Elara1/AI/ChatService.cs`, `Elara1/AI/RoleCache.cs`),
  prompts (`Elara1/Prompts`), and a not-yet-implemented memory
  abstraction (`Elara1/Memory/IMemoryStore.cs` — currently an empty
  interface; the actual "facts about the user" retrieval today is a
  simple keyword-match over the `MemoryFacts` table done inline in
  `ChatService`, not through this interface).
- **`Elara1.DataAccess`** — EF Core 8 + SQL Server persistence layer
  (`ElaraDbContext`, `History/{Conversation,Message,Role}.cs`,
  `Memory/MemoryFact.cs`). Chat/journal history and the small "facts"
  table live here.
- **`ElaraReact/ElaraReact`** — Vite + React 19 + TypeScript frontend.
  Currently a minimal manual test console (`src/App.tsx`) for hitting
  `/api/chat` directly, not a real UI yet.

Backend runs on `http://localhost:5199`; dev CORS policy allows
`http://localhost:5173` (the Vite dev server). Local LLM via
`OllamaSharp`, pointed at `http://localhost:11434/` with model
`dolphin-llama3` (alternatives noted in `ChatService`: `qwen2.5:7b`,
`llama3.1:8b`) — the Ollama server has to actually be running locally for
the backend to do anything useful. DB connection string
(`Elara1/appsettings.json`) points at a local SQL Server Express
instance with a trusted connection — again, keeping data local.

## Layering rule established for this project

`Elara1.DataAccess` must not reference `Microsoft.Extensions.AI` (or any
AI SDK). Translating between AI-SDK types (`ChatRole`, etc.) and
persistence entities belongs in `Elara1` (the AI layer), not in
DataAccess — DataAccess should stay a plain persistence layer that could
in principle be reused without dragging in a chat-model dependency. This
came up because `Role.cs` originally had a `FromAIRole(ChatRole)` method
depending on `Microsoft.Extensions.AI`; that's been moved out (see
`Elara1/AI/RoleCache.cs`, which now owns the `ChatRole → Role` mapping).

`RoleCache` is registered as a DI singleton (alongside `ChatService`,
which is also a singleton) and caches the handful of `Role` rows
(Assistant/User/System/Tool/Unknown) in memory for the process lifetime
instead of querying — or worse, inserting a fresh duplicate row — per
message. It's self-healing: on first use it loads whatever role rows
already exist and creates any that are missing, rather than relying on
migration-time `HasData` seeding (deliberately avoided here because the
`Roles` table already has a handful of duplicate rows from the earlier
per-message-insert bug, and a migration with hardcoded IDs risked
colliding with that existing data).

## Known in-progress / rough edges

The project was left mid-implementation, so some of this is genuinely
unfinished rather than a design decision:

- **Messages likely aren't actually being persisted end-to-end right
  now.** In `ChatService.SendMessageAsync`, a `Conversation` is
  constructed locally but never saved (`CreateConversation` exists but
  isn't called from this path), so `conv.Id` stays `0`. The subsequent
  `AddMessageToConversation(msg, conv.Id)` call looks up a conversation
  by that id, finds none, and silently no-ops. Worth fixing before
  relying on journal history actually being saved.
- Only the assistant's reply is ever handed to
  `AddMessageToConversation` — the user's own message is appended to the
  in-memory `chatHistory` list (used for model context) but never saved
  as a `Message` row.
- `ChatService` is a DI singleton holding one shared in-memory
  `chatHistory` list — fine for a single local user with one ongoing
  conversation, but it doesn't distinguish between conversations/sessions.
  Multi-conversation support would need this rethought.
- There's an **uncommitted, not-yet-migrated schema change**: `Message.Role`
  was changed from `string` to the `Role` navigation entity (and
  `ElaraDbContext`'s FK mapping updated to match), but no EF migration
  has been generated for that shape yet. The live SQL Server `Elara`
  database may still reflect the older shape. Generate/apply a migration
  once this shape is considered final (`dotnet ef migrations add ... `
  from `Elara1.DataAccess`, using `Elara1` as the startup project — the
  `dotnet-ef` tool isn't currently restored locally, see
  `dotnet tool restore`).
- No automated tests exist yet.
- `IMemoryStore` (`Elara1/Memory/IMemoryStore.cs`) is an empty marker
  interface — a placeholder for a proper memory/fact-retrieval
  abstraction that hasn't been built. The current "facts about the user"
  retrieval is a hardcoded keyword-match (`ContainsRelevantKeywords` in
  `ChatService`) over `MemoryFacts` seeded via `HasData` in
  `ElaraDbContext`.
