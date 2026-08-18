using Elara1.AI;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug)
);
var logger = loggerFactory.CreateLogger<ChatService>();

var chatService = new ChatService(logger);

while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.Trim().ToLower() == "exit")
        break;

    var response = await chatService.SendMessageAsync(input);

    Console.WriteLine($"\nAI: {response}");
}
