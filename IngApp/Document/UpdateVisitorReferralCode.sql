-- ============================================
-- به‌روزرسانی کد معرف Visitor به فرمت جدید (4 کاراکتر)
-- ============================================
-- این اسکریپت کد معرف Visitor موجود را به فرمت جدید (4 کاراکتر) تغییر می‌دهد
-- فرمت جدید: حرف-عدد-حرف-عدد یا عدد-حرف-عدد-حرف (مثال: A1B2 یا 1A2B)

-- ============================================
-- مثال 1: به‌روزرسانی کد معرف برای Visitor خاص (با استفاده از PhoneNumber)
-- ============================================
-- در این مثال، کد معرف برای Visitor با شماره موبایل 09123823632 به "A1B2" تغییر می‌یابد
-- شما می‌توانید این مقدار را به کد مورد نظر خود تغییر دهید

DECLARE @VisitorId UNIQUEIDENTIFIER;
DECLARE @NewReferralCode NVARCHAR(10) = 'A1B2'; -- کد معرف جدید (4 کاراکتر)

-- پیدا کردن Visitor بر اساس PhoneNumber
SELECT @VisitorId = vp.Id
FROM VisitorProfiles vp
INNER JOIN Users u ON vp.UserId = u.Id
WHERE u.PhoneNumber = '09123823632'; -- شماره موبایل Visitor

-- بررسی اینکه آیا کد معرف جدید تکراری نیست
IF EXISTS (SELECT 1 FROM VisitorProfiles WHERE ReferralCode = @NewReferralCode AND Id != @VisitorId)
BEGIN
    PRINT 'خطا: کد معرف "' + @NewReferralCode + '" قبلاً استفاده شده است. لطفاً کد دیگری انتخاب کنید.';
END
ELSE IF @VisitorId IS NOT NULL
BEGIN
    -- به‌روزرسانی کد معرف
    UPDATE VisitorProfiles
    SET ReferralCode = @NewReferralCode,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @VisitorId;
    
    PRINT 'کد معرف Visitor با موفقیت به "' + @NewReferralCode + '" تغییر یافت.';
    
    -- نمایش اطلاعات به‌روز شده
    SELECT 
        u.PhoneNumber AS 'شماره موبایل',
        u.DisplayName AS 'نام',
        vp.ReferralCode AS 'کد معرف جدید',
        vp.UpdatedAt AS 'تاریخ به‌روزرسانی'
    FROM VisitorProfiles vp
    INNER JOIN Users u ON vp.UserId = u.Id
    WHERE vp.Id = @VisitorId;
END
ELSE
BEGIN
    PRINT 'خطا: Visitor با شماره موبایل مورد نظر یافت نشد.';
END

-- ============================================
-- مثال 2: به‌روزرسانی کد معرف برای همه Visitor ها با کدهای قدیمی (8 کاراکتری)
-- ============================================
-- این بخش کدهای معرف قدیمی (8 کاراکتری) را به کدهای جدید 4 کاراکتری تغییر می‌دهد
-- توجه: این بخش به صورت خودکار کد تولید نمی‌کند، فقط مثال است

/*
-- لیست Visitor هایی که کد معرف 8 کاراکتری دارند
SELECT 
    vp.Id,
    vp.ReferralCode AS 'کد قدیمی',
    u.PhoneNumber,
    u.DisplayName
FROM VisitorProfiles vp
INNER JOIN Users u ON vp.UserId = u.Id
WHERE LEN(vp.ReferralCode) = 8;

-- برای هر Visitor، باید کد جدید را به صورت دستی تنظیم کنید
-- مثال:
UPDATE VisitorProfiles
SET ReferralCode = 'K3M5',  -- کد جدید 4 کاراکتری
    UpdatedAt = GETUTCDATE()
WHERE Id = 'AA086B57-59FE-455F-80EC-6F2BD2EC404D'; -- شناسه Visitor
*/

-- ============================================
-- مثال 3: تولید کد معرف جدید برای Visitor خاص
-- ============================================
-- این بخش یک کد معرف جدید 4 کاراکتری تولید می‌کند و آن را به Visitor اختصاص می‌دهد
-- توجه: این کد ممکن است تکراری باشد، بنابراین باید بررسی کنید

/*
DECLARE @VisitorId2 UNIQUEIDENTIFIER = 'AA086B57-59FE-455F-80EC-6F2BD2EC404D'; -- شناسه Visitor
DECLARE @NewCode NVARCHAR(10);
DECLARE @Counter INT = 0;
DECLARE @MaxAttempts INT = 100;

-- تولید کد جدید تا زمانی که یکتا باشد
WHILE @Counter < @MaxAttempts
BEGIN
    -- الگوی: حرف-عدد-حرف-عدد (مثال: A1B2)
    SET @NewCode = 
        CHAR(65 + ABS(CHECKSUM(NEWID())) % 22) + -- حرف اول (A-Z بدون I, O, Q)
        CAST(ABS(CHECKSUM(NEWID())) % 10 AS NVARCHAR(1)) + -- عدد اول (0-9)
        CHAR(65 + ABS(CHECKSUM(NEWID())) % 22) + -- حرف دوم
        CAST(ABS(CHECKSUM(NEWID())) % 10 AS NVARCHAR(1)); -- عدد دوم
    
    -- بررسی یکتایی
    IF NOT EXISTS (SELECT 1 FROM VisitorProfiles WHERE ReferralCode = @NewCode AND Id != @VisitorId2)
    BEGIN
        UPDATE VisitorProfiles
        SET ReferralCode = @NewCode,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @VisitorId2;
        
        PRINT 'کد معرف جدید تولید شد: ' + @NewCode;
        BREAK;
    END
    
    SET @Counter = @Counter + 1;
END

IF @Counter >= @MaxAttempts
BEGIN
    PRINT 'خطا: امکان تولید کد معرف یکتا وجود ندارد.';
END
*/

-- ============================================
-- نکات مهم:
-- ============================================
-- 1. قبل از اجرای UPDATE، حتماً کد معرف جدید را بررسی کنید که تکراری نباشد
-- 2. کد معرف باید دقیقاً 4 کاراکتر باشد
-- 3. فرمت پیشنهادی: حرف-عدد-حرف-عدد (مثال: A1B2) یا عدد-حرف-عدد-حرف (مثال: 1A2B)
-- 4. از حروف I, O, Q استفاده نکنید (ممکن است با اعداد 1, 0 اشتباه شوند)
-- 5. کدهای نمونه: A1B2, K3M5, 7H9L, 2C4D, B5A7


