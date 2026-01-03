using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Kyc;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Kyc.DTO;
using IngApp.Domain.Entities.Kyc;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Kyc
{
    public class KycService : IKycService
    {
        private readonly AppDbContext _db;

        public KycService(AppDbContext db)
        {
            _db = db;
        }

        // ==========================================================
        // 0) Paging برای پنل ادمین – لیست مدارک KYC
        // ==========================================================
        public async Task<PagedResult<UserDocumentDto>> GetPagedAsync(KycListQueryDto filter)
        {
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var query =
                from d in _db.UserDocuments.AsNoTracking()
                where !d.IsDeleted
                join def in _db.KycAttributeDefinitions.AsNoTracking()
                    on d.KycAttributeDefinitionId equals def.Id
                join u in _db.Users.AsNoTracking()
                    on d.UserId equals u.Id
                join sp in _db.SupplierProfiles.AsNoTracking()
                    on u.Id equals sp.UserId into spg
                from sp in spg.DefaultIfEmpty()
                select new
                {
                    Doc = d,
                    Def = def,
                    User = u,
                    Supplier = sp
                };

            // ---------------- Filtering ----------------
            if (filter.UserId.HasValue)
                query = query.Where(x => x.Doc.UserId == filter.UserId.Value);

            if (filter.AttributeDefinitionId.HasValue)
                query = query.Where(x => x.Def.Id == filter.AttributeDefinitionId.Value);

            if (filter.SupplierTypeId.HasValue)
                query = query.Where(x => x.Supplier != null &&
                                         x.Supplier.SupplierTypeId == filter.SupplierTypeId.Value);

            if (filter.Status.HasValue)
                query = query.Where(x => x.Doc.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.BusinessName))
            {
                var name = filter.BusinessName.Trim();
                query = query.Where(x => x.Supplier != null &&
                                         x.Supplier.BusinessName.Contains(name));
            }

            // ---------------- Sorting ----------------
            var sortBy = (filter.SortBy ?? "").ToLowerInvariant();
            var desc = filter.SortDesc;

            query = sortBy switch
            {
                "status" =>
                    desc
                        ? query.OrderByDescending(x => x.Doc.Status)
                               .ThenByDescending(x => x.Doc.UploadedAt)
                        : query.OrderBy(x => x.Doc.Status)
                               .ThenByDescending(x => x.Doc.UploadedAt),

                "displayname" =>
                    desc
                        ? query.OrderByDescending(x => x.Def.DisplayName)
                        : query.OrderBy(x => x.Def.DisplayName),

                "uploadedat" =>
                    desc
                        ? query.OrderByDescending(x => x.Doc.UploadedAt)
                        : query.OrderBy(x => x.Doc.UploadedAt),

                _ =>
                    desc
                        ? query.OrderByDescending(x => x.Doc.UploadedAt)
                        : query.OrderByDescending(x => x.Doc.UploadedAt)
            };

            // ---------------- Paging ----------------
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserDocumentDto
                {
                    Id = x.Doc.Id,
                    AttributeDefinitionId = x.Def.Id,
                    AttributeDisplayName = x.Def.DisplayName,
                    Value = x.Doc.Value,
                    FilePath = x.Doc.FilePath,
                    Status = x.Doc.Status,
                    AdminNote = x.Doc.AdminNote,
                    UploadedAt = x.Doc.UploadedAt,
                    ReviewedAt = x.Doc.ReviewedAt
                })
                .ToListAsync();

            return new PagedResult<UserDocumentDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        // ==========================================================
        // 1) Requirements برای ساخت فرم KYC کاربر
        // ==========================================================
        public async Task<List<KycRequirementDto>> GetRequirementsForUserAsync(Guid userId)
        {
            var supplier = await _db.SupplierProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (supplier == null)
                return new List<KycRequirementDto>();

            // بررسی اینکه آیا SupplierTypeId معتبر است
            if (supplier.SupplierTypeId <= 0)
                return new List<KycRequirementDto>();

            // گرفتن templates فعال برای این SupplierTypeId
            var templates = await _db.KycTemplates
                .Where(x => x.SupplierTypeId == supplier.SupplierTypeId && x.IsActive == true)
                .AsNoTracking()
                .ToListAsync();

            // اگر template فعالی پیدا نشد، لیست خالی برمی‌گردانیم
            // (frontend خودش پیام مناسب را نمایش می‌دهد)
            if (!templates.Any())
                return new List<KycRequirementDto>();

            var defIds = templates.Select(t => t.KycAttributeDefinitionId).Distinct().ToList();

            // گرفتن KycAttributeDefinitions (بدون فیلتر IsActive - چون template فعال است پس definition هم باید فعال باشد)
            var defs = await _db.KycAttributeDefinitions
                .Where(d => defIds.Contains(d.Id))
                .AsNoTracking()
                .ToListAsync();

            // مدارک فعلی کاربر (فقط غیرحذف‌شده)
            var docs = await _db.UserDocuments
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<KycRequirementDto>();

            foreach (var t in templates.OrderBy(x => x.SortOrder))
            {
                var def = defs.FirstOrDefault(x => x.Id == t.KycAttributeDefinitionId);
                if (def == null) continue;

                var doc = docs.FirstOrDefault(x => x.KycAttributeDefinitionId == def.Id);

                result.Add(new KycRequirementDto
                {
                    AttributeDefinitionId = def.Id,
                    AttributeDisplayName = def.DisplayName,
                    Description = def.Description,
                    DataType = def.DataType,
                    IsRequired = t.IsRequired,

                    CurrentStatus = doc?.Status,
                    CurrentFilePath = doc?.FilePath,
                    CurrentValue = doc?.Value,
                    AdminNote = doc?.AdminNote
                });
            }

            return result;
        }

        // ==========================================================
        // 2) Submit مدارک کاربر
        // ==========================================================
        public async Task SubmitDocumentsAsync(Guid userId, List<SubmitKycDocumentItemDto> items)
        {
            if (items == null || items.Count == 0)
                throw new ValidationException(new() { "هیچ مدرکی ارسال نشده است." });

            var supplier = await _db.SupplierProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (supplier == null)
            {
                throw new ValidationException(new()
                {
                    "برای ارسال مدارک ابتدا باید پروفایل تأمین‌کننده را تکمیل کنید."
                });
            }

            // قالب‌های فعال این نوع تأمین‌کننده
            var templates = await _db.KycTemplates
                .Where(t => t.SupplierTypeId == supplier.SupplierTypeId &&
                            t.IsActive)
                .AsNoTracking()
                .ToListAsync();

            if (!templates.Any())
                throw new ValidationException(new() { "برای نوع تأمین‌کننده شما مدرکی تعریف نشده است." });

            var templateDefIds = templates.Select(t => t.KycAttributeDefinitionId).ToHashSet();
            var attributeIds = items.Select(i => i.AttributeDefinitionId).Distinct().ToList();

            // فقط attribute هایی مجازند که در template فعال هستند
            var invalid = attributeIds.Where(id => !templateDefIds.Contains(id)).ToList();
            if (invalid.Any())
                throw new ValidationException(new() { "برخی مدارک ارسال‌شده برای نوع تأمین‌کننده شما معتبر نیست." });

            var defs = await _db.KycAttributeDefinitions
                .Where(d => attributeIds.Contains(d.Id))
                .AsNoTracking()
                .ToListAsync();

            // Validation required
            var errors = new List<string>();
            foreach (var t in templates.Where(x => x.IsRequired))
            {
                var hasItem = items.Any(i => i.AttributeDefinitionId == t.KycAttributeDefinitionId);
                if (!hasItem)
                {
                    var def = defs.FirstOrDefault(d => d.Id == t.KycAttributeDefinitionId);
                    errors.Add(def == null ? "ارسال یکی از مدارک اجباری الزامی است." : $"ارسال «{def.DisplayName}» اجباری است.");
                }
            }

            // Validation datatype/value
            foreach (var item in items)
            {
                var def = defs.FirstOrDefault(d => d.Id == item.AttributeDefinitionId);
                if (def == null)
                {
                    errors.Add("یکی از فیلدهای ارسالی معتبر نیست.");
                    continue;
                }

                var template = templates.First(t => t.KycAttributeDefinitionId == def.Id);
                var isRequired = template.IsRequired;

                // اگر optional است و مقدار خالی است → کاملاً رد شو
                var hasValue =
                    def.DataType == KycDataType.File
                        ? !string.IsNullOrWhiteSpace(item.FilePath)
                        : !string.IsNullOrWhiteSpace(item.Value);

                if (!hasValue && !isRequired)
                    continue;

                // اگر required است یا optional ولی مقدار داده شده → اعتبارسنجی کن
                if (def.DataType == KycDataType.File)
                {
                    if (string.IsNullOrWhiteSpace(item.FilePath))
                        errors.Add($"آپلود فایل برای «{def.DisplayName}» اجباری است.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.Value))
                        errors.Add($"وارد کردن مقدار برای «{def.DisplayName}» اجباری است.");
                }

                if (item.DataType != def.DataType)
                    errors.Add($"نوع داده برای «{def.DisplayName}» نامعتبر است.");
            }


            if (errors.Any())
                throw new ValidationException(errors);

            // Save
            foreach (var item in items)
            {
                var doc =
                    await _db.UserDocuments.FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.KycAttributeDefinitionId == item.AttributeDefinitionId &&
                        !x.IsDeleted);

                if (doc == null)
                {
                    doc = new UserDocument
                    {
                        UserId = userId,
                        KycAttributeDefinitionId = item.AttributeDefinitionId,
                        UploadedAt = DateTime.UtcNow,
                        Status = DocumentStatus.Pending
                    };

                    _db.UserDocuments.Add(doc);
                }
                else
                {
                    doc.UploadedAt = DateTime.UtcNow;
                }

                doc.Value = item.Value;
                doc.FilePath = item.FilePath;
                doc.Status = DocumentStatus.Pending;
                doc.ReviewedAt = null;
            }

            // وضعیت کلی تأمین‌کننده بعد از ارسال مدارک باید «در حال بررسی» شود
            supplier.VerificationStatus = VerificationStatus.Pending;
            supplier.RejectionReason = null;
            supplier.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ==========================================================
        // 3) مدارک فعلی کاربر
        // ==========================================================
        public async Task<List<UserDocumentDto>> GetUserDocumentsAsync(Guid userId)
        {
            var query =
                from d in _db.UserDocuments.AsNoTracking()
                where !d.IsDeleted
                join def in _db.KycAttributeDefinitions.AsNoTracking()
                    on d.KycAttributeDefinitionId equals def.Id
                where d.UserId == userId
                orderby d.UploadedAt descending
                select new UserDocumentDto
                {
                    Id = d.Id,
                    AttributeDefinitionId = def.Id,
                    AttributeDisplayName = def.DisplayName,
                    Value = d.Value,
                    FilePath = d.FilePath,
                    Status = d.Status,
                    AdminNote = d.AdminNote,
                    UploadedAt = d.UploadedAt,
                    ReviewedAt = d.ReviewedAt
                };

            return await query.ToListAsync();
        }

        // ==========================================================
        // 4) جزئیات مدرک
        // ==========================================================
        public async Task<UserDocumentDto?> GetDocumentByIdAsync(Guid documentId)
        {
            var query =
                from d in _db.UserDocuments.AsNoTracking()
                where !d.IsDeleted
                join def in _db.KycAttributeDefinitions.AsNoTracking()
                    on d.KycAttributeDefinitionId equals def.Id
                where d.Id == documentId
                select new UserDocumentDto
                {
                    Id = d.Id,
                    AttributeDefinitionId = def.Id,
                    AttributeDisplayName = def.DisplayName,
                    Value = d.Value,
                    FilePath = d.FilePath,
                    Status = d.Status,
                    AdminNote = d.AdminNote,
                    UploadedAt = d.UploadedAt,
                    ReviewedAt = d.ReviewedAt
                };

            return await query.FirstOrDefaultAsync();
        }

        // ==========================================================
        // 5) بررسی/تأیید/رد مدرک
        // ==========================================================
        public async Task ReviewDocumentAsync(Guid documentId, ReviewKycDocumentRequest request)
        {
            var doc = await _db.UserDocuments
                .FirstOrDefaultAsync(x => x.Id == documentId && !x.IsDeleted)
                ?? throw new NotFoundException("مدرک موردنظر یافت نشد.");

            if (request.Status == DocumentStatus.Rejected &&
                string.IsNullOrWhiteSpace(request.AdminNote))
            {
                throw new ValidationException(new()
                {
                    "در صورت رد کردن مدرک، درج دلیل الزامی است."
                });
            }

            doc.Status = request.Status;
            doc.AdminNote = request.AdminNote;
            doc.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
