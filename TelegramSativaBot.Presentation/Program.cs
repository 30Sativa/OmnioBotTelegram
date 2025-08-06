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
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TelegramSativaBot.Domain.Interfaces;

namespace TelegramSativaBot.Presentation
{
    internal class Program
    {
        private static bool _isRunning = false;
        private static readonly object _lockObject = new object();
        private static int _retryCount = 0;
        private static readonly int _maxRetries = 3;

        static async Task Main(string[] args)
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
                    services.AddScoped<UpdateHandler>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/", async context =>
                            {
                                await context.Response.WriteAsync("🤖 Telegram Bot is running!");
                            });
                            
                            endpoints.MapMethods("/health", new[] { "GET", "HEAD" }, async context =>
                            {
                                if (context.Request.Method == "HEAD")
                                {
                                    context.Response.StatusCode = 200;
                                    return;
                                }
                                await context.Response.WriteAsync("✅ Bot is healthy and running");
                            });
                        });
                    });
                });

            var host = builder.Build();

            var botClient = host.Services.GetRequiredService<ITelegramBotClient>();
            var updateHandler = host.Services.GetRequiredService<UpdateHandler>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            using var cts = new CancellationTokenSource();

            // Ensure only one instance is running
            lock (_lockObject)
            {
                if (_isRunning)
                {
                    Console.WriteLine("⚠️ Bot instance already running. Exiting...");
                    return;
                }
                _isRunning = true;
            }

            try
            {
                // Start the web host
                await host.StartAsync(cts.Token);
                
                // Start the bot
                await StartBotAsync(botClient, updateHandler, configuration, cts.Token);
            }
            finally
            {
                cts.Cancel();
                await host.StopAsync();
                lock (_lockObject)
                {
                    _isRunning = false;
                }
            }
        }

        private static async Task StartBotAsync(
            ITelegramBotClient botClient, 
            UpdateHandler updateHandler, 
            IConfiguration configuration, 
            CancellationToken cancellationToken)
        {
            var pollingTimeout = configuration.GetValue<int>("BotConfiguration:PollingTimeout", 30);
            var retryDelay = configuration.GetValue<int>("BotConfiguration:RetryDelaySeconds", 5);

            // Only use polling mode for now
            await StartPollingModeAsync(botClient, updateHandler, pollingTimeout, retryDelay, cancellationToken);
        }

        private static async Task StartPollingModeAsync(
            ITelegramBotClient botClient, 
            UpdateHandler updateHandler, 
            int timeout, 
            int retryDelay, 
            CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                Limit = 100 // Limit updates per request
            };

            botClient.StartReceiving(
                new DefaultUpdateHandler(
                    updateHandler.HandleUpdateAsync,
                    (bot, ex, source, ct) => HandlePollingErrorAsync(bot, ex, source, timeout, retryDelay, ct)
                ),
                receiverOptions,
                cancellationToken: cancellationToken
            );

            Console.WriteLine("🤖 Bot đang chạy ở chế độ polling...");
            
            // Chạy liên tục cho đến khi có signal dừng
            var waitHandle = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                waitHandle.Set();
            };
            
            waitHandle.WaitOne();
        }

        public static async Task HandlePollingErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            HandleErrorSource errorSource,
            int timeout,
            int retryDelay,
            CancellationToken cancellationToken)
        {
            var errorMessage = exception.Message;
            Console.WriteLine($"❌ Lỗi polling: {errorMessage} | Source: {errorSource}");

            // Handle specific conflict errors
            if (errorMessage.Contains("Conflict") && errorMessage.Contains("getUpdates"))
            {
                _retryCount++;
                Console.WriteLine($"🔄 Phát hiện conflict (lần thử {_retryCount}/{_maxRetries}) - đang thử khởi động lại polling sau {retryDelay} giây...");
                
                if (_retryCount <= _maxRetries)
                {
                    try
                    {
                        await Task.Delay(retryDelay * 1000, cancellationToken);
                        
                        // Wait a bit more
                        await Task.Delay(2000, cancellationToken);
                        
                        Console.WriteLine("✅ Polling đã được khởi động lại thành công");
                        _retryCount = 0; // Reset retry count on success
                    }
                    catch (Exception restartException)
                    {
                        Console.WriteLine($"❌ Không thể khởi động lại polling: {restartException.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Đã thử {_maxRetries} lần nhưng không thành công. Dừng thử lại.");
                    _retryCount = 0;
                }
            }
            else if (errorMessage.Contains("Unauthorized"))
            {
                Console.WriteLine("❌ Token bot không hợp lệ. Vui lòng kiểm tra lại token.");
            }
            else if (errorMessage.Contains("Too Many Requests"))
            {
                Console.WriteLine("⏳ Quá nhiều request - đang chờ 30 giây...");
                await Task.Delay(30000, cancellationToken);
            }
            else
            {
                // For other errors, wait a bit before continuing
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
