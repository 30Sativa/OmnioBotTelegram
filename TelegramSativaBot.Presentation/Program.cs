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
                    
                    var botToken = configToken ?? envToken ?? fallbackToken;
                    
                    // Debug: In ra token để kiểm tra
                    Console.WriteLine($"🔍 Config token: {configToken}");
                    Console.WriteLine($"🔍 Env token: {envToken}");
                    Console.WriteLine($"🔍 Final bot token length: {botToken?.Length ?? 0}");
                    Console.WriteLine($"🔍 Final bot token starts with: {botToken?.Substring(0, Math.Min(10, botToken?.Length ?? 0))}");
                    
                    // Debug: In ra tất cả biến môi trường
                    Console.WriteLine("🔍 Environment variables:");
                    foreach (var env in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>())
                    {
                        if (env.Key.ToString().Contains("BOT", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"  {env.Key}: {env.Value}");
                        }
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
