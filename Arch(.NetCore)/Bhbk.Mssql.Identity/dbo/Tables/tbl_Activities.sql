CREATE TABLE [dbo].[tbl_Activities] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [UserId]         UNIQUEIDENTIFIER NULL,
    [AudienceId]     UNIQUEIDENTIFIER NULL,
    [ActivityType]   NVARCHAR (64)    NOT NULL,
    [TableName]      NVARCHAR (MAX)   NULL,
    [KeyValues]      NVARCHAR (MAX)   NULL,
    [OriginalValues] NVARCHAR (MAX)   NULL,
    [CurrentValues]  NVARCHAR (MAX)   NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [Immutable]      BIT              NOT NULL,
    CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Activities_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audiences] ([Id]),
    CONSTRAINT [FK_Activities_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id])
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Activity]
    ON [dbo].[tbl_Activities]([Id] ASC);

