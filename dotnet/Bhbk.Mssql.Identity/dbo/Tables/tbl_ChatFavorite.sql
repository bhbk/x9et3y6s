CREATE TABLE [dbo].[tbl_ChatFavorite] (
    [Id]       UNIQUEIDENTIFIER   NOT NULL,
    [UserId]   UNIQUEIDENTIFIER   NOT NULL,
    [Name]     NVARCHAR (256)     NOT NULL,
    [Prompt]   NVARCHAR (2048)    NOT NULL,
    [Pinned]   BIT                NOT NULL DEFAULT 0,
    [Created]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_ChatFavorite] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_ChatFavorite_UserID] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_ChatFavorite]
    ON [dbo].[tbl_ChatFavorite]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_ChatFavorite_UserId]
    ON [dbo].[tbl_ChatFavorite]([UserId] ASC);
