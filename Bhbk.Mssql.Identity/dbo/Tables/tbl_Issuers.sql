CREATE TABLE [dbo].[tbl_Issuers] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (MAX)   NOT NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [IssuerKey]   NVARCHAR (MAX)   NOT NULL,
    [Enabled]     BIT              CONSTRAINT [DF_TIssuers_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_TIssuers_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Issuers] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Issuers]
    ON [dbo].[tbl_Issuers]([Id] ASC);

