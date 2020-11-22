CREATE TABLE [dbo].[tbl_User] (
    [Id]                   UNIQUEIDENTIFIER   NOT NULL,
    [UserName]             NVARCHAR (128)     NOT NULL,
    [EmailAddress]         NVARCHAR (128)     NULL,
    [EmailConfirmed]       BIT                NOT NULL,
    [FirstName]            NVARCHAR (128)     NOT NULL,
    [LastName]             NVARCHAR (128)     NOT NULL,
    [PhoneNumber]          NVARCHAR (16)      NULL,
    [PhoneNumberConfirmed] BIT                NULL,
    [ConcurrencyStamp]     NVARCHAR (1024)    NOT NULL,
    [PasswordHashPBKDF2]   NVARCHAR (2048)    NULL,
    [PasswordHashSHA256]   NVARCHAR (2048)    NULL,
    [PasswordConfirmed]    BIT                NOT NULL,
    [SecurityStamp]        NVARCHAR (1024)    NOT NULL,
    [IsHumanBeing]         BIT                NOT NULL,
    [IsLockedOut]          BIT                NOT NULL,
    [IsDeletable]          BIT                NOT NULL,
    [AccessFailedCount]    INT                NOT NULL,
    [AccessSuccessCount]   INT                NOT NULL,
    [LockoutEndUtc]        DATETIMEOFFSET (7) NULL,
    [LastLoginSuccessUtc]  DATETIMEOFFSET (7) NULL,
    [LastLoginFailureUtc]  DATETIMEOFFSET (7) NULL,
    [CreatedUtc]           DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]       DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);






















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_User]
    ON [dbo].[tbl_User]([Id] ASC);

