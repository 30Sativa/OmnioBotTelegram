# OmnioBotTelegram-

Bot Telegram sử dụng .NET 8, kiến trúc Clean Architecture.

## 🚀 Tính năng
- Xử lý tin nhắn Telegram
- Clean Architecture (Domain, Application, Infrastructure, Presentation)
- Dependency Injection
- Health check endpoint
- Xử lý lỗi polling conflict

## 🗂️ Cấu trúc thư mục
```
OmnioBotTelegram-/
├── TelegramSativaBot.Domain/
├── TelegramSativaBot.Application/
├── TelegramSativaBot.Infrastructure/
└── TelegramSativaBot.Presentation/
```

## ⚙️ Cấu hình & chạy bot

### 1. Clone project
```bash
git clone <repository-url>
cd OmnioBotTelegram-
```

### 2. Cấu hình token
- **Cách 1:** Đặt biến môi trường `BOT_TOKEN`
- **Cách 2:**
  - Copy file `TelegramSativaBot.Presentation/appsettings.template.json` thành `appsettings.json`
  - Thay giá trị `Token` bằng token thật của bạn

### 3. Build & chạy local
```bash
dotnet build
dotnet run --project TelegramSativaBot.Presentation
```

### 4. Chạy bằng Docker
```bash
docker build -t telegram-bot .
docker run -e BOT_TOKEN="your_bot_token" -p 8080:8080 telegram-bot
```

## 🩺 Health check
- Truy cập: `http://localhost:8080/health`
- Trả về: `✅ Bot is healthy and running`
- Endpoint hỗ trợ cả GET và HEAD (phù hợp cho UptimeRobot)

## 🔒 Lưu ý bảo mật
- **Không commit file `appsettings.json` chứa token thật lên git**
- Luôn dùng biến môi trường cho production
- Chỉ commit file `appsettings.template.json` (mẫu)

## 📄 License
MIT