CREATE TABLE [dbo].[AppUserToken] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [Code]       VARCHAR (MAX)    NOT NULL,
    [IssuedUtc]  DATETIME2 (7)    NOT NULL,
    [ExpiresUtc] DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_AppUserToken] PRIMARY KEY CLUSTERED ([Id] ASC, [UserId] ASC),
    CONSTRAINT [FK_AppUserToken_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUserToken]
    ON [dbo].[AppUserToken]([Id] ASC, [UserId] ASC);

