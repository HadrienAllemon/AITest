using Elara1.AI;
using Elara1.DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug);

const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

// Factory (not AddDbContext) because ChatService below is a singleton, and a scoped
// DbContext must never be injected directly into a singleton.
builder.Services.AddDbContextFactory<ElaraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Singleton so chat history persists across requests, matching the old REPL behavior.
builder.Services.AddSingleton<ChatService>();

builder.WebHost.UseUrls("http://localhost:5199");

var app = builder.Build();

app.UseCors(DevCorsPolicy);

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/chat", async (ChatRequest request, ChatService chatService) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
        return Results.BadRequest(new { error = "Message must not be empty." });

    var reply = await chatService.SendMessageAsync(request.Message);
    return Results.Ok(new ChatResponse(reply));
});

app.Run();

record ChatRequest(string Message);
record ChatResponse(string Reply);
