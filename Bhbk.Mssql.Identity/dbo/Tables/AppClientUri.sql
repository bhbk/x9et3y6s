CREATE TABLE [dbo].[AppClientUri] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ClientId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [AbsoluteUri] NVARCHAR (MAX)   NOT NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_AppClientUri] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppClientUri_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[AppClient] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppClientUri]
    ON [dbo].[AppClientUri]([Id] ASC);

