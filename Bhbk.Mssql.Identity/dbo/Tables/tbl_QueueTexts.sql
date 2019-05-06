CREATE TABLE [dbo].[tbl_QueueTexts] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [ActorId]         UNIQUEIDENTIFIER NOT NULL,
    [FromId]          UNIQUEIDENTIFIER NOT NULL,
    [FromPhoneNumber] VARCHAR (15)     NULL,
    [ToId]            UNIQUEIDENTIFIER NOT NULL,
    [ToPhoneNumber]   VARCHAR (15)     NULL,
    [Body]            VARCHAR (MAX)    NOT NULL,
    [Created]         DATETIME2 (7)    NOT NULL,
    [SendAt]          DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_QueueTexts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_QueueTexts_UserID] FOREIGN KEY ([FromId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_QueueTexts]
    ON [dbo].[tbl_QueueTexts]([Id] ASC);

