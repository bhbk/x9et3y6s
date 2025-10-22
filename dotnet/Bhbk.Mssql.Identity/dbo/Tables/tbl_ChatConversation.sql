CREATE TABLE [dbo].[tbl_ChatConversation] (
    [Id]          UNIQUEIDENTIFIER   NOT NULL,
    [UserId]      UNIQUEIDENTIFIER   NOT NULL,
    [Title]       NVARCHAR (256)     NULL,
    [Started]  DATETIMEOFFSET (7) NOT NULL,
    [Ended]    DATETIMEOFFSET (7) NULL,
    [IsDeleted]   BIT                NOT NULL,
    [Created]  DATETIMEOFFSET (7) NOT NULL,
    [Modified] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_ChatConversation] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_ChatConversation_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_ChatConversation]
    ON [dbo].[tbl_ChatConversation]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_ChatConversation_UserId]
    ON [dbo].[tbl_ChatConversation]([UserId] ASC);
