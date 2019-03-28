CREATE TABLE [dbo].[AppLogin] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [LoginKey]    NVARCHAR (MAX)   NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_AppLogin] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppLogin]
    ON [dbo].[AppLogin]([Id] ASC);

