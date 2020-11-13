CREATE TABLE [dbo].[tbl_User] (
    [Id]                   UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]              UNIQUEIDENTIFIER   NULL,
    [UserName]             NVARCHAR (128)     NOT NULL,
    [EmailAddress]         NVARCHAR (128)     NULL,
    [EmailConfirmed]       BIT                CONSTRAINT [DF_tbl_User_EmailConfirmed] DEFAULT ((0)) NOT NULL,
    [FirstName]            NVARCHAR (128)     NOT NULL,
    [LastName]             NVARCHAR (128)     NOT NULL,
    [PhoneNumber]          NVARCHAR (16)      NULL,
    [PhoneNumberConfirmed] BIT                CONSTRAINT [DF_tbl_User_PhoneNumberConfirmed] DEFAULT ((0)) NULL,
    [ConcurrencyStamp]     NVARCHAR (1024)    NOT NULL,
    [PasswordHashPBKDF2]   NVARCHAR (2048)    NULL,
    [PasswordHashSHA256]   NVARCHAR (2048)    NULL,
    [PasswordConfirmed]    BIT                CONSTRAINT [DF_tbl_User_PasswordConfirmed] DEFAULT ((0)) NOT NULL,
    [SecurityStamp]        NVARCHAR (1024)    NOT NULL,
    [IsHumanBeing]         BIT                NOT NULL,
    [IsMultiFactor]        BIT                NOT NULL,
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

