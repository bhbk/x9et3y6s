CREATE TABLE [dbo].[TRoles] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [ClientId]         UNIQUEIDENTIFIER NOT NULL,
    [ActorId]          UNIQUEIDENTIFIER NULL,
    [Name]             NVARCHAR (MAX)   NOT NULL,
    [Description]      NVARCHAR (MAX)   NULL,
    [Enabled]          BIT              CONSTRAINT [DF_AppRole_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]          DATETIME2 (7)    NOT NULL,
    [LastUpdated]      DATETIME2 (7)    NULL,
    [ConcurrencyStamp] NVARCHAR (MAX)   NULL,
    [Immutable]        BIT              CONSTRAINT [DF_AppRole_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TRoles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TRoles_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[TClients] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TRoles]
    ON [dbo].[TRoles]([Id] ASC);

