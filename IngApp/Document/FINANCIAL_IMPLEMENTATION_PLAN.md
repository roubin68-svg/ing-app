# برنامه پیاده‌سازی سیستم مالی (Financial System Implementation Plan)

## 📋 خلاصه تحلیل

### وضعیت فعلی سیستم:
- ✅ `User` entity با `UserType` Enum (Buyer, Supplier, Admin)
- ✅ `SupplierProfile` وجود دارد
- ❌ `BuyerProfile` وجود ندارد
- ❌ `VisitorProfile` وجود ندارد
- ❌ `Wallet` و `WalletTransaction` وجود ندارند
- ❌ `OfferContactUnlock` وجود ندارد
- ✅ `SupplierProfileService.SubmitForUserAsync` برای Submit نهایی Onboarding

### تصمیم‌های کلیدی:
1. **UserType**: تبدیل از Enum به Lookup Table (فاز 1)
2. **Visitor**: یک UserType جدید + VisitorProfile (مثل SupplierProfile)
3. **Onboarding Fee**: یک بار پرداخت موفق = همیشه معتبر (حتی بعد از Reject)
4. **Lookup Tables**: مرحله به مرحله ساخته می‌شوند
5. **Migration**: باید نوشته شود برای تبدیل UserType

---

## 🎯 فازبندی پیاده‌سازی

### **فاز 1: تبدیل UserType از Enum به Lookup Table** ⭐ (اولویت بالا)
**هدف**: تبدیل UserType از Enum به Lookup Table برای انعطاف‌پذیری بیشتر

**Backend:**
1. ✅ ساخت Entity `UserType` (Lookup Table)
2. ✅ ساخت Migration برای:
   - ایجاد جدول `UserTypes`
   - اضافه کردن `UserTypeId` به `Users`
   - Migrate داده‌های موجود (Buyer=1, Supplier=2, Admin=3)
   - حذف ستون `UserType` (Enum)
3. ✅ Update `User` entity: `UserTypeId` به جای `UserType`
4. ✅ Update تمام Services که از `UserType` استفاده می‌کنند:
   - `AuthService`
   - `SupplierProfileService`
   - `UserService`
   - `JwtTokenService`
5. ✅ Seed Data: ایجاد UserType های اولیه (Buyer, Supplier, Admin, Visitor)

**Frontend:**
- بررسی و تست که همه چیز کار می‌کند

**تست:**
- ✅ Login/Register
- ✅ Supplier Onboarding
- ✅ User Management

---

### **فاز 2: ساخت Lookup Tables پایه مالی** ⭐ (اولویت بالا)
**هدف**: ساخت Lookup Tables مورد نیاز برای Wallet و Transaction

**Backend:**
1. ✅ ساخت Entities:
   - `Currency` (IRR)
   - `WalletType` (Main)
   - `TransactionDirection` (Credit, Debit)
2. ✅ ساخت Configurations
3. ✅ ساخت Migration
4. ✅ Seed Data اولیه

**تست:**
- ✅ بررسی Lookup Tables در دیتابیس

---

### **فاز 3: Wallet و WalletTransaction (Core Financial)** ⭐⭐⭐ (اولویت خیلی بالا)
**هدف**: ساخت هسته سیستم مالی - Wallet و Ledger

**Backend:**
1. ✅ ساخت Entities:
   - `Wallet`
   - `WalletTransaction`
2. ✅ ساخت Lookup Tables:
   - `FinancialTransactionStatus` (Pending, Committed, Failed, Reversed)
   - `FinancialReferenceType` (Offer, Subscription, Payment, SupplierOnboarding, WalletTransaction)
3. ✅ ساخت Services:
   - `IWalletService` / `WalletService`
   - متدهای: `GetWalletAsync`, `GetTransactionsAsync`, `CreditAsync`, `DebitAsync`
   - Idempotency handling
   - Concurrency control (RowVersion)
4. ✅ ساخت API Controllers:
   - `GET /api/v1/wallet/me`
   - `GET /api/v1/wallet/me/transactions`
