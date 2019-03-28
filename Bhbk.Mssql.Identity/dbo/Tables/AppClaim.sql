CREATE TABLE [dbo].[AppClaim] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Subject]     NVARCHAR (MAX)   NULL,
    [Type]        NVARCHAR (MAX)   NOT NULL,
    [Value]       NVARCHAR (MAX)   NOT NULL,
    [ValueType]   NVARCHAR (MAX)   NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_AppClaim] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppClaim_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_AppClaim_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[AppIssuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppClaim]
    ON [dbo].[AppClaim]([Id] ASC);

