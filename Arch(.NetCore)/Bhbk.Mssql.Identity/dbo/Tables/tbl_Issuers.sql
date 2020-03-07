CREATE TABLE [dbo].[tbl_Issuers] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [IssuerKey]   NVARCHAR (1024)  NOT NULL,
    [Enabled]     BIT              CONSTRAINT [DF_tbl_Issuers_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_Issuers_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Issuers] PRIMARY KEY CLUSTERED ([Id] ASC)
);




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Issuers]
    ON [dbo].[tbl_Issuers]([Id] ASC);

