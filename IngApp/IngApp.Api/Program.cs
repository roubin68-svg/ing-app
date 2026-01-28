using IngApp.Api.Authorization;
using IngApp.Api.Common;
using IngApp.Api.Common.Swagger;
using IngApp.Api.Middlewares;
using IngApp.Application.Common.Security;
using IngApp.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------
// 1. Add Services
// -------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IngApp.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }});

    c.OperationFilter<SwaggerAuthOperationFilter>();
});


// Infrastructure (DbContext + Auth Services + Others)
builder.Services.AddInfrastructure(builder.Configuration);



// -------------------------------------------------
// 2. CORS
// -------------------------------------------------
var corsPolicyName = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// -------------------------------------------------
// 3. JWT Authentication
// -------------------------------------------------
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization(options =>
{
    // ===================== Products ===========================
    options.AddPolicy(Permissions.Products.ViewOwn,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Products.ViewOwn)));

    options.AddPolicy(Permissions.Products.ViewAll,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Products.ViewAll)));

    options.AddPolicy(Permissions.Products.Create,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Products.Create)));

    options.AddPolicy(Permissions.Products.Update,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Products.Update)));

    options.AddPolicy(Permissions.Products.Delete,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Products.Delete)));

    // ===================== Roles ==============================
    options.AddPolicy(Permissions.Roles.Manage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Roles.Manage)));

    // ===================== Users ==============================
    options.AddPolicy(Permissions.Users.Manage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Users.Manage)));

    options.AddPolicy(Permissions.Users.View,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Users.View)));

    // ===================== Permissions Module ==================
    options.AddPolicy(Permissions.PermissionsModule.Manage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.PermissionsModule.Manage)));

    // ===================== Menus ===============================
    options.AddPolicy(Permissions.Menus.Manage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Menus.Manage)));

    // ===================== Visitors =============================
    options.AddPolicy(Permissions.Visitors.View,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Visitors.View)));

    options.AddPolicy(Permissions.Visitors.Manage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Visitors.Manage)));

    // ===================== Financial =============================
    options.AddPolicy(Permissions.Financial.Manage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Financial.Manage)));

    options.AddPolicy(Permissions.Financial.CommissionRuleManage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Financial.CommissionRuleManage)));

    options.AddPolicy(Permissions.Financial.WalletManage,
        p => p.Requirements.Add(new PermissionRequirement(Permissions.Financial.WalletManage)));
});

builder.Services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();


// -------------------------------------------------
// 4. Build App
// -------------------------------------------------
var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
// -------------------------------------------------
// 5. Middlewares
// -------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -------------------------------------------------
app.Run();
