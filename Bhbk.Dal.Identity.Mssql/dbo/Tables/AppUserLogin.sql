CREATE TABLE [dbo].[AppUserLogin] (
    [UserId]              UNIQUEIDENTIFIER NOT NULL,
    [LoginId]             UNIQUEIDENTIFIER NOT NULL,
    [ActorId]             UNIQUEIDENTIFIER NULL,
    [LoginProvider]       NVARCHAR (256)   NOT NULL,
    [ProviderDisplayName] NVARCHAR (256)   NOT NULL,
    [ProviderDescription] NVARCHAR (256)   NULL,
    [ProviderKey]         NVARCHAR (256)   NULL,
    [Enabled]             BIT              NOT NULL,
    [Created]             DATETIME2 (7)    NOT NULL,
    [LastUpdated]         DATETIME2 (7)    NULL,
    [Immutable]           BIT              NOT NULL,
    CONSTRAINT [PK_AppUserLogin] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginId] ASC),
    CONSTRAINT [FK_AppUserLogin_LoginID] FOREIGN KEY ([LoginId]) REFERENCES [dbo].[AppLogin] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_AppUserLogin_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUserLogin]
    ON [dbo].[AppUserLogin]([UserId] ASC, [LoginId] ASC);

