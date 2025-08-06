# Telegram Bot - OmnioBot

Bot Telegram được xây dựng bằng .NET 8 với kiến trúc Clean Architecture.

## 🚀 Tính Năng

- Xử lý tin nhắn Telegram
- Kiến trúc Clean Architecture
- Dependency Injection
- Error handling và retry logic
- Health check endpoints
- Hỗ trợ deployment trên Render

## 🛠️ Cấu Trúc Project

```
TelegramSativaBot/
├── TelegramSativaBot.Domain/          # Domain layer
├── TelegramSativaBot.Application/      # Application layer
├── TelegramSativaBot.Infrastructure/   # Infrastructure layer
└── TelegramSativaBot.Presentation/     # Presentation layer
```

## 📋 Yêu Cầu

- .NET 8.0
- Docker (tùy chọn)
- Telegram Bot Token

## 🔧 Cài Đặt

### 1. Clone Repository
```bash
git clone <repository-url>
cd OmnioBotTelegram-
```

### 2. Cấu Hình Bot Token

#### Cách 1: Environment Variable
```bash
export BOT_TOKEN="your_bot_token_here"
```

#### Cách 2: appsettings.json
Copy `appsettings.template.json` thành `appsettings.json` và cập nhật token:
```json
{
  "BotConfiguration": {
    "Token": "your_bot_token_here"
  }
}
```

### 3. Build và Chạy

#### Local Development
```bash
dotnet build
dotnet run --project TelegramSativaBot.Presentation
```

#### Docker
```bash
docker build -t telegram-bot .
docker run -e BOT_TOKEN="your_bot_token" -p 8080:8080 telegram-bot
```

## 🚀 Deployment trên Render

### 1. Tạo Service trên Render

1. Kết nối repository với Render
2. Chọn **Web Service**
3. Cấu hình build settings:
   - **Build Command**: `dotnet publish TelegramSativaBot.Presentation/TelegramSativaBot.Presentation.csproj -c Release -o out`
   - **Start Command**: `dotnet out/TelegramSativaBot.Presentation.dll`

### 2. Environment Variables

Thêm các biến môi trường sau:
- `BOT_TOKEN`: Token bot của bạn
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ASPNETCORE_URLS`: `http://0.0.0.0:8080`

### 3. Health Check

Bot cung cấp các endpoints:
- `GET /` - Trang chính
- `GET /health` - Health check

## 🔧 Cấu Hình Nâng Cao

### Polling Configuration
```json
{
  "BotConfiguration": {
    "PollingTimeout": 30,
    "MaxRetries": 3,
    "RetryDelaySeconds": 5
  }
}
```

### Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Telegram.Bot": "Information"
    }
  }
}
```

## 🐛 Troubleshooting

### Lỗi Polling Conflict

Nếu gặp lỗi "Conflict: terminated by other getUpdates request":

1. **Kiểm tra instance duy nhất**: Code đã được cập nhật để đảm bảo chỉ có một instance chạy
2. **Xóa webhook cũ** (nếu có):
   ```bash
   curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/deleteWebhook"
   ```
3. **Kiểm tra logs**: Bot sẽ tự động retry khi gặp conflict

### Lỗi Port Detection

Nếu Render báo "Timed out: Port scan timeout reached":

1. Bot đã được cấu hình để bind port 8080
2. Health check endpoints có sẵn tại `/` và `/health`
3. Đảm bảo `ASPNETCORE_URLS` được set đúng

### Lỗi Token

- Kiểm tra token có hợp lệ không
- Đảm bảo bot chưa được sử dụng bởi service khác
- Kiểm tra quyền của bot

## 📊 Monitoring

### Log Messages

- `🤖 Bot đang chạy ở chế độ polling...` - Bot khởi động thành công
- `✅ Polling đã được khởi động lại thành công` - Khôi phục từ conflicts
- `❌ Lỗi polling:` - Thông báo lỗi chi tiết
- `🔄 Phát hiện conflict` - Đang xử lý conflict

### Health Check

Kiểm tra trạng thái bot:
```bash
curl https://your-app.onrender.com/health
```

## 🔒 Security

- Không commit token vào repository
- Sử dụng environment variables cho production
- Token được ẩn trong logs

## 📝 License

MIT License

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push to branch
5. Tạo Pull Request