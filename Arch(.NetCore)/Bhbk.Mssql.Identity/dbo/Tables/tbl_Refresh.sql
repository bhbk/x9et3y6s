CREATE TABLE [dbo].[tbl_Refresh] (
    [Id]           UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]     UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]   UNIQUEIDENTIFIER   NULL,
    [UserId]       UNIQUEIDENTIFIER   NULL,
    [RefreshValue] NVARCHAR (2048)    NOT NULL,
    [RefreshType]  NVARCHAR (64)      NOT NULL,
    [ValidFromUtc] DATETIMEOFFSET (7) NOT NULL,
    [ValidToUtc]   DATETIMEOFFSET (7) NOT NULL,
    [IssuedUtc]    DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Refresh] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Refresh_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]),
    CONSTRAINT [FK_tbl_Refresh_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Refresh_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Refresh]
    ON [dbo].[tbl_Refresh]([Id] ASC);

