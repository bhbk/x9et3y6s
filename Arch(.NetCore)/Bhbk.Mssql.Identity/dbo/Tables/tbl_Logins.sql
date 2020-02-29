CREATE TABLE [dbo].[tbl_Logins] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [LoginKey]    NVARCHAR (256)   NULL,
    [Enabled]     BIT              CONSTRAINT [DF_TLogins_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_TLogins_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Logins] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Logins_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[tbl_Users] ([Id])
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Logins]
    ON [dbo].[tbl_Logins]([Id] ASC);

