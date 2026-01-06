# 📋 گزارش جامع سیستم IngApp

## 🏗️ معماری کلی سیستم

سیستم IngApp یک پلتفرم B2B برای مدیریت تأمین‌کنندگان و آگهی‌های محصولات است که با معماری Clean Architecture و الگوی Repository/Service پیاده‌سازی شده است.

### ساختار پروژه

```
IngApp/
├── IngApp.Api/              # لایه API (Controllers, Middlewares)
├── IngApp.Application/      # لایه Business Logic (Services, DTOs)
├── IngApp.Domain/           # لایه Domain (Entities, Enums)
├── IngApp.Infrastructure/   # لایه Infrastructure (Database, File Storage)
└── ingapp-admin/           # Frontend (React + Ant Design)
```

---

## 🔐 1. سیستم احراز هویت و مدیریت کاربران

### 1.1 احراز هویت (Authentication)

**Backend:**
- **Entity:** `User` (Guid Id, PhoneNumber, DisplayName, UserType, VerificationStatus)
- **Service:** `AuthService` در `IngApp.Infrastructure/Services/Auth/`
- **Controller:** `AuthController` در `IngApp.Api/Controllers/v1/`

**منطق کسب‌وکاری:**
1. **ارسال OTP:** کاربر شماره موبایل را وارد می‌کند → سیستم OTP 6 رقمی ارسال می‌کند
2. **تأیید OTP:** کاربر کد را وارد می‌کند → سیستم JWT Token تولید می‌کند
3. **ذخیره OTP:** OTP ها در جدول `OtpCodes` با TTL (زمان انقضا) ذخیره می‌شوند
4. **JWT Token:** شامل Claims (uid, phoneNumber, userType) برای احراز هویت درخواست‌های بعدی

**Frontend:**
- **صفحه:** `LoginPage.jsx`
- **API:** `authApi.js` (sendOtp, verifyOtp)
- **جریان:** فرم ورود → ارسال OTP → تأیید OTP → ذخیره Token → هدایت به Dashboard

### 1.2 مدیریت کاربران

**Backend:**
- **Entity:** `User`, `UserRole`
- **Service:** `UserService`
- **Controller:** `UsersController`

**قابلیت‌ها:**
- ایجاد/ویرایش/حذف کاربر
- تخصیص نقش به کاربر
- فعال/غیرفعال کردن کاربر
- فیلتر و جستجو بر اساس UserType (Admin, Supplier)

**Frontend:**
- **صفحه:** `UsersPage.jsx`
- **قابلیت‌ها:** جدول کاربران، Modal ایجاد/ویرایش، تخصیص نقش

---

## 👥 2. سیستم مدیریت نقش‌ها و دسترسی‌ها (RBAC)

### 2.1 نقش‌ها (Roles)

**Backend:**
- **Entity:** `Role` (Name, DisplayName, Description)
- **Service:** `RoleService`
- **Controller:** `RolesController`

**منطق:**
- هر نقش می‌تواند چندین Permission داشته باشد
- کاربران می‌توانند چندین نقش داشته باشند
- دسترسی‌ها از طریق Policy-based Authorization کنترل می‌شوند

### 2.2 دسترسی‌ها (Permissions)

**Backend:**
- **Entity:** `Permission` (Code, DisplayName, Module)
- **Service:** `PermissionService`
- **Controller:** `PermissionsController`

**دسترسی‌های تعریف شده:**
- `Products.ViewOwn`, `Products.ViewAll`, `Products.Create`, `Products.Update`, `Products.Delete`
- `Roles.Manage`
- `Users.Manage`, `Users.View`
- `PermissionsModule.Manage`
- `Menus.Manage`

**Frontend:**
- **صفحات:** `RolesPage.jsx`, `PermissionsPage.jsx`
- **قابلیت‌ها:** مدیریت نقش‌ها، تخصیص Permission به Role

---

## 🏢 3. سیستم مدیریت تأمین‌کنندگان (Suppliers)

