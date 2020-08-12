CREATE TABLE [dbo].[tbl_QueueEmails] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [ActorId]          UNIQUEIDENTIFIER NULL,
    [FromId]           UNIQUEIDENTIFIER NULL,
    [FromEmail]        VARCHAR (128)    NULL,
    [FromDisplay]      VARCHAR (128)    NULL,
    [ToId]             UNIQUEIDENTIFIER NOT NULL,
    [ToEmail]          VARCHAR (128)    NOT NULL,
    [ToDisplay]        VARCHAR (128)    NULL,
    [Subject]          VARCHAR (256)    NOT NULL,
    [HtmlContent]      VARCHAR (MAX)    NULL,
    [PlaintextContent] VARCHAR (MAX)    NULL,
    [Created]          DATETIME2 (7)    NOT NULL,
    [SendAt]           DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_QueueEmails] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_QueueEmails_UserID] FOREIGN KEY ([FromId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_QueueEmails]
    ON [dbo].[tbl_QueueEmails]([Id] ASC);

