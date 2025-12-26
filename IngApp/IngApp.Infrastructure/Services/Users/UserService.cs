using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Users.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // -------------------- Create User --------------------
        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var phone = dto.PhoneNumber?.Trim();

            if (string.IsNullOrWhiteSpace(phone))
                throw new ValidationException(new() { "شماره موبایل اجباری است." });

            var exists = await _context.Users.AnyAsync(u => u.PhoneNumber == phone);

            if (exists)
                throw new ValidationException(new() { "کاربری با این شماره موبایل قبلاً ثبت شده است." });

            var user = new User
            {
                PhoneNumber = phone,
                DisplayName = dto.DisplayName?.Trim(),
                UserType = dto.UserType,
                SubscriptionLevel = dto.SubscriptionLevel,
                VerificationStatus = dto.VerificationStatus,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // نقش‌های اولیه
            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                var roles = await _context.Roles
                    .Where(r => dto.RoleIds.Contains(r.Id))
                    .Select(r => r.Id)
                    .ToListAsync();

                foreach (var roleId in roles)
                {
                    user.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId
                    });
                }
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var created = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (created == null)
                throw new AppException("خطا در خواندن کاربر بعد از ایجاد.");

            return MapToUserDto(created);
        }

        // -------------------- Get All --------------------
        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .ToListAsync();

            return users.Select(MapToUserDto).ToList();
        }

        // -------------------- Get Paged with Filters --------------------
        public async Task<PagedResult<UserDto>> GetPagedAsync(UserListQueryDto filter)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
                query = query.Where(u => u.PhoneNumber.Contains(filter.PhoneNumber.Trim()));

            if (!string.IsNullOrWhiteSpace(filter.DisplayName))
                query = query.Where(u =>
                    u.DisplayName != null &&
                    u.DisplayName.Contains(filter.DisplayName.Trim()));

            if (filter.UserType.HasValue)
                query = query.Where(u => u.UserType == filter.UserType.Value);

            if (filter.SubscriptionLevel.HasValue)
                query = query.Where(u => u.SubscriptionLevel == filter.SubscriptionLevel.Value);

            if (filter.VerificationStatus.HasValue)
                query = query.Where(u => u.VerificationStatus == filter.VerificationStatus.Value);

            if (filter.RoleId.HasValue)
                query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == filter.RoleId.Value));

            var totalCount = await query.CountAsync();

            var sortBy = (filter.SortBy ?? "").ToLowerInvariant();
            var desc = filter.SortDesc;

            query = sortBy switch
            {
                "phonenumber" or "mobile" =>
                    desc ? query.OrderByDescending(u => u.PhoneNumber)
                         : query.OrderBy(u => u.PhoneNumber),

                "displayname" =>
                    desc ? query.OrderByDescending(u => u.DisplayName)
                         : query.OrderBy(u => u.DisplayName),

                "usertype" =>
                    desc ? query.OrderByDescending(u => u.UserType)
                         : query.OrderBy(u => u.UserType),

                "subscriptionlevel" =>
                    desc ? query.OrderByDescending(u => u.SubscriptionLevel)
                         : query.OrderBy(u => u.SubscriptionLevel),

                "verificationstatus" =>
                    desc ? query.OrderByDescending(u => u.VerificationStatus)
                         : query.OrderBy(u => u.VerificationStatus),

                "createdat" =>
                    desc ? query.OrderByDescending(u => u.CreatedAt)
                         : query.OrderBy(u => u.CreatedAt),

                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtoList = users.Select(MapToUserDto).ToList();

            return new PagedResult<UserDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = dtoList
            };
        }

        // -------------------- Get By Id --------------------
        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : MapToUserDto(user);
        }

        // -------------------- Assign Role --------------------
        public async Task AssignRoleAsync(Guid userId, Guid roleId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new NotFoundException("کاربر پیدا نشد.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new NotFoundException("نقش یافت نشد.");

            if (user.UserRoles.Any(ur => ur.RoleId == roleId))
                return;

            user.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });

            await _context.SaveChangesAsync();
        }

        // -------------------- Remove Role --------------------
        public async Task RemoveRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
                return;

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }

        // -------------------- Update User --------------------
        public async Task UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new NotFoundException("کاربر پیدا نشد.");

            user.PhoneNumber = dto.PhoneNumber.Trim();
            user.DisplayName = dto.DisplayName?.Trim();
            user.UserType = dto.UserType;
            user.SubscriptionLevel = dto.SubscriptionLevel;
            user.VerificationStatus = dto.VerificationStatus;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // -------------------- Change Status --------------------
        public async Task ChangeStatusAsync(Guid userId, ChangeUserStatusDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new NotFoundException("کاربر پیدا نشد.");

            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // -------------------- Private Mapper --------------------
        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                DisplayName = user.DisplayName,
                UserType = user.UserType,
                SubscriptionLevel = user.SubscriptionLevel,
                VerificationStatus = user.VerificationStatus,
                IsActive = user.IsActive,
                Roles = user.UserRoles
                    .Where(ur => ur.Role != null)
                    .Select(ur => ur.Role!.Name)
                    .ToList()
            };
        }
    }
}
