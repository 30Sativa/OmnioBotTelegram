using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramSativaBot.Application.Handlers;
using TelegramSativaBot.Infrastructure;
using Telegram.Bot.Types;
using System;

namespace TelegramSativaBot.Presentation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var botToken = context.Configuration["BotConfiguration:Token"];
                    if (string.IsNullOrWhiteSpace(botToken))
                        throw new Exception("❌ Thiếu bot token trong appsettings.json");

                    services.AddInfrastructure(botToken);
                    services.AddSingleton<UpdateHandler>();
                });

            var host = builder.Build();

            var botClient = host.Services.GetRequiredService<ITelegramBotClient>();
            var updateHandler = host.Services.GetRequiredService<UpdateHandler>();

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                new DefaultUpdateHandler(
                    updateHandler.HandleUpdateAsync,
                    HandlePollingErrorAsync
                ),
                receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine("🤖 Bot đang chạy... Nhấn Enter để dừng");
            Console.ReadLine();
            cts.Cancel();
        }

        
        public static Task HandlePollingErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            HandleErrorSource errorSource,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"❌ Lỗi polling: {exception.Message} | Source: {errorSource}");
            return Task.CompletedTask;
        }
    }
}
