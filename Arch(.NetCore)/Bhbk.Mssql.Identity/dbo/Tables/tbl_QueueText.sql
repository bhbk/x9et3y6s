CREATE TABLE [dbo].[tbl_QueueText] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [ActorId]         UNIQUEIDENTIFIER NULL,
    [FromId]          UNIQUEIDENTIFIER NULL,
    [FromPhoneNumber] VARCHAR (15)     NULL,
    [ToId]            UNIQUEIDENTIFIER NOT NULL,
    [ToPhoneNumber]   VARCHAR (15)     NOT NULL,
    [Body]            VARCHAR (MAX)    NOT NULL,
    [Created]         DATETIME2 (7)    NOT NULL,
    [SendAt]          DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_QueueText] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_QueueText_UserID] FOREIGN KEY ([FromId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_QueueText]
    ON [dbo].[tbl_QueueText]([Id] ASC);

