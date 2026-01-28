-- اضافه کردن Permission های Visitor به دیتابیس
-- این اسکریپت را در SSMS اجرا کنید

USE [IngApp_Local];
GO

-- بررسی اینکه آیا Permission ها از قبل وجود دارند
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'Visitor.View')
BEGIN
    INSERT INTO [Permissions] ([Id], [Code], [DisplayName], [IsActive])
    VALUES ('aaaaaaaa-0000-0000-0000-00000000000c', 'Visitor.View', N'مشاهده بازاریاب‌ها', 1);
    PRINT 'Permission Visitor.View اضافه شد';
END
ELSE
BEGIN
    PRINT 'Permission Visitor.View از قبل وجود دارد';
END
GO

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'Visitor.Manage')
BEGIN
    INSERT INTO [Permissions] ([Id], [Code], [DisplayName], [IsActive])
    VALUES ('aaaaaaaa-0000-0000-0000-00000000000d', 'Visitor.Manage', N'مدیریت بازاریاب‌ها', 1);
    PRINT 'Permission Visitor.Manage اضافه شد';
END
ELSE
BEGIN
    PRINT 'Permission Visitor.Manage از قبل وجود دارد';
END
GO

-- اضافه کردن Permission ها به Admin Role (اگر Admin role وجود دارد)
-- ابتدا باید Admin Role ID را پیدا کنیم
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
SELECT @AdminRoleId = [Id] FROM [Roles] WHERE [Name] = 'Admin';

IF @AdminRoleId IS NOT NULL
BEGIN
    -- اضافه کردن Visitor.View به Admin
    IF NOT EXISTS (SELECT 1 FROM [RolePermissions] WHERE [RoleId] = @AdminRoleId AND [PermissionId] = 'aaaaaaaa-0000-0000-0000-00000000000c')
    BEGIN
        INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
        VALUES (@AdminRoleId, 'aaaaaaaa-0000-0000-0000-00000000000c');
        PRINT 'Permission Visitor.View به Admin Role اضافه شد';
    END

    -- اضافه کردن Visitor.Manage به Admin
    IF NOT EXISTS (SELECT 1 FROM [RolePermissions] WHERE [RoleId] = @AdminRoleId AND [PermissionId] = 'aaaaaaaa-0000-0000-0000-00000000000d')
    BEGIN
        INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
        VALUES (@AdminRoleId, 'aaaaaaaa-0000-0000-0000-00000000000d');
        PRINT 'Permission Visitor.Manage به Admin Role اضافه شد';
    END
END
ELSE
BEGIN
    PRINT 'Admin Role یافت نشد. لطفاً دستی Permission ها را به Admin Role اضافه کنید.';
END
GO

-- بررسی نتیجه
SELECT 
    p.[Code],
    p.[DisplayName],
    p.[IsActive],
    CASE WHEN rp.[RoleId] IS NOT NULL THEN 'دارد' ELSE 'ندارد' END AS [در Admin Role]
FROM [Permissions] p
LEFT JOIN [RolePermissions] rp ON p.[Id] = rp.[PermissionId] 
    AND rp.[RoleId] = (SELECT [Id] FROM [Roles] WHERE [Name] = 'Admin')
WHERE p.[Code] IN ('Visitor.View', 'Visitor.Manage');
GO



