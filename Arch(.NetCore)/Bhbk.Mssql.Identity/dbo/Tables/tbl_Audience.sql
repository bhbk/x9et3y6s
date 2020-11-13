CREATE TABLE [dbo].[tbl_Audience] (
    [Id]                  UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]            UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]             UNIQUEIDENTIFIER   NULL,
    [Name]                NVARCHAR (128)     NOT NULL,
    [Description]         NVARCHAR (256)     NULL,
    [ConcurrencyStamp]    NVARCHAR (256)     NULL,
    [PasswordHashPBKDF2]  NVARCHAR (256)     NULL,
    [PasswordHashSHA256]  NVARCHAR (256)     NULL,
    [SecurityStamp]       NVARCHAR (256)     NULL,
    [IsLockedOut]         BIT                NOT NULL,
    [IsEnabled]           BIT                NOT NULL,
    [IsDeletable]         BIT                NOT NULL,
    [AccessFailedCount]   INT                NOT NULL,
    [AccessSuccessCount]  INT                NOT NULL,
    [LockoutEndUtc]       DATETIMEOFFSET (7) NULL,
    [LastLoginSuccessUtc] DATETIMEOFFSET (7) NULL,
    [LastLoginFailureUtc] DATETIMEOFFSET (7) NULL,
    [CreatedUtc]          DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]      DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Audience] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Audience_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id])
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Audience]
    ON [dbo].[tbl_Audience]([Id] ASC);

