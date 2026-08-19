using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elara1.DataAccess;
using Elara1.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace Elara1.AI
{
    internal class ChatService
    {
        string modelName = "dolphin-llama3"; // Or "qwen2.5:7b" / "llama3.1:8b"
        public IChatClient chatClient;

        private readonly ILogger<ChatService> _logger;
        private readonly IDbContextFactory<ElaraDbContext> _dbContextFactory;
        private List<ChatMessage> chatHistory = new();

        public ChatService(ILogger<ChatService> logger, IDbContextFactory<ElaraDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            chatClient = new OllamaApiClient(
                new Uri("http://localhost:11434/"),
                modelName
            );
        }

        public async Task<string> SendMessageAsync(string userPrompt)
        {
            // --- MIDDLEWARE STEP 1: Search FAQ Database ---
            // Simple keyword/relevance match simulation:
            _logger.LogDebug("Looking into DB facts");
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var facts = await db.MemoryFacts.ToListAsync();
            var matchedFacts = facts
                .Where(fact => ContainsRelevantKeywords(userPrompt, fact.Content))
                .Select(fact => fact.Content)
                .ToList();

            // --- MIDDLEWARE STEP 2: Build System Instructions ---
            string systemPrompt = SystemPrompt.PersonaClearCut;

            if (matchedFacts.Any())
            {
                systemPrompt +=
                    "\n[RELEVANT USER FACTS RETRIEVED FROM FAQ]:\n"
                    + string.Join('\n', matchedFacts.Select(f => $"- {f}"));
            }

            _logger.LogDebug("Added {FactCount} fact(s) to context", matchedFacts.Count);

            // Update or set the pinned System Message
            chatHistory.RemoveAll(m => m.Role == ChatRole.System);
            chatHistory.Insert(0, new ChatMessage(ChatRole.System, systemPrompt));

            // Append current user message
            chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

            // --- MIDDLEWARE STEP 3: Stream response from Ollama ---
            string fullResponse = "";
            var options = new ChatOptions
            {
                Temperature = 0.6f,

                // Frequency & Presence Penalties: Discourages repetitive phrases and predictable structures
                FrequencyPenalty = 0.5f,
                PresencePenalty = 0.3f,
            };

            _logger.LogDebug("AI is thinking...");

            await foreach (var update in chatClient.GetStreamingResponseAsync(chatHistory, options))
            {
                fullResponse += update;
            }

            // Append AI response to maintain conversational context
            chatHistory.Add(new ChatMessage(ChatRole.Assistant, fullResponse));

            return fullResponse;
        }

        // Simple helper to simulate basic FAQ relevance matching
        private static bool ContainsRelevantKeywords(string prompt, string fact)
        {
            string lowerPrompt = prompt.ToLower();
            if (lowerPrompt.Contains("work") || lowerPrompt.Contains("job") || lowerPrompt.Contains("deadline"))
                return fact.Contains("software engineer") || fact.Contains("deadlines");
            if (lowerPrompt.Contains("partner") || lowerPrompt.Contains("alex"))
                return fact.Contains("Alex");
            return false;
        }
    }
}
