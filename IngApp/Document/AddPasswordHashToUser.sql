-- Migration: AddPasswordHashToUser
-- تاریخ: 2026-01-27
-- توضیحات: افزودن فیلد PasswordHash به جدول Users برای Login با Password

-- افزودن ستون PasswordHash
ALTER TABLE [Users]
ADD [PasswordHash] NVARCHAR(500) NULL;

-- توضیحات:
-- این ستون برای ذخیره Hash رمز عبور کاربران استفاده می‌شود
-- NULL است چون کاربران قدیمی ممکن است Password نداشته باشند
-- حداکثر طول 500 کاراکتر برای BCrypt Hash کافی است












