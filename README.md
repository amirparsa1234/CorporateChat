# CorporateChat

CorporateChat یک سامانه چت سازمانی تحت وب است که با **ASP.NET Core (.NET 8)** توسعه داده شده و از **SQL Server** به‌عنوان دیتابیس، **Entity Framework Core** برای دسترسی به داده، **JWT + Refresh Token** برای احراز هویت و **SignalR** برای پیام‌رسانی بلادرنگ استفاده می‌کند.

## قابلیت‌ها

- ثبت‌نام و ورود کاربران با احراز هویت JWT و Refresh Token
- چت خصوصی (یک‌به‌یک) و چت گروهی
- مدیریت گروه‌ها (ساخت گروه، افزودن/حذف عضو، تغییر نقش اعضا، خروج از گروه)
- پیام‌رسانی بلادرنگ با SignalR
- آپلود و مدیریت فایل با محدودیت حجم قابل تنظیم
- رابط کاربری وب (صفحات ورود، ثبت‌نام و چت)
- مستندات API با Swagger
- اجرای Migrationها به‌صورت خودکار هنگام بالا آمدن برنامه

---

## بخش اول: ساختار و معماری پروژه

### معماری کلی

پروژه از دو پروژه‌ی .NET تشکیل شده که در یک Solution (`CorporateChat.slnx`) قرار دارند:

| پروژه | نقش |
|---|---|
| **Server** | برنامه اصلی ASP.NET Core شامل API Controllerها، SignalR Hub، دسترسی به دیتابیس (EF Core)، احراز هویت JWT، مدیریت فایل و فایل‌های استاتیک رابط کاربری |
| **Shared** | Class Library مشترک شامل Entityها (مدل‌های دیتابیس) و DTOها (مدل‌های انتقال داده بین کلاینت و سرور) |

جریان کلی درخواست‌ها:

```text
کلاینت (wwwroot / هر کلاینت HTTP)
        │
        ├── REST API  ──►  Controllers  ──►  ChatDbContext (EF Core)  ──►  SQL Server
        │
        └── WebSocket ──►  ChatHub (SignalR)  ──►  ارسال بلادرنگ پیام به کاربران آنلاین
```

### ساختار پوشه‌ها

```text
CorporateChat/
├── Server/
│   ├── Controllers/
│   │   ├── AuthController.cs      # ثبت‌نام، ورود، تمدید توکن (refresh)
│   │   ├── UsersController.cs     # لیست کاربران
│   │   ├── MessagesController.cs  # پیام‌های خصوصی و تاریخچه گفتگو
│   │   ├── GroupController.cs     # مدیریت گروه‌ها و اعضا
│   │   └── FilesController.cs     # آپلود فایل
│   ├── Hubs/
│   │   └── ChatHub.cs             # هاب SignalR برای چت بلادرنگ
│   ├── Data/
│   │   └── ChatDbContext.cs       # DbContext و پیکربندی EF Core
│   ├── Migrations/                # Migrationهای دیتابیس
│   ├── wwwroot/                   # رابط کاربری وب (HTML/CSS/JS)
│   ├── Program.cs                 # نقطه شروع و پیکربندی سرویس‌ها
│   ├── appsettings.sample.json    # نمونه فایل تنظیمات (برای اجرای با .NET)
│   └── Server.csproj
├── Shared/
│   ├── Models/                    # Entityها: User, Message, Group, GroupMember,
│   │   │                          #   FileAttachment, RefreshToken, ...
│   │   └── DTOs/                  # DTOها: LoginDto, RegisterDto, MessageDto, ...
│   └── Shared.csproj
├── .env.example                   # نمونه متغیرهای محیطی (برای اجرای با Docker)
├── docker-compose.yml             # اجرای برنامه + SQL Server با Docker
├── Dockerfile                     # ایمیج چندمرحله‌ای برنامه
└── CorporateChat.slnx
```

### نقاط مهم معماری

- **احراز هویت**: توکن JWT با کلید متقارن امضا می‌شود و برای اتصال SignalR نیز از طریق `access_token` در Query String پشتیبانی می‌شود.
- **دیتابیس**: هنگام اجرا، برنامه به‌صورت خودکار `db.Database.Migrate()` را اجرا می‌کند؛ بنابراین نیازی به ساخت دستی دیتابیس نیست (کافی است SQL Server در دسترس باشد).
- **CORS**: دامنه‌های مجاز از طریق تنظیمات `Cors:AllowedOrigins` کنترل می‌شوند.
- **فایل‌ها**: مسیر آپلود و حداکثر حجم فایل از طریق `FileStorage` قابل تنظیم است.
- **Swagger**: پس از اجرا در مسیر `/swagger` در دسترس است.
- **API اصلی**:

| مسیر | توضیح |
|---|---|
| `POST /api/auth/register` , `login` , `refresh` | احراز هویت |
| `GET /api/users` | لیست کاربران |
| `GET /api/messages/conversation` , `POST /api/messages` | چت خصوصی |
| `GET/POST/DELETE /api/group/...` | مدیریت گروه‌ها و اعضا |
| `POST /api/files/upload` | آپلود فایل |
| `/hubs/chat` | هاب SignalR |

---

## بخش دوم: نصب و راه‌اندازی

پروژه به دو روش قابل اجراست. در هر روش فقط به **یک فایل تنظیمات** نیاز دارید:

| روش | فایل نمونه | فایل نهایی که باید بسازید |
|---|---|---|
| Docker | `.env.example` | `.env` (در ریشه پروژه) |
| .NET مستقیم | `Server/appsettings.sample.json` | `Server/appsettings.json` |

