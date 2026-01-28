-- اضافه کردن آیتم‌های منو برای سیستم مالی
-- این اسکریپت باید در دیتابیس IngAppUser اجرا شود

-- ============================================
-- 1. آیتم والد: "سیستم مالی"
-- ============================================
DECLARE @FinancialParentId INT;

INSERT INTO [IngAppUser].[MenuItems] 
    ([Key], [Title], [Route], [Icon], [ParentId], [Order], [IsActive], [RequiredPermissionCode])
VALUES 
    ('financial', N'سیستم مالی', '#', 'WalletOutlined', NULL, 7, 1, NULL);

SET @FinancialParentId = SCOPE_IDENTITY();

-- ============================================
-- 2. آیتم‌های فرزند زیر "سیستم مالی"
-- ============================================

-- 2.1 کیف پول
INSERT INTO [IngAppUser].[MenuItems] 
    ([Key], [Title], [Route], [Icon], [ParentId], [Order], [IsActive], [RequiredPermissionCode])
VALUES 
    ('wallet', N'کیف پول', '/wallet', NULL, @FinancialParentId, 1, 1, NULL);

-- 2.2 شارژ کیف پول (زیرمجموعه کیف پول - اختیاری)
-- اگر می‌خواهید شارژ کیف پول زیرمجموعه کیف پول باشد، از این استفاده کنید:
-- DECLARE @WalletId INT;
-- SET @WalletId = SCOPE_IDENTITY();
-- INSERT INTO [IngAppUser].[MenuItems] 
--     ([Key], [Title], [Route], [Icon], [ParentId], [Order], [IsActive], [RequiredPermissionCode])
-- VALUES 
--     ('wallet-topup', N'شارژ کیف پول', '/payments/topup', NULL, @WalletId, 1, 1, NULL);

-- 2.3 اشتراک‌ها
INSERT INTO [IngAppUser].[MenuItems] 
    ([Key], [Title], [Route], [Icon], [ParentId], [Order], [IsActive], [RequiredPermissionCode])
VALUES 
    ('subscriptions', N'اشتراک‌ها', '/subscriptions', NULL, @FinancialParentId, 2, 1, NULL);

-- 2.4 پورسانت‌ها (فقط برای Visitor)
INSERT INTO [IngAppUser].[MenuItems] 
    ([Key], [Title], [Route], [Icon], [ParentId], [Order], [IsActive], [RequiredPermissionCode])
VALUES 
    ('commissions', N'پورسانت‌ها', '/commissions', NULL, @FinancialParentId, 3, 1, NULL);

-- ============================================
-- توضیحات:
-- ============================================
-- 1. Order = 7 برای آیتم والد "سیستم مالی" انتخاب شده است
--    (بعد از Settings که Order = 6 است)
--
-- 2. RequiredPermissionCode = NULL برای همه آیتم‌ها تنظیم شده است
--    اگر می‌خواهید دسترسی را محدود کنید، می‌توانید Permission Code اضافه کنید:
--    - Wallet.ViewOwn
--    - Subscription.View
--    - Commission.ViewOwn
--
-- 3. اگر می‌خواهید "شارژ کیف پول" زیرمجموعه "کیف پول" باشد،
--    کدهای مربوطه را از حالت کامنت خارج کنید.
--
-- 4. برای Visitor ها، می‌توانید RequiredPermissionCode را به 
--    'Commission.ViewOwn' تغییر دهید تا فقط Visitor ها ببینند.

