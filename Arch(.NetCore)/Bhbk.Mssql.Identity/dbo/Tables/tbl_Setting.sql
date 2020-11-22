CREATE TABLE [dbo].[tbl_Setting] (
    [Id]          UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]    UNIQUEIDENTIFIER   NULL,
    [AudienceId]  UNIQUEIDENTIFIER   NULL,
    [UserId]      UNIQUEIDENTIFIER   NULL,
    [ConfigKey]   VARCHAR (128)      NOT NULL,
    [ConfigValue] VARCHAR (1024)     NOT NULL,
    [IsDeletable] BIT                NOT NULL,
    [CreatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Setting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Setting_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Setting_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Setting_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Setting]
    ON [dbo].[tbl_Setting]([Id] ASC);

