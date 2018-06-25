CREATE TABLE [dbo].[AppUserRole] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [RoleId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              NOT NULL,
    CONSTRAINT [PK_AppUserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_AppUserRole_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AppRole] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_AppUserRole_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUserRole]
    ON [dbo].[AppUserRole]([UserId] ASC, [RoleId] ASC);

