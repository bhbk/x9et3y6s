CREATE TABLE [dbo].[tbl_Audience] (
    [Id]                 UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]           UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]            UNIQUEIDENTIFIER   NULL,
    [Name]               NVARCHAR (128)     NOT NULL,
    [Description]        NVARCHAR (256)     NULL,
    [ConcurrencyStamp]   NVARCHAR (256)     NULL,
    [PasswordHashPBKDF2] NVARCHAR (256)     NULL,
    [PasswordHashSHA256] NVARCHAR (256)     NULL,
    [SecurityStamp]      NVARCHAR (256)     NULL,
    [Enabled]            BIT                CONSTRAINT [DF_tbl_Audience_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]            DATETIME2 (7)      NOT NULL,
    [LockoutEnabled]     BIT                NOT NULL,
    [LockoutEnd]         DATETIMEOFFSET (7) NULL,
    [LastLoginSuccess]   DATETIME2 (7)      NULL,
    [LastLoginFailure]   DATETIME2 (7)      NULL,
    [AccessFailedCount]  INT                CONSTRAINT [DF_tbl_Client_AccessFailedCount] DEFAULT ((0)) NOT NULL,
    [AccessSuccessCount] INT                CONSTRAINT [DF_tbl_Client_AccessSuccessCount] DEFAULT ((0)) NOT NULL,
    [Immutable]          BIT                CONSTRAINT [DF_tbl_Audience_Immutable] DEFAULT ((0)) NOT NULL,
    [LastUpdated]        DATETIME2 (7)      NULL,
    CONSTRAINT [PK_tbl_Audience] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Audience_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Audience]
    ON [dbo].[tbl_Audience]([Id] ASC);

