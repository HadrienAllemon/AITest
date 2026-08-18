using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elara1.Memory;
using Elara1.Prompts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace Elara1.AI
{
    internal class ChatService
    {
        string modelName = "mannix/llama3.1-8b-abliterated"; // Or "qwen2.5:7b" / "llama3.1:8b"
        public IChatClient chatClient;

        private readonly ILogger<ChatService> _logger;
        private List<string> mockFaqDatabase = MockMemoryStore.mockFaqDatabase;
        private List<ChatMessage> chatHistory = new();

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
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
            var matchedFacts = mockFaqDatabase
                .Where(fact => ContainsRelevantKeywords(userPrompt, fact))
                .ToList();

            // --- MIDDLEWARE STEP 2: Build System Instructions ---
            string systemPrompt = SystemPrompt.Persona;

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
                // Temperature: Slightly higher means more creative/dynamic phrasing (0.7 to 0.8)
                Temperature = 0.75f,

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
