CREATE TABLE [dbo].[AppRole] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [ClientId]         UNIQUEIDENTIFIER NOT NULL,
    [ActorId]          UNIQUEIDENTIFIER NULL,
    [Name]             NVARCHAR (128)   NOT NULL,
    [NormalizedName]   NVARCHAR (128)   NULL,
    [Description]      NVARCHAR (256)   NULL,
    [Enabled]          BIT              CONSTRAINT [DF_AppRole_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]          DATETIME2 (7)    NOT NULL,
    [LastUpdated]      DATETIME2 (7)    NULL,
    [ConcurrencyStamp] NVARCHAR (MAX)   NULL,
    [Immutable]        BIT              CONSTRAINT [DF_AppRole_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AppRole_ID] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppRole_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[AppClient] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppRole_ID]
    ON [dbo].[AppRole]([Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AppRole_ClientID]
    ON [dbo].[AppRole]([ClientId] ASC);

