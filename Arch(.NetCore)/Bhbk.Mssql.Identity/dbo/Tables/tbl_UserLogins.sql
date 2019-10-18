CREATE TABLE [dbo].[tbl_UserLogins] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [LoginId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              CONSTRAINT [DF_TUserLogins_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_UserLogins] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginId] ASC),
    CONSTRAINT [FK_UserLogins_LoginID] FOREIGN KEY ([LoginId]) REFERENCES [dbo].[tbl_Logins] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_UserLogins_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserLogins]
    ON [dbo].[tbl_UserLogins]([UserId] ASC, [LoginId] ASC);

