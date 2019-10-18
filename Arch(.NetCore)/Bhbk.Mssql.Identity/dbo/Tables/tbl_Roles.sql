CREATE TABLE [dbo].[tbl_Roles] (
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
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Roles_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[tbl_Clients] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Roles]
    ON [dbo].[tbl_Roles]([Id] ASC);

