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
CREATE TABLE [Admins] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Admins] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
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

CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

CREATE TABLE [Clients] (
    [Id] uniqueidentifier NOT NULL,
    [DateOfBirth] datetime2 NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);

CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] nvarchar(max) NOT NULL,
    [TokenHash] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [RevokedAt] datetime2 NULL,
    [ReplacedByTokenHash] nvarchar(max) NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id])
);

CREATE TABLE [Staffs] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Staffs] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Services] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Duration] int NOT NULL,
    [TimeStart] time NOT NULL,
    [TimeEnd] time NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [IsDeleted] bit NOT NULL,
    [StaffId] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Services_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Services_Staffs_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staffs] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Bookings] (
    [Id] uniqueidentifier NOT NULL,
    [ServiceId] uniqueidentifier NOT NULL,
    [ClientId] uniqueidentifier NOT NULL,
    [Date] datetime2 NOT NULL,
    [Time] time NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bookings_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Bookings_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_Bookings_ClientId] ON [Bookings] ([ClientId]);

CREATE INDEX [IX_Bookings_ServiceId] ON [Bookings] ([ServiceId]);

CREATE INDEX [IX_Services_CategoryId] ON [Services] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Services_StaffId] ON [Services] ([StaffId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260303123245_InitialGuid', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [FAQs] (
    [Id] uniqueidentifier NOT NULL,
    [Question] nvarchar(max) NOT NULL,
    [Answer] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_FAQs] PRIMARY KEY ([Id])
);

CREATE TABLE [SupportTickets] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [Subject] nvarchar(50) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260304100916_SupportTicket_FAQ_Added', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [SupportTickets] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260304110604_AddCreatedAtToSupportTicket', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [ContactInfo] (
    [Id] uniqueidentifier NOT NULL,
    [Country] nvarchar(max) NOT NULL,
    [AddressLine_1] nvarchar(max) NULL,
    [AddressLine_2] nvarchar(max) NULL,
    [Email] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NOT NULL,
    [CallHourFrom] time NOT NULL,
    [CallHourTo] time NOT NULL,
    [CallDayFrom] int NOT NULL,
    [CallDayTo] int NOT NULL,
    CONSTRAINT [PK_ContactInfo] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260305212243_ContactInfo-Entity-Added', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [SupportTickets] ADD [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260307150242_SupportTickets_Updated', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Staffs] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Clients] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Admins] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260307182838_User_IsActive_Added', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AspNetUsers] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260307183914_UserEntities-Add-IsActive', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Services] ADD [Rating] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [Services] ADD [ReviewCount] int NOT NULL DEFAULT 0;

CREATE TABLE [Reviews] (
    [Id] uniqueidentifier NOT NULL,
    [ServiceId] uniqueidentifier NOT NULL,
    [ClientId] uniqueidentifier NOT NULL,
    [BookingId] uniqueidentifier NOT NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reviews_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Reviews_BookingId] ON [Reviews] ([BookingId]);

CREATE INDEX [IX_Reviews_ClientId] ON [Reviews] ([ClientId]);

CREATE INDEX [IX_Reviews_ServiceId] ON [Reviews] ([ServiceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260308114133_AddServiceRatings', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Staffs] ADD [Gender] int NOT NULL DEFAULT 0;

ALTER TABLE [Staffs] ADD [ImagePath] nvarchar(max) NULL;

ALTER TABLE [Clients] ADD [Gender] int NOT NULL DEFAULT 0;

ALTER TABLE [Clients] ADD [ImagePath] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260312115324_Update-User-entities', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Services] ADD [ImagePath] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260315194947_AddServiceImagePath', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260316194729_AddResetOtpFields', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AspNetUsers] ADD [ResetOtp] nvarchar(max) NULL;

ALTER TABLE [AspNetUsers] ADD [ResetOtpExpiry] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260316202735_Add-OTPsettings-ToUser', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [ServiceApprovalRequests] (
    [Id] uniqueidentifier NOT NULL,
    [ServiceId] uniqueidentifier NULL,
    [StaffId] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [ProposedData] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ActionedAt] datetime2 NULL,
    [ActionedBy] uniqueidentifier NULL,
    [AdminComment] nvarchar(max) NULL,
    CONSTRAINT [PK_ServiceApprovalRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceApprovalRequests_Admins_ActionedBy] FOREIGN KEY ([ActionedBy]) REFERENCES [Admins] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ServiceApprovalRequests_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ServiceApprovalRequests_Staffs_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staffs] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ServiceApprovalRequests_ActionedBy] ON [ServiceApprovalRequests] ([ActionedBy]);

CREATE INDEX [IX_ServiceApprovalRequests_ServiceId] ON [ServiceApprovalRequests] ([ServiceId]);

CREATE INDEX [IX_ServiceApprovalRequests_StaffId] ON [ServiceApprovalRequests] ([StaffId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260316214117_AddServiceApprovalRequest', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Payments] (
    [Id] uniqueidentifier NOT NULL,
    [StripePaymentIntentId] nvarchar(max) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [ClientId] uniqueidentifier NOT NULL,
    [ServiceId] uniqueidentifier NOT NULL,
    [BookingId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Payments_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Payments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]);

CREATE INDEX [IX_Payments_ClientId] ON [Payments] ([ClientId]);

CREATE INDEX [IX_Payments_ServiceId] ON [Payments] ([ServiceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260323185102_AddPaymentEntity', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Services] ADD [DeletedAt] datetime2 NULL;

ALTER TABLE [Services] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260323201947_Service-entity-updated', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_Payments_BookingId] ON [Payments];

CREATE UNIQUE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]) WHERE [BookingId] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260325184625_update-payment-entity', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_Reviews_BookingId] ON [Reviews];

CREATE UNIQUE INDEX [IX_Reviews_BookingId] ON [Reviews] ([BookingId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260403134158_Booking_with_Review', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260403134357_Booking_with_Review_updated', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Bookings] DROP CONSTRAINT [FK_Bookings_User_ClientId];

ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_User_ClientId];

ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_User_ClientId];

