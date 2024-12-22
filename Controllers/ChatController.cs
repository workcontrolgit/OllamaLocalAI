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

        public ChatController(IChatClient chatClient, ILogger<ChatController> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Chat(ChatPrompt chatPrompt)
        {
            // Ground the initial chat messages
            var messages = GroundMessages(chatPrompt);
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
            var messages = GroundMessages(chatPrompt);
            try
            {
                // Get the response from the chat client
                var response = await _chatClient.CompleteAsync(messages);

                // Add the assistant's response to the chat history
                messages.Add(new ChatMessage(ChatRole.Assistant, response.Message.ToString()));

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

        private List<ChatMessage> GroundMessages(ChatPrompt chatPrompt)
        {
            return new List<ChatMessage>
    {
        new ChatMessage(ChatRole.System, """
            Hi there! I’m your AI assistant for PickleIQ, your go-to online training program for all things Pickleball! 🏓✨
            I’m here to help you with questions about Pickleball skills, strategies, training tips, and the exciting resources available on the PickleIQ website.

            Whether you're a beginner or a seasoned player, PickleIQ is packed with tools, tutorials, and insights to help you elevate your game.

            I focus solely on Pickleball, so if you ask about something unrelated, I’ll kindly redirect you to Pickleball topics. Got a question about your game, training, or how PickleIQ can help you improve? Let’s dive in!
            """),
        new ChatMessage(ChatRole.Assistant, """
            Hi! I'm the PickleIQ Coach. How can I help?
            """),
        new ChatMessage(ChatRole.User, chatPrompt.Message)
    };
        }
    }
}