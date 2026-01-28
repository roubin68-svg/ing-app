# راهنمای راه‌اندازی دیتابیس محلی

## مراحل راه‌اندازی

### 1. ایجاد دیتابیس محلی

دو روش دارید:

#### روش 1: استفاده از SQL Server Management Studio (SSMS)

1. SSMS را باز کنید
2. به `(localdb)\MSSQLLocalDB` یا `.\SQLEXPRESS` وصل شوید
3. فایل `CreateLocalDatabase.sql` را اجرا کنید

یا دستی این SQL را اجرا کنید:

```sql
CREATE DATABASE [IngApp_Local]
COLLATE SQL_Latin1_General_CP1_CI_AS;
```

#### روش 2: استفاده از dotnet CLI (خودکار)

```powershell
# اجرای Migration که خودش دیتابیس را ایجاد می‌کند
cd C:\Projects\Ing\IngApp\IngApp.Infrastructure
dotnet ef database update --startup-project ../IngApp.Api/IngApp.Api.csproj --context AppDbContext
```

### 2. بررسی Connection String

Connection String در `appsettings.Development.json` تنظیم شده است:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=IngApp_Local;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=300;"
  }
}
```

**نکات مهم:**
- `(localdb)\\MSSQLLocalDB` برای LocalDB است
- اگر SQL Server Express دارید، از `.\SQLEXPRESS` استفاده کنید
- اگر SQL Server Full دارید، از `localhost` یا `.` استفاده کنید
- `Trusted_Connection=True` یعنی از Windows Authentication استفاده می‌کند

### 3. تغییر Connection String (در صورت نیاز)

اگر SQL Server Express دارید:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=IngApp_Local;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=300;"
```

اگر SQL Server Full دارید:
```json
"DefaultConnection": "Server=localhost;Database=IngApp_Local;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=300;"
```

اگر از SQL Authentication استفاده می‌کنید:
```json
"DefaultConnection": "Server=localhost;Database=IngApp_Local;User Id=sa;Password=YourPassword;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=300;"
```

### 4. اجرای Migration ها

بعد از ایجاد دیتابیس، Migration ها را اجرا کنید:

```powershell
cd C:\Projects\Ing\IngApp\IngApp.Infrastructure
dotnet ef database update --startup-project ../IngApp.Api/IngApp.Api.csproj --context AppDbContext
```

### 5. تست اتصال

برای تست اتصال، می‌توانید:

1. پروژه را Run کنید: `dotnet run` در `IngApp.Api`
2. اگر خطایی نبود، یعنی اتصال برقرار است
3. در SSMS می‌توانید دیتابیس `IngApp_Local` را ببینید

### 6. بازگشت به دیتابیس سرور (بعداً)

وقتی می‌خواهید دوباره به دیتابیس سرور وصل شوید:

1. `appsettings.Development.json` را به حالت قبلی برگردانید
2. یا Connection String را در `appsettings.json` تغییر دهید

## عیب‌یابی

### خطا: Cannot open database

**راه حل:** دیتابیس را دستی ایجاد کنید (مرحله 1)

### خطا: Login failed

**راه حل:** 
- از `Trusted_Connection=True` استفاده کنید (Windows Authentication)
- یا User Id و Password صحیح را وارد کنید

### خطا: Server not found

**راه حل:**
- نام Server را بررسی کنید
- LocalDB را فعال کنید: `sqllocaldb start MSSQLLocalDB`
- یا از SQL Server Express استفاده کنید



