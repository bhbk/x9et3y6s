CREATE TABLE [dbo].[TClientUrls] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ClientId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Url]         NVARCHAR (MAX)   NULL,
    [Path]        NVARCHAR (MAX)   NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_TClientUrls] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TClientUrls_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[TClients] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TClientUrls]
    ON [dbo].[TClientUrls]([Id] ASC);

