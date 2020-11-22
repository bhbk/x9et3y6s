CREATE TABLE [dbo].[tbl_UserRole] (
    [UserId]      UNIQUEIDENTIFIER   NOT NULL,
    [RoleId]      UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable] BIT                NOT NULL,
    [CreatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_UserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_tbl_UserRole_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Role] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserRole_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserRole]
    ON [dbo].[tbl_UserRole]([UserId] ASC, [RoleId] ASC);

