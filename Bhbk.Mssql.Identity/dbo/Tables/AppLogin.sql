CREATE TABLE [dbo].[AppLogin] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [ActorId]       UNIQUEIDENTIFIER NULL,
    [LoginProvider] NVARCHAR (128)   NOT NULL,
    [Immutable]     BIT              NOT NULL,
    CONSTRAINT [PK_AppLogin] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppLogin]
    ON [dbo].[AppLogin]([Id] ASC);

