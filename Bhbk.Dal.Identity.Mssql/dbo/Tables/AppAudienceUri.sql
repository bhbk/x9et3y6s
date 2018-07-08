CREATE TABLE [dbo].[AppAudienceUri] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [AudienceId]  UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NOT NULL,
    [AbsoluteUri] VARCHAR (MAX)    NOT NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_AppAudienceUri] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppAudienceUri_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[AppUser] ([Id]),
    CONSTRAINT [FK_AppAudienceUri_ID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[AppAudience] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppAudienceUri]
    ON [dbo].[AppAudienceUri]([Id] ASC);

