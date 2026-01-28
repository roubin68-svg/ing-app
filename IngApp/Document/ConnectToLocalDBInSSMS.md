# راهنمای اتصال به LocalDB در SQL Server Management Studio (SSMS)

## مشکل
دیتابیس `IngApp_Local` روی **LocalDB** ایجاد شده است، اما در SSMS به SQL Server اصلی متصل هستید.

## راه حل: اتصال به LocalDB در SSMS

### روش 1: استفاده از Server Name
1. در SSMS، روی **Connect** کلیک کنید
2. در فیلد **Server name** این را وارد کنید:
   ```
   (localdb)\MSSQLLocalDB
   ```
3. Authentication را روی **Windows Authentication** بگذارید
4. روی **Connect** کلیک کنید

### روش 2: استفاده از Browse
1. در SSMS، روی **Connect** کلیک کنید
2. روی دکمه **Browse for more...** کنار Server name کلیک کنید
3. در تب **Local Servers** → **Database Engines** → **LocalDB** را پیدا کنید
4. `MSSQLLocalDB` را انتخاب کنید
5. روی **OK** کلیک کنید
6. Authentication را روی **Windows Authentication** بگذارید
7. روی **Connect** کلیک کنید

### بعد از اتصال
بعد از اتصال به LocalDB، باید دیتابیس `IngApp_Local` را در لیست Databases ببینید.

## اگر می‌خواهید روی SQL Server اصلی کار کنید

اگر ترجیح می‌دهید دیتابیس را روی SQL Server اصلی (که الان در SSMS به آن متصل هستید) ایجاد کنید:

### مرحله 1: پیدا کردن نام دقیق SQL Server Instance
در SSMS، در Object Explorer، نام Server را ببینید. معمولاً به این شکل است:
- `DESKTOP-80LL6BQ\SQLEXPRESS`
- `DESKTOP-80LL6BQ\Ali`
- یا فقط `DESKTOP-80LL6BQ`

### مرحله 2: تغییر Connection String
در فایل `appsettings.Development.json`، Connection String را تغییر دهید:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-80LL6BQ\\SQLEXPRESS;Database=IngApp_Local;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=300;"
  }
}
```

**نکته:** نام Server را با نام دقیق SQL Server خودتان جایگزین کنید.

### مرحله 3: ایجاد دیتابیس
در SSMS، این SQL را اجرا کنید:

```sql
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IngApp_Local')
BEGIN
    CREATE DATABASE [IngApp_Local]
    COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO
```

### مرحله 4: اجرای Migration
در PowerShell:

```powershell
cd C:\Projects\Ing\IngApp\IngApp.Infrastructure
dotnet ef database update --startup-project ../IngApp.Api/IngApp.Api.csproj --context AppDbContext
```

## بررسی وضعیت LocalDB

برای بررسی وضعیت LocalDB در PowerShell:

```powershell
sqllocaldb info MSSQLLocalDB
sqllocaldb start MSSQLLocalDB  # اگر متوقف است
```

## نکات مهم

- **LocalDB** یک نسخه سبک SQL Server است که برای Development استفاده می‌شود
- LocalDB فقط روی همان کامپیوتر قابل دسترسی است
- برای Production باید از SQL Server Full استفاده کنید
- اگر LocalDB را نمی‌بینید، ممکن است نیاز به نصب SQL Server LocalDB داشته باشید



