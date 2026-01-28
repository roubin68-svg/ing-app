-- اسکریپت برای بررسی و رفع مشکل PaymentGateways
-- این اسکریپت باید در دیتابیس اصلی (نه IngAppUser) اجرا شود

USE [YourDatabaseName]; -- نام دیتابیس خود را وارد کنید
GO

-- بررسی وجود جدول PaymentGateways
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentGateways' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'جدول PaymentGateways وجود ندارد. لطفاً migration را اجرا کنید.';
    RETURN;
END
GO

-- بررسی وجود داده‌ها
IF NOT EXISTS (SELECT * FROM PaymentGateways)
BEGIN
    PRINT 'در حال اضافه کردن داده‌های اولیه PaymentGateways...';
    
    INSERT INTO PaymentGateways (Id, Code, Title, Description, IsActive)
    VALUES 
        (1, 'Mock', N'درگاه پرداخت آزمایشی', N'درگاه پرداخت Mock برای تست', 1),
        (2, 'Zarinpal', N'زرین‌پال', N'درگاه پرداخت زرین‌پال', 0);
    
    PRINT 'داده‌های اولیه با موفقیت اضافه شد.';
END
ELSE
BEGIN
    PRINT 'داده‌های PaymentGateways از قبل وجود دارند.';
    
    -- نمایش داده‌های موجود
    SELECT * FROM PaymentGateways;
END
GO

-- بررسی PaymentStatuses
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentStatuses' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'جدول PaymentStatuses وجود ندارد. لطفاً migration را اجرا کنید.';
    RETURN;
END
GO

IF NOT EXISTS (SELECT * FROM PaymentStatuses)
BEGIN
    PRINT 'در حال اضافه کردن داده‌های اولیه PaymentStatuses...';
    
    INSERT INTO PaymentStatuses (Id, Code, Title, Description, IsActive)
    VALUES 
        (1, 'Pending', N'در انتظار', N'پرداخت در انتظار است', 1),
        (2, 'Success', N'موفق', N'پرداخت با موفقیت انجام شد', 1),
        (3, 'Failed', N'ناموفق', N'پرداخت ناموفق بود', 1),
        (4, 'Cancelled', N'لغو شده', N'پرداخت لغو شد', 1);
    
    PRINT 'داده‌های اولیه PaymentStatuses با موفقیت اضافه شد.';
END
GO











