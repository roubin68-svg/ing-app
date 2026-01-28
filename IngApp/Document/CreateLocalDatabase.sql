-- ============================================
-- ایجاد دیتابیس محلی برای Development
-- ============================================

-- اگر دیتابیس از قبل وجود دارد، حذف می‌کنیم (اختیاری - فقط برای تست)
-- DROP DATABASE IF EXISTS [IngApp_Local];
-- GO

-- ایجاد دیتابیس جدید
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IngApp_Local')
BEGIN
    CREATE DATABASE [IngApp_Local]
    COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO

-- استفاده از دیتابیس
USE [IngApp_Local];
GO

-- بررسی اینکه دیتابیس ایجاد شد
SELECT name, database_id, create_date 
FROM sys.databases 
WHERE name = 'IngApp_Local';
GO