5. ✅ Auto-create Wallet هنگام ایجاد User جدید

**Frontend:**
1. ✅ ساخت `WalletPage`:
   - نمایش Balance (تومان)
   - جدول تراکنش‌ها
   - دکمه "شارژ کیف پول" (فعلاً placeholder)

**تست:**
- ✅ ایجاد Wallet برای User جدید
- ✅ Credit/Debit Wallet
- ✅ نمایش تراکنش‌ها
- ✅ Idempotency test
- ✅ Concurrency test

---

### **فاز 4: Pricing و FinancialOperationType** ⭐⭐ (اولویت بالا)
**هدف**: ساخت سیستم Pricing و Operation Types

**Backend:**
1. ✅ ساخت Entities:
   - `FinancialOperationType` (UnlockContactFee, SubscriptionPurchase, OnboardingFee, TopUp, CommissionEarned)
   - `Pricing` (یا یک جدول ساده برای نگه‌داری تعرفه‌ها)
2. ✅ ساخت Services:
   - `IPricingService` / `PricingService`
   - `GetPricingAsync` - دریافت تعرفه‌های فعلی
3. ✅ ساخت API:
   - `GET /api/v1/pricing`
4. ✅ Seed Data: تعرفه‌های اولیه

**Frontend:**
- (فعلاً نیاز نیست، در فاز بعدی استفاده می‌شود)

**تست:**
- ✅ دریافت Pricing از API

---

### **فاز 5: Unlock Contact (Core Feature)** ⭐⭐⭐ (اولویت خیلی بالا)
**هدف**: پیاده‌سازی Unlock Contact با پرداخت از Wallet

**Backend:**
1. ✅ ساخت Entity:
   - `OfferContactUnlock`
   - `UnlockSourceType` (Lookup: Paid, Subscription)
2. ✅ Update `OfferClickService`:
   - حذف `LogContactClick` (یا تبدیل به Unlock)
3. ✅ ساخت Service:
   - `UnlockContactAsync` در `IOfferService`:
     - بررسی Unlock قبلی
     - بررسی Subscription فعال
     - Debit از Wallet (اگر نیاز باشد)
     - ثبت `OfferContactUnlock`
     - محاسبه Commission (اگر Visitor دارد)
4. ✅ Update API:
   - `POST /api/v1/offers/{offerId}/unlock-contact`
   - حذف `POST /api/v1/offers/{offerId}/contact-click`
   - Update `GET /api/v1/offers/{offerId}/has-viewed-contact` → بررسی Unlock یا Subscription
5. ✅ Update `GetSupplierContact`: بررسی Unlock یا Subscription

**Frontend:**
1. ✅ Update `OfferDetailDrawer`:
   - بررسی Unlock/Subscription
   - نمایش دکمه "نمایش اطلاعات تماس" (اگر Unlock نشده)
   - نمایش ContactInfo (اگر Unlock شده یا Subscribed)
   - Handle خطای "موجودی کافی نیست"
2. ✅ Update `DashboardPage` و `OffersSearchPage`:
   - همان منطق Unlock

**تست:**
- ✅ Unlock Contact با موجودی کافی
- ✅ Unlock Contact بدون موجودی (خطا)
- ✅ نمایش ContactInfo بعد از Unlock
- ✅ عدم نمایش دکمه بعد از Unlock
- ✅ Unlock دائمی (بعد از Refresh)

---

### **فاز 6: Supplier Onboarding Fee** ⭐⭐ (اولویت بالا)
**هدف**: اضافه کردن مرحله پرداخت به Onboarding

**Backend:**
1. ✅ ساخت Entity:
   - `SupplierOnboardingPayment` (یا استفاده از WalletTransaction با ReferenceType)
