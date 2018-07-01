CREATE TABLE [dbo].[AppActivity] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER NOT NULL,
    [ActivityType]   VARCHAR (32)     NOT NULL,
    [TableName]      VARCHAR (MAX)    NULL,
    [KeyValues]      VARCHAR (MAX)    NULL,
    [OriginalValues] VARCHAR (MAX)    NULL,
    [CurrentValues]  VARCHAR (MAX)    NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [Immutable]      BIT              NOT NULL,
    CONSTRAINT [PK_AppActivity] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppActivity]
    ON [dbo].[AppActivity]([Id] ASC);

