CREATE TABLE [dbo].[AppActivity] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [ActivityType] VARCHAR (32)     NOT NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    [TableName]    VARCHAR (MAX)    NOT NULL,
    [KeyValues]    VARCHAR (MAX)    NULL,
    [OldValues]    VARCHAR (MAX)    NULL,
    [NewValues]    VARCHAR (MAX)    NULL,
    CONSTRAINT [PK_AppActivity] PRIMARY KEY CLUSTERED ([Id] ASC)
);

