CREATE TABLE [dbo].[tbl_Issuer] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [Name]           NVARCHAR (128)     NOT NULL,
    [Description]    NVARCHAR (256)     NULL,
    [IssuerKey]      NVARCHAR (1024)    NOT NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Issuer] PRIMARY KEY CLUSTERED ([Id] ASC)
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Issuer]
    ON [dbo].[tbl_Issuer]([Id] ASC);

