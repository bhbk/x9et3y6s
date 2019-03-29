CREATE TABLE [dbo].[AppLogin] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (MAX)   NOT NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [LoginKey]    NVARCHAR (MAX)   NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_AppLogin] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppLogin_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[AppUser] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppLogin]
    ON [dbo].[AppLogin]([Id] ASC);

