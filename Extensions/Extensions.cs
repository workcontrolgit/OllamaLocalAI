using Microsoft.Extensions.AI;

namespace OllamaLocalAI.Extensions
{
    public static class Extensions
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddAIServices();
        }

        private static void AddAIServices(this IHostApplicationBuilder builder)
        {
            var loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();

            string? ollamaEndpoint = builder.Configuration["AI:Ollama:Endpoint"];
            if (!string.IsNullOrWhiteSpace(ollamaEndpoint))
            {
                builder.Services.AddChatClient(new OllamaChatClient(ollamaEndpoint, builder.Configuration["AI:Ollama:ChatModel"] ?? "llama3.1"))
                    .UseFunctionInvocation()
                    .UseOpenTelemetry(configure: t => t.EnableSensitiveData = true)
                    .UseLogging(loggerFactory)
                    .Build();
            }
        }
    }
}