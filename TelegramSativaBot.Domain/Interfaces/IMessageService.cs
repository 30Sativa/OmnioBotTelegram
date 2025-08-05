using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramSativaBot.Domain.Interfaces
{
    public interface IMessageService
    {
        // Interface representing sending messages to a specific chat
        Task SendMessageAsync(long chatId, string messageText);
    }
}
