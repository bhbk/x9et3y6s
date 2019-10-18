CREATE TABLE [dbo].[tbl_UserRoles] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [RoleId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              CONSTRAINT [DF_TUserRoles_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_UserRoles_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Roles] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_UserRoles_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserRoles]
    ON [dbo].[tbl_UserRoles]([UserId] ASC, [RoleId] ASC);

