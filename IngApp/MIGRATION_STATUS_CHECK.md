# راهنمای بررسی وضعیت Migration ها و تست دسترسی‌ها

## وضعیت فعلی

دو migration داریم که باید به ترتیب اجرا شوند:

1. **AddOfferManagePermission** (20251227014145) - اضافه کردن permission جدید و role-permission assignments
2. **FixRolePermissionRelationship** (20251227020334) - حذف ستون اضافی PermissionId1

## مراحل بررسی و تست

### 1. بررسی وضعیت Migration ها در دیتابیس

در SQL Server Management Studio یا Azure Data Studio، این query را اجرا کنید:

```sql
-- بررسی اینکه کدام migration ها اجرا شده‌اند
SELECT * FROM [__EFMigrationsHistory] 
ORDER BY MigrationId DESC;
```

### 2. اگر migration ها هنوز اعمال نشده‌اند

در Visual Studio:
1. Package Manager Console را باز کنید
2. Startup Project: `IngApp.Api`
3. Default Project: `IngApp.Infrastructure`
4. دستور زیر را اجرا کنید:
   ```
   Update-Database -Context AppDbContext
   ```

### 3. بررسی داده‌های Permission و RolePermission

این query ها را اجرا کنید تا مطمئن شوید داده‌ها درست اضافه شده‌اند:

```sql
-- بررسی Permission جدید
SELECT * FROM [Permissions] 
WHERE Code = 'Offer.Manage';
-- باید یک ردیف با DisplayName = 'مدیریت آگهی‌ها' داشته باشید

-- بررسی RolePermission برای Admin
SELECT rp.*, p.Code AS PermissionCode, r.Name AS RoleName
FROM [RolePermissions] rp
INNER JOIN [Permissions] p ON rp.PermissionId = p.Id
INNER JOIN [IngAppUser].[Roles] r ON rp.RoleId = r.Id
WHERE p.Code = 'Offer.Manage';
-- باید 2 ردیف داشته باشید: یکی برای Admin و یکی برای Supplier

-- بررسی که ستون PermissionId1 حذف شده
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'RolePermissions' AND COLUMN_NAME = 'PermissionId1';
-- نباید هیچ ردیفی برگرداند
```

### 4. تست دسترسی کاربر

برای تست اینکه کاربر با شماره `09352423632` دیگر دسترسی ندارد:

#### روش 1: بررسی مستقیم در دیتابیس

```sql
-- بررسی User و Role
SELECT u.MobileNumber, ur.RoleId, r.Name AS RoleName
FROM [IngAppUser].[Users] u
LEFT JOIN [IngAppUser].[UserRoles] ur ON u.Id = ur.UserId
LEFT JOIN [IngAppUser].[Roles] r ON ur.RoleId = r.Id
WHERE u.MobileNumber = '09352423632';

-- بررسی RolePermission های کاربر
SELECT DISTINCT p.Code AS PermissionCode, p.DisplayName
FROM [IngAppUser].[Users] u
INNER JOIN [IngAppUser].[UserRoles] ur ON u.Id = ur.UserId
INNER JOIN [RolePermissions] rp ON ur.RoleId = rp.RoleId
INNER JOIN [Permissions] p ON rp.PermissionId = p.Id
WHERE u.MobileNumber = '09352423632'
ORDER BY p.Code;
-- نباید 'Offer.Manage' را در لیست ببینید
```

#### روش 2: تست در UI

1. با کاربر `09352423632` لاگین کنید
2. بررسی کنید که منوی "آگهی‌ها" یا "مدیریت آگهی‌ها" نمایش داده نمی‌شود
3. اگر منو نمایش داده می‌شود، مشکل از کش یا JWT token است - باید دوباره لاگین کنید

### 5. بررسی Role IDs

برای اطمینان از Role IDs درست:

```sql
-- Admin Role ID باید: a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1
-- Supplier Role ID باید: 33333333-3333-3333-3333-333333333333
SELECT Id, Name, DisplayName 
FROM [IngAppUser].[Roles]
WHERE Name IN ('Admin', 'Supplier');
```

### 6. اگر مشکل داشتید - اجرای دستی داده‌ها

اگر migration کار نکرد، می‌توانید دستی این داده‌ها را اضافه کنید:

```sql
-- اضافه کردن Permission (اگر وجود ندارد)
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE Code = 'Offer.Manage')
BEGIN
    INSERT INTO [Permissions] (Id, Code, Description, DisplayName, IsActive)
    VALUES ('aaaaaaaa-0000-0000-0000-00000000000b', 'Offer.Manage', '', 'مدیریت آگهی‌ها', 1);
END

-- اضافه کردن RolePermission برای Admin (اگر وجود ندارد)
IF NOT EXISTS (
    SELECT 1 FROM [RolePermissions] 
    WHERE RoleId = 'a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1' 
    AND PermissionId = 'aaaaaaaa-0000-0000-0000-00000000000b'
)
BEGIN
    INSERT INTO [RolePermissions] (RoleId, PermissionId)
    VALUES ('a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1', 'aaaaaaaa-0000-0000-0000-00000000000b');
END

-- اضافه کردن RolePermission برای Supplier (اگر وجود ندارد)
IF NOT EXISTS (
    SELECT 1 FROM [RolePermissions] 
    WHERE RoleId = '33333333-3333-3333-3333-333333333333' 
    AND PermissionId = 'aaaaaaaa-0000-0000-0000-00000000000b'
)
BEGIN
    INSERT INTO [RolePermissions] (RoleId, PermissionId)
    VALUES ('33333333-3333-3333-3333-333333333333', 'aaaaaaaa-0000-0000-0000-00000000000b');
END

-- بررسی نتیجه
SELECT rp.*, p.Code AS PermissionCode, r.Name AS RoleName
FROM [RolePermissions] rp
INNER JOIN [Permissions] p ON rp.PermissionId = p.Id
INNER JOIN [IngAppUser].[Roles] r ON rp.RoleId = r.Id
WHERE p.Code = 'Offer.Manage';
```

## خلاصه تغییرات

✅ **PermissionConfiguration.cs**: اضافه شدن `OfferManageId` و seed data
✅ **RolePermissionConfiguration.cs**: اضافه شدن role-permission assignments برای Admin و Supplier
✅ **Migration AddOfferManagePermission**: اضافه شدن permission و role-permissions (اصلاح شده - بدون PermissionId1)
✅ **Migration FixRolePermissionRelationship**: حذف ستون اضافی PermissionId1
✅ **RolePermissionConfiguration.cs**: اصلاح relationship برای استفاده از navigation property صحیح

## نکات مهم

- همیشه migration ها را به ترتیب زمانی اجرا کنید
- بعد از اجرای migration، اگر JWT token قدیمی دارید، باید دوباره لاگین کنید
- اگر در UI تغییری نمی‌بینید، ممکن است نیاز به refresh یا clear cache داشته باشید

