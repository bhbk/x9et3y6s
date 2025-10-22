CREATE TABLE [dbo].[tbl_ChatPromptHistory] (
    [Id]         UNIQUEIDENTIFIER   NOT NULL,
    [UserId]     UNIQUEIDENTIFIER   NOT NULL,
    [PromptText] NVARCHAR (2048)    NOT NULL,
    [Created]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_ChatPromptHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_ChatPromptHistory_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_ChatPromptHistory]
    ON [dbo].[tbl_ChatPromptHistory]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_ChatPromptHistory_UserId]
    ON [dbo].[tbl_ChatPromptHistory]([UserId] ASC);
