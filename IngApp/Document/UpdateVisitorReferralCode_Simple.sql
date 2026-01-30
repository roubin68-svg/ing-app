-- ============================================
-- به‌روزرسانی ساده کد معرف Visitor به فرمت جدید (4 کاراکتر)
-- ============================================
-- این اسکریپت کد معرف Visitor موجود را به یک کد 4 کاراکتری جدید تغییر می‌دهد
-- 
-- نحوه استفاده:
-- 1. شماره موبایل Visitor را در خط 10 وارد کنید
-- 2. کد معرف جدید را در خط 11 وارد کنید (مثال: A1B2, K3M5, 7H9L)
-- 3. اسکریپت را اجرا کنید

-- ============================================
-- تنظیمات
-- ============================================
DECLARE @PhoneNumber NVARCHAR(20) = '09123823632'; -- شماره موبایل Visitor
DECLARE @NewReferralCode NVARCHAR(10) = 'A1B2'; -- کد معرف جدید (4 کاراکتر)

-- ============================================
-- اجرای به‌روزرسانی
-- ============================================
BEGIN TRANSACTION;

BEGIN TRY
    -- بررسی اینکه Visitor وجود دارد
    IF NOT EXISTS (
        SELECT 1 
        FROM VisitorProfiles vp
        INNER JOIN Users u ON vp.UserId = u.Id
        WHERE u.PhoneNumber = @PhoneNumber
    )
    BEGIN
        THROW 50001, N'Visitor با شماره موبایل مورد نظر یافت نشد.', 1;
    END

    -- بررسی اینکه کد معرف جدید تکراری نیست
    IF EXISTS (
        SELECT 1 
        FROM VisitorProfiles vp
        INNER JOIN Users u ON vp.UserId = u.Id
        WHERE vp.ReferralCode = @NewReferralCode 
        AND u.PhoneNumber != @PhoneNumber
    )
    BEGIN
        THROW 50002, N'کد معرف "' + @NewReferralCode + '" قبلاً استفاده شده است. لطفاً کد دیگری انتخاب کنید.', 1;
    END

    -- بررسی طول کد معرف
    IF LEN(@NewReferralCode) != 4
    BEGIN
        THROW 50003, N'کد معرف باید دقیقاً 4 کاراکتر باشد.', 1;
    END

    -- به‌روزرسانی کد معرف
    UPDATE vp
    SET 
        vp.ReferralCode = @NewReferralCode,
        vp.UpdatedAt = GETUTCDATE()
    FROM VisitorProfiles vp
    INNER JOIN Users u ON vp.UserId = u.Id
    WHERE u.PhoneNumber = @PhoneNumber;

    -- نمایش نتیجه
    SELECT 
        u.PhoneNumber AS 'شماره موبایل',
        u.DisplayName AS 'نام',
        vp.ReferralCode AS 'کد معرف جدید',
        vp.UpdatedAt AS 'تاریخ به‌روزرسانی'
    FROM VisitorProfiles vp
    INNER JOIN Users u ON vp.UserId = u.Id
    WHERE u.PhoneNumber = @PhoneNumber;

    PRINT N'کد معرف Visitor با موفقیت به "' + @NewReferralCode + '" تغییر یافت.';
    
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT N'خطا: ' + @ErrorMessage;
    THROW;
END CATCH;

-- ============================================
-- مثال‌های کد معرف 4 کاراکتری:
-- ============================================
-- A1B2  (حرف-عدد-حرف-عدد)
-- K3M5  (حرف-عدد-حرف-عدد)
-- 7H9L  (عدد-حرف-عدد-حرف)
-- 2C4D  (عدد-حرف-عدد-حرف)
-- B5A7  (حرف-عدد-حرف-عدد)
-- 
-- نکته: از حروف I, O, Q استفاده نکنید (ممکن است با اعداد 1, 0 اشتباه شوند)