### 3.1 انواع تأمین‌کنندگان (Supplier Types)

**Backend:**
- **Entity:** `SupplierType` (Name, DisplayName, Description)
- **Service:** `SupplierTypeService`
- **Controller:** `SupplierTypesController`

**منطق:**
- هر نوع تأمین‌کننده می‌تواند دسترسی به دسته‌های خاصی از محصولات داشته باشد
- مثال: "تولیدکننده" می‌تواند فقط محصولات دسته "مواد اولیه" را ببیند

### 3.2 پروفایل تأمین‌کننده (Supplier Profile)

**Backend:**
- **Entity:** `SupplierProfile` (BusinessName, BusinessType, ContactName, ContactPosition, ContactMobile, ...)
- **Service:** `SupplierProfileService`
- **Controller:** `SuppliersController`

**فیلدهای مهم:**
- `BusinessType`: حقیقی (Natural) یا حقوقی (Legal)
- `ContactPosition`: مسئول خرید (PurchaseManager) یا مدیر عامل (CEO)
- `NationalId`: کد ملی / شماره ملی
- `RegistrationNumber`: شماره ثبت
- `ContactMobile`: شماره موبایل رابط

**Frontend:**
- **صفحه:** `SupplierOnboardingPage.jsx` (4 مرحله)
  - مرحله 1: انتخاب نوع تأمین‌کننده
  - مرحله 2: اطلاعات پروفایل (BusinessType, BusinessName, ContactInfo, ...)
  - مرحله 3: آپلود مدارک KYC
  - مرحله 4: بازبینی و تأیید نهایی

### 3.3 فرآیند تأیید تأمین‌کننده (Verification)

**Backend:**
- **Entity:** `SupplierVerificationHistory` (تاریخچه تغییرات وضعیت)
- **Service:** `SupplierProfileService.UpdateVerificationStatusAsync`

**وضعیت‌ها:**
- `NotSubmitted`: هنوز مدارک ارسال نشده
- `Pending`: در انتظار بررسی
- `Approved`: تأیید شده
- `Rejected`: رد شده

**منطق:**
- Admin می‌تواند وضعیت را تغییر دهد
- هر تغییر در `SupplierVerificationHistory` ثبت می‌شود
- فقط تأمین‌کنندگان تأیید شده می‌توانند آگهی ایجاد کنند

**Frontend:**
- **صفحه:** `SuppliersPage.jsx` (لیست تأمین‌کنندگان)
- **Component:** `SupplierCaseDrawer.jsx` (جزئیات + تاریخچه تأیید)

### 3.4 دسترسی به دسته‌بندی محصولات

**Backend:**
- **Entity:** `SupplierCategoryAccess` (SupplierTypeId, ProductCategoryId)
- **Service:** `SupplierCategoryAccessService`
- **Controller:** `SupplierCategoryAccessController`

**منطق:**
- هر نوع تأمین‌کننده می‌تواند به دسته‌های خاصی دسترسی داشته باشد
- Admin می‌تواند دسترسی‌ها را مدیریت کند

---

## 📄 4. سیستم KYC (Know Your Customer)

### 4.1 تعریف فیلدهای KYC

**Backend:**
- **Entity:** `KycAttributeDefinition` (DisplayName, DataType, IsRequired, Order)
- **Service:** `KycAttributeDefinitionService`
- **Controller:** `KycAttributeDefinitionsController`

**انواع داده:**
- `Text`: متن
- `Number`: عدد
- `Boolean`: بله/خیر
- `Date`: تاریخ
- `File`: فایل (PDF, Word, Image)

### 4.2 Template های KYC

**Backend:**
- **Entity:** `KycTemplate` (Name, SupplierTypeId)
- **Entity:** `KycTemplateItem` (TemplateId, AttributeDefinitionId, Order)
- **Service:** `KycTemplateService`
- **Controller:** `KycTemplatesController`

