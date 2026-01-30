# PowerShell Script برای راه‌اندازی دیتابیس محلی
# این اسکریپت را در PowerShell با Run as Administrator اجرا کنید

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "راه‌اندازی دیتابیس محلی IngApp" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# بررسی LocalDB
Write-Host "`nبررسی LocalDB..." -ForegroundColor Yellow
$localdbExists = sqllocaldb info MSSQLLocalDB 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "LocalDB یافت نشد. در حال ایجاد..." -ForegroundColor Yellow
    sqllocaldb create MSSQLLocalDB
    if ($LASTEXITCODE -eq 0) {
        Write-Host "LocalDB با موفقیت ایجاد شد." -ForegroundColor Green
    } else {
        Write-Host "خطا در ایجاد LocalDB. لطفاً SQL Server LocalDB را نصب کنید." -ForegroundColor Red
        exit 1
    }
}

# شروع LocalDB
Write-Host "`nشروع LocalDB..." -ForegroundColor Yellow
sqllocaldb start MSSQLLocalDB
if ($LASTEXITCODE -eq 0) {
    Write-Host "LocalDB با موفقیت شروع شد." -ForegroundColor Green
} else {
    Write-Host "خطا در شروع LocalDB." -ForegroundColor Red
}

# نمایش اطلاعات LocalDB
Write-Host "`nاطلاعات LocalDB:" -ForegroundColor Yellow
sqllocaldb info MSSQLLocalDB

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "مرحله بعدی:" -ForegroundColor Cyan
Write-Host "1. دیتابیس را با Migration ایجاد کنید:" -ForegroundColor White
Write-Host "   cd IngApp\IngApp.Infrastructure" -ForegroundColor Gray
Write-Host "   dotnet ef database update --startup-project ../IngApp.Api/IngApp.Api.csproj" -ForegroundColor Gray
Write-Host "`n2. یا از SQL Server Management Studio استفاده کنید" -ForegroundColor White
Write-Host "   Server: (localdb)\MSSQLLocalDB" -ForegroundColor Gray
Write-Host "   Database: IngApp_Local" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan












