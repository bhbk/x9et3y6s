CREATE TABLE [dbo].[tbl_Users] (
    [Id]                   UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]              UNIQUEIDENTIFIER   NULL,
    [UserName]             NVARCHAR (128)     NOT NULL,
    [EmailAddress]         NVARCHAR (128)     NULL,
    [EmailConfirmed]       BIT                CONSTRAINT [DF_tbl_Users_UserConfirmed] DEFAULT ((0)) NOT NULL,
    [FirstName]            NVARCHAR (128)     NOT NULL,
    [LastName]             NVARCHAR (128)     NOT NULL,
    [PhoneNumber]          NVARCHAR (16)      NULL,
    [PhoneNumberConfirmed] BIT                CONSTRAINT [DF_tbl_Users_PhoneNumberConfirmed] DEFAULT ((0)) NULL,
    [Created]              DATETIME2 (7)      NOT NULL,
    [LastUpdated]          DATETIME2 (7)      NULL,
    [LockoutEnabled]       BIT                CONSTRAINT [DF_tbl_Users_LockoutEnabled] DEFAULT ((0)) NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LastLoginSuccess]     DATETIME2 (7)      NULL,
    [LastLoginFailure]     DATETIME2 (7)      NULL,
    [AccessFailedCount]    INT                CONSTRAINT [DF_tbl_Users_LoginFailedCount] DEFAULT ((0)) NOT NULL,
    [AccessSuccessCount]   INT                CONSTRAINT [DF_tbl_Users_AccessSuccessCount] DEFAULT ((0)) NOT NULL,
    [ConcurrencyStamp]     NVARCHAR (1024)    NOT NULL,
    [PasswordHashPBKDF2]   NVARCHAR (2048)    NULL,
    [PasswordHashSHA256]   NVARCHAR (2048)    NULL,
    [PasswordConfirmed]    BIT                CONSTRAINT [DF_tbl_Users_PasswordConfirmed] DEFAULT ((0)) NOT NULL,
    [SecurityStamp]        NVARCHAR (1024)    NOT NULL,
    [TwoFactorEnabled]     BIT                CONSTRAINT [DF_tbl_Users_TwoFactorEnabled] DEFAULT ((0)) NOT NULL,
    [HumanBeing]           BIT                CONSTRAINT [DF_tbl_Users_HumanBeing] DEFAULT ((0)) NOT NULL,
    [Immutable]            BIT                CONSTRAINT [DF_tbl_Users_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users]
    ON [dbo].[tbl_Users]([Id] ASC);

