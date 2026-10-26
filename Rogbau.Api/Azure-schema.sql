IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [MustChangePassword] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902195004_Config'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260902195004_Config', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903171027_InitialIdentity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903171027_InitialIdentity', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906171814_EquipmentModel'
)
BEGIN
    CREATE TABLE [Equipments] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Unit] nvarchar(20) NOT NULL DEFAULT N'szt.',
        [Weightkg] decimal(18,2) NOT NULL,
        [PurchasedQuantity] int NOT NULL,
        [QuanityTotal] int NOT NULL,
        [QuanityAvailable] int NOT NULL,
        [QuanityScrapped] int NOT NULL,
        [DayPrice] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_Equipments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906171814_EquipmentModel'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Equipments_Name] ON [Equipments] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906171814_EquipmentModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906171814_EquipmentModel', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906173253_M2'
)
BEGIN
    EXEC sp_rename N'[Equipments].[QuanityTotal]', N'QuantityTotal', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906173253_M2'
)
BEGIN
    EXEC sp_rename N'[Equipments].[QuanityAvailable]', N'QuantityAvailable', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906173253_M2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906173253_M2', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906175504_M3'
)
BEGIN
    ALTER TABLE [Equipments] ADD [InitialQuantity] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906175504_M3'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906175504_M3', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907085659_ChangeColumnNameInEquipment'
)
BEGIN
    EXEC sp_rename N'[Equipments].[Weightkg]', N'WeightKg', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907085659_ChangeColumnNameInEquipment'
)
BEGIN
    EXEC sp_rename N'[Equipments].[QuanityScrapped]', N'QuantityScrapped', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907085659_ChangeColumnNameInEquipment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907085659_ChangeColumnNameInEquipment', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907094426_AddIsActiveToTableEquipment'
)
BEGIN
    ALTER TABLE [Equipments] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907094426_AddIsActiveToTableEquipment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907094426_AddIsActiveToTableEquipment', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908104335_AddPurchaseEntity'
)
BEGIN
    CREATE TABLE [Supplier] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_Supplier] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908104335_AddPurchaseEntity'
)
BEGIN
    CREATE TABLE [Purchases] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseNumber] nvarchar(50) NOT NULL,
        [SupplierId] uniqueidentifier NOT NULL,
        [PurchaseDate] date NOT NULL,
        [TotalQuantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_Purchases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Purchases_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908104335_AddPurchaseEntity'
)
BEGIN
    CREATE TABLE [PurchaseItems] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseId] uniqueidentifier NOT NULL,
        [EquipmentId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_PurchaseItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseItems_Equipments_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Equipments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseItems_Purchases_PurchaseId] FOREIGN KEY ([PurchaseId]) REFERENCES [Purchases] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908104335_AddPurchaseEntity'
)
BEGIN
    CREATE INDEX [IX_PurchaseItems_EquipmentId] ON [PurchaseItems] ([EquipmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908104335_AddPurchaseEntity'
)
BEGIN
    CREATE INDEX [IX_PurchaseItems_PurchaseId] ON [PurchaseItems] ([PurchaseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908104335_AddPurchaseEntity'
)
BEGIN
    CREATE INDEX [IX_Purchases_SupplierId] ON [Purchases] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908104335_AddPurchaseEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908104335_AddPurchaseEntity', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908120226_AddConfigurationinSupplier'
)
BEGIN
    ALTER TABLE [Purchases] DROP CONSTRAINT [FK_Purchases_Supplier_SupplierId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908120226_AddConfigurationinSupplier'
)
BEGIN
    ALTER TABLE [Supplier] DROP CONSTRAINT [PK_Supplier];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908120226_AddConfigurationinSupplier'
)
BEGIN
    EXEC sp_rename N'[Supplier]', N'Suppliers', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908120226_AddConfigurationinSupplier'
)
BEGIN
    ALTER TABLE [Suppliers] ADD CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908120226_AddConfigurationinSupplier'
)
BEGIN
    ALTER TABLE [Purchases] ADD CONSTRAINT [FK_Purchases_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908120226_AddConfigurationinSupplier'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908120226_AddConfigurationinSupplier', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908151434_AddEntitySites'
)
BEGIN
    CREATE TABLE [Sites] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(500) NULL,
        [StartDate] date NULL,
        [EndDate] date NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_Sites] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908151434_AddEntitySites'
)
BEGIN
    CREATE TABLE [SiteEquipments] (
        [Id] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NOT NULL,
        [EquipmentId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_SiteEquipments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SiteEquipments_Equipments_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Equipments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SiteEquipments_Sites_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [Sites] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908151434_AddEntitySites'
)
BEGIN
    CREATE INDEX [IX_SiteEquipments_EquipmentId] ON [SiteEquipments] ([EquipmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908151434_AddEntitySites'
)
BEGIN
    CREATE INDEX [IX_SiteEquipments_SiteId] ON [SiteEquipments] ([SiteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908151434_AddEntitySites'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908151434_AddEntitySites', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908193722_EntitySite'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sites_Name] ON [Sites] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908193722_EntitySite'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908193722_EntitySite', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    ALTER TABLE [Equipments] ADD [CategoryId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    ALTER TABLE [Equipments] ADD [PurchasePrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    ALTER TABLE [Equipments] ADD [SubCategoryId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    CREATE TABLE [SubCategories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdateedAt] datetime2 NULL,
        CONSTRAINT [PK_SubCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    CREATE INDEX [IX_Equipments_CategoryId] ON [Equipments] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    CREATE INDEX [IX_Equipments_SubCategoryId] ON [Equipments] ([SubCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    CREATE INDEX [IX_SubCategories_CategoryId] ON [SubCategories] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    ALTER TABLE [Equipments] ADD CONSTRAINT [FK_Equipments_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    ALTER TABLE [Equipments] ADD CONSTRAINT [FK_Equipments_SubCategories_SubCategoryId] FOREIGN KEY ([SubCategoryId]) REFERENCES [SubCategories] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910093834_AddCategoryEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910093834_AddCategoryEntity', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916150858_NotificationEntity'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [Message] nvarchar(200) NOT NULL,
        [ActionUrl] nvarchar(100) NULL,
        [CreatetAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916150858_NotificationEntity'
)
BEGIN
    CREATE TABLE [Recipients] (
        [Id] uniqueidentifier NOT NULL,
        [NotificationId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        CONSTRAINT [PK_Recipients] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Recipients_Notifications_NotificationId] FOREIGN KEY ([NotificationId]) REFERENCES [Notifications] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916150858_NotificationEntity'
)
BEGIN
    CREATE INDEX [IX_Recipients_NotificationId] ON [Recipients] ([NotificationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916150858_NotificationEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260916150858_NotificationEntity', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916153758_M5'
)
BEGIN
    ALTER TABLE [Recipients] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916153758_M5'
)
BEGIN
    ALTER TABLE [Recipients] ADD [UpdateedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916153758_M5'
)
BEGIN
    ALTER TABLE [Notifications] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916153758_M5'
)
BEGIN
    ALTER TABLE [Notifications] ADD [UpdateedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916153758_M5'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260916153758_M5', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    DROP INDEX [IX_Equipments_Name] ON [Equipments];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'CreatetAt');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Notifications] DROP COLUMN [CreatetAt];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[Suppliers].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[SubCategories].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[Sites].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[SiteEquipments].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[Recipients].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[Purchases].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[PurchaseItems].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[Notifications].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[Equipments].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC sp_rename N'[Categories].[UpdateedAt]', N'UpdatedAt', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Equipments_Name] ON [Equipments] ([Name]) WHERE [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919081132_M6'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919081132_M6', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919161011_Add-Migration M7'
)
BEGIN
    ALTER TABLE [Purchases] ADD [TotalPurchasePrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919161011_Add-Migration M7'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [PurchasePrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919161011_Add-Migration M7'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919161011_Add-Migration M7', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001171718_AddOrderEntity'
)
BEGIN
    CREATE TABLE [Order] (
        [Id] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(450) NOT NULL,
        [SiteId] uniqueidentifier NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Order] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Order_Sites_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [Sites] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001171718_AddOrderEntity'
)
BEGIN
    CREATE TABLE [OrderItem] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [EquipmentId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_OrderItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItem_Equipments_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Equipments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItem_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001171718_AddOrderEntity'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Order_OrderNumber] ON [Order] ([OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001171718_AddOrderEntity'
)
BEGIN
    CREATE INDEX [IX_Order_SiteId] ON [Order] ([SiteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001171718_AddOrderEntity'
)
BEGIN
    CREATE INDEX [IX_OrderItem_EquipmentId] ON [OrderItem] ([EquipmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001171718_AddOrderEntity'
)
BEGIN
    CREATE INDEX [IX_OrderItem_OrderId] ON [OrderItem] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001171718_AddOrderEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001171718_AddOrderEntity', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Sites_SiteId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    ALTER TABLE [OrderItem] DROP CONSTRAINT [FK_OrderItem_Order_OrderId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    ALTER TABLE [Order] DROP CONSTRAINT [PK_Order];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    EXEC sp_rename N'[Order]', N'Orders', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    EXEC sp_rename N'[Orders].[IX_Order_SiteId]', N'IX_Orders_SiteId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    EXEC sp_rename N'[Orders].[IX_Order_OrderNumber]', N'IX_Orders_OrderNumber', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    ALTER TABLE [OrderItem] ADD CONSTRAINT [FK_OrderItem_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Sites_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [Sites] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001184421_AddOrders'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001184421_AddOrders', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    CREATE TABLE [EquipmentMovements] (
        [Id] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [Status] int NOT NULL,
        [FromConstructionSiteId] uniqueidentifier NULL,
        [ToConstructionSiteId] uniqueidentifier NULL,
        [AddedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EquipmentMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EquipmentMovements_AspNetUsers_AddedByUserId] FOREIGN KEY ([AddedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EquipmentMovements_Sites_FromConstructionSiteId] FOREIGN KEY ([FromConstructionSiteId]) REFERENCES [Sites] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EquipmentMovements_Sites_ToConstructionSiteId] FOREIGN KEY ([ToConstructionSiteId]) REFERENCES [Sites] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    CREATE TABLE [EquipmentMovementItems] (
        [Id] uniqueidentifier NOT NULL,
        [EquipmentMovementId] uniqueidentifier NOT NULL,
        [EquipmentId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [ScrappedQuantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EquipmentMovementItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EquipmentMovementItems_EquipmentMovements_EquipmentMovementId] FOREIGN KEY ([EquipmentMovementId]) REFERENCES [EquipmentMovements] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EquipmentMovementItems_Equipments_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Equipments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    CREATE INDEX [IX_EquipmentMovementItems_EquipmentId] ON [EquipmentMovementItems] ([EquipmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    CREATE INDEX [IX_EquipmentMovementItems_EquipmentMovementId] ON [EquipmentMovementItems] ([EquipmentMovementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    CREATE INDEX [IX_EquipmentMovements_AddedByUserId] ON [EquipmentMovements] ([AddedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    CREATE INDEX [IX_EquipmentMovements_FromConstructionSiteId] ON [EquipmentMovements] ([FromConstructionSiteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    CREATE INDEX [IX_EquipmentMovements_ToConstructionSiteId] ON [EquipmentMovements] ([ToConstructionSiteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002144432_AddTransferEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002144432_AddTransferEntity', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003112631_AddQuantityScrapped'
)
BEGIN
    ALTER TABLE [Equipments] ADD [QuantityInTransit] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003112631_AddQuantityScrapped'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003112631_AddQuantityScrapped', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003124130_AddNumber'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Equipments]') AND [c].[name] = N'QuantityTotal');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Equipments] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Equipments] DROP COLUMN [QuantityTotal];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003124130_AddNumber'
)
BEGIN
    ALTER TABLE [EquipmentMovements] ADD [Number] nvarchar(450) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003124130_AddNumber'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EquipmentMovements_Number] ON [EquipmentMovements] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003124130_AddNumber'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003124130_AddNumber', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193705_AddSiteUserEntity'
)
BEGIN
    CREATE TABLE [SiteUser] (
        [SiteId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_SiteUser] PRIMARY KEY ([SiteId], [UserId]),
        CONSTRAINT [FK_SiteUser_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SiteUser_Sites_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [Sites] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193705_AddSiteUserEntity'
)
BEGIN
    CREATE INDEX [IX_SiteUser_UserId] ON [SiteUser] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193705_AddSiteUserEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003193705_AddSiteUserEntity', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    ALTER TABLE [SiteUser] DROP CONSTRAINT [FK_SiteUser_AspNetUsers_UserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    ALTER TABLE [SiteUser] DROP CONSTRAINT [FK_SiteUser_Sites_SiteId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    ALTER TABLE [SiteUser] DROP CONSTRAINT [PK_SiteUser];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    EXEC sp_rename N'[SiteUser]', N'SiteUsers', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    EXEC sp_rename N'[SiteUsers].[IX_SiteUser_UserId]', N'IX_SiteUsers_UserId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    ALTER TABLE [SiteUsers] ADD CONSTRAINT [PK_SiteUsers] PRIMARY KEY ([SiteId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    ALTER TABLE [SiteUsers] ADD CONSTRAINT [FK_SiteUsers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    ALTER TABLE [SiteUsers] ADD CONSTRAINT [FK_SiteUsers_Sites_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [Sites] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003195129_M10'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003195129_M10', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004145429_AddEquipmentAllocation'
)
BEGIN
    CREATE TABLE [SiteEquipmentAllocations] (
        [Id] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NOT NULL,
        [EquipmentId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [DateFrom] datetime2 NOT NULL,
        [DateTo] datetime2 NULL,
        [DayPrice] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SiteEquipmentAllocations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SiteEquipmentAllocations_Equipments_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Equipments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SiteEquipmentAllocations_Sites_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [Sites] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004145429_AddEquipmentAllocation'
)
BEGIN
    CREATE INDEX [IX_SiteEquipmentAllocations_EquipmentId] ON [SiteEquipmentAllocations] ([EquipmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004145429_AddEquipmentAllocation'
)
BEGIN
    CREATE INDEX [IX_SiteEquipmentAllocations_SiteId] ON [SiteEquipmentAllocations] ([SiteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004145429_AddEquipmentAllocation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004145429_AddEquipmentAllocation', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073845_EditPrice'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Equipments]') AND [c].[name] = N'DayPrice');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Equipments] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Equipments] ALTER COLUMN [DayPrice] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073845_EditPrice'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006073845_EditPrice', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006100804_EquipmentOrdersFulfillment'
)
BEGIN
    ALTER TABLE [Orders] ADD [Note] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006100804_EquipmentOrdersFulfillment'
)
BEGIN
    ALTER TABLE [EquipmentMovements] ADD [OrderId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006100804_EquipmentOrdersFulfillment'
)
BEGIN
    CREATE INDEX [IX_EquipmentMovements_OrderId] ON [EquipmentMovements] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006100804_EquipmentOrdersFulfillment'
)
BEGIN
    ALTER TABLE [EquipmentMovements] ADD CONSTRAINT [FK_EquipmentMovements_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006100804_EquipmentOrdersFulfillment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006100804_EquipmentOrdersFulfillment', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009140000_PzReceiptQuantities'
)
BEGIN
    ALTER TABLE [EquipmentMovementItems] ADD [ReceivedQuantity] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009140000_PzReceiptQuantities'
)
BEGIN
    ALTER TABLE [EquipmentMovementItems] ADD [ReceivedScrappedQuantity] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009140000_PzReceiptQuantities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261009140000_PzReceiptQuantities', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009200000_MovementNotes'
)
BEGIN
    ALTER TABLE [EquipmentMovements] ADD [Note] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009200000_MovementNotes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261009200000_MovementNotes', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012120000_AllocationDayPricePrecision'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SiteEquipmentAllocations]') AND [c].[name] = N'DayPrice');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [SiteEquipmentAllocations] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [SiteEquipmentAllocations] ALTER COLUMN [DayPrice] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012120000_AllocationDayPricePrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261012120000_AllocationDayPricePrecision', N'10.0.11');
END;

COMMIT;
GO