2. ✅ Update `SupplierProfileService.SubmitForUserAsync`:
   - بررسی پرداخت قبلی (یک بار پرداخت موفق = همیشه معتبر)
   - اگر پرداخت نشده:
     - بررسی موجودی Wallet
     - Debit از Wallet
     - ثبت تراکنش
     - ثبت رکورد پرداخت
   - سپس Submit
3. ✅ ساخت API:
   - (فعلاً در SubmitForUserAsync انجام می‌شود)

**Frontend:**
1. ✅ Update `SupplierOnboardingPage`:
   - مرحله جدید: "پرداخت هزینه" (قبل از Submit نهایی)
   - نمایش مبلغ
   - دکمه "پرداخت از کیف پول"
   - Handle خطای "موجودی کافی نیست"
   - دکمه "شارژ کیف پول"

**تست:**
- ✅ پرداخت Onboarding Fee با موجودی کافی
- ✅ پرداخت Onboarding Fee بدون موجودی (خطا)
- ✅ Submit بعد از پرداخت موفق
- ✅ عدم نیاز به پرداخت مجدد بعد از Reject

---

### **فاز 7: Subscription (Plan + Purchase)** ⭐⭐ (اولویت متوسط)
**هدف**: پیاده‌سازی Subscription با UnlimitedContactViews

**Backend:**
1. ✅ ساخت Entities:
   - `SubscriptionPlan`
   - `UserSubscription`
   - `SubscriptionStatus` (Lookup: Active, Expired, Canceled)
2. ✅ ساخت Services:
   - `ISubscriptionService` / `SubscriptionService`
   - `GetPlansAsync`
   - `GetMySubscriptionAsync`
   - `PurchaseAsync` (از Wallet یا Gateway)
3. ✅ ساخت API:
   - `GET /api/v1/subscriptions/plans`
   - `GET /api/v1/subscriptions/me`
   - `POST /api/v1/subscriptions/purchase`
4. ✅ Update `UnlockContact`: بررسی Subscription فعال

**Frontend:**
1. ✅ ساخت `SubscriptionPage`:
   - لیست Planها
   - دکمه "خرید"
   - نمایش اشتراک فعال
2. ✅ Update `OfferDetailDrawer`: بررسی Subscription

**تست:**
- ✅ خرید Subscription از Wallet
- ✅ نمایش ContactInfo با Subscription فعال
- ✅ عدم کسر هزینه با Subscription فعال

---

### **فاز 8: Payment Gateway (Mock) + TopUp** ⭐ (اولویت متوسط)
**هدف**: آماده‌سازی زیرساخت پرداخت (شبیه‌سازی)

**Backend:**
1. ✅ ساخت Entities:
   - `Payment`
   - `PaymentGateway` (Lookup: Mock, Zarinpal)
   - `PaymentStatus` (Lookup: Created, Redirected, Verified, Failed)
2. ✅ ساخت Services:
   - `IPaymentService` / `PaymentService`
   - `CreateTopUpIntentAsync` (Mock)
   - `MockCompleteTopUpAsync` (شبیه‌سازی)
   - `VerifyTopUpAsync` (آماده برای Gateway واقعی)
3. ✅ ساخت API:
   - `POST /api/v1/payments/topup/intents`
   - `POST /api/v1/payments/topup/mock-complete`
   - `POST /api/v1/payments/topup/verify`

**Frontend:**
1. ✅ Update `WalletPage`:
   - دکمه "شارژ کیف پول"
   - Modal برای وارد کردن مبلغ
   - Redirect به Mock Payment (یا Modal)
   - نمایش نتیجه

**تست:**
- ✅ TopUp با Mock Payment
- ✅ Credit Wallet بعد از پرداخت موفق

---

### **فاز 9: Visitor (UserType + Profile)** ⭐ (اولویت پایین - وابسته به فاز 10)
**هدف**: اضافه کردن Visitor به سیستم

**Backend:**
1. ✅ Seed Data: اضافه کردن UserType "Visitor"
2. ✅ ساخت Entity:
   - `VisitorProfile`
   - `BuyerProfile` (با VisitorId nullable)
