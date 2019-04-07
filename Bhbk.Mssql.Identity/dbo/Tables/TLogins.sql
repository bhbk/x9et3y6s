﻿CREATE TABLE [dbo].[TLogins] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (MAX)   NOT NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [LoginKey]    NVARCHAR (MAX)   NULL,
    [Enabled]     BIT              CONSTRAINT [DF_TLogins_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_TLogins_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TLogins] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TLogins_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[TUsers] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TLogins]
    ON [dbo].[TLogins]([Id] ASC);

