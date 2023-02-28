﻿using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramBot.Interfaces;
using TelegramBot.Configuration;
using Microsoft.Extensions.Options;
using OpenAI.GPT3.ObjectModels.RequestModels;
using static OpenAI.GPT3.ObjectModels.Models;
using OpenAI.GPT3.Managers;

namespace TelegramBot.Handlers;

public class QuestionHandler : IQuestionHandler
{
    private readonly OpenAIService _openAIClient;
    private readonly OpenAIConfiguration _openAIConfiguration;
    private readonly ILogger<QuestionHandler> _logger;

    public QuestionHandler(
        OpenAIService openAIClient,
        IOptions<OpenAIConfiguration> openAIConfiguration,
        ILogger<QuestionHandler> logger)
    {
        _openAIClient = openAIClient;
        _openAIConfiguration = openAIConfiguration.Value;
        _logger = logger;
    }

    public async Task<Message> AnswerTheQuestion(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Answer the question started.");
        
        var result = await _openAIClient.Completions.CreateCompletion(new CompletionCreateRequest
            {
                MaxTokens = int.TryParse(_openAIConfiguration.CompletionTokens, out var maxTokens) ? maxTokens : 2048,
                Temperature = float.TryParse(_openAIConfiguration.CompletionTemperature, out var temperature) ? temperature : 0.8f,
                Model = string.IsNullOrWhiteSpace(_openAIConfiguration.CompletionModel) ? _openAIConfiguration.CompletionModel : TextDavinciV3,
            }, cancellationToken: cancellationToken);

        if (result.Successful)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: result.ToString(),
                cancellationToken: cancellationToken);
        }

        return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Failed to create completion.",
                cancellationToken: cancellationToken);
    }
}
