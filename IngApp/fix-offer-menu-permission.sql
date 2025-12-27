-- ===========================================
-- اسکریپت برای رفع مشکل دسترسی منوی آگهی
-- ===========================================
-- این اسکریپت:
-- 1. Permission جدید "Offer.Manage" را اضافه می‌کند (اگر وجود نداشته باشد)
-- 2. این Permission را به نقش‌های Admin و Supplier می‌دهد
-- 3. منوی آگهی را پیدا می‌کند و RequiredPermissionCode را تنظیم می‌کند

-- ===========================================
-- 1. اضافه کردن Permission جدید
-- ===========================================
IF NOT EXISTS (SELECT 1 FROM IngAppUser.Permissions WHERE Code = 'Offer.Manage')
BEGIN
    INSERT INTO IngAppUser.Permissions (Id, Code, DisplayName, IsActive)
    VALUES ('aaaaaaaa-0000-0000-0000-00000000000b', 'Offer.Manage', N'مدیریت آگهی‌ها', 1);
END
GO

-- ===========================================
-- 2. اضافه کردن Permission به نقش Admin
-- ===========================================
DECLARE @AdminRoleId UNIQUEIDENTIFIER = 'a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1';
DECLARE @OfferManagePermissionId UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-00000000000b';

IF NOT EXISTS (SELECT 1 FROM IngAppUser.RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @OfferManagePermissionId)
BEGIN
    INSERT INTO IngAppUser.RolePermissions (RoleId, PermissionId)
    VALUES (@AdminRoleId, @OfferManagePermissionId);
END
GO

-- ===========================================
-- 3. اضافه کردن Permission به نقش Supplier
-- ===========================================
DECLARE @SupplierRoleId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @OfferManagePermissionId2 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-00000000000b';

IF NOT EXISTS (SELECT 1 FROM IngAppUser.RolePermissions WHERE RoleId = @SupplierRoleId AND PermissionId = @OfferManagePermissionId2)
BEGIN
    INSERT INTO IngAppUser.RolePermissions (RoleId, PermissionId)
    VALUES (@SupplierRoleId, @OfferManagePermissionId2);
END
GO

-- ===========================================
-- 4. پیدا کردن منوی آگهی و تنظیم RequiredPermissionCode
-- ===========================================
-- منوی آگهی ممکن است با Route "/my-offers" یا "my-offers" یا Key "my-offers" باشد

-- اگر منوی آگهی با Route "/my-offers" وجود دارد
IF EXISTS (SELECT 1 FROM MenuItems WHERE Route = '/my-offers' OR Route = 'my-offers')
BEGIN
    UPDATE MenuItems
    SET RequiredPermissionCode = 'Offer.Manage'
    WHERE Route = '/my-offers' OR Route = 'my-offers';
    
    PRINT N'منوی آگهی پیدا شد و RequiredPermissionCode تنظیم شد (Route)';
END
ELSE IF EXISTS (SELECT 1 FROM MenuItems WHERE [Key] = 'my-offers' OR [Key] = 'offers' OR [Key] = 'offers-management')
BEGIN
    UPDATE MenuItems
    SET RequiredPermissionCode = 'Offer.Manage'
    WHERE [Key] = 'my-offers' OR [Key] = 'offers' OR [Key] = 'offers-management';
    
    PRINT N'منوی آگهی پیدا شد و RequiredPermissionCode تنظیم شد (Key)';
END
ELSE
BEGIN
    -- اگر منوی آگهی وجود ندارد، باید آن را ایجاد کنیم
    -- اما بهتر است از طریق UI یا Migration انجام شود
    PRINT N'هشدار: منوی آگهی پیدا نشد. لطفاً از طریق UI یا Migration ایجاد کنید.';
END
GO

-- ===========================================
-- 5. بررسی نهایی: نمایش منوهای آگهی
-- ===========================================
SELECT 
    Id,
    [Key],
    Title,
    Route,
    RequiredPermissionCode,
    IsActive
FROM MenuItems
WHERE Route LIKE '%offer%' OR [Key] LIKE '%offer%' OR Title LIKE '%آگهی%';
GO

-- ===========================================
-- 6. بررسی: نمایش Permission‌های نقش Supplier
-- ===========================================
SELECT 
    r.Name AS RoleName,
    p.Code AS PermissionCode,
    p.DisplayName AS PermissionDisplayName
FROM IngAppUser.RolePermissions rp
INNER JOIN IngAppUser.Roles r ON rp.RoleId = r.Id
INNER JOIN IngAppUser.Permissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'Supplier' OR r.Name = 'Admin'
ORDER BY r.Name, p.Code;
GO



