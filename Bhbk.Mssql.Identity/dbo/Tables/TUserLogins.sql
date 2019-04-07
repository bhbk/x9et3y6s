CREATE TABLE [dbo].[TUserLogins] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [LoginId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              CONSTRAINT [DF_TUserLogins_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TUserLogins] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginId] ASC),
    CONSTRAINT [FK_TUserLogins_LoginID] FOREIGN KEY ([LoginId]) REFERENCES [dbo].[TLogins] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_TUserLogins_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TUserLogins]
    ON [dbo].[TUserLogins]([UserId] ASC, [LoginId] ASC);

