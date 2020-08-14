CREATE TABLE [dbo].[tbl_UserLogin] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [LoginId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Immutable] BIT              CONSTRAINT [DF_tbl_UserLogin_Immutable] DEFAULT ((0)) NOT NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_UserLogin] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginId] ASC),
    CONSTRAINT [FK_tbl_UserLogin_LoginID] FOREIGN KEY ([LoginId]) REFERENCES [dbo].[tbl_Login] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserLogin_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserLogin]
    ON [dbo].[tbl_UserLogin]([UserId] ASC, [LoginId] ASC);

