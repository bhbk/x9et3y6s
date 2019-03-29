CREATE TABLE [dbo].[AppActivity] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER NULL,
    [ActivityType]   NVARCHAR (128)   NOT NULL,
    [TableName]      NVARCHAR (MAX)   NULL,
    [KeyValues]      NVARCHAR (MAX)   NULL,
    [OriginalValues] NVARCHAR (MAX)   NULL,
    [CurrentValues]  NVARCHAR (MAX)   NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [Immutable]      BIT              NOT NULL,
    CONSTRAINT [PK_AppActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppActivity_ID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[AppUser] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppActivity]
    ON [dbo].[AppActivity]([Id] ASC);

