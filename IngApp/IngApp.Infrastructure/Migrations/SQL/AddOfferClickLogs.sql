-- ============================================================
-- Migration: Add OfferClickLogs Table
-- Date: 2026-01-03
-- Description: جدول لاگ کلیک‌های روی آگهی‌ها (View و ContactClick)
-- ============================================================

USE [AliHoor_IngApp]
GO

-- بررسی وجود جدول
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[IngAppUser].[OfferClickLogs]') AND type in (N'U'))
BEGIN
    PRINT 'Creating table OfferClickLogs...'
    
    CREATE TABLE [IngAppUser].[OfferClickLogs] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OfferId] int NOT NULL,
        [UserId] uniqueidentifier NULL,
        [ClickType] int NOT NULL,
        [IpAddress] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [ClickedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_OfferClickLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OfferClickLogs_Offers_OfferId] FOREIGN KEY ([OfferId]) 
            REFERENCES [IngAppUser].[Offers] ([Id]) ON DELETE NO ACTION
    );

    PRINT 'Creating indexes...'
    
    -- Index برای بهبود performance در query‌های آمار
    CREATE INDEX [IX_OfferClickLogs_OfferId_ClickType] 
        ON [IngAppUser].[OfferClickLogs] ([OfferId], [ClickType]);
    
    CREATE INDEX [IX_OfferClickLogs_ClickedAt] 
        ON [IngAppUser].[OfferClickLogs] ([ClickedAt]);

    PRINT 'Table OfferClickLogs created successfully!'
END
ELSE
BEGIN
    PRINT 'Table OfferClickLogs already exists. Skipping...'
END
GO

-- ============================================================
-- توضیحات:
-- ClickType: 1 = View (بازدید آگهی), 2 = ContactClick (کلیک روی اطلاعات تماس)
-- ============================================================

