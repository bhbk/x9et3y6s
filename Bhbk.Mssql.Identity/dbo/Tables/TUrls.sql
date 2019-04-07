CREATE TABLE [dbo].[TUrls] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ClientId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [UrlHost]     NVARCHAR (MAX)   NULL,
    [UrlPath]     NVARCHAR (MAX)   NULL,
    [Enabled]     BIT              CONSTRAINT [DF_TUrls_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_TUrls_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TUrls] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TUrls_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[TClients] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TUrls]
    ON [dbo].[TUrls]([Id] ASC);

