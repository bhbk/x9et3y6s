CREATE TABLE [dbo].[AppLogin] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [LoginProvider] NVARCHAR (256)   NOT NULL,
    CONSTRAINT [PK_AppLogin] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppLogin]
    ON [dbo].[AppLogin]([Id] ASC);

