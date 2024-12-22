using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OllamaLocalAI.Models;

namespace OllamaLocalAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatController> _logger;
        private readonly IConfiguration _configuration;

        public ChatController(IChatClient chatClient, ILogger<ChatController> logger, IConfiguration configuration)
        {
            _chatClient = chatClient;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Chat(ChatPrompt chatPrompt)
        {
            // Ground the initial chat messages
            var messages = GroundPrompt(chatPrompt);
            try
            {
                // Get the response from the chat client
                var response = await _chatClient.CompleteAsync(messages);

                // Return the full chat history
                return Ok(response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat prompt");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("chathistory")]
        public async Task<IActionResult> ChatHistory(ChatPrompt chatPrompt)
        {
            // Ground the initial chat messages
            var messages = GroundPrompt(chatPrompt);
            try
            {
                // Get the response from the chat client
                var response = await _chatClient.CompleteAsync(messages);

                // Add the assistant's response to the chat history
                messages.Add(new ChatMessage(ChatRole.Assistant, response.Message.Contents));

                // Return the full chat history
                return Ok(messages.Select(m => new
                {
                    Role = m.Role.ToString(),
                    Message = m.Contents
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat prompt");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<ChatMessage> GroundPrompt(ChatPrompt chatPrompt)
        {
            // Load the configuration
            var systemMessage = _configuration["ChatSettings:SystemMessage"];
            var assistantMessage = _configuration["ChatSettings:AssistantMessage"];

            return new List<ChatMessage>
    {
        new ChatMessage(ChatRole.System, systemMessage ?? "Default system message."),
        new ChatMessage(ChatRole.Assistant, assistantMessage ?? "Default assistant message."),
        new ChatMessage(ChatRole.User, chatPrompt.Message)
    };
        }
    }
}