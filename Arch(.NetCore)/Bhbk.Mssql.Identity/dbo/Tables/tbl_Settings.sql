CREATE TABLE [dbo].[tbl_Settings] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]    UNIQUEIDENTIFIER NULL,
    [ClientId]    UNIQUEIDENTIFIER NULL,
    [UserId]      UNIQUEIDENTIFIER NULL,
    [ConfigKey]   VARCHAR (MAX)    NOT NULL,
    [ConfigValue] VARCHAR (MAX)    NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Settings_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[tbl_Clients] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Settings_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Settings_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Settings]
    ON [dbo].[tbl_Settings]([Id] ASC);

