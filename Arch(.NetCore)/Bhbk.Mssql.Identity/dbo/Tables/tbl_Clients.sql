CREATE TABLE [dbo].[tbl_Clients] (
    [Id]                 UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]           UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]            UNIQUEIDENTIFIER   NULL,
    [Name]               NVARCHAR (MAX)     NOT NULL,
    [Description]        NVARCHAR (MAX)     NULL,
    [ClientKey]          NVARCHAR (MAX)     NOT NULL,
    [ClientType]         NVARCHAR (64)      NOT NULL,
    [Enabled]            BIT                CONSTRAINT [DF_AppAudience_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]            DATETIME2 (7)      NOT NULL,
    [LockoutEnabled]     BIT                NOT NULL,
    [LockoutEnd]         DATETIMEOFFSET (7) NULL,
    [LastLoginSuccess]   DATETIME2 (7)      NULL,
    [LastLoginFailure]   DATETIME2 (7)      NULL,
    [AccessFailedCount]  INT                CONSTRAINT [DF_tbl_Clients_AccessFailedCount] DEFAULT ((0)) NOT NULL,
    [AccessSuccessCount] INT                CONSTRAINT [DF_tbl_Clients_AccessSuccessCount] DEFAULT ((0)) NOT NULL,
    [LastUpdated]        DATETIME2 (7)      NULL,
    [Immutable]          BIT                CONSTRAINT [DF_AppAudience_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Clients_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuers] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Clients]
    ON [dbo].[tbl_Clients]([Id] ASC);

