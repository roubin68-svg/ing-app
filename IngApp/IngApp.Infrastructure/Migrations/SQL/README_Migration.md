# راهنمای اجرای Migration: AddOfferClickLogs

## روش 1: استفاده از Entity Framework (پیشنهادی)

### گام 1: ساخت Migration
در PowerShell یا Command Prompt، به مسیر پروژه `IngApp.Infrastructure` بروید و دستور زیر را اجرا کنید:

```powershell
cd C:\Projects\Ing\IngApp\IngApp.Infrastructure
dotnet ef migrations add AddOfferClickLogs --startup-project ..\IngApp.Api\IngApp.Api.csproj --context AppDbContext
```

**نکته:** اگر migration با نام دیگری ساخته شد، مشکلی نیست. فقط مطمئن شوید که فایل migration درست ساخته شده است.

### گام 2: اجرای Migration
بعد از ساخت migration، برای اعمال تغییرات در دیتابیس:

```powershell
dotnet ef database update --startup-project ..\IngApp.Api\IngApp.Api.csproj --context AppDbContext
```

---

## روش 2: اجرای مستقیم SQL Script (اگر EF مشکل داشت)

### گام 1: باز کردن SQL Server Management Studio (SSMS)

### گام 2: اتصال به دیتابیس
- Server: `wh031.irandns.com,2019`
- Database: `AliHoor_IngApp`
- Authentication: SQL Server Authentication
- Username: `IngAppUser`
- Password: `VJXvcbzgh85n0%k!`

### گام 3: اجرای Script
1. فایل `AddOfferClickLogs.sql` را باز کنید
2. محتوای آن را در SSMS کپی کنید
3. دکمه Execute (F5) را بزنید

**یا** می‌توانید مستقیماً این دستورات را اجرا کنید:

```sql
USE [AliHoor_IngApp]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[IngAppUser].[OfferClickLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [IngAppUser].[OfferClickLogs] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OfferId] int NOT NULL,
        [UserId] uniqueidentifier NULL,
        [ClickType] int NOT NULL,
        [IpAddress] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [ClickedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_OfferClickLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OfferClickLogs_Offers_OfferId] FOREIGN KEY ([OfferId]) 
            REFERENCES [IngAppUser].[Offers] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_OfferClickLogs_OfferId_ClickType] 
        ON [IngAppUser].[OfferClickLogs] ([OfferId], [ClickType]);
    
    CREATE INDEX [IX_OfferClickLogs_ClickedAt] 
        ON [IngAppUser].[OfferClickLogs] ([ClickedAt]);
END
GO
```

---

## بررسی موفقیت Migration

بعد از اجرای migration، می‌توانید با این query بررسی کنید که جدول درست ساخته شده است:

```sql
SELECT * FROM [IngAppUser].[OfferClickLogs]
```

اگر جدول وجود داشت و ساختار آن درست بود، migration موفق بوده است.

---

## توضیحات فیلدها

- **Id**: شناسه یکتا (Primary Key, Identity)
- **OfferId**: شناسه آگهی (Foreign Key به Offers)
- **UserId**: شناسه کاربری که کلیک کرده (می‌تواند NULL باشد برای بازدیدهای بدون لاگین)
- **ClickType**: نوع کلیک (1 = View, 2 = ContactClick)
- **IpAddress**: آدرس IP کاربر (اختیاری)
- **UserAgent**: User Agent مرورگر (اختیاری)
- **ClickedAt**: زمان کلیک

---

## در صورت بروز خطا

اگر خطایی رخ داد:
1. مطمئن شوید که جدول `Offers` وجود دارد
2. مطمئن شوید که schema `IngAppUser` وجود دارد
3. بررسی کنید که دسترسی‌های لازم را دارید