> نکته: در روش Docker نیازی به `appsettings.json` نیست (همه تنظیمات از `.env` به کانتینر تزریق می‌شود) و در روش .NET نیازی به `.env` نیست.

### روش اول: اجرا با Docker (پیشنهادی)

#### پیش‌نیازها

- Docker Desktop (یا Docker Engine) + Docker Compose

#### 1. ساخت فایل `.env`

فایل `.env.example` را در ریشه پروژه به `.env` کپی کنید:

```bash
# Linux / macOS
cp .env.example .env

# Windows (PowerShell)
Copy-Item .env.example .env
```

#### 2. تنظیم `.env`

فایل `.env` را باز کرده و مقادیر را متناسب با محیط خود تنظیم کنید:

| متغیر | توضیح | مقدار پیش‌فرض |
|---|---|---|
| `APP_PORT` | پورت برنامه روی سیستم شما | `8080` |
| `SQLSERVER_PORT` | پورت SQL Server روی سیستم شما | `1433` |
| `ASPNETCORE_ENVIRONMENT` | محیط اجرا (`Production` / `Development`) | `Production` |
| `SQLSERVER_SA_PASSWORD` | رمز کاربر `sa` — **حتماً تغییر دهید** (باید شامل حرف بزرگ، کوچک، عدد و کاراکتر خاص باشد) | — |
| `JWT_KEY` | کلید امضای JWT — **حتماً تغییر دهید** (حداقل ۶۴ کاراکتر تصادفی) | — |
| `JWT_ISSUER` / `JWT_AUDIENCE` | صادرکننده و مخاطب توکن | `CorporateChat` / `CorporateChatUsers` |
| `JWT_EXPIRE_MINUTES` | مدت اعتبار Access Token (دقیقه) | `60` |
| `JWT_REFRESH_TOKEN_EXPIRE_DAYS` | مدت اعتبار Refresh Token (روز) | `7` |
| `CORS_ORIGIN_0` | آدرس فرانت‌اند مجاز | `http://localhost:8080` |
| `UPLOAD_PATH` | مسیر ذخیره فایل‌ها داخل کانتینر | `/app/data/uploads` |
| `MAX_FILE_SIZE_MB` | حداکثر حجم فایل آپلودی (مگابایت) | `50` |

#### 3. اجرا

```bash
docker compose up --build -d
```

این دستور دو کانتینر بالا می‌آورد:

- `corporatechat-sqlserver`: دیتابیس SQL Server 2022 (با Volume دائمی)
- `corporatechat-server`: برنامه اصلی (بعد از سالم شدن دیتابیس اجرا می‌شود و Migrationها را خودکار اعمال می‌کند)

#### 4. دسترسی

- رابط کاربری: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

#### دستورات مفید

```bash
docker compose logs -f server   # مشاهده لاگ برنامه
docker compose down             # توقف (داده‌ها حفظ می‌شود)
docker compose down -v          # توقف + حذف کامل داده‌ها
```

### روش دوم: اجرا مستقیم با .NET

#### پیش‌نیازها

- .NET 8 SDK
- یک نمونه SQL Server در دسترس (لوکال، Docker یا ریموت)

#### 1. ساخت فایل `appsettings.json`

فایل `Server/appsettings.sample.json` را به `Server/appsettings.json` کپی کنید:

```bash
# Linux / macOS
cp Server/appsettings.sample.json Server/appsettings.json

# Windows (PowerShell)
Copy-Item Server\appsettings.sample.json Server\appsettings.json
```

#### 2. تنظیم `appsettings.json`

فایل را باز کرده و بخش‌های زیر را متناسب با محیط خود تنظیم کنید:

- **`ConnectionStrings:DefaultConnection`**: آدرس، نام دیتابیس، نام کاربری و رمز SQL Server خودتان را وارد کنید. مثال:

  ```json
  "DefaultConnection": "Server=localhost,1433;Database=CorporateChatDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True"
  ```

- **`Jwt:Key`**: یک کلید تصادفی حداقل ۶۴ کاراکتری قرار دهید. سایر مقادیر (`Issuer`، `Audience`، `ExpireMinutes`، `RefreshTokenExpireDays`) را در صورت نیاز تغییر دهید.
- **`Cors:AllowedOrigins`**: آدرس‌هایی که اجازه فراخوانی API را دارند.
- **`FileStorage`**: مسیر آپلود (پیش‌فرض `wwwroot/uploads`) و حداکثر حجم فایل.

#### 3. اجرا

```bash
dotnet restore Server/Server.csproj
dotnet build Server/Server.csproj
dotnet run --project Server/Server.csproj
```

Migrationهای دیتابیس هنگام اجرا به‌صورت خودکار اعمال می‌شوند و نیازی به اجرای دستی `dotnet ef database update` نیست.

#### 4. دسترسی

آدرس اجرا در خروجی ترمینال نمایش داده می‌شود (مثلاً `http://localhost:5000`):

- رابط کاربری: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

---

## نکات امنیتی

- فایل‌های `.env` و `Server/appsettings.json` حاوی اطلاعات حساس هستند و نباید در Git کامیت شوند.
- برای `JWT_KEY` حتماً یک مقدار طولانی و تصادفی (حداقل ۶۴ کاراکتر) استفاده کنید.
- رمز پیش‌فرض `sa` را در محیط واقعی حتماً تغییر دهید.
- Connection String و Secretهای واقعی را عمومی نکنید.
