CREATE TABLE [dbo].[tbl_ChatFile] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [ConversationId] UNIQUEIDENTIFIER   NOT NULL,
    [MessageId]      UNIQUEIDENTIFIER   NULL,
    [FileName]       NVARCHAR (256)     NOT NULL,
    [ContentType]    VARCHAR (128)      NOT NULL,
    [FileSize]       BIGINT             NOT NULL,
    [FileContent]    VARBINARY (MAX)    NOT NULL,
    [Summary]        NVARCHAR (512)     NULL,
    [Expires]     DATETIMEOFFSET (7) NOT NULL,
    [Created]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_ChatFile] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_ChatFile_ConversationID] FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[tbl_ChatConversation] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_ChatFile_MessageID] FOREIGN KEY ([MessageId]) REFERENCES [dbo].[tbl_ChatMessage] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_ChatFile]
    ON [dbo].[tbl_ChatFile]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_ChatFile_ConversationId]
    ON [dbo].[tbl_ChatFile]([ConversationId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_ChatFile_Expires]
    ON [dbo].[tbl_ChatFile]([Expires] ASC);
