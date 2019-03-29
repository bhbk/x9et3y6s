CREATE TABLE [dbo].[AppUser] (
    [Id]                   UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]              UNIQUEIDENTIFIER   NULL,
    [Email]                NVARCHAR (MAX)     NOT NULL,
    [EmailConfirmed]       BIT                CONSTRAINT [DF_AppUser_EmailConfirmed] DEFAULT ((0)) NOT NULL,
    [FirstName]            NVARCHAR (MAX)     NOT NULL,
    [LastName]             NVARCHAR (MAX)     NOT NULL,
    [PhoneNumber]          NVARCHAR (16)      NULL,
    [PhoneNumberConfirmed] BIT                CONSTRAINT [DF_AppUser_PhoneNumberConfirmed] DEFAULT ((0)) NULL,
    [Created]              DATETIME2 (7)      NOT NULL,
    [LastUpdated]          DATETIME2 (7)      NULL,
    [LockoutEnabled]       BIT                CONSTRAINT [DF_AppUser_LockoutEnabled] DEFAULT ((0)) NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LastLoginSuccess]     DATETIME           NULL,
    [LastLoginFailure]     DATETIME           NULL,
    [AccessFailedCount]    INT                CONSTRAINT [DF_AppUser_LoginFailedCount] DEFAULT ((0)) NOT NULL,
    [AccessSuccessCount]   INT                CONSTRAINT [DF_AppUser_AccessSuccessCount] DEFAULT ((0)) NOT NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
    [PasswordHash]         NVARCHAR (MAX)     NULL,
    [PasswordConfirmed]    BIT                CONSTRAINT [DF_AppUser_PasswordConfirmed] DEFAULT ((0)) NOT NULL,
    [SecurityStamp]        NVARCHAR (MAX)     NULL,
    [TwoFactorEnabled]     BIT                CONSTRAINT [DF_AppUser_TwoFactorEnabled] DEFAULT ((0)) NOT NULL,
    [HumanBeing]           BIT                CONSTRAINT [DF_AppUser_HumanBeing] DEFAULT ((0)) NOT NULL,
    [Immutable]            BIT                CONSTRAINT [DF_AppUser_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AppUser_ID] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_AppUser_UserName]
    ON [dbo].[AppUser]([Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AppUser_Email]
    ON [dbo].[AppUser]([Id] ASC);

