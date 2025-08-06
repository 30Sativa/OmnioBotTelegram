# Deployment Guide for Telegram Bot

## Render Deployment

### Environment Variables
Set these environment variables in your Render service:

- `BOT_TOKEN`: Your Telegram bot token
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ASPNETCORE_URLS`: `http://0.0.0.0:8080`

### Build Command
```bash
dotnet publish TelegramSativaBot.Presentation/TelegramSativaBot.Presentation.csproj -c Release -o out
```

### Start Command
```bash
dotnet out/TelegramSativaBot.Presentation.dll
```

### Port Configuration
The application will automatically bind to port 8080 as configured in `appsettings.json`.

### Health Check
The bot provides health check endpoints:
- `GET /` - Main status page
- `GET /health` - Health check endpoint

### Troubleshooting

#### Polling Conflict Errors
If you see "Conflict: terminated by other getUpdates request" errors:

1. **Ensure only one instance is running**: The updated code includes instance locking to prevent multiple instances.

2. **Check for duplicate deployments**: Make sure you don't have multiple services running the same bot.

3. **Clear webhook if needed**: If you previously used webhooks, clear them:
   ```bash
   curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/deleteWebhook"
   ```

4. **Monitor logs**: The bot now includes better error handling and retry logic.

#### Port Detection Issues
If Render shows "Timed out: Port scan timeout reached":

1. The application now includes a web server that binds to port 8080
2. Health check endpoints are available at `/` and `/health`
3. Ensure the `ASPNETCORE_URLS` environment variable is set correctly

### Configuration

The bot supports both polling and webhook modes:

#### Polling Mode (Default)
- Set `BotConfiguration:UseWebhook` to `false` or leave empty
- Best for development and simple deployments

#### Webhook Mode
- Set `BotConfiguration:UseWebhook` to `true`
- Set `BotConfiguration:WebhookUrl` to your webhook URL
- Requires HTTPS endpoint

### Error Handling

The bot now includes:
- Automatic retry logic for polling conflicts
- Better error logging
- Graceful shutdown handling
- Instance locking to prevent duplicates

### Monitoring

Check the application logs for:
- `🤖 Bot đang chạy ở chế độ polling...` - Bot started successfully
- `✅ Polling đã được khởi động lại thành công` - Recovery from conflicts
- `❌ Lỗi polling:` - Error messages with details

### Performance Tips

1. **Use appropriate timeouts**: Adjust `PollingTimeout` in configuration
2. **Limit retries**: Configure `MaxRetries` to prevent infinite loops
3. **Monitor memory usage**: The bot includes cleanup logic for long-running instances 