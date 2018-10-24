﻿CREATE TABLE [dbo].[AppAudience] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [ClientId]     UNIQUEIDENTIFIER NOT NULL,
    [ActorId]      UNIQUEIDENTIFIER NULL,
    [Name]         NVARCHAR (128)   NOT NULL,
    [Description]  NVARCHAR (256)   NULL,
    [AudienceType] NVARCHAR (64)    NOT NULL,
    [Enabled]      BIT              CONSTRAINT [DF_AppAudience_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    [LastUpdated]  DATETIME2 (7)    NULL,
    [Immutable]    BIT              CONSTRAINT [DF_AppAudience_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AppAudience_ID] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppAudience_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[AppClient] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE NONCLUSTERED INDEX [IX_AppAudience_ClientID]
    ON [dbo].[AppAudience]([ClientId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppAudience_ID]
    ON [dbo].[AppAudience]([Id] ASC);
