CREATE TABLE [dbo].[tbl_AudienceRole] (
    [AudienceId]      UNIQUEIDENTIFIER                                   NOT NULL,
    [RoleId]          UNIQUEIDENTIFIER                                   NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN  NOT NULL DEFAULT (GETUTCDATE()),
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_AudienceRole] PRIMARY KEY CLUSTERED ([AudienceId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_tbl_AudienceRole_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_AudienceRole_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Role] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_AudienceRole], DATA_CONSISTENCY_CHECK=ON));




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_AudienceRole]
    ON [dbo].[tbl_AudienceRole]([AudienceId] ASC, [RoleId] ASC);