**منطق:**
- هر نوع تأمین‌کننده یک Template KYC دارد
- Template شامل لیستی از فیلدهای مورد نیاز است
- در مرحله 3 Onboarding، کاربر باید تمام فیلدهای Template را پر کند

### 4.3 مدارک کاربر (User Documents)

**Backend:**
- **Entity:** `UserDocument` (UserId, AttributeDefinitionId, Value, FilePath)
- **Service:** `KycService`
- **Controller:** `KycController`

**ذخیره‌سازی فایل:**
- فایل‌ها در `KycFileStorage:RootPath` ذخیره می‌شوند
- ساختار: `{RootPath}/{userId}/{attributeDefinitionId}/{guid}{ext}`
- API: `POST /api/v1/kyc/upload-file` برای آپلود
- API: `GET /api/v1/kyc/file` برای دانلود

**Frontend:**
- **Component:** `SupplierDocumentsTab.jsx` (نمایش مدارک در Drawer)
- **قابلیت‌ها:** آپلود فایل، نمایش فایل‌های آپلود شده، دانلود

---

## 📦 5. سیستم مدیریت محصولات

### 5.1 دسته‌بندی محصولات

**Backend:**
- **Entity:** `ProductCategory` (Name, ParentId) - ساختار درختی
- **Service:** `ProductCategoryService`
- **Controller:** `ProductCategoriesController`

**منطق:**
- دسته‌بندی‌ها به صورت درختی (Tree) هستند
- هر دسته می‌تواند زیردسته داشته باشد
- در Frontend با `CategoryTreeSelect` نمایش داده می‌شود

### 5.2 محصولات

**Backend:**
- **Entity:** `Product` (Name, CategoryId, Unit, ImagePath, IsActive)
- **Service:** `ProductService`
- **Controller:** `ProductsController`

**قابلیت‌ها:**
- ایجاد/ویرایش/حذف محصول
- آپلود تصویر محصول
- فعال/غیرفعال کردن محصول

**ذخیره‌سازی تصویر:**
- تصاویر در `ProductFileStorage:RootPath` ذخیره می‌شوند
- ساختار: `{RootPath}/{productId}/{guid}{ext}`
- API: `POST /api/v1/products/upload-image`
- API: `GET /api/v1/products/upload-image/image` برای نمایش

**Frontend:**
- **صفحه:** `ProductsPage.jsx`
- **قابلیت‌ها:** جدول محصولات، Modal ایجاد/ویرایش، آپلود تصویر، نمایش Thumbnail

### 5.3 ویژگی‌های محصول (Product Attributes)

**Backend:**
- **Entity:** `ProductAttributeDefinition` (DisplayName, DataType, IsRequired)
- **Service:** `ProductAttributeDefinitionService`
- **Controller:** `ProductAttributeDefinitionsController`

**انواع داده:** مشابه KYC (Text, Number, Boolean, Date, File)

### 5.4 Template ویژگی‌های محصول

**Backend:**
- **Entity:** `ProductAttributeTemplate` (ProductId, AttributeDefinitionId, Order)
- **Service:** `ProductAttributeTemplateService`
- **Controller:** `ProductAttributeTemplatesController`

**منطق:**
- هر محصول می‌تواند یک Template از ویژگی‌ها داشته باشد
- در زمان ایجاد آگهی، کاربر باید این ویژگی‌ها را پر کند

---

## 📢 6. سیستم مدیریت آگهی‌ها (Offers)

### 6.1 موجودیت آگهی

**Backend:**
- **Entity:** `Offer` (ProductId, SupplierUserId, UnitPrice, TotalPrice, Quantity, Unit, HasTax, TaxAmount, Status, ...)
- **Service:** `OfferService`
- **Controller:** `OffersController` (Public), `MyOffersController` (Supplier), `AdminOffersController` (Admin)

**فیلدهای مهم:**
- `Status`: Draft, Pending, Published, Cancel, Rejected
- `WizardStep`: 1, 2, 3, 4 (مراحل ایجاد آگهی)
- `HasTax`: آیا مالیات دارد؟
- `TaxAmount`: مبلغ مالیات
- `RejectedReason`: دلیل رد (اگر Admin رد کرده باشد)

