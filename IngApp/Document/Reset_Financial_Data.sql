-- =====================================================
-- Script Reset تمام داده‌های مالی سیستم
-- =====================================================
-- این Script تمام تراکنش‌های مالی، اشتراک‌ها، کیف پول‌ها و پرداخت‌ها را پاک می‌کند
-- برای تست دوباره از اول
-- =====================================================

USE [IngAppDb]; -- نام دیتابیس خود را وارد کنید
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'شروع Reset داده‌های مالی...';
    
    -- =====================================================
    -- 1. پاک کردن Commission Transactions (پورسانت‌ها)
    -- =====================================================
    PRINT 'پاک کردن Commission Transactions...';
    DELETE FROM CommissionTransactions;
    PRINT '✓ Commission Transactions پاک شد';
    
    -- =====================================================
    -- 2. پاک کردن Commission Rules (اختیاری - می‌توانید نگه دارید)
    -- =====================================================
    -- DELETE FROM CommissionRules;
    -- PRINT '✓ Commission Rules پاک شد';
    
    -- =====================================================
    -- 3. پاک کردن Offer Contact Unlocks (باز شدن اطلاعات تماس)
    -- =====================================================
    PRINT 'پاک کردن Offer Contact Unlocks...';
    DELETE FROM OfferContactUnlocks;
    PRINT '✓ Offer Contact Unlocks پاک شد';
    
    -- =====================================================
    -- 4. پاک کردن User Subscriptions (اشتراک‌های کاربران)
    -- =====================================================
    PRINT 'پاک کردن User Subscriptions...';
    DELETE FROM UserSubscriptions;
    PRINT '✓ User Subscriptions پاک شد';
    
    -- =====================================================
    -- 5. پاک کردن Payments (پرداخت‌ها)
    -- =====================================================
    PRINT 'پاک کردن Payments...';
    DELETE FROM Payments;
    PRINT '✓ Payments پاک شد';
    
    -- =====================================================
    -- 6. پاک کردن Wallet Transactions (تراکنش‌های کیف پول)
    -- =====================================================
    PRINT 'پاک کردن Wallet Transactions...';
    DELETE FROM WalletTransactions;
    PRINT '✓ Wallet Transactions پاک شد';
    
    -- =====================================================
    -- 7. Reset کردن موجودی کیف پول‌ها به صفر
    -- =====================================================
    PRINT 'Reset کردن موجودی کیف پول‌ها...';
    UPDATE Wallets 
    SET BalanceRial = 0,
        UpdatedAt = GETUTCDATE();
    PRINT '✓ موجودی کیف پول‌ها Reset شد';
    
    -- =====================================================
    -- 8. بررسی: آیا می‌خواهید کیف پول‌ها را هم پاک کنید؟
    -- =====================================================
    -- اگر می‌خواهید کیف پول‌ها را هم پاک کنید، خط زیر را uncomment کنید:
    -- DELETE FROM Wallets;
    -- PRINT '✓ Wallets پاک شد';
    
    -- =====================================================
    -- 9. بررسی: آیا می‌خواهید Visitor Profiles را Reset کنید؟
    -- =====================================================
    -- اگر می‌خواهید Visitor Profiles را Reset کنید (برای تست Commission):
    -- DELETE FROM VisitorProfiles;
    -- DELETE FROM BuyerProfiles;
    -- PRINT '✓ Visitor Profiles و Buyer Profiles پاک شد';
    
    -- =====================================================
    -- 10. Reset کردن RowVersion برای جلوگیری از Concurrency Issues
    -- =====================================================
    PRINT 'Reset کردن RowVersion...';
    UPDATE Wallets 
    SET RowVersion = CAST(RAND() * 1000000 AS VARBINARY(8));
    PRINT '✓ RowVersion Reset شد';
    
    COMMIT TRANSACTION;
    PRINT '=====================================================';
    PRINT '✓ Reset با موفقیت انجام شد!';
    PRINT '✓ تمام تراکنش‌های مالی، اشتراک‌ها و پرداخت‌ها پاک شدند';
    PRINT '✓ موجودی کیف پول‌ها به صفر Reset شد';
    PRINT '=====================================================';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '=====================================================';
    PRINT '✗ خطا در Reset:';
    PRINT ERROR_MESSAGE();
    PRINT '=====================================================';
    THROW;
END CATCH;
GO

-- =====================================================
-- بررسی: نمایش تعداد رکوردهای باقی‌مانده
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT 'بررسی تعداد رکوردهای باقی‌مانده:';
PRINT '=====================================================';

SELECT 'WalletTransactions' AS TableName, COUNT(*) AS RecordCount FROM WalletTransactions
UNION ALL
SELECT 'UserSubscriptions', COUNT(*) FROM UserSubscriptions
UNION ALL
SELECT 'Payments', COUNT(*) FROM Payments
UNION ALL
SELECT 'OfferContactUnlocks', COUNT(*) FROM OfferContactUnlocks
UNION ALL
SELECT 'CommissionTransactions', COUNT(*) FROM CommissionTransactions
UNION ALL
SELECT 'Wallets', COUNT(*) FROM Wallets;

PRINT '';
PRINT '=====================================================';
PRINT 'Reset کامل شد!';
PRINT '=====================================================';




















