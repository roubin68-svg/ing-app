using IngApp.Application.Common.Interfaces.Authentication;
using IngApp.Application.Common.Interfaces.Kyc;
using IngApp.Application.Common.Interfaces.Menus;
using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Application.Common.Interfaces.Permissions;
using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Interfaces.Roles;
using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Common.Interfaces.Users;
using IngApp.Infrastructure.Persistence;
using IngApp.Infrastructure.Repositories;
using IngApp.Infrastructure.Services.Auth;
using IngApp.Infrastructure.Services.Kyc;
using IngApp.Infrastructure.Services.Menus;
using IngApp.Infrastructure.Services.Offers;
using IngApp.Infrastructure.Services.Permissions;
using IngApp.Infrastructure.Services.Products;
using IngApp.Infrastructure.Services.Roles;
using IngApp.Infrastructure.Services.Sms;
using IngApp.Infrastructure.Services.Suppliers;
using IngApp.Infrastructure.Services.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IngApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.CommandTimeout(300) // 5 دقیقه timeout برای migration های طولانی
            ));

        // Auth Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Role Services
        services.AddScoped<IRoleService, RoleService>();

        // Permission Services
        services.AddScoped<IPermissionService, PermissionService>();

        // Menu Services
        services.AddScoped<IMenuService, MenuService>();

        // User Services  🔥 این خطِ مهم بود که نداشتی
        services.AddScoped<IUserService, UserService>();

        // OTP / SMS
        services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<SmsIrSender>();
        services.AddHttpClient<SmsIrSender>();

        // Supplier Services
        services.AddScoped<ISupplierProfileService, SupplierProfileService>();
        services.AddScoped<IKycService, KycService>();
        services.AddScoped<ISupplierTypeService, SupplierTypeService>();
        services.AddScoped<IKycFileStorageService, KycFileStorageService>();
        services.AddScoped<IKycAttributeDefinitionService, KycAttributeDefinitionService>();
        services.AddScoped<IKycTemplateService, KycTemplateService>();

        // Product Services
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductAttributeDefinitionService, ProductAttributeDefinitionService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductAttributeTemplateService, ProductAttributeTemplateService>();
        services.AddScoped<ISupplierCategoryAccessService, SupplierCategoryAccessService>();

        //Offer Services
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IOfferClickService, OfferClickService>();
        services.AddScoped<IOfferFileStorageService, OfferFileStorageService>();
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IOfferClickService, OfferClickService>();



        return services;
    }
}
