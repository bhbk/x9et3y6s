CREATE TABLE [dbo].[tbl_Urls] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ClientId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [UrlHost]     NVARCHAR (MAX)   NULL,
    [UrlPath]     NVARCHAR (MAX)   NULL,
    [Enabled]     BIT              CONSTRAINT [DF_TUrls_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_TUrls_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Urls] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Urls_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[tbl_Clients] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Urls]
    ON [dbo].[tbl_Urls]([Id] ASC);