### 6.2 فرآیند ایجاد آگهی (4 مرحله)

**مرحله 1: انتخاب محصول**
- Supplier محصول مورد نظر را از لیست محصولات مجاز انتخاب می‌کند
- API: `POST /api/v1/offers/my` → ایجاد Draft Offer

**مرحله 2: اطلاعات اصلی**
- قیمت واحد، مقدار، واحد، مالیات، تاریخ انقضا
- API: `PUT /api/v1/offers/my/{offerId}/header`

**مرحله 3: ویژگی‌ها و مدارک**
- پر کردن ویژگی‌های محصول (از Template)
- آپلود فایل‌های مورد نیاز
- API: `PUT /api/v1/offers/my/{offerId}/documents`
- API: `POST /api/v1/offers/my/upload-file` برای آپلود فایل

**مرحله 4: بازبینی و انتشار**
- نمایش خلاصه اطلاعات
- دکمه "انتشار آگهی"
- API: `POST /api/v1/offers/my/{offerId}/submit` → تغییر Status به Published

**Frontend:**
- **صفحه:** `OfferManagementPage.jsx`
- **جریان:** Wizard 4 مرحله‌ای با Navigation بین مراحل

### 6.3 مدارک آگهی (Offer Documents)

**Backend:**
- **Entity:** `OfferDocument` (OfferId, AttributeDefinitionId, Value, FilePath)
- **Service:** `OfferService`
- **ذخیره‌سازی:** مشابه KYC، در `OfferFileStorage:RootPath`

**منطق:**
- هر آگهی می‌تواند چندین Document داشته باشد
- Document ها بر اساس `ProductAttributeTemplate` ایجاد می‌شوند

### 6.4 مدیریت آگهی‌ها توسط Admin

**Backend:**
- **Controller:** `AdminOffersController`
- **Service:** `OfferService.GetAdminOffersAsync`, `GetAdminOfferDetailAsync`

**قابلیت‌ها:**
- مشاهده تمام آگهی‌ها (همه Status ها)
- فیلتر بر اساس Supplier, Status, ProductCategory
- رد کردن آگهی‌های Published (با دلیل)
- مشاهده تاریخچه تغییرات Status

**Frontend:**
- **صفحه:** `AdminOffersPage.jsx`
- **قابلیت‌ها:** جدول آگهی‌ها، فیلتر، Drawer جزئیات + تاریخچه، دکمه رد

### 6.5 تاریخچه تغییرات Status

**Backend:**
- **Entity:** `OfferStatusHistory` (OfferId, OldStatus, NewStatus, AdminUserId, Note, CreatedAt)
- **Service:** `OfferService.GetOfferStatusHistoryAsync`

**منطق:**
- هر تغییر Status ثبت می‌شود:
  - Draft → Published: "آگهی توسط تأمین‌کننده منتشر شد"
  - Published → Cancel: توسط Supplier (بدون دلیل)
  - Published → Rejected: توسط Admin (با دلیل)

**Frontend:**
- **Component:** Tab "تاریخچه" در Drawer جزئیات آگهی
- **نمایش:** جدول با OldStatus, NewStatus, Admin, Note, CreatedAt

### 6.6 آمار کلیک‌ها (Click Logs)

**Backend:**
- **Entity:** `OfferClickLog` (OfferId, ClickType, UserId, CreatedAt)
- **ClickType:** View (مشاهده جزئیات), ContactClick (کلیک روی نمایش اطلاعات تماس)
- **Service:** `OfferService.LogContactClickAsync`

**منطق:**
- هر بار که کاربر جزئیات آگهی را می‌بیند → View Log
- هر بار که روی "نمایش اطلاعات تماس" کلیک می‌کند → ContactClick Log
- در Admin Panel، آمار ViewCount و ContactClickCount نمایش داده می‌شود

