CREATE TABLE [dbo].[AppAudienceUri] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [AudienceId]  UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NOT NULL,
    [AbsoluteUri] NVARCHAR (MAX)   NOT NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_AppAudienceUri] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppAudienceUri_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[AppUser] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_AppAudienceUri_ID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[AppAudience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppAudienceUri]
    ON [dbo].[AppAudienceUri]([Id] ASC);

