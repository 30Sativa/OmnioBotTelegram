using System;
using System.Threading;
using System.Threading.Tasks;
using TelegramSativaBot.Domain.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramSativaBot.Application.Handlers
{
    public class UpdateHandler
    {
        private readonly IMessageService _messageService;

        public UpdateHandler(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message || update.Message?.Text == null)
                return;

            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            switch (text.ToLower())
            {
                case "/start":
                    await _messageService.SendMessageAsync(chatId, "👋 Xin chào! Tôi là bot.");
                    break;

                case "/help":
                    await _messageService.SendMessageAsync(chatId, "📖 Các lệnh:\n/start\n/help\n/echo <nội dung>");
                    break;

                default:
                    if (text.StartsWith("/echo "))
                        await _messageService.SendMessageAsync(chatId, text.Substring(6));
                    else
                        await _messageService.SendMessageAsync(chatId, $"❓ Không hiểu lệnh: {text}");
                    break;
            }
        }
    }
}
