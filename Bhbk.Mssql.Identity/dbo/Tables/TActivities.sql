CREATE TABLE [dbo].[TActivities] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER NULL,
    [ActivityType]   NVARCHAR (64)    NOT NULL,
    [TableName]      NVARCHAR (MAX)   NULL,
    [KeyValues]      NVARCHAR (MAX)   NULL,
    [OriginalValues] NVARCHAR (MAX)   NULL,
    [CurrentValues]  NVARCHAR (MAX)   NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [Immutable]      BIT              NOT NULL,
    CONSTRAINT [PK_TActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TActivities_ID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[TUsers] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TActivity]
    ON [dbo].[TActivities]([Id] ASC);

