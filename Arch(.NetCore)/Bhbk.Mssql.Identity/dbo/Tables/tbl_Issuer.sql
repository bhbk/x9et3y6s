CREATE TABLE [dbo].[tbl_Issuer] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [IssuerKey]   NVARCHAR (1024)  NOT NULL,
    [Enabled]     BIT              CONSTRAINT [DF_tbl_Issuer_Enabled] DEFAULT ((0)) NOT NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_Issuer_Immutable] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_Issuer] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Issuer]
    ON [dbo].[tbl_Issuer]([Id] ASC);