ALTER TABLE [ServiceApprovalRequests] DROP CONSTRAINT [FK_ServiceApprovalRequests_User_ActionedBy];

ALTER TABLE [ServiceApprovalRequests] DROP CONSTRAINT [FK_ServiceApprovalRequests_User_StaffId];

ALTER TABLE [Services] DROP CONSTRAINT [FK_Services_User_StaffId];

DROP TABLE [Notifications];

CREATE TABLE [Admins] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Admins] PRIMARY KEY ([Id])
);

CREATE TABLE [Clients] (
    [Id] uniqueidentifier NOT NULL,
    [Gender] int NOT NULL,
    [DateOfBirth] datetime2 NULL,
    [ImagePath] nvarchar(max) NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);

INSERT INTO Clients (Id, FullName, Phone, IsActive, Gender, DateOfBirth, ImagePath) SELECT Id, FullName, Phone, IsActive, Gender, DateOfBirth, ImagePath FROM [User] WHERE Discriminator = 'Client'

INSERT INTO Admins (Id, FullName, Phone, IsActive) SELECT Id, FullName, Phone, IsActive FROM [User] WHERE Discriminator = 'Admin'

DELETE FROM [User] WHERE Discriminator != 'Staff'

ALTER TABLE [User] DROP CONSTRAINT [PK_User];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'DateOfBirth');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [User] DROP COLUMN [DateOfBirth];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'Discriminator');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [User] DROP COLUMN [Discriminator];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'FcmToken');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [User] DROP COLUMN [FcmToken];

EXEC sp_rename N'[User]', N'Staffs', 'OBJECT';

ALTER TABLE [Staffs] ADD CONSTRAINT [PK_Staffs] PRIMARY KEY ([Id]);

ALTER TABLE [Bookings] ADD CONSTRAINT [FK_Bookings_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [ServiceApprovalRequests] ADD CONSTRAINT [FK_ServiceApprovalRequests_Admins_ActionedBy] FOREIGN KEY ([ActionedBy]) REFERENCES [Admins] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [ServiceApprovalRequests] ADD CONSTRAINT [FK_ServiceApprovalRequests_Staffs_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staffs] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Services] ADD CONSTRAINT [FK_Services_Staffs_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staffs] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260404140948_CleanupFailedNotificationAttempt', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Bookings] DROP CONSTRAINT [FK_Bookings_Clients_ClientId];

ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Clients_ClientId];

ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_Clients_ClientId];

ALTER TABLE [ServiceApprovalRequests] DROP CONSTRAINT [FK_ServiceApprovalRequests_Admins_ActionedBy];

ALTER TABLE [ServiceApprovalRequests] DROP CONSTRAINT [FK_ServiceApprovalRequests_Staffs_StaffId];

ALTER TABLE [Services] DROP CONSTRAINT [FK_Services_Staffs_StaffId];

ALTER TABLE [Staffs] DROP CONSTRAINT [PK_Staffs];

EXEC sp_rename N'[Staffs]', N'User', 'OBJECT';

ALTER TABLE [User] ADD CONSTRAINT [PK_User] PRIMARY KEY ([Id]);

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'Gender');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [User] ALTER COLUMN [Gender] int NULL;

ALTER TABLE [User] ADD [Client_Gender] int NULL;

ALTER TABLE [User] ADD [Client_ImagePath] nvarchar(max) NULL;

ALTER TABLE [User] ADD [DateOfBirth] datetime2 NULL;

ALTER TABLE [User] ADD [Discriminator] nvarchar(8) NOT NULL DEFAULT N'';

UPDATE [User] SET Discriminator = 'Staff'

INSERT INTO [User] (Id, FullName, Phone, IsActive, Discriminator, Client_Gender, DateOfBirth, Client_ImagePath) SELECT Id, FullName, Phone, IsActive, 'Client', Gender, DateOfBirth, ImagePath FROM Clients

INSERT INTO [User] (Id, FullName, Phone, IsActive, Discriminator) SELECT Id, FullName, Phone, IsActive, 'Admin' FROM Admins

DROP TABLE [Admins];

DROP TABLE [Clients];

CREATE TABLE [Notifications] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Message] nvarchar(500) NOT NULL,
    [Type] int NOT NULL,
    [IsRead] bit NOT NULL,
    [ReferenceId] uniqueidentifier NULL,
    [RedirectUrl] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Notifications_UserId_CreatedAt] ON [Notifications] ([UserId], [CreatedAt]);

ALTER TABLE [Bookings] ADD CONSTRAINT [FK_Bookings_User_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [User] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_User_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_User_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [ServiceApprovalRequests] ADD CONSTRAINT [FK_ServiceApprovalRequests_User_ActionedBy] FOREIGN KEY ([ActionedBy]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [ServiceApprovalRequests] ADD CONSTRAINT [FK_ServiceApprovalRequests_User_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Services] ADD CONSTRAINT [FK_Services_User_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260404144349_AddNotifications', N'10.0.2');

COMMIT;
GO

