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
using System.Collections;

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
                    // Try multiple possible locations for appsettings.json
                    var possiblePaths = new[]
                    {
                        "appsettings.json",
                        "TelegramSativaBot.Presentation/appsettings.json"
                    };

                    foreach (var path in possiblePaths)
                    {
                        if (System.IO.File.Exists(path))
                        {
                            config.AddJsonFile(path, optional: false, reloadOnChange: true);
                            break;
                        }
                    }
                    
                    // Add environment variables
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // Đọc token từ nhiều nguồn khác nhau
                    var configToken = context.Configuration["BotConfiguration:Token"];
                    var envToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
                    var fallbackToken = "8348243210:AAFVQVv7oiEHc4IjblVqoKYb7ozapvUCfKw";
                    
                    // Logic đọc token
                    string botToken;
                    if (!string.IsNullOrWhiteSpace(configToken))
                    {
                        botToken = configToken;
                    }
                    else if (!string.IsNullOrWhiteSpace(envToken))
                    {
                        botToken = envToken;
                    }
                    else
                    {
                        botToken = fallbackToken;
                    }
                    
                    if (string.IsNullOrWhiteSpace(botToken))
                        throw new Exception("❌ Thiếu bot token trong appsettings.json hoặc biến môi trường BOT_TOKEN");

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

            Console.WriteLine("🤖 Bot đang chạy...");
            
            // Chạy liên tục cho đến khi có signal dừng
            var waitHandle = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                waitHandle.Set();
            };
            
            waitHandle.WaitOne();
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
