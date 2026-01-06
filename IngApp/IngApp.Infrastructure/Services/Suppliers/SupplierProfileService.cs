using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Suppliers.DTO;
using IngApp.Domain.Entities.Suppliers;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using IngApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Suppliers
{
    public class SupplierProfileService : ISupplierProfileService
    {
        private readonly AppDbContext _db;

        public SupplierProfileService(AppDbContext db)
        {
            _db = db;
        }

        // ----------------------------------------------------------------
        // 1) Admin – لیست صفحه‌بندی‌شده تأمین‌کنندگان
        // ----------------------------------------------------------------
        public async Task<PagedResult<SupplierProfileDto>> GetPagedAsync(SupplierListQueryDto filter)
        {
            var query = _db.SupplierProfiles.Include(x => x.User)
                .Include(x => x.SupplierType)
                .AsNoTracking()
                .AsQueryable();

            // ------------------ فیلترها ------------------
            if (!string.IsNullOrWhiteSpace(filter.BusinessName))
            {
                var name = filter.BusinessName.Trim();
                query = query.Where(x => x.BusinessName.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(filter.userPhoneNumber))
            {
                var phone = filter.userPhoneNumber.Trim();
                query = query.Where(x => x.ContactPhone != null && x.ContactPhone.Contains(phone));
            }

            if (!string.IsNullOrWhiteSpace(filter.province))
            {
                var province = filter.province.Trim();
                query = query.Where(x => x.Province != null && x.Province.Contains(province));
            }

            if (!string.IsNullOrWhiteSpace(filter.city))
            {
                var city = filter.city.Trim();
                query = query.Where(x => x.City != null && x.City.Contains(city));
            }

            if (filter.SupplierTypeId.HasValue && filter.SupplierTypeId.Value > 0)
            {
                query = query.Where(x => x.SupplierTypeId == filter.SupplierTypeId.Value);
            }

            if (filter.VerificationStatus.HasValue)
            {
                query = query.Where(x =>
                    x.VerificationStatus == filter.VerificationStatus.Value
                );
            }


            // ------------------ Sort ------------------
            var sortBy = (filter.SortBy ?? "").Trim().ToLowerInvariant();
            var desc = filter.SortDesc;

            query = sortBy switch
            {
                "businessname" => desc ? query.OrderByDescending(x => x.BusinessName) : query.OrderBy(x => x.BusinessName),
                "province" => desc ? query.OrderByDescending(x => x.Province) : query.OrderBy(x => x.Province),
                "city" => desc ? query.OrderByDescending(x => x.City) : query.OrderBy(x => x.City),
                "supplierTypeName" => desc ? query.OrderByDescending(x => x.SupplierType.Name) : query.OrderBy(x => x.SupplierType.Name),
                "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                "verificationstatus" => desc ? query.OrderByDescending(x => x.VerificationStatus) : query.OrderBy(x => x.VerificationStatus),
                _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
            };

            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SupplierProfileDto
                {
                    Id = x.Id,
                    UserId = x.UserId,

                    SupplierTypeId = x.SupplierTypeId,
                    SupplierTypeName = x.SupplierType.Name,

                    BusinessName = x.BusinessName,
                    NationalId = x.NationalId,
                    LicenseNumber = x.LicenseNumber,

                    Province = x.Province,
                    City = x.City,
                    Address = x.Address,

                    BusinessType = x.BusinessType,
                    ContactName = x.ContactName,
                    ContactPosition = x.ContactPosition,
                    ContactMobile = x.ContactMobile,
                    ContactPhone = x.ContactPhone,

                    VerificationStatus = x.VerificationStatus.ToString(),
                    RejectionReason = x.RejectionReason,

                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserPhoneNumber = x.User.PhoneNumber

                })
                .ToListAsync();

            return new PagedResult<SupplierProfileDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        // ----------------------------------------------------------------
        // 2) User – دریافت پروفایل بر اساس UserId
        // ----------------------------------------------------------------
        public async Task<SupplierProfileDto?> GetByUserIdAsync(Guid userId)
        {
            var entity = await _db.SupplierProfiles
                .Include(x => x.SupplierType)
                .Include(x => x.User) // اضافه کردن User برای دریافت PhoneNumber
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (entity == null)
                return null;

            return new SupplierProfileDto
            {
                Id = entity.Id,
                UserId = entity.UserId,

                SupplierTypeId = entity.SupplierTypeId,
                SupplierTypeName = entity.SupplierType.Name,

                BusinessName = entity.BusinessName,
                NationalId = entity.NationalId,
                LicenseNumber = entity.LicenseNumber,

                Province = entity.Province,
                City = entity.City,
                Address = entity.Address,

                BusinessType = entity.BusinessType,
                ContactName = entity.ContactName,
                ContactPosition = entity.ContactPosition,
                ContactMobile = entity.ContactMobile,
                ContactPhone = entity.ContactPhone,

                VerificationStatus = entity.VerificationStatus.ToString(),
                RejectionReason = entity.RejectionReason,

                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                UserPhoneNumber = entity.User.PhoneNumber // موبایل از جدول Users
            };
        }

        // ----------------------------------------------------------------
        // 3) User – ایجاد/ویرایش پروفایل
        // ----------------------------------------------------------------
        public async Task<SupplierProfileDto> UpsertForUserAsync(Guid userId, UpsertSupplierProfileRequest request)
        {
            // 1) Validation ورودی
            var errors = new List<string>();

            if (request.SupplierTypeId <= 0)
                errors.Add("انتخاب نوع تأمین‌کننده اجباری است.");

            if (string.IsNullOrWhiteSpace(request.BusinessName))
                errors.Add("نام کسب‌وکار تأمین‌کننده اجباری است.");

            if (!Enum.IsDefined(typeof(BusinessType), request.BusinessType))
                errors.Add("نوع کسب‌وکار معتبر نیست.");

            if (string.IsNullOrWhiteSpace(request.ContactName))
                errors.Add("نام رابط اجباری است.");

            if (!Enum.IsDefined(typeof(ContactPosition), request.ContactPosition))
                errors.Add("سمت رابط معتبر نیست.");

            if (string.IsNullOrWhiteSpace(request.ContactMobile))
                errors.Add("شماره موبایل رابط اجباری است.");

            if (errors.Any())
                throw new ValidationException(errors);

            // نوع تأمین‌کننده باید معتبر و فعال باشد
            var supplierType = await _db.SupplierTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SupplierTypeId && x.IsActive);

            if (supplierType == null)
                throw new ValidationException(new() { "نوع تأمین‌کننده انتخاب‌شده معتبر نیست." });

            // 2) پیدا کردن یا ساختن
            var entity = await _db.SupplierProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            int? oldSupplierTypeId = null;

            if (entity == null)
            {
                entity = new SupplierProfile
                {
                    UserId = userId,
                    VerificationStatus = VerificationStatus.NotSubmitted,
                    CreatedAt = DateTime.UtcNow
                };

                _db.SupplierProfiles.Add(entity);
            }
            else
            {
                // در حالت Pending/Approved اجازه ویرایش نداریم
                if (entity.VerificationStatus == VerificationStatus.Pending ||
                    entity.VerificationStatus == VerificationStatus.Approved)
                {
                    throw new ValidationException(new()
                    {
                        "در وضعیت «در حال بررسی» یا «تأیید شده»، امکان ویرایش پروفایل وجود ندارد."
                    });
                }

                oldSupplierTypeId = entity.SupplierTypeId;

                var supplierTypeChanged = oldSupplierTypeId != request.SupplierTypeId;
                //var wasRejected = entity.VerificationStatus == VerificationStatus.Rejected;

                // اگر نوع تأمین‌کننده تغییر کند یا کاربر در حال اصلاح پروفایل رد شده باشد،
                // مدارک KYC قبلی باید بی‌اعتبار شوند (Soft Delete)
                if (supplierTypeChanged)
                {
                    await InvalidateUserKycDocumentsAsync(userId);                    
                }
                entity.VerificationStatus = VerificationStatus.NotSubmitted;
                entity.RejectionReason = null;
            }

            // 3) به‌روزرسانی فیلدها
            entity.SupplierTypeId = request.SupplierTypeId;
            entity.BusinessName = request.BusinessName.Trim();
            entity.NationalId = request.NationalId;
            entity.LicenseNumber = request.LicenseNumber;

            entity.Province = request.Province;
            entity.City = request.City;
            entity.Address = request.Address;

            entity.BusinessType = request.BusinessType;
            entity.ContactName = request.ContactName?.Trim();
            entity.ContactPosition = request.ContactPosition;
            entity.ContactMobile = request.ContactMobile.Trim();
            entity.ContactPhone = request.ContactPhone?.Trim();

            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // 4) برگرداندن DTO تازه
            var result = await GetByUserIdAsync(userId);

            if (result == null)
                throw new AppException("خطا در خواندن پروفایل تأمین‌کننده بعد از ثبت.");

            return result;
        }

        /// <summary>
        /// ارسال نهایی پروفایل برای بررسی (Draft -> Pending)
        /// </summary>
        public async Task SubmitForUserAsync(Guid userId)
        {
            var entity = await _db.SupplierProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (entity == null)
                throw new ValidationException(new() { "پروفایل تأمین‌کننده یافت نشد." });

            if (entity.VerificationStatus == VerificationStatus.Pending)
                throw new ValidationException(new() { "پروفایل شما قبلاً ارسال شده و در حال بررسی است." });

            if (entity.VerificationStatus == VerificationStatus.Approved)
                throw new ValidationException(new() { "پروفایل شما قبلاً تأیید شده است." });

            entity.VerificationStatus = VerificationStatus.Pending;
            entity.RejectionReason = null;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------------
        // 4) Admin – تغییر وضعیت تأمین‌کننده
        // ----------------------------------------------------------------
        public async Task UpdateVerificationStatusAsync(Guid supplierId, VerificationStatus newStatus, string? note, string adminUserId)
        {
            var supplier = await _db.SupplierProfiles
                .FirstOrDefaultAsync(x => x.Id == supplierId);

            if (supplier == null)
                throw new NotFoundException("تأمین‌کننده پیدا نشد.");

            if (string.IsNullOrWhiteSpace(adminUserId))
                throw new ValidationException(new() { "شناسه ادمین برای تغییر وضعیت الزامی است." });

            var oldStatus = supplier.VerificationStatus;

            // اگر تغییری نیست، لاگ و ذخیره لازم نیست
            if (oldStatus == newStatus)
                return;

            if (newStatus == VerificationStatus.Rejected && string.IsNullOrWhiteSpace(note))
            {
                throw new ValidationException(new() { "علت رد کردن تأمین‌کننده اجباری است." });
            }

            supplier.VerificationStatus = newStatus;
            supplier.RejectionReason = newStatus == VerificationStatus.Rejected ? note : null;
            supplier.UpdatedAt = DateTime.UtcNow;

            // تاریخچه
            _db.SupplierVerificationHistories.Add(new SupplierVerificationHistory
            {
                SupplierProfileId = supplier.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                AdminUserId = adminUserId,
                Note = note,
                CreatedAt = DateTime.UtcNow
            });

            // --- نقش‌دهی/نقش‌گیری Supplier + تغییر UserType ---
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == supplier.UserId);
            if (user == null)
                throw new NotFoundException("کاربر مرتبط با تأمین‌کننده پیدا نشد.");

            var supplierRoleId = RoleConfiguration.SupplierRoleId;
            var buyerUserType = UserType.Buyer;
            var supplierUserType = UserType.Supplier;

            if (newStatus == VerificationStatus.Approved)
            {
                // 1) UserType => Supplier
                if (user.UserType != supplierUserType)
                    user.UserType = supplierUserType;

                // 2) افزودن Role Supplier اگر وجود ندارد
                var hasSupplierRole = await _db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == supplierRoleId);
                if (!hasSupplierRole)
                    _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = supplierRoleId });
            }
            else
            {
                // هر وضعیتی غیر از Approved => Role Supplier برداشته شود و UserType برگردد Buyer
                if (user.UserType == supplierUserType)
                    user.UserType = buyerUserType;

                var existing = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == user.Id && x.RoleId == supplierRoleId);
                if (existing != null)
                    _db.UserRoles.Remove(existing);
            }


            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------------
        // 5) تاریخچه‌ی تغییر وضعیت تأمین‌کننده
        // ----------------------------------------------------------------
        public async Task<List<SupplierVerificationHistoryDto>> GetVerificationHistoryAsync(Guid supplierId)
        {
            var query =
                from h in _db.SupplierVerificationHistories.AsNoTracking()
                where h.SupplierProfileId == supplierId
                join u in _db.Users.AsNoTracking()
                    on h.AdminUserId equals u.Id.ToString() into uj
                from u in uj.DefaultIfEmpty()
                orderby h.CreatedAt descending
                select new SupplierVerificationHistoryDto
                {
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    AdminUserId = h.AdminUserId,
                    AdminDisplayName = u != null ? u.DisplayName : null,
                    Note = h.Note,
                    CreatedAt = h.CreatedAt
                };

            return await query.ToListAsync();
        }


        // ----------------------------------------------------------------
        // 6) لاگ فعالیت تأمین‌کننده
        // ----------------------------------------------------------------
        public async Task<List<SupplierActivityLogDto>> GetActivityLogsAsync(Guid supplierId)
        {
            return await _db.SupplierActivityLogs
                .Where(x => x.SupplierProfileId == supplierId)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new SupplierActivityLogDto
                {
                    ActionType = x.ActionType,
                    MetadataJson = x.MetadataJson,
                    UserId = x.UserId,
                    AdminUserId = x.AdminUserId,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        // ----------------------------------------------------------------
        // 7) Admin – جزئیات تأمین‌کننده
        // ----------------------------------------------------------------
        public async Task<SupplierDetailDto?> GetSupplierDetailAsync(Guid supplierId)
        {
            var supplier = await _db.SupplierProfiles
                .Include(x => x.User)
                .Include(x => x.SupplierType)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == supplierId);

            if (supplier == null)
                return null;

            // ======== KYC SUMMARY =========
            var userDocs = await _db.UserDocuments
                .Where(x => x.UserId == supplier.UserId && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            int totalDocs = userDocs.Count;
            int pendingDocs = userDocs.Count(x => x.Status == DocumentStatus.Pending);
            int approvedDocs = userDocs.Count(x => x.Status == DocumentStatus.Approved);
            int rejectedDocs = userDocs.Count(x => x.Status == DocumentStatus.Rejected);

            // ======== VERIFICATION HISTORY =========
            var history = await GetVerificationHistoryAsync(supplierId);

            // ======== ACTIVITY LOGS =========
            var logs = await GetActivityLogsAsync(supplierId);

            return new SupplierDetailDto
            {
                Id = supplier.Id,
                UserId = supplier.UserId,

                SupplierTypeId = supplier.SupplierTypeId,
                SupplierTypeName = supplier.SupplierType.Name,

                BusinessName = supplier.BusinessName,
                NationalId = supplier.NationalId,
                LicenseNumber = supplier.LicenseNumber,

                Province = supplier.Province,
                City = supplier.City,
                Address = supplier.Address,

                BusinessType = supplier.BusinessType,
                ContactName = supplier.ContactName,
                ContactPosition = supplier.ContactPosition,
                ContactMobile = supplier.ContactMobile,
                ContactPhone = supplier.ContactPhone,

                VerificationStatus = supplier.VerificationStatus,
                RejectionReason = supplier.RejectionReason,

                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt,
                UserPhoneNumber = supplier.User.PhoneNumber,

                TotalDocuments = totalDocs,
                PendingDocuments = pendingDocs,
                ApprovedDocuments = approvedDocs,
                RejectedDocuments = rejectedDocs,

                VerificationHistory = history,
                ActivityLogs = logs
            };
        }

        // ----------------------------------------------------------------
        // 8) Shared – لیست نوع تأمین‌کننده‌های فعال
        // ----------------------------------------------------------------
        public async Task<List<SupplierTypeDto>> GetActiveSupplierTypesAsync()
        {
            return await _db.SupplierTypes
                .Where(x => x.IsActive)
                .AsNoTracking()
                .Select(x => new SupplierTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        private async Task InvalidateUserKycDocumentsAsync(Guid userId)
        {
            var docs = await _db.UserDocuments
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync();

            if (!docs.Any())
                return;

            foreach (var d in docs)
            {
                d.IsDeleted = true;

                // وضعیت را برای جلوگیری از استفاده دوباره «رد شده» می‌کنیم
                d.Status = DocumentStatus.Rejected;
                d.AdminNote = "INVALIDATED_BY_PROFILE_CHANGE";
                d.ReviewedAt = DateTime.UtcNow;
            }
        }

        // ----------------------------------------------------------------
        // 9) Shared – اضافه کردن لاگ
        // ----------------------------------------------------------------
        public async Task AddActivityAsync(Guid supplierId, string actionType, string? metadataJson, string? userId, string? adminUserId)
        {
            _db.SupplierActivityLogs.Add(new SupplierActivityLog
            {
                SupplierProfileId = supplierId,
                ActionType = actionType,
                MetadataJson = metadataJson,
                UserId = userId,
                AdminUserId = adminUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
        public async Task<SupplierProfileDto?> GetMyAsync(Guid userId)
        {
            var entity = await _db.SupplierProfiles
                .Include(x => x.SupplierType)
                .Include(x => x.User)
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new SupplierProfileDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    SupplierTypeId = x.SupplierTypeId,
                    SupplierTypeName = x.SupplierType.Name,
                    BusinessName = x.BusinessName,
                    NationalId = x.NationalId,
                    LicenseNumber = x.LicenseNumber,
                    Province = x.Province,
                    City = x.City,
                    Address = x.Address,
                    BusinessType = x.BusinessType,
                    ContactName = x.ContactName,
                    ContactPosition = x.ContactPosition,
                    ContactMobile = x.ContactMobile,
                    ContactPhone = x.ContactPhone,
                    VerificationStatus = x.VerificationStatus.ToString(),
                    RejectionReason = x.RejectionReason,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserPhoneNumber = x.User.PhoneNumber
                })
                .FirstOrDefaultAsync();

            return entity;
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _db.SupplierProfiles
                .CountAsync(x => x.VerificationStatus == VerificationStatus.Pending);
        }



    }
}
