using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramSativaBot.Domain.Interfaces;
using TelegramSativaBot.Infrastructure.Services;

namespace TelegramSativaBot.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string botToken)
        {
            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
            services.AddSingleton<IMessageService, TelegramMessageService>();

            return services;
        }
    }
}
