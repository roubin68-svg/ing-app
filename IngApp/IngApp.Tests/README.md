# IngApp Tests

این پروژه شامل تست‌های Backend برای سیستم IngApp است.

## ساختار تست‌ها

### 1. Unit Tests
- **Services**: تست‌های واحد برای Services
- **Domain**: تست‌های Domain Logic

### 2. Integration Tests
- **API Controllers**: تست‌های Integration برای API Endpoints
- **Database**: تست‌های Integration با InMemory Database

## اجرای تست‌ها

```bash
# اجرای همه تست‌ها
dotnet test

# اجرای تست‌های خاص
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# با Coverage
dotnet test /p:CollectCoverage=true
```

## دسته‌بندی تست‌ها

- `[Fact]` - تست‌های عادی
- `[Theory]` - تست‌های پارامتری
- `[Trait("Category", "Unit")]` - تست‌های Unit
- `[Trait("Category", "Integration")]` - تست‌های Integration