### 6.7 جستجوی آگهی‌ها (Public Search)

**Backend:**
- **Controller:** `OffersController`
- **Service:** `OfferService.SearchPublicAsync`

**فیلترها:**
- ProductCategoryId
- ProductName
- MinPrice, MaxPrice
- MinQuantity, MaxQuantity
- SortBy: newest, oldest, priceAsc, priceDesc, quantityAsc, quantityDesc

**Frontend:**
- **صفحه:** `OffersSearchPage.jsx`
- **قابلیت‌ها:** فیلتر پیشرفته، کارت‌های آگهی، Drawer جزئیات

### 6.8 نمایش اطلاعات تماس Supplier

**Backend:**
- **API:** `GET /api/v1/offers/{offerId}/supplier-contact`
- **Service:** `OfferService.GetSupplierContactAsync`

**منطق:**
- فقط برای آگهی‌های Published
- ثبت ContactClick Log
- نمایش: BusinessName, SupplierType, Mobile, ContactPhone, Province, City, Address

**Frontend:**
- **Component:** `OfferDetailDrawer.jsx`
- **دکمه:** "نمایش اطلاعات تماس" → Modal با اطلاعات Supplier

---

## 🎨 7. سیستم مدیریت منو (Menu Settings)

**Backend:**
- **Entity:** `MenuItem` (Key, Title, Route, Icon, ParentId, Order, RequiredPermissionCode)
- **Service:** `MenuService`
- **Controller:** `MenuSettingsController`

**منطق:**
- منو به صورت درختی است
- هر آیتم می‌تواند Parent داشته باشد
- می‌تواند نیاز به Permission خاصی داشته باشد
- Admin می‌تواند ترتیب، Parent، Permission را تغییر دهد

**Frontend:**
- **صفحه:** `MenuSettingsPage.jsx`
- **قابلیت‌ها:** Drag & Drop برای تغییر ترتیب، تغییر Parent، تغییر Permission

---

## 📊 8. Dashboard

**Frontend:**
- **صفحه:** `DashboardPage.jsx`

**قابلیت‌ها:**
- نمایش 12 آگهی اخیر (Published)
- کارت‌های آگهی با:
  - تصویر محصول (Thumbnail 80x80)
  - کد آگهی (#ID)
  - نام محصول (Bold)
  - دسته محصول (Tag)
  - قیمت واحد، مقدار، قیمت کل
  - مالیات (اگر دارد: مبلغ مالیات + قیمت کل + مالیات)
  - پیام "این کالا مالیات ندارد" (اگر مالیات ندارد)
  - تاریخ انتشار
  - دکمه "مشاهده جزئیات"

---

## 🔄 9. جریان‌های کاری اصلی

### 9.1 جریان Onboarding تأمین‌کننده

```
1. کاربر ثبت‌نام می‌کند (OTP) → User ایجاد می‌شود
2. وارد صفحه SupplierOnboarding می‌شود
3. مرحله 1: انتخاب SupplierType
4. مرحله 2: پر کردن اطلاعات پروفایل
   - BusinessType (حقیقی/حقوقی)
   - BusinessName
   - NationalId / RegistrationNumber
   - ContactName, ContactPosition, ContactMobile
5. مرحله 3: آپلود مدارک KYC (بر اساس Template)
6. مرحله 4: بازبینی و تأیید
7. Admin مدارک را بررسی می‌کند
8. Admin وضعیت را به Approved یا Rejected تغییر می‌دهد
```

### 9.2 جریان ایجاد و انتشار آگهی

```
1. Supplier وارد "آگهی‌های من" می‌شود
2. کلیک روی "ایجاد آگهی جدید"
3. مرحله 1: انتخاب محصول (از لیست محصولات مجاز)
4. مرحله 2: وارد کردن اطلاعات قیمت و مقدار
5. مرحله 3: پر کردن ویژگی‌ها و آپلود مدارک
6. مرحله 4: بازبینی و کلیک روی "انتشار آگهی"
7. Status تغییر می‌کند به Published
8. آگهی در جستجو و Dashboard نمایش داده می‌شود
```

### 9.3 جریان رد آگهی توسط Admin

```
1. Admin وارد "مدیریت آگهی‌ها" می‌شود
2. آگهی Published را پیدا می‌کند
3. کلیک روی "مشاهده" → Drawer باز می‌شود
4. کلیک روی "رد کردن آگهی"
5. Modal باز می‌شود → Admin دلیل را وارد می‌کند
6. Status تغییر می‌کند به Rejected
7. در تاریخچه ثبت می‌شود: Published → Rejected
8. Supplier در "آگهی‌های من" می‌بیند که رد شده + دلیل
```

---

## 🗄️ 10. ساختار دیتابیس

### جداول اصلی:

1. **Users**: کاربران سیستم
2. **Roles**: نقش‌ها
3. **Permissions**: دسترسی‌ها
4. **UserRoles**: ارتباط کاربر-نقش
5. **RolePermissions**: ارتباط نقش-دسترسی
6. **SupplierTypes**: انواع تأمین‌کنندگان
7. **SupplierProfiles**: پروفایل‌های تأمین‌کنندگان
8. **SupplierVerificationHistories**: تاریخچه تأیید
9. **KycAttributeDefinitions**: تعریف فیلدهای KYC
10. **KycTemplates**: Template های KYC
11. **UserDocuments**: مدارک کاربران
12. **ProductCategories**: دسته‌بندی محصولات
13. **Products**: محصولات
14. **ProductAttributeDefinitions**: تعریف ویژگی‌های محصول
15. **ProductAttributeTemplates**: Template ویژگی‌های محصول
16. **Offers**: آگهی‌ها
17. **OfferDocuments**: مدارک آگهی‌ها
18. **OfferStatusHistories**: تاریخچه تغییرات Status
19. **OfferClickLogs**: لاگ کلیک‌ها
20. **MenuItems**: آیتم‌های منو
21. **OtpCodes**: کدهای OTP

---

## 📁 11. ساختار فایل‌ها

### Backend File Storage:

1. **KYC Files:** `{KycFileStorage:RootPath}/{userId}/{attributeDefinitionId}/{guid}{ext}`
2. **Offer Files:** `{OfferFileStorage:RootPath}/{offerId}/{attributeDefinitionId}/{guid}{ext}`
3. **Product Images:** `{ProductFileStorage:RootPath}/{productId}/{guid}{ext}`

### Frontend File Access:

- فایل‌ها از طریق API با Authentication دانلود می‌شوند
- برای نمایش تصاویر در Frontend، از Blob URL استفاده می‌شود:
  - API call با `responseType: "blob"`
  - `URL.createObjectURL(blob)` برای ایجاد URL موقت
  - `URL.revokeObjectURL()` برای آزادسازی حافظه

---

## 🔒 12. امنیت

### Authentication:
- JWT Token-based
- Token شامل Claims: uid, phoneNumber, userType
- Middleware: `ApiExceptionMiddleware` برای مدیریت خطاها

### Authorization:
- Policy-based Authorization
- هر Policy نیاز به Permission خاصی دارد
- `AuthorizationHandler` بررسی می‌کند که آیا کاربر Permission دارد یا نه

### CORS:
- فقط `http://localhost:3000` مجاز است
- `AllowCredentials: true` برای ارسال Cookie/Token

---

## 🎯 13. منطق کسب‌وکاری کلیدی

### 13.1 کنترل دسترسی به محصولات:
- هر SupplierType می‌تواند به دسته‌های خاصی دسترسی داشته باشد
- در زمان ایجاد آگهی، فقط محصولات مجاز نمایش داده می‌شوند

### 13.2 فرآیند تأیید:
- Supplier باید ابتدا تأیید شود (Approved) تا بتواند آگهی ایجاد کند
- Admin می‌تواند Supplier را رد کند و دلیل وارد کند

### 13.3 مدیریت آگهی‌ها:
- فقط آگهی‌های Published در جستجو نمایش داده می‌شوند
- Admin می‌تواند آگهی‌های Published را رد کند
- Supplier می‌تواند آگهی‌های خود را Cancel کند

### 13.4 مالیات:
- هر آگهی می‌تواند مالیات داشته باشد یا نداشته باشد
- اگر `HasTax = true` → `TaxAmount` محاسبه می‌شود
- قیمت نهایی = `TotalPrice + TaxAmount`

### 13.5 تاریخچه و Audit:
- تمام تغییرات Status در `OfferStatusHistory` ثبت می‌شوند
- تمام تغییرات Verification Status در `SupplierVerificationHistory` ثبت می‌شوند
- Admin که تغییر را انجام داده، در تاریخچه ثبت می‌شود

---

## 📱 14. Frontend Architecture

### ساختار:
- **React 18** با Hooks (useState, useEffect, useMemo, useCallback)
- **Ant Design** برای UI Components
- **React Router DOM** برای Routing
- **Axios** برای API Calls
- **Jalaali-js** برای تبدیل تاریخ به شمسی

### State Management:
- Local State با useState
- Context API برای Authentication State
- API Client با Interceptors برای مدیریت Token و خطاها

### Routing:
- `/login`: صفحه ورود
- `/`: Dashboard
- `/suppliers`: لیست تأمین‌کنندگان
- `/supplier-onboarding`: فرآیند Onboarding
- `/products`: لیست محصولات
- `/my-offers`: آگهی‌های من (Supplier)
- `/supplier/offers/manage/:id`: مدیریت آگهی
- `/offers-search`: جستجوی آگهی‌ها
- `/admin-offers`: مدیریت آگهی‌ها (Admin)

---

## 🚀 15. API Endpoints

### Authentication:
- `POST /api/v1/auth/send-otp`
- `POST /api/v1/auth/verify-otp`
- `GET /api/v1/auth/me`

### Suppliers:
- `GET /api/v1/suppliers`
- `GET /api/v1/suppliers/{id}`
- `PUT /api/v1/suppliers/{id}/verification-status`
- `GET /api/v1/suppliers/my/profile`
- `PUT /api/v1/suppliers/my/profile`

### Offers:
- `GET /api/v1/offers` (Public Search)
- `GET /api/v1/offers/{id}` (Public Detail)
- `GET /api/v1/offers/my` (My Offers)
- `POST /api/v1/offers/my` (Create Draft)
- `PUT /api/v1/offers/my/{id}/header`
- `PUT /api/v1/offers/my/{id}/documents`
- `POST /api/v1/offers/my/{id}/submit`
- `PUT /api/v1/offers/my/{id}/cancel`
- `GET /api/v1/admin/offers` (Admin List)
- `PUT /api/v1/admin/offers/{id}/reject`

### Products:
- `GET /api/v1/products`
- `POST /api/v1/products`
- `PUT /api/v1/products/{id}`
- `POST /api/v1/products/upload-image`

### KYC:
- `GET /api/v1/kyc/my/requirements`
- `POST /api/v1/kyc/my/documents`
- `POST /api/v1/kyc/upload-file`
- `GET /api/v1/kyc/file`

---

## 📝 16. خلاصه

سیستم IngApp یک پلتفرم جامع B2B است که شامل:

1. **مدیریت کاربران و دسترسی‌ها** (RBAC)
2. **فرآیند Onboarding تأمین‌کنندگان** (4 مرحله + KYC)
3. **مدیریت محصولات و دسته‌بندی‌ها**
4. **سیستم آگهی‌دهی** (4 مرحله ایجاد + مدیریت توسط Admin)
5. **جستجو و نمایش آگهی‌ها**
6. **مدیریت منو و تنظیمات**

تمام بخش‌ها با معماری Clean Architecture و الگوی Repository/Service پیاده‌سازی شده‌اند و از Entity Framework Core برای دسترسی به دیتابیس استفاده می‌کنند.