3. ✅ ساخت Services:
   - `IVisitorService` / `VisitorService`
   - `CreateBuyerAsync` (ثبت Buyer توسط Visitor)
4. ✅ ساخت API:
   - `POST /api/v1/visitor/buyers`
   - `GET /api/v1/visitor/commissions` (در فاز 10)

**Frontend:**
1. ✅ ساخت `VisitorPanel`:
   - فرم ثبت Buyer
   - لیست Buyerهای معرفی‌شده

**تست:**
- ✅ ثبت Buyer توسط Visitor
- ✅ اتصال Visitor به Buyer

---

### **فاز 10: Visitor Commission** ⭐ (اولویت پایین - وابسته به فاز 9)
**هدف**: محاسبه و پرداخت پورسانت Visitor

**Backend:**
1. ✅ ساخت Entities:
   - `VisitorCommissionRule`
   - `VisitorCommissionRuleLog`
2. ✅ ساخت Services:
   - `ICommissionService` / `CommissionService`
   - محاسبه Commission در:
     - `UnlockContact` (فقط اگر Debit واقعی)
     - `PurchaseSubscription` (همیشه)
   - ثبت CommissionTransaction (WalletTransaction با OperationType=CommissionEarned)
3. ✅ ساخت API:
   - `GET /api/v1/visitor/commissions`
   - Admin APIs برای مدیریت Commission Rules

**Frontend:**
1. ✅ Update `VisitorPanel`:
   - گزارش پورسانت‌ها
2. ✅ Admin Pages برای مدیریت Commission Rules

**تست:**
- ✅ محاسبه Commission برای Unlock Contact
- ✅ محاسبه Commission برای Subscription Purchase
- ✅ عدم Commission برای Unlock رایگان (با Subscription)

---

## 📊 نمودار وابستگی فازها

```
فاز 1 (UserType) 
  ↓
فاز 2 (Lookup Tables پایه)
  ↓
فاز 3 (Wallet Core) ←──┐
  ↓                     │
فاز 4 (Pricing)         │
  ↓                     │
فاز 5 (Unlock Contact) ←┤ (وابسته به فاز 3 و 4)
  ↓                     │
فاز 6 (Onboarding Fee) ←┤ (وابسته به فاز 3 و 4)
  ↓                     │
فاز 7 (Subscription) ←─┤ (وابسته به فاز 3 و 4)
  ↓                     │
فاز 8 (Payment Gateway)←┘ (وابسته به فاز 3)
  ↓
فاز 9 (Visitor) ←──┐
  ↓                │
فاز 10 (Commission)┘ (وابسته به فاز 9 و 3)
```

---

## ✅ چک‌لیست شروع کار

قبل از شروع هر فاز:
- [ ] بررسی وابستگی‌های فاز قبلی
- [ ] تست کامل فاز قبلی
- [ ] Commit و Push فاز قبلی
- [ ] مستندسازی تغییرات

---

## 🎯 اولویت‌بندی نهایی

**اولویت خیلی بالا (Core):**
1. فاز 1: UserType
2. فاز 2: Lookup Tables پایه
3. فاز 3: Wallet Core
4. فاز 4: Pricing
5. فاز 5: Unlock Contact

**اولویت بالا:**
6. فاز 6: Onboarding Fee
7. فاز 7: Subscription

**اولویت متوسط:**
8. فاز 8: Payment Gateway (Mock)

**اولویت پایین:**
9. فاز 9: Visitor
10. فاز 10: Commission

---

## 📝 نکات مهم

1. **Idempotency**: همه عملیات مالی باید Idempotent باشند
2. **Concurrency**: استفاده از RowVersion برای Wallet
3. **Transaction**: همه Debit/Credit در یک Transaction
4. **Currency**: UI تومان، DB ریال
5. **Migration**: حتماً Migration برای UserType بنویسیم
6. **Testing**: هر فاز باید کاملاً تست شود قبل از فاز بعدی

---

**آماده برای شروع فاز 1! 🚀**











