using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramSativaBot.Domain.Interfaces;
using Telegram.Bot.Types;

namespace TelegramSativaBot.Infrastructure.Services
{
    public class TelegramMessageService : IMessageService
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramMessageService(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task SendMessageAsync(long chatId, string messageText)
        {
            await _telegramBotClient.SendMessage(chatId, messageText);
        }
    }
}
