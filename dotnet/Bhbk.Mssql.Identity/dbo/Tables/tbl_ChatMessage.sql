CREATE TABLE [dbo].[tbl_ChatMessage] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [ConversationId] UNIQUEIDENTIFIER   NOT NULL,
    [Role]           VARCHAR (32)       NOT NULL,
    [Content]        NVARCHAR (MAX)     NULL,
    [ToolCalls]      NVARCHAR (MAX)     NULL,
    [ToolResults]    NVARCHAR (MAX)     NULL,
    [InputTokens]    INT                NULL,
    [OutputTokens]   INT                NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_ChatMessage] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_ChatMessage_ConversationID] FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[tbl_ChatConversation] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_ChatMessage]
    ON [dbo].[tbl_ChatMessage]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_ChatMessage_ConversationId]
    ON [dbo].[tbl_ChatMessage]([ConversationId] ASC);
